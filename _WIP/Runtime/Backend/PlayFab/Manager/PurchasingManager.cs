//-----------------------------------------------------------------------
// <copyright file="PurchasingManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY && USING_PLAYFAB

#if false && USING_UNITY_PURCHASING && !UNITY_XBOXONE && !UNITY_LUMIN
#define PURCHASING_ENABLED
#endif

namespace OGT.PlayFab
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using global::PlayFab;
    using global::PlayFab.ClientModels;
    using OGT.IAP;
    using UnityEngine;

#if PURCHASING_ENABLED
    using UnityEngine.Purchasing;
#endif

    using UnityEngine.SceneManagement;

    public class PurchasingManager
    {
        private static readonly OGTLogger Logger = new OGTLogger("Purchasing");

        private readonly PlayFabManager playfabManager;

        private bool isInitializationRunning;

        public PurchasingManager(PlayFabManager playfabManager)
        {
            this.playfabManager = playfabManager;
        }

        public bool IsInitialized { get; private set; }

        public UnityTask<bool> Initialize()
        {
            return UnityTask<bool>.Run(InitializeCoroutine());

            IEnumerator<bool> InitializeCoroutine()
            {
                if (this.IsInitialized)
                {
                    yield return true;
                    yield break;
                }

                // If it's already running, then wait for it to finish and leave
                if (this.isInitializationRunning)
                {
                    while (this.isInitializationRunning)
                    {
                        yield return default;
                    }

                    yield return this.IsInitialized;
                    yield break;
                }

                this.isInitializationRunning = true;

                // TODO [bgish]: Run initialization code
                var getCatalog = this.playfabManager.Catalog.GetCatalog();

                while (getCatalog.IsDone == false)
                {
                    yield return default;
                }

#if PURCHASING_ENABLED
                // initializing purchasing, but no need to wait on it
                if (getCatalog.HasError == false && UnityPurchasingManager.Instance.IsIAPInitialized == false)
                {
                    var initializeUnityIap = UnityPurchasingManager.Instance.InitializeUnityPurchasing((store, builder) =>
                    {
                        foreach (var catalogItem in getCatalog.Value)
                        {
                            if (catalogItem.GetVirtualCurrenyPrice("RM") > 0)
                            {
                                builder.AddProduct(catalogItem.ItemId, this.GetProductType(catalogItem));
                            }
                        }
                    });

                    // Waiting for initialization to finish
                    while (initializeUnityIap.IsDone == false)
                    {
                        yield return default;
                    }
                }
#endif

                this.isInitializationRunning = false;

#if PURCHASING_ENABLED
                this.IsInitialized = getCatalog.HasError == false && UnityPurchasingManager.IsInitialized;
#else
                this.IsInitialized = getCatalog.HasError == false;
#endif

                yield return this.IsInitialized;
            }
        }

        public UnityTask<bool> PurchaseStoreItem(string storeId, StoreItem storeItem, Action showStore = null)
        {
            bool isIapItem = storeItem.GetVirtualCurrenyPrice("RM") > 0;

            if (isIapItem)
            {
                return UnityTask<bool>.Run(PurchaseIAPStoreItemCoroutine());
            }
            else
            {
                return UnityTask<bool>.Run(PurchaseStoreItemCoroutine());
            }

            UnityTask<bool> Initialize()
            {
                return UnityTask<bool>.Run(InitializeCoroutine());

                IEnumerator<bool> InitializeCoroutine()
                {
#if PURCHASING_ENABLED
                    // Making sure we're properly initialized
                    if (this.IsInitialized == false)
                    {
                        var initialize = this.Initialize();

                        while (initialize.IsDone == false)
                        {
                            yield return default;
                        }

                        // Early out if initialization failed
                        if (initialize.HasError || this.IsInitialized == false)
                        {
                            if (initialize.HasError)
                            {
                                PlayFabMessages.HandleError(initialize.Exception);
                            }

                            yield return false;
                            yield break;
                        }
                    }
#endif

                    yield break;
                }
            }

            IEnumerator<bool> PurchaseIAPStoreItemCoroutine()
            {
#if PURCHASING_ENABLED

                // Making sure we're initialized
                var initialize = Initialize();

                while (initialize.IsDone == false)
                {
                    yield return false;
                }

                if (initialize.HasError || this.IsInitialized == false)
                {
                    yield break;
                }

                // Start with the iap purchasing
                var iapPurchaseItem = UnityPurchasingManager.Instance.PurchaseProduct(storeItem.ItemId);

                while (iapPurchaseItem.IsDone == false)
                {
                    yield return default;
                }

                if (iapPurchaseItem.HasError)
                {
                    PlayFabMessages.HandleError(iapPurchaseItem.Exception);
                }
                else
                {
                    if (Debug.isDebugBuild || Application.isEditor)
                    {
                        var purchase = UnityTask<bool>.Run(DebugPurchaseStoreItemCoroutine(iapPurchaseItem.Value));

                        while (purchase.IsDone == false)
                        {
                            yield return default;
                        }

                        yield return purchase.Value;
                    }
                    else
                    {
                        var purchase = UnityTask<bool>.Run(ProcessPurchaseCoroutine(iapPurchaseItem.Value));

                        while (purchase.IsDone == false)
                        {
                            yield return default;
                        }

                        yield return purchase.Value;
                    }
                }

#else
                throw new NotImplementedException("Trying to buy IAP Store Item when USING_UNITY_PURCHASING is not defined!");
#endif
            }

            IEnumerator<bool> PurchaseStoreItemCoroutine()
            {
                // Making sure we're initialized
                var initialize = Initialize();

                while (initialize.IsDone == false)
                {
                    yield return false;
                }

                if (initialize.HasError || this.IsInitialized == false)
                {
                    yield break;
                }

                // Start with the purchasing
                string virtualCurrency = storeItem.GetVirtualCurrencyId();
                int storeItemCost = storeItem.GetCost(virtualCurrency);

                bool hasSufficientFunds = this.playfabManager.VirtualCurrency[virtualCurrency] >= storeItemCost;

                if (hasSufficientFunds == false)
                {
                    var messageBoxDialog = PlayFabMessages.ShowInsufficientCurrency();

                    while (messageBoxDialog.IsRunning())
                    {
                        yield return default;
                    }

                    if (messageBoxDialog.GetAwaiter().GetResult() == YesNoResult.Yes)
                    {
                        showStore?.Invoke();
                    }

                    yield break;
                }

                var coroutine = this.Do(new PurchaseItemRequest
                {
                    ItemId = storeItem.ItemId,
                    Price = storeItemCost,
                    VirtualCurrency = virtualCurrency,
                    StoreId = storeId,
                });

                while (coroutine.keepWaiting)
                {
                    yield return default;
                }

                bool isSuccessful = coroutine.Value != null;

                if (isSuccessful)
                {
                    this.playfabManager.Inventory.InvalidateUserInventory();
                }

                yield return isSuccessful;
            }

#if PURCHASING_ENABLED
            IEnumerator<bool> DebugPurchaseStoreItemCoroutine(PurchaseEventArgs e)
            {
                // TODO [bgish]: Need to find a way to do this that works for all projects
                UnityTask<object> coroutine = null; /* PF.CloudScript.Execute("DebugPurchaseItem", new
                {
                    itemId = e.purchasedProduct.definition.id,
                    catalogVersion = this.catalogVersion,
                });*/

                while (coroutine.keepWaiting)
                {
                    yield return default;
                }

                if (coroutine.Value != null)
                {
                    var getCatalogItem = this.playfabManager.Catalog.GetCatalogItem(e.purchasedProduct.definition.id);

                    while (getCatalogItem.IsDone == false)
                    {
                        yield return default;
                    }

                    if (getCatalogItem.Value != null)
                    {
                        this.playfabManager.Inventory.InvalidateUserInventory();
                        yield return true;
                    }
                    else
                    {
                        Lost.Logger.LogErrorFormat("DebugPurchaseStoreItem found unknown ItemId {0}", e.purchasedProduct.definition.id);
                    }
                }
            }

            IEnumerator<bool> ProcessPurchaseCoroutine(PurchaseEventArgs e)
            {
                var getCatalogItem = this.playfabManager.Catalog.GetCatalogItem(e.purchasedProduct.definition.id);

                while (getCatalogItem.IsDone == false)
                {
                    yield return default;
                }

                var catalogItem = getCatalogItem.Value;

                Lost.Logger.AssertFormat(catalogItem != null, "Couln't find CatalogItem {0}", e.purchasedProduct.definition.id);

                Lost.Logger.AssertFormat(e.purchasedProduct.hasReceipt, "Purchased item {0} doesn't have a receipt", e.purchasedProduct.definition.id);
                var receipt = JsonUtil.Deserialize<Dictionary<string, object>>(e.purchasedProduct.receipt);
                Lost.Logger.AssertFormat(receipt != null, "Unable to parse receipt {0}", e.purchasedProduct.receipt);

                var store = (string)receipt["Store"];
                var transactionId = (string)receipt["TransactionID"];
                var payload = (string)receipt["Payload"];

                Lost.Logger.AssertFormat(string.IsNullOrEmpty(store) == false, "Unable to parse store from {0}", e.purchasedProduct.receipt);
                Lost.Logger.AssertFormat(string.IsNullOrEmpty(transactionId) == false, "Unable to parse transactionID from {0}", e.purchasedProduct.receipt);
                Lost.Logger.AssertFormat(string.IsNullOrEmpty(payload) == false, "Unable to parse payload from {0}", e.purchasedProduct.receipt);

                var currencyCode = e.purchasedProduct.metadata.isoCurrencyCode;
                var purchasePrice = this.GetPurchasePrice(e);
                bool wasSuccessfull = false;

                if (Platform.CurrentDevicePlatform == DevicePlatform.iOS)
                {
                    var validate = this.playfabManager.Do(new ValidateIOSReceiptRequest()
                    {
                        CurrencyCode = currencyCode,
                        PurchasePrice = purchasePrice,
                        ReceiptData = payload,
                    });

                    while (validate.IsDone == false)
                    {
                        yield return default;
                    }

                    if (validate.HasError)
                    {
                        Lost.Logger.LogErrorFormat("Unable to validate iOS IAP Purchase: {0}", validate?.Exception?.ToString());
                        this.LogErrorReceiptFailedInfo(catalogItem, e);
                        Lost.Logger.LogErrorFormat("ReceiptData = " + payload);

                        PlayFabMessages.HandleError(validate.Exception);
                    }
                    else
                    {
                        wasSuccessfull = true;
                    }
                }
                else if (Platform.CurrentDevicePlatform == DevicePlatform.Android)
                {
                    var details = JsonUtil.Deserialize<Dictionary<string, object>>(payload);
                    Lost.Logger.AssertFormat(details != null, "Unable to parse Receipt Payload {0}", payload);

                    Lost.Logger.AssertFormat(details.ContainsKey("json"), "Receipt Payload doesn't have \"json\" key: {0}", payload);
                    Lost.Logger.AssertFormat(details.ContainsKey("signature"), "Receipt Payload doesn't have \"signature\" key: {0}", payload);

                    var receiptJson = (string)details["json"];
                    var signature = (string)details["signature"];

                    var validate = this.playfabManager.Do(new ValidateGooglePlayPurchaseRequest()
                    {
                        CurrencyCode = currencyCode,
                        PurchasePrice = (uint)purchasePrice,
                        ReceiptJson = receiptJson,
                        Signature = signature,
                    });

                    while (validate.IsDone == false)
                    {
                        yield return default;
                    }

                    if (validate.HasError)
                    {
                        Lost.Logger.LogErrorFormat("Unable to validate Google Play IAP Purchase: {0}", validate?.Exception?.ToString());
                        this.LogErrorReceiptFailedInfo(catalogItem, e);
                        Lost.Logger.LogErrorFormat("Receipt Json = " + receiptJson);
                        Lost.Logger.LogErrorFormat("Signature = " + signature);

                        PlayFabMessages.HandleError(validate.Exception);
                    }
                    else
                    {
                        wasSuccessfull = true;
                    }
                }
                else
                {
                    Lost.Logger.LogErrorFormat("Tried to make IAP Purchase on Unknown Platform {0}", Application.platform.ToString());
                }

                if (wasSuccessfull == false)
                {
                    yield break;
                }

                float price = catalogItem.GetVirtualCurrenyPrice("RM") / 100.0f;
                string itemId = e.purchasedProduct.definition.id;
                string itemType = catalogItem.ItemClass;
                string level = SceneManager.GetActiveScene().name;

                Analytics.AnalyticsEvent.IAPTransaction(storeId, price, itemId, itemType, level, transactionId, new Dictionary<string, object>
                {
                    { "localized_price", e.purchasedProduct.metadata.localizedPrice },
                    { "iso_currency_code", e.purchasedProduct.metadata.isoCurrencyCode },
                    { "store", store },
                });

                this.playfabManager.Inventory.InvalidateUserInventory();

                yield return true;
            }

#endif
        }

#if PURCHASING_ENABLED

        private int GetPurchasePrice(PurchaseEventArgs e)
        {
            int fractionalUnit;

            switch (e.purchasedProduct.metadata.isoCurrencyCode)
            {
                //// https://en.wikipedia.org/wiki/List_of_circulating_currencies
                ////
                //// case "BHD": // Bahrain
                //// case "IQD": // Iraq
                //// case "KWD": // Kuwait
                //// case "LYD": // Libya
                //// case "OMR": // Oman
                //// case "TND": // Tunisia
                ////     {
                ////         fractionalUnit = 1000;
                ////         break;
                ////     }
                ////
                //// case "MGA": // Madagascar
                //// case "MRU": // Mauritania, Sahrawi Republic (Mauritanian ouguiya)
                ////     {
                ////         fractionalUnit = 5;
                ////         break;
                ////     }
                ////
                //// case "VUV": // Vanuatu
                ////     {
                ////         fractionalUnit = 1;
                ////         break;
                ////     }
                ////
                //// NOTE [bgish]: May need this for IAP to work in Vietname
                //// case "VND": // Vietname
                ////     {
                ////         fractionalUnit = 10;
                ////         break;
                ////     }

                default:
                    {
                        fractionalUnit = 100;
                        break;
                    }
            }

            return (int)(e.purchasedProduct.metadata.localizedPrice * fractionalUnit);
        }

        private void LogErrorReceiptFailedInfo(CatalogItem catalogItem, PurchaseEventArgs e)
        {
            StringBuilder itemDataBuilder = new StringBuilder();
            itemDataBuilder.Append("Item Id = ");
            itemDataBuilder.Append(catalogItem?.ItemId);
            itemDataBuilder.AppendLine();
            itemDataBuilder.Append("Item RM = ");
            itemDataBuilder.Append(catalogItem?.GetVirtualCurrenyPrice("RM"));
            itemDataBuilder.AppendLine();
            itemDataBuilder.Append("Currency Code = ");
            itemDataBuilder.Append(e.purchasedProduct.metadata.isoCurrencyCode);
            itemDataBuilder.AppendLine();
            itemDataBuilder.Append("Localized Price = ");
            itemDataBuilder.Append(e.purchasedProduct.metadata.localizedPrice);
            itemDataBuilder.AppendLine();
            itemDataBuilder.Append("Purchase Price = ");
            itemDataBuilder.Append(this.GetPurchasePrice(e));

            Lost.Logger.LogError(itemDataBuilder.ToString());
        }

        private ProductType GetProductType(CatalogItem catalogItem)
        {
            return catalogItem.Consumable != null ?
                UnityEngine.Purchasing.ProductType.Consumable :
                UnityEngine.Purchasing.ProductType.NonConsumable;
        }

#endif

        private UnityTask<PurchaseItemResult> Do(PurchaseItemRequest request)
        {
            if (string.IsNullOrEmpty(request.CatalogVersion))
            {
                request.CatalogVersion = this.playfabManager.CatalogVersion;
            }

            return this.playfabManager.Do<PurchaseItemRequest, PurchaseItemResult>(request, PlayFabClientAPI.PurchaseItemAsync);
        }
    }
}

#endif
