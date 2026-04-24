//-----------------------------------------------------------------------
// <copyright file="AzureAddressablesUploadRuntimeSettings.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Settings
{
    public static class AzureAddressablesUploadRuntimeSettings
    {
        public static string DownloadUrl
        {
            get => RuntimeSettings.GetSetting<string>("AzureAddressablesUpload.DownloadUrl");
            set => RuntimeSettings.SetSetting("AzureAddressablesUpload.DownloadUrl", value);
        }
    }
}
