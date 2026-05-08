//-----------------------------------------------------------------------
// <copyright file="PurchaseItem.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

#if false && USING_UNITY_PURCHASING && !UNITY_XBOXONE && !UNITY_LUMIN
#define PURCHASING_ENABLED
#endif

namespace OGT
{
    using System;
    using System.Collections;
    using System.Linq;
    using Lost;
    using OGT.Backend;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public enum PurchaseResult
    {
        Cancel,
        Buy,
    }

    public class PurchaseItem : PanelLogic
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

#pragma warning disable 0649, 0044
        [Header("Purchase Item")]
        [SerializeField] private Button cancelButton;

        [Header("Item")]
        [SerializeField] private Image storeItemIcon;
        [SerializeField] private TMP_Text storeItemTitle;
        [SerializeField] private TMP_Text storeItemDescription;

        [Header("Virtual Currency")]
        [SerializeField] private Button virtualCurrencyBuyButton;
        [SerializeField] private TMP_Text virtualCurrencyBuyButtonText;
        [SerializeField] private Image virtualCurrencyBuyButtonIcon;

        [Header("IAP")]
        [SerializeField] private Button iapBuyButton;
        [SerializeField] private TMP_Text iapBuyButtonText;

        [Header("Virtual Currencies")]
        [SerializeField] private VirtualCurrencyIcon[] virtualCurrencyIcons;
#pragma warning restore 0649, 0044

        private IBackend backend;

        private string storeId;
        private Backend.StoreItem storeItem;
        private PurchaseResult result;
        private bool isCoroutineRunning;
        private bool automaticallyPerformPurchase;

        public void ShowStoreItem(
            bool automaticallyPerformPurchase,
            string storeId,
            Backend.StoreItem storeItem,
            Sprite icon,
            string title,
            string description,
            Action insufficientFundsStore,
            Action<PurchaseResult> onPurchaseComplete)
        {
            this.automaticallyPerformPurchase = automaticallyPerformPurchase;

            CoroutineRunner.Instance.StartCoroutine(Coroutine());

            IEnumerator Coroutine()
            {
                // resetting the result, and caching the items
                this.result = PurchaseResult.Cancel;
                this.storeId = storeId;
                this.storeItem = storeItem;

                // Figuring out which currecy this item costs
                string virtualCurrencyId = null;
                uint virtualCurrencyCost = 0;

                foreach (var virtualCurrencyPrice in storeItem.VirtualCurrencyPrices)
                {
                    if (virtualCurrencyPrice.Value > 0)
                    {
                        virtualCurrencyId = virtualCurrencyPrice.Key;
                        virtualCurrencyCost = virtualCurrencyPrice.Value;
                    }
                }

                if (virtualCurrencyId == null)
                {
                    Logger.LogErrorFormat("StoreItem {0} has unknown currency.", storeItem.ItemId);
                    onPurchaseComplete?.Invoke(PurchaseResult.Cancel);
                    yield break;
                }

                bool isIapItem = virtualCurrencyId == "RM";

                // Turning on the correct button
                this.iapBuyButton.gameObject.SafeSetActive(isIapItem);
                this.virtualCurrencyBuyButton.gameObject.SafeSetActive(!isIapItem);

                if (isIapItem)
                {
#if PURCHASING_ENABLED

                    this.iapBuyButtonText.text = IAP.UnityPurchasingManager.Instance.GetLocalizedPrice(storeItem.ItemId);

#else

                    uint dollars = virtualCurrencyCost / 100;
                    uint cents = virtualCurrencyCost % 100;

                    BetterStringBuilder.New()
                        .Append("$")
                        .Append(dollars)
                        .Append(".")
                        .Append(cents < 10 ? "0" : string.Empty)
                        .Append(cents)
                        .Set(this.iapBuyButtonText);

#endif
                }
                else
                {
                    int virtualCurrencyPrice = storeItem.GetVirtualCurrencyPrice(virtualCurrencyId);
                    bool hasSufficientFunds = this.backend.GetCurrency(virtualCurrencyId) >= virtualCurrencyPrice;

                    if (hasSufficientFunds == false && insufficientFundsStore != null)
                    {
                        bool isDone = false;
                        bool wasCanceled = false;

                        this.PanelManager.ShowInsufficientCurrency(
                            () =>
                            {
                                isDone = true;
                                insufficientFundsStore.Invoke();    
                            },
                            () =>
                            {
                                wasCanceled = true;
                                isDone = true;
                            });

                        while (isDone == false)
                        {
                            yield return null;
                        }

                        if (wasCanceled)
                        {
                            onPurchaseComplete?.Invoke(PurchaseResult.Cancel);
                            yield break;
                        }
                    }

                    this.virtualCurrencyBuyButton.interactable = hasSufficientFunds;
                    this.virtualCurrencyBuyButtonIcon.sprite = this.GetSprite(virtualCurrencyId);
                    this.virtualCurrencyBuyButtonText.text = virtualCurrencyPrice.ToString();
                }

                // Setting the item image/texts
                this.storeItemIcon.sprite = icon;
                this.storeItemTitle.text = title;
                this.storeItemDescription.text = description;

                this.Panel.Show();

                // Waiting for it to start showing
                while (this.Panel.IsShown == false)
                {
                    yield return null;
                }

                // Waiting for it to return to the hidden state
                while (this.Panel.IsShown == false)
                {
                    yield return null;
                }

                onPurchaseComplete?.Invoke(this.result);
            }
        }

