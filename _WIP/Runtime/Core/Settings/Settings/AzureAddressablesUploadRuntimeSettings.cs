//-----------------------------------------------------------------------
// <copyright file="AzureAddressablesUploadRuntimeSettings.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Settings
{
    public static class AzureAddressablesUploadRuntimeSettings
    {
        private static string downloadUrl;

        public static string DownloadUrl
        {
            get
            {
                if (string.IsNullOrEmpty(downloadUrl))
                {
                    downloadUrl = RuntimeSettings.GetSetting<string>("AzureAddressablesUpload.DownloadUrl");
                }

                return downloadUrl;
            }

#if UNITY_EDITOR
            set
            {
                downloadUrl = value;
                RuntimeSettings.SetSetting("AzureAddressablesUpload.DownloadUrl", value);
            }
#endif
        }

#if UNITY_6000
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            downloadUrl = null;
        }
#endif
    }
}
