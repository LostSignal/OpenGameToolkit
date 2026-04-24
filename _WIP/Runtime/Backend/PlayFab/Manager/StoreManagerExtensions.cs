//-----------------------------------------------------------------------
// <copyright file="StoreManagerExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_PLAYFAB

namespace OGT.PlayFab
{
    using global::PlayFab.ClientModels;

    public static class StoreManagerExtensions
    {
        public static int GetVirtualCurrenyPrice(this StoreItem storeItem, string currency)
        {
            if (storeItem.VirtualCurrencyPrices.TryGetValue(currency, out uint price))
            {
                return (int)price;
            }

            return 0;
        }

        public static string GetVirtualCurrencyId(this StoreItem storeItem)
        {
            foreach (var currency in storeItem.VirtualCurrencyPrices)
            {
                if (currency.Key != "RM")
                {
                    return currency.Key;
                }
            }

            return null;
        }

        public static int GetCost(this StoreItem storeItem, string virtualCurrencyId)
        {
            if (storeItem.VirtualCurrencyPrices.TryGetValue(virtualCurrencyId, out uint cost))
            {
                return (int)cost;
            }

            return 0;
        }
    }
}

#endif
