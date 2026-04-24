//-----------------------------------------------------------------------
// <copyright file="InventoryManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY && USING_PLAYFAB

namespace OGT.PlayFab
{
    using System.Collections.Generic;
    using global::PlayFab.ClientModels;
    using global::PlayFab.Internal;

    public class InventoryManager
    {
        private readonly PlayFabManager playfabManager;

        private List<ItemInstance> usersInventoryCache;
        private bool getInventoryCoroutineRunning;

        public InventoryManager(PlayFabManager playfabManager, List<ItemInstance> userInventory)
        {
            this.playfabManager = playfabManager;
            this.usersInventoryCache = userInventory;
        }

        public delegate void OnInventoryChangedDelegate();

        public event OnInventoryChangedDelegate InventoryChanged;

        //// public InventoryHelper()
        //// {
        ////     PF.GlobalPlayFabResultHandler += this.OnGlobalPlayFabResultHandler;
        //// }

        public void InvalidateUserInventory()
        {
            if (this.getInventoryCoroutineRunning == false)
            {
                this.usersInventoryCache = null;
                this.InventoryChanged?.Invoke();
            }
        }

        public UnityTask<List<ItemInstance>> GetInventoryItems()
        {
            if (this.usersInventoryCache != null)
            {
                return UnityTask<List<ItemInstance>>.Empty(this.usersInventoryCache);
            }
            else
            {
                return UnityTask<List<ItemInstance>>.Run(GetInventoryItemsCoroutine());
            }

            IEnumerator<List<ItemInstance>> GetInventoryItemsCoroutine()
            {
                // If it's already running, then wait for it to finish
                if (this.getInventoryCoroutineRunning)
                {
                    while (this.getInventoryCoroutineRunning)
                    {
                        yield return default;
                    }

                    yield return this.usersInventoryCache;
                    yield break;
                }

                this.getInventoryCoroutineRunning = true;

                var playfabGetInventory = this.playfabManager.Do(new GetUserInventoryRequest());

                while (playfabGetInventory.IsDone == false)
                {
                    yield return default;
                }

                this.getInventoryCoroutineRunning = false;

                yield return this.usersInventoryCache;
            }
        }

        public UnityTask<int> GetInventoryCount(string itemId)
        {
            return UnityTask<int>.Run(GetInventoryCountCoroutine());

            IEnumerator<int> GetInventoryCountCoroutine()
            {
                var inventory = this.GetInventoryItems();

                while (inventory.IsDone == false)
                {
                    yield return default;
                }

                int count = 0;

                if (inventory.HasError == false)
                {
                    List<ItemInstance> items = inventory.Value;

                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i].ItemId == itemId)
                        {
                            int? remainingUses = items[i].RemainingUses;

                            if (remainingUses.HasValue)
                            {
                                count += remainingUses.Value;
                            }
                        }
                    }
                }

