//-----------------------------------------------------------------------
// <copyright file="AzureAddressablesUpload.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Settings
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using OGT.Addressables;
    using OGT.CloudBuild;
    using UnityEditor;
    using UnityEditor.Build.Reporting;

    public class AzureAddressablesUpload : Settings
    {
        public string DownloadUrl { get; set; }
        public string AssetBundleFolderName { get; set; }
        public string StorageAccountName { get; set; }
        public string StorageAccountKey { get; set; }
        public string ContainerName { get; set; }
        public string BlobPrefix { get; set; }

        //// //// [OGT.AppSettings.AzureAddressablesUploadSettings.BuildPath]
        //// public static string BuildPath
        //// {
        ////     get
        ////     {
        ////         var azureSettings = ProjectSettingsBuildConfigs.GetActiveSettings<UploadAddressableToAzureSettings>();
        ////         return azureSettings?.CalculateBuildPath();
        ////     }
        //// }
        //// 
        //// //// [OGT.AppSettings.AzureAddressablesUploadSettings.LoadPath]
        //// public static string LoadPath
        //// {
        ////     get
        ////     {
        ////         var azureSettings = ProjectSettingsBuildConfigs.GetActiveSettings<UploadAddressableToAzureSettings>();
        ////         return azureSettings != null ? (SanitizeAndRemoveTrailingForwardSlash(azureSettings.downloadUrl) + "/" + GetBlobPrefix(azureSettings)) : null;
        ////     }
        //// }

        public override void ApplySettingsOnEnterPlayMode()
        {
            this.ApplyRuntimeSettings();
        }

        public override void ApplySettings()
        {
            this.ApplyRuntimeSettings();
        }

        public override void ApplySettingsPostBuild(BuildReport buildReport)
        {
            var azureConfig = new AzureStorage.Config
            {
                StorageAccountName = this.StorageAccountName,
                StorageAccountKey = this.StorageAccountKey,
                ContainerName = this.ContainerName,
            };

            // makding sure the asset bundle path doesn't end with a forward slash
            string assetBundlePath = this.CalculateBuildPath();

            if (Directory.Exists(assetBundlePath) == false)
            {
                if (IsBeingUsedByAddressableSystem())
                {
                    Logger.LogErrorFormat("Unable to Upload AssetBundles To Azure.  Directory {0} does not exist.", assetBundlePath);
                }

                return;
            }

            string blobPrefix = this.GetBlobPrefix();
            HashSet<string> existingFiles = GetExistingFiles(azureConfig, blobPrefix);

            foreach (var filePath in Directory.GetFiles(assetBundlePath, "*", SearchOption.AllDirectories))
            {
                string relativeFileName = filePath.Substring(assetBundlePath.Length + 1).Replace("\\", "/");
                string blobName = blobPrefix + "/" + relativeFileName;

                if (existingFiles.Contains(blobName))
                {
                    Logger.LogFormat("Skipping AssetBundle {0}, it already exists", blobName, azureConfig.StorageAccountName);
                    continue;
                }

                Logger.LogFormat("Uploading AssetBundle {0}", blobName, azureConfig.StorageAccountName);

                try
                {
                    AzureStorage.UploadFile(azureConfig, blobName, File.ReadAllBytes(filePath));
                }
                catch (Exception exception)
                {
                    Logger.LogErrorFormat("Error uploading AssetBundle {0} To Azure.", blobName);
                    Logger.LogException(exception);

#if UNITY_CLOUD_BUILD
                    EditorApplication.Exit(1);
#endif

                    return;
                }
            }
        }

        private string GetBlobPrefix()
        {
            string blobPrefix = string.Empty;

            // making sure it uses all forward slashes and doesn't start with a forward slash
            if (string.IsNullOrWhiteSpace(this.BlobPrefix) == false)
            {
                blobPrefix = SanitizeAndRemoveTrailingForwardSlash(this.BlobPrefix) + "/";
                blobPrefix = blobPrefix.StartsWith("/") ? blobPrefix.Substring(1) : blobPrefix;
            }

            var cloudBuildConfig = CloudBuildManifest.Find();

            string buildFolder = cloudBuildConfig != null ?
                "cloud_build/" + cloudBuildConfig.ScmCommitId + "/" :
                "local_build/" + Environment.MachineName + "/"; // TODO [bgish]: Make sure to sanitize for special characters;

            return blobPrefix + buildFolder + EditorUserBuildSettings.activeBuildTarget;
        }

        private static string SanitizeAndRemoveTrailingForwardSlash(string str)
        {
            if (str == null)
            {
                return null;
            }

            str = str.Replace("\\", "/");
            return str.EndsWith("/") ? str.Substring(0, str.Length - 1) : str;
        }

        private static HashSet<string> GetExistingFiles(AzureStorage.Config azureConfig, string blobPrefix)
        {
            HashSet<string> result = new HashSet<string>();

            if (blobPrefix.EndsWith("/") == false)
            {
                blobPrefix += "/";
            }

            foreach (string file in AzureStorage.ListFiles(azureConfig, blobPrefix))
            {
                result.Add(file);
            }

            return result;
        }

        private string CalculateBuildPath()
        {
            return SanitizeAndRemoveTrailingForwardSlash(this.AssetBundleFolderName) + "/" + EditorUserBuildSettings.activeBuildTarget;
        }

        private static bool IsBeingUsedByAddressableSystem()
        {
            //FIX_ADDRESSABLES

            // UnityEditor.AddressableAssets.Settings.AddressableAssetSettings settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            // string buildPath = typeof(UploadAddressableToAzureSettings).FullName + "." + nameof(BuildPath);
            // 
            // if (settings == null)
            // {
            //     return false;
            // }
            // 
            // foreach (var group in settings.groups.Where(x => x.entries.IsNullOrEmpty() == false))
            // {
            //     foreach (var schema in group.Schemas.OfType<UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema>())
            //     {
            //         if (schema?.BuildPath?.GetValue(settings) == buildPath)
            //         {
            //             return true;
            //         }
            //     }
            // }

            return false;
        }

        private void ApplyRuntimeSettings()
        {
            if (string.IsNullOrWhiteSpace(this.DownloadUrl) == false)
            {
                AzureAddressablesUploadRuntimeSettings.DownloadUrl = this.DownloadUrl;
            }
        }
    }
}
