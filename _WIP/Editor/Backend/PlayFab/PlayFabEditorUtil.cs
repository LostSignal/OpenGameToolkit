#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="PlayFabEditorUtil.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_PLAYFAB

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Text;
    using global::PlayFab;
    using global::PlayFab.AdminModels;
    using UnityEngine;

    public static class PlayFabEditorUtil
    {
        private static readonly OGTLogger Logger = OGTLogger.OGTEditor;

        // Title Data
        public static GetTitleDataResult GetTitleData(string titleId, string secretKey, string key)
        {
            return SendRequest<GetTitleDataResult>("/Admin/GetTitleData", titleId, secretKey, new GetTitleDataRequest { Keys = new List<string> { key } });
        }

        public static void SetTitleData(string titleId, string secretKey, string key, object value)
        {
            SetTitleData(titleId, secretKey, key, JsonUtil.Serialize(value));
        }

        public static void SetTitleData(string titleId, string secretKey, string key, string value)
        {
            SendRequest<object>("/Admin/SetTitleData", titleId, secretKey, new SetTitleDataRequest { Key = key, Value = value });
        }

        // Currencies
        public static ListVirtualCurrencyTypesResult GetCurrencies(string titleId, string secretKey)
        {
            return SendRequest<ListVirtualCurrencyTypesResult>("/Admin/ListVirtualCurrencyTypes", titleId, secretKey, new ListVirtualCurrencyTypesRequest {});
        }

        public static BlankResult AddCurrencies(string titleId, string secretKey, List<VirtualCurrencyData> virtualCurrencies)
        {
            return SendRequest<BlankResult>("/Admin/AddVirtualCurrencyTypes", titleId, secretKey, new AddVirtualCurrencyTypesRequest { VirtualCurrencies = virtualCurrencies });
        }

        // Drop Tables
        public static GetRandomResultTablesResult GetDropTables(string titleId, string secretKey, string catalogVersion)
        {
            return SendRequest<GetRandomResultTablesResult>("/Admin/GetRandomResultTables", titleId, secretKey, new GetRandomResultTablesRequest { CatalogVersion = catalogVersion });
        }

        public static UpdateRandomResultTablesResult UpdateDropTables(string titleId, string secretKey, string catalogVersion, List<RandomResultTable> randomResultTables)
        {
            return SendRequest<UpdateRandomResultTablesResult>("/Admin/UpdateRandomResultTables", titleId, secretKey, new UpdateRandomResultTablesRequest { CatalogVersion = catalogVersion, Tables = randomResultTables });
        }

        // Catalog Items
        public static GetCatalogItemsResult GetCatalogItems(string titleId, string secretKey, string catalogVersion)
        {
            return SendRequest<GetCatalogItemsResult>("/Admin/GetCatalogItems", titleId, secretKey, new GetCatalogItemsRequest { CatalogVersion = catalogVersion });
        }

        public static UpdateCatalogItemsResult UpdateCatalogItems(string titleId, string secretKey, string catalogVersion, List<CatalogItem> catalog, bool isDefault)
        {
            return SendRequest<UpdateCatalogItemsResult>("/Admin/UpdateCatalogItems", titleId, secretKey, new UpdateCatalogItemsRequest { Catalog = catalog, CatalogVersion = catalogVersion, SetAsDefaultCatalog = isDefault });
        }

        // Stores
        public static GetStoreItemsResult GetStoreItems(string titleId, string secretKey, string catalogVersion, string storeId)
        {
            return SendRequest<GetStoreItemsResult>("/Admin/GetStoreItems", titleId, secretKey, new GetStoreItemsRequest { CatalogVersion = catalogVersion, StoreId = storeId });
        }

        public static UpdateStoreItemsResult UpdateStoreItems(string titleId, string secretKey, string catalogVersion, string storeId, List<StoreItem> storeItems)
        {
            return SendRequest<UpdateStoreItemsResult>("/Admin/UpdateStoreItems", titleId, secretKey, new UpdateStoreItemsRequest { CatalogVersion = catalogVersion, StoreId = storeId, Store = storeItems });
        }

        public static T SendRequest<T>(string endpoint, string titleId, string secretKey, object request)
        {
            try
            {
                // Converting Request object to json and byte[]
                string jsonRequest = JsonUtil.Serialize(request);
                byte[] data = Encoding.UTF8.GetBytes(jsonRequest);

                // Constructing the WebResponse
                Uri uri = new Uri("https://" + titleId + ".playfabapi.com" + endpoint);
                WebRequest webRequest = WebRequest.Create(uri);
                webRequest.Method = "POST";
                webRequest.Headers.Clear();
                webRequest.Headers.Add("X-SecretKey", secretKey);
                webRequest.ContentType = "application/json";
                webRequest.ContentLength = data.Length;

                Stream requestStream = webRequest.GetRequestStream();
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();

                // Sending and getting a response
                WebResponse webResponse = webRequest.GetResponse();
                Stream responseStream = webResponse.GetResponseStream();
                StreamReader responseReader = new StreamReader(responseStream);

                string json = responseReader.ReadToEnd();
                Logger.Log(json);

                // Closing everything up
                responseStream.Close();
                webResponse.Close();

                var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                var httpResult = jsonPlugin.DeserializeObject<HttpResult<T>>(json);
                return httpResult.data;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return default(T);
            }
        }

        public class HttpResult<T>
        {
#pragma warning disable
            public string status { get; set; }

            public int code { get; set; }

            public T data { get; set; }
#pragma warning restore
        }
    }
}

#endif