                yield return count;
            }
        }

        private void OnGlobalPlayFabResultHandler(PlayFabRequestCommon request, PlayFabResultCommon result)
        {
            if (result is LoginResult loginResult)
            {
                this.UpdateInventory(loginResult.InfoResultPayload?.UserInventory);
            }
            else if (result is GetUserInventoryResult getUserInventoryResult)
            {
                this.UpdateInventory(getUserInventoryResult?.Inventory);
            }
            else if (result is GetPlayerCombinedInfoResult getPlayerCombinedInfoResult)
            {
                this.UpdateInventory(getPlayerCombinedInfoResult.InfoResultPayload?.UserInventory);
            }
        }

        private void UpdateInventory(List<ItemInstance> inventory)
        {
            if (this.usersInventoryCache == null)
            {
                this.usersInventoryCache = new List<ItemInstance>();
            }
            else
            {
                this.usersInventoryCache.Clear();
            }

            if (inventory.IsNullOrEmpty() == false)
            {
                this.usersInventoryCache.AddRange(inventory);
                this.InventoryChanged?.Invoke();
            }
        }

        // public delegate void OnUserInventoryChagnedDelegate();
        //// public event OnUserInventoryChagnedDelegate UserInventoryChanged;
        ////
        //// private void InternalAddCatalogItemToInventory(string catalogItemId, int count)
        //// {
        ////     var inventoryItem = this.userInventory.FirstOrDefault(x => x.Id == catalogItemId);
        ////
        ////     if (inventoryItem != null)
        ////     {
        ////         inventoryItem.Count += count;
        ////     }
        ////     else
        ////     {
        ////         this.userInventory.Add(new InventoryItem() { Id = catalogItemId, Count = count });
        ////     }
        ////
        ////     this.OnVirtualCurrenciesChanged();
        //// }
        ////
        //// public void InternalAddBundleItemToInventory(BundleItem bundleItem)
        //// {
        ////     foreach (var item in bundleItem.Items)
        ////     {
        ////         if (item.Type == BundleItemType.VirtualCurrency)
        ////         {
        ////             this.InternalAddVirtualCurrencyToInventory(item.Id, item.Count);
        ////         }
        ////         else if (item.Type == BundleItemType.CatalogItem)
        ////         {
        ////             this.InternalAddCatalogItemToInventory(item.Id, item.Count);
        ////         }
        ////         else
        ////         {
        ////             Lost.Logger.LogErrorFormat("PlayFabServer.InternalAddBundleItemToInventory found unknown BundleItemType {0} on bundle item id {1}", item.Type, bundleItem.Id);
        ////         }
        ////     }
        //// }
        ////
        //// private void UpdateUserInventory(List<ItemInstance> userInventory)
        //// {
        ////     this.userInventory.Clear();
        ////
        ////     foreach (ItemInstance item in userInventory)
        ////     {
        ////         this.userInventory.Add(new InventoryItem()
        ////         {
        ////             Id = item.ItemId,
        ////             Count = item.RemainingUses.HasValue ? item.RemainingUses.Value : 1,
        ////             Data = item.CustomData,
        ////             InstanceId = item.ItemInstanceId,
        ////         });
        ////     }
        ////
        ////     this.OnUserInventoryChanged();
        //// }

        ////
        //// public int GetInventoryCount(string itemId)
        //// {
        ////     var inventoryItem = this.userInventory.FirstOrDefault(x => x.Id == itemId);
        ////     return inventoryItem != null ? inventoryItem.Count : 0;
        //// }

        //// protected void InternalAddStoreItemToInventory(StoreItem storeItem)
        //// {
        ////     // deducting the virtual currency
        ////     this.InternalAddVirtualCurrencyToInventory(storeItem.CostCurrencyId, -storeItem.Cost);
        ////
        ////     // adding the item to the inventory
        ////     if (storeItem.Type == StoreItemType.BundleItem)
        ////     {
        ////         this.InternalAddBundleItemToInventory(storeItem.BundleItem);
        ////     }
        ////     else if (storeItem.Type == StoreItemType.CatalogItem)
        ////     {
        ////         this.InternalAddCatalogItemToInventory(storeItem.CatalogItem.Id, 1);
        ////     }
        ////     else
        ////     {
        ////         Lost.Logger.LogErrorFormat("PlayFabServer.InternalAddStoreItemToInventory found unknown StoreItemType {0} on store item id {1}", storeItem.Type, storeItem.ItemId);
        ////     }
        //// }
        ////
        //// protected void OnUserInventoryChanged()
        //// {
        ////     if (this.UserInventoryChanged != null)
        ////     {
        ////         this.UserInventoryChanged();
        ////     }
        //// }

        //// // inventory
        //// private List<InventoryItem> userInventory = new List<InventoryItem>();
        //// public IEnumerable<IInventoryItem> UserInventory
        //// {
        ////     get
        ////     {
        ////         for (int i = 0; i < this.userInventory.Count; i++)
        ////         {
        ////             yield return this.userInventory[i];
        ////         }
        ////     }
        //// }
        ////
        //// public bool HasInventoryItem(string itemId)
        //// {
        ////     return this.GetInventoryCount(itemId) > 0;
        //// }
    }
}

#endif
