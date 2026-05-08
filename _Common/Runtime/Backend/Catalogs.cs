//-----------------------------------------------------------------------
// <copyright file="Catalog.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Backend
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    public class Catalogs : ScriptableObject
    {
        public List<Catalog> catalogs = new();

        [Serializable]
        public class Catalog
        {
            public string version;
            public List<string> itemClasses = new();
            public List<VirtualCurrency> virtualCurrencies = new();
            public List<CatalogItem> catalogItems = new();
            public List<BundleItem> bundleItems = new();
            public List<Store> stores = new();
        }

        [Serializable]
        public class VirtualCurrency
        {
            public string id;
            public string name;
            public int initialDeposit;
            public int rechargeRate;
            public int rechargeMax;
            public AssetReferenceSprite icon;
        }

        [Serializable]
        public class CatalogItem
        {
            public string id;
            public string itemClass;
            public int usageType;
            public LocalizedString displayName;
            public LocalizedString description;
            public bool isStackable;
            public int realMoneyCost;
        }

        [Serializable]
        public class BundleItem
        {
            public string id;
            public string itemClass;
            public LocalizedString displayName;
            public LocalizedString description;
            public List<BundleEntry> items = new();
            public int realMoneyCost;
        }

        [Serializable]
        public class BundleEntry
        {
            public string id;
            public int type;
            public int count;
        }

        [Serializable]
        public class Store
        {
            public string id;
            public List<StoreItem> storeItems = new();
        }

        [Serializable]
        public class StoreItem
        {
            public int type;
            public string itemId;
            public string costCurrencyId;
            public int cost;
            public string purchaseDescription;
            public AssetReferenceSprite purchaseIcon;
        }

#if !UNITY
        public class AssetReferenceSprite
        {
            public string m_AssetGUID;
            public string m_SubObjectName;
            public string m_SubObjectType = null;
            public string m_SubObjectGUID;
        }
#endif
    }
}
