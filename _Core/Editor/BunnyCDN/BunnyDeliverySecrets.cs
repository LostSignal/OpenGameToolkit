//-----------------------------------------------------------------------
// <copyright file="BunnyDeliverySecrets.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    //// NOTE [bgish]: Keys of the OGT Secrets written by the BunnyDeliveryEditorWindow Config tab.  Anything
    ////               that talks to Bunny (ie. BunnyCDNBuildStep) should default its SecretStrings to these.
    public static class BunnyDeliverySecrets
    {
        public const string ApiKey = "BD-ApiKey";
        public const string PullZoneId = "BD-PullZoneId";
        public const string StorageZoneName = "BD-StorageZoneName";
        public const string StorageZoneApiAccessKey = "BD-StorageZoneApiAccessKey";
        public const string StorageZoneMainReplicationRegion = "BD-StorageZoneMainReplicationRegion";
        public const string StoragePullZoneUrl = "BD-StoragePullZoneUrl";
        public const string BuildsPassword = "BD-BuildsPassword";

        // Every request to the builds folder must send this header with the BuildsPassword as its value
        public const string BuildsPasswordHeader = "X-BD-Password";

        public static readonly string[] All =
        {
            ApiKey,
            PullZoneId,
            StorageZoneName,
            StorageZoneApiAccessKey,
            StorageZoneMainReplicationRegion,
            StoragePullZoneUrl,
            BuildsPassword,
        };

        public static string Get(string key)
        {
            return SecretsStore.TryGetValue(key, out var value) ? value : string.Empty;
        }

        public static bool Has(string key)
        {
            return string.IsNullOrEmpty(Get(key)) == false;
        }
    }
}
