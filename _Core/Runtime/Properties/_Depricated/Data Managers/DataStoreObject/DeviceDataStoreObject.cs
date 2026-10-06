//-----------------------------------------------------------------------
// <copyright file="DeviceDataStoreObject.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Lost/Data Stores/Device Data Store")]
    public class DeviceDataStoreObject : DataStoreObject
    {
        public override void Load()
        {
            if (Platform.DoesLocalFileExist(this.name))
            {
                Platform.GetLocalFile(this.name, Caching.ByteBuffer);
                this.DataStore.Deserialize(Caching.ByteBuffer);
            }
        }

        public override void Save()
        {
            if (this.DataStore.IsDirty)
            {
                int length = this.DataStore.Serialize(Caching.ByteBuffer);
                Platform.SaveLocalFile(this.name, Caching.ByteBuffer, 0, length);
            }
        }
    }
}

#endif
