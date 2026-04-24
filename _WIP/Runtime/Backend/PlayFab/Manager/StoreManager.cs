//-----------------------------------------------------------------------
// <copyright file="StoreManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY && USING_PLAYFAB

namespace OGT.PlayFab
{
    using System.Collections.Generic;
    using global::PlayFab;
    using global::PlayFab.ClientModels;

    public class StoreManager
    {
        private readonly GetStoreItemsRequest getStoreRequest = new GetStoreItemsRequest();
        private readonly Dictionary<string, List<StoreItem>> cachedStores = new Dictionary<string, List<StoreItem>>();
        private readonly PlayFabManager playfabManager;

        public StoreManager(PlayFabManager playfabManager)
        {
            this.playfabManager = playfabManager;
        }

        public UnityTask<List<StoreItem>> GetStore(string storeId, bool forceRefresh = false)
        {
            if (forceRefresh)
            {
                this.cachedStores.Remove(storeId);
            }

            if (this.cachedStores.ContainsKey(storeId))
            {
                return UnityTask<List<StoreItem>>.Empty(this.cachedStores[storeId]);
            }
            else
            {
                return UnityTask<List<StoreItem>>.Run(FetchStore());
            }

            IEnumerator<List<StoreItem>> FetchStore()
            {
                this.getStoreRequest.StoreId = storeId;
                this.getStoreRequest.CatalogVersion = this.playfabManager.CatalogVersion;

                var getStore = this.playfabManager.Do<GetStoreItemsRequest, GetStoreItemsResult>(this.getStoreRequest, PlayFabClientAPI.GetStoreItemsAsync);

                while (getStore.IsDone == false)
                {
                    yield return null;
                }

                var store = getStore.Value?.Store;

                // Caching off the store in case the user requests it again
                if (store != null)
                {
                    this.cachedStores.Add(storeId, store);
                }

                yield return store;
            }
        }

        public void InvalidateStoreCache()
        {
            this.cachedStores.Clear();
        }
    }
}

#endif
