//-----------------------------------------------------------------------
// <copyright file="UgsBackend.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UGS_AUTH && USING_UGS_ECONOMY && USING_UGS_CLOUDSAVE && USING_UGS_CLOUDCODE

namespace OGT.Backend
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Unity.Services.Authentication;
    using Unity.Services.CloudCode;
    using Unity.Services.CloudSave;
    using Unity.Services.Core;
    using Unity.Services.Economy;
    using Unity.Services.Economy.Model;
    using UnityEngine;

    public sealed class UgsBackend : Manager, IBackend
    {
        public event Action OnLogIn;
        public event Action OnReLogInRequired;
        public event Action<string, int> OnCurrencyChanged;

        private readonly Dictionary<string, int> currencyBalances = new();
        private readonly Dictionary<string, string> userDataCache = new();
        private bool configurationSynced;
        private bool initialized;

        public bool IsLoggedIn =>
            initialized &&
            AuthenticationService.Instance != null &&
            AuthenticationService.Instance.IsSignedIn;

        protected override Task InitializeManager(Bootloader bootloader)
        {
            return Task.CompletedTask;
        }

        public async Task<LoginResult> AnonymousLogin(LoginInfo loginInfo)
        {
            await EnsureInitialized();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.Expired += HandleSessionExpired;
                AuthenticationService.Instance.SignedOut += HandleSignedOut;

                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            await SyncConfiguration();
            await RefreshVirtualCurrency();

            // Preload user data keys if provided
            if (loginInfo?.UserDataKeys != null && loginInfo.UserDataKeys.Count > 0)
            {
                await PreloadUserData(loginInfo.UserDataKeys);
            }

            OnLogIn?.Invoke();

            return new LoginResult
            {
                PlayerId = AuthenticationService.Instance.PlayerId,
                PlayerDisplayName = AuthenticationService.Instance.PlayerName,
                UserData = new Dictionary<string, string>(userDataCache),
                TitleData = new Dictionary<string, string>()
            };
        }

        public async Task<string> GetUserData(string key)
        {
            // Check cache first
            if (userDataCache.TryGetValue(key, out string cachedValue))
            {
                return cachedValue;
            }

            await EnsureLoggedIn();

            try
            {
                // Load the specific key from Cloud Save
                var keys = new HashSet<string> { key };
                var result = await CloudSaveService.Instance.Data.Player.LoadAsync(keys, new Unity.Services.CloudSave.Models.Data.Player.LoadOptions());

                if (result != null && result.TryGetValue(key, out var item))
                {
                    string value = item.Value?.GetAsString();
                    if (value != null)
                    {
                        userDataCache[key] = value;
                    }
                    return value;
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        public async Task<List<StoreItem>> GetStoreItems(string storeId)
        {
            await EnsureLoggedIn();
            await SyncConfiguration();

            // In UGS Economy, "store items" are usually Virtual Purchases.
            // This assumes your storeId maps to CustomData["storeId"].
            var purchases = EconomyService.Instance.Configuration.GetVirtualPurchases();

            return purchases
                .Where(p => BelongsToStore(p, storeId))
                .Select(ToStoreItem)
                .OrderBy(x => x)
                .ToList();
        }

        public async Task<CatalogItem> GetCatalogItem(string itemId)
        {
            await EnsureLoggedIn();
            await SyncConfiguration();

            var inventoryItems = EconomyService.Instance.Configuration.GetInventoryItems();
            var item = inventoryItems.FirstOrDefault(x => x.Id == itemId);

            if (item == null)
            {
                return null;
            }

            return ToCatalogItem(item);
        }

        public async Task<int> GetInventoryCount(string itemId)
        {
            await EnsureLoggedIn();

            var options = new GetInventoryOptions
            {
                InventoryItemIds = new List<string> { itemId },
                ItemsPerFetch = 100,
                PlayersInventoryItemIds = null
            };

            var result = await EconomyService.Instance.PlayerInventory.GetInventoryAsync(options);
            return result.PlayersInventoryItems.Count;
        }

        public async Task PurchaseStoreItem(StoreItem storeItem)
        {
            await EnsureLoggedIn();

            if (storeItem == null || string.IsNullOrWhiteSpace(storeItem.ItemId))
            {
                throw new ArgumentException("Invalid store item.");
            }

            await EconomyService.Instance.Purchases.MakeVirtualPurchaseAsync(storeItem.ItemId);

            await RefreshVirtualCurrency();
        }

        public async Task<bool> CanAfford(StoreItem storeItem)
        {
            await EnsureLoggedIn();

            if (storeItem == null)
            {
                return false;
            }

            // Check virtual currency prices
            if (storeItem.VirtualCurrencyPrices != null)
            {
                foreach (var price in storeItem.VirtualCurrencyPrices)
                {
                    int currentBalance = GetCurrency(price.Key);
                    if (currentBalance < (int)price.Value)
                    {
                        return false;
                    }
                }
            }

            // If we have real currency prices but no virtual currency prices,
            // we can't determine affordability client-side (requires platform-specific checks)
            // Return true to allow the purchase attempt (it will fail server-side if not affordable)
            if (storeItem.RealCurrencyPrices != null && storeItem.RealCurrencyPrices.Count > 0 &&
                (storeItem.VirtualCurrencyPrices == null || storeItem.VirtualCurrencyPrices.Count == 0))
            {
                return true;
            }

            return true;
        }

        public async Task RefreshVirtualCurrency()
        {
            await EnsureLoggedIn();

            var balances = await EconomyService.Instance.PlayerBalances.GetBalancesAsync();

            foreach (var balance in balances.Balances)
            {
                int oldAmount = GetCurrency(balance.CurrencyId);
                int newAmount = (int)balance.Balance;

                currencyBalances[balance.CurrencyId] = newAmount;

                if (oldAmount != newAmount)
                {
                    OnCurrencyChanged?.Invoke(balance.CurrencyId, newAmount);
                }
            }
        }

        public void InternalAddVirtualCurrencyToInventory(string virtualCurrencyId, int amount)
        {
            int newAmount = GetCurrency(virtualCurrencyId) + amount;
            currencyBalances[virtualCurrencyId] = newAmount;
            OnCurrencyChanged?.Invoke(virtualCurrencyId, newAmount);
        }

        public int GetCurrency(string currencyId)
        {
            return currencyBalances.TryGetValue(currencyId, out int amount)
                ? amount
                : 0;
        }

        public async Task<CloudScriptResult> ExecuteCloudScript(
            string functionName,
            object functionParameter = null)
        {
            await EnsureLoggedIn();

            try
            {
                var args = ToArgs(functionParameter);

                object response = await CloudCodeService.Instance.CallEndpointAsync<object>(
                    functionName,
                    args);

                return new CloudScriptResult
                {
                    Success = true,
                    ErrorCode = 0,
                    ErrorMessage = null,
                    JsonResult = JsonUtil.Serialize(response)
                };
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);

                return new CloudScriptResult
                {
                    Success = false,
                    ErrorCode = ex.HResult,
                    ErrorMessage = ex.Message,
                    JsonResult = null
                };
            }
        }

        public async Task<CloudScriptResult<T>> ExecuteCloudScript<T>(
            string functionName,
            object functionParameter = null)
        {
            await EnsureLoggedIn();

            try
            {
                var args = ToArgs(functionParameter);

                T response = await CloudCodeService.Instance.CallEndpointAsync<T>(
                    functionName,
                    args);

                return new CloudScriptResult<T>
                {
                    Success = true,
                    ErrorCode = 0,
                    ErrorMessage = null,
                    JsonResult = JsonUtil.Serialize(response)
                };
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);

                return new CloudScriptResult<T>
                {
                    Success = false,
                    ErrorCode = ex.HResult,
                    ErrorMessage = ex.Message,
                    JsonResult = null
                };
            }
        }

        private async Task EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            await UnityServices.InitializeAsync();
            initialized = true;
        }

        private async Task EnsureLoggedIn()
        {
            await EnsureInitialized();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                OnReLogInRequired?.Invoke();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                OnLogIn?.Invoke();
            }
        }

        private async Task SyncConfiguration()
        {
            if (!configurationSynced)
            {
                await EconomyService.Instance.Configuration.SyncConfigurationAsync();
                configurationSynced = true;
            }
        }

        private async Task PreloadUserData(List<string> keys)
        {
            if (keys == null || keys.Count == 0)
            {
                return;
            }

            try
            {
                // Load all requested keys from Cloud Save in a single call
                var keySet = new HashSet<string>(keys);
                var result = await CloudSaveService.Instance.Data.Player.LoadAsync(keySet, new Unity.Services.CloudSave.Models.Data.Player.LoadOptions());

                if (result != null)
                {
                    // Cache all loaded values
                    foreach (var kvp in result)
                    {
                        string value = kvp.Value?.Value?.GetAsString();
                        if (value != null)
                        {
                            userDataCache[kvp.Key] = value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private void HandleSessionExpired()
        {
            OnReLogInRequired?.Invoke();
        }

        private void HandleSignedOut()
        {
            OnReLogInRequired?.Invoke();
        }

        private static Dictionary<string, object> ToArgs(object functionParameter)
        {
            if (functionParameter == null)
            {
                return new Dictionary<string, object>();
            }

            if (functionParameter is Dictionary<string, object> dict)
            {
                return dict;
            }

            return new Dictionary<string, object>
            {
                { "parameter", functionParameter }
            };
        }

        private static StoreItem ToStoreItem(VirtualPurchaseDefinition purchase)
        {
            var item = new StoreItem
            {
                ItemId = purchase.Id,
                RealCurrencyPrices = new Dictionary<string, uint>(),
                VirtualCurrencyPrices = new Dictionary<string, uint>()
            };

            foreach (var cost in purchase.Costs)
            {
                item.VirtualCurrencyPrices[cost.Item.GetReferencedConfigurationItem().Id] = (uint)cost.Amount;
            }

            return item;
        }

        private static CatalogItem ToCatalogItem(InventoryItemDefinition item)
        {
            return new CatalogItem
            {
                ItemId = item.Id,
                DisplayName = item.Name,
                CustomData = item.CustomDataDeserializable?.GetAsString(),
                Tags = item.CustomDataDeserializable == null ? new List<string>() : new List<string>(),
                VirtualCurrencyPrices = new Dictionary<string, uint>(),
                RealCurrencyPrices = new Dictionary<string, uint>(),
                Bundle = new CatalogItemBundleInfo
                {
                    BundledItems = new List<string>(),
                    BundledResultTables = new List<string>(),
                    BundledVirtualCurrencies = new Dictionary<string, uint>()
                }
            };
        }

        private static bool BelongsToStore(VirtualPurchaseDefinition purchase, string storeId)
        {
            if (string.IsNullOrWhiteSpace(storeId))
            {
                return true;
            }

            string customData = purchase.CustomDataDeserializable?.GetAsString();

            return !string.IsNullOrEmpty(customData) && customData.Contains(storeId, StringComparison.OrdinalIgnoreCase);
        }
    }
}

#endif