        public override void OnAwake(Bootloader bootloader)
        {
            base.OnAwake(bootloader);

            //// Logger.Assert(this.cancelButton != null, "PurchaseItem didn't define cancel button", this);
            //// Logger.Assert(this.storeItemIcon != null, "PurchaseItem didn't define StoreItem icon", this);
            //// Logger.Assert(this.storeItemTitle != null, "PurchaseItem didn't define StoreItem title", this);
            //// Logger.Assert(this.storeItemDescription != null, "PurchaseItem didn't define StoreItem description", this);
            //// 
            //// Logger.Assert(this.virtualCurrencyBuyButton != null, "PurchaseItem didn't define virtual currency buy button", this);
            //// Logger.Assert(this.virtualCurrencyBuyButtonText != null, "PurchaseItem didn't define virtual currency buy button text", this);
            //// Logger.Assert(this.virtualCurrencyBuyButtonIcon != null, "PurchaseItem didn't define buy virtual currency button icon", this);
            //// 
            //// Logger.Assert(this.iapBuyButton != null, "PurchaseItem didn't define iap buy button", this);
            //// Logger.Assert(this.iapBuyButtonText != null, "PurchaseItem didn't define iap buy button text", this);

            //// this.virtualCurrencyBuyButton.onClick.AddListener(this.BuyButtonClicked);
            //// this.iapBuyButton.onClick.AddListener(this.BuyButtonClicked);
            //// this.cancelButton.onClick.AddListener(this.CancelButtonClicked);
            //// 
            //// this.Panel.OnBackButtonPressed.AddListener(this.OnBackButtonPressed);
        }

        private void OnBackButtonPressed()
        {
            this.BuyButtonClicked();
        }

        private void CancelButtonClicked()
        {
            this.result = PurchaseResult.Cancel;
            this.Panel.Hide();
        }

        private void BuyButtonClicked()
        {
            this.result = PurchaseResult.Buy;

            if (this.automaticallyPerformPurchase)
            {
                this.backend.PurchaseStoreItem(this.storeItem);
            }

            this.Panel.Hide();
        }

        private Sprite GetSprite(string virtualCurrencyId)
        {
            return this.virtualCurrencyIcons.First(x => x.Id == virtualCurrencyId).Icon;
        }

#endif

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2235:Mark all non-serializable fields", Justification = "Using Unity Serialization")]
        [Serializable]
        private class VirtualCurrencyIcon
        {
#pragma warning disable 0649
            [SerializeField] private string id;
            [SerializeField] private Sprite icon;
#pragma warning restore 0649

            public string Id
            {
                get { return this.id; }
            }

            public Sprite Icon
            {
                get { return this.icon; }
            }
        }
    }
}
