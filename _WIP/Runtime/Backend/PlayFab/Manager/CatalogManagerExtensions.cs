//-----------------------------------------------------------------------
// <copyright file="CatalogManagerExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY && USING_PLAYFAB

namespace OGT.PlayFab
{
    using System.Collections.Generic;
    using global::PlayFab.ClientModels;

    public static class CatalogManagerExtensions
    {
        public static Dictionary<string, string> GetCustomData(this CatalogItem catalogItem)
        {
            return PlayFabManager.Instance.Catalog.GetCustomData(catalogItem);
        }

        public static int GetVirtualCurrenyPrice(this CatalogItem catalogItem, string currency)
        {
            if (catalogItem.VirtualCurrencyPrices.TryGetValue(currency, out uint price))
            {
                return (int)price;
            }

            return 0;
        }
    }
}

#endif
