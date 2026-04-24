//-----------------------------------------------------------------------
// <copyright file="UnityPurchasingManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

#if false && USING_UNITY_PURCHASING && !UNITY_XBOXONE && !UNITY_LUMIN
#define PURCHASING_ENABLED
#endif

namespace OGT.IAP
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;

#if PURCHASING_ENABLED
    using UnityEngine.Purchasing;
#endif

#if PURCHASING_ENABLED
    public sealed class UnityPurchasingManager : Manager<UnityPurchasingManager>, IStoreListener
#else
    public sealed class UnityPurchasingManager : Manager
#endif
    {
#if PURCHASING_ENABLED
        // initialization
        private IStoreController controller;
        private ConfigurationBuilder builder;
        private InitializationState initializationState;
        private InitializationFailureReason initializationFailureReason;

        // purchasing
        private PurchasingState purchasingState;
        private PurchaseFailureReason purchaseFailureReason;
        private PurchaseEventArgs purchaseEventArgs;

        private enum InitializationState
        {
            Initializing,
            InitializeFailed,
            InitializedSucceeded,
        }

        private enum PurchasingState
        {
            PurchasingWaiting,
            Purchasing,
            PurchasingFailed,
            PurchasingSucceeded,
        }

        public bool IsIAPInitialized
        {
            get { return this.initializationState == InitializationState.InitializedSucceeded; }
        }
#endif

        public static UnityPurchasingManager Instance
        {
            get
            {
                Debug.LogError("UnityPurchasingManager.Instance no longer supported");
                return GameObject.FindAnyObjectByType<Bootloader>().FindManager<UnityPurchasingManager>();
            }
        }

#if !USING_UNITY_PURCHASING && UNITY_EDITOR

        //// [ShowEditorInfo]
        public string GetInfoMessage() => "Unity IAP Package is not present.  Unity Purchasing Manager will be ignored.";

        //// [ExposeInEditor("Add Unity IAP Package")]/
        public void AddUnityIAPPackage()
        {
            PackageManagerUtil.Add("com.unity.purchasing");
        }

#endif

        protected override Task InitializeManager(Bootloader bootloader)
        {
            throw new System.NotImplementedException();
        }

        //// public override void Initialize()
        //// {
        ////     this.SetInstance(this);
        //// }

#if !PURCHASING_ENABLED

        public string GetLocalizedPrice(string itemId)
        {
            return "N/A";
        }

#else

        public string GetLocalizedPrice(string itemId)
        {
            if (this.controller == null)
            {
                Lost.Logger.LogError("Tried to get Localized Price of {0} before inittializing UnityIAP!");
                return null;
            }

            Product product = this.controller.products.WithID(itemId);

            if (product != null)
            {
                return product.metadata.localizedPriceString;
            }

            return null;
        }

        public UnityTask<bool> InitializeUnityPurchasing(System.Action<AppStore, ConfigurationBuilder> configurationBuilder)
        {
            return UnityTask<bool>.Run(InitializeUnityPurchasingCoroutine());

            IEnumerator<bool> InitializeUnityPurchasingCoroutine()
            {
                if (this.initializationState == InitializationState.InitializedSucceeded)
                {
                    yield return true;
                    yield break;
                }

                float startTime = Time.realtimeSinceStartup;
                this.initializationState = InitializationState.Initializing;
                UnityPurchasing.Initialize(this, this.GetConfigurationBuilder(configurationBuilder));

                while (this.initializationState == InitializationState.Initializing)
                {
                    yield return default;

                    if (Time.realtimeSinceStartup - startTime > 5.0f)
                    {
                        throw new PurchasingInitializationTimeOutException();
                    }
                }

                if (this.initializationState == InitializationState.InitializedSucceeded)
                {
                    yield return true;
                }
                else
                {
                    throw new PurchasingInitializationException(this.initializationFailureReason);
                }
            }
        }

        public UnityTask<PurchaseEventArgs> PurchaseProduct(string itemId)
        {
            return UnityTask<PurchaseEventArgs>.Run(this.PurchaseProductCoroutine(itemId));
        }

        void IStoreListener.OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            this.initializationState = InitializationState.InitializedSucceeded;
            this.controller = controller;
        }

        void IStoreListener.OnInitializeFailed(InitializationFailureReason error)
        {
            this.initializationState = InitializationState.InitializeFailed;
            this.initializationFailureReason = error;
        }

        PurchaseProcessingResult IStoreListener.ProcessPurchase(PurchaseEventArgs e)
        {
            this.purchasingState = PurchasingState.PurchasingSucceeded;
            this.purchaseEventArgs = e;

            return PurchaseProcessingResult.Complete;
        }

        void IStoreListener.OnPurchaseFailed(Product i, PurchaseFailureReason p)
        {
            this.purchasingState = PurchasingState.PurchasingFailed;
            this.purchaseFailureReason = p;
        }

        private ConfigurationBuilder GetConfigurationBuilder(System.Action<AppStore, ConfigurationBuilder> configurationBuilder)
        {
            if (this.builder == null)
            {
                var module = StandardPurchasingModule.Instance();

                if (Debug.isDebugBuild || Application.isEditor)
                {
                    module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
                }

                this.builder = ConfigurationBuilder.Instance(module);

                configurationBuilder?.Invoke(module.appStore, this.builder);
            }

            return this.builder;
        }

        private IEnumerator<PurchaseEventArgs> PurchaseProductCoroutine(string itemId)
        {
            if (this.purchasingState != PurchasingState.PurchasingWaiting)
            {
                throw new PurchasingException(PurchaseFailureReason.ExistingPurchasePending);
            }

            this.purchasingState = PurchasingState.Purchasing;
            this.purchaseFailureReason = PurchaseFailureReason.Unknown;
            this.purchaseEventArgs = null;

            this.controller.InitiatePurchase(itemId);

            while (this.purchasingState == PurchasingState.Purchasing)
            {
                yield return default;
            }

            bool wasSuccessful = this.purchasingState == PurchasingState.PurchasingSucceeded;

            this.purchasingState = PurchasingState.PurchasingWaiting;

            if (wasSuccessful)
            {
                yield return this.purchaseEventArgs;
            }
            else
            {
                throw new PurchasingException(this.purchaseFailureReason);
            }
        }

#endif
    }
}

#endif
