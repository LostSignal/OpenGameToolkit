//-----------------------------------------------------------------------
// <copyright file="DataStoreManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public enum DataStoreLocation
    {
        Device,
        Player,
        Game,
    }

    public class DataStoreManager : Manager
    {
        //// private Dictionary<int, DataStore> dataStores = new Dictionary<int, DataStore>();

        protected override Task InitializeManager(Bootloader bootloader)
        {
            return Task.CompletedTask;
        }

        //// public DataStore GetDataStore(DataStoreLocation location, string name = null)
        //// {
        ////     int key = this.GetKey(location, name);
        ////
        ////     if (this.dataStores.TryGetValue(key, out DataStore dataStore) == false)
        ////     {
        ////         dataStore = new DataStore();
        ////         this.dataStores.Add(key, dataStore);
        ////
        ////         //// NOTE [bgish]: Using local files is a hack for now.  Not all platforms support it (like consoles), and
        ////         ////               will need a mechanism for saving this type of data to places like cloud storage.
        ////         var localFileName = this.GetLocalFileName(location, name);
        ////
        ////         if (Platform.DoesLocalFileExist(localFileName))
        ////         {
        ////             Platform.GetLocalFile(localFileName, Caching.ByteBuffer);
        ////             dataStore.Deserialize(Caching.ByteBuffer);
        ////         }
        ////     }
        ////
        ////     return dataStore;
        //// }
        ////
        //// public void SaveDataStore(DataStoreLocation location, string name = null)
        //// {
        ////     var dataStore = this.GetDataStore(location, name);
        ////
        ////     if (dataStore.IsDirty)
        ////     {
        ////         //// NOTE [bgish]: Using local files is a hack for now.  Not all platforms support it (like consoles), and
        ////         ////               will need a mechanism for saving this type of data to places like cloud storage.
        ////         int length = dataStore.Serialize(Caching.ByteBuffer);
        ////         var localFileName = this.GetLocalFileName(location, name);
        ////         Platform.SaveLocalFile(localFileName, Caching.ByteBuffer, 0, length);
        ////     }
        //// }
        ////
        //// private string GetLocalFileName(DataStoreLocation location, string name)
        //// {
        ////     return string.IsNullOrWhiteSpace(name) ? $"{location}" : $"{location}_{name}";
        //// }
        ////
        //// private int GetKey(DataStoreLocation location, string name)
        //// {
        ////     int key = location.GetHashCode();
        ////
        ////     if (string.IsNullOrWhiteSpace(name) == false)
        ////     {
        ////         key = HashCode.Combine(key, name.GetHashCode());
        ////     }
        ////
        ////     return key;
        //// }
    }
}

#endif
