//-----------------------------------------------------------------------
// <copyright file="BunnyCDNBuildStep.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BuildProfile
{
    using System;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Bunny CDN Upload", menuName = "OGT/Build Steps/Upload to BunnyCDN")]
    public class BunnyCDNBuildStep : BuildStep
    {
        [SerializeField] private string storageZoneName;
        [SerializeField] private SecretString apiAccessKey;
        [SerializeField] private string mainReplicationRegion;

        public override void OnPostprocessBuild(BuildProfileExtender.Environment environment, BuildTarget target, string path)
        {
#if USING_BUNNY_CDN
            Logger.Log($"[BunnyCDNBuildStep] Uploading '{path}' Build to `{this.storageZoneName}`");

            if (this.apiAccessKey.HasKey == false)
            {
                throw new UnityEditor.Build.BuildFailedException("BunnyCDN API Access Key is not set!");
            }

            try
            {
                var storage = new BunnyCDN.Net.Storage.BunnyCDNStorage(this.storageZoneName, this.apiAccessKey.Value, this.mainReplicationRegion);

                // NOTE: Must happen on a seperate thread in the Unity Editor or else it will hang
                var uploadTaask = System.Threading.Tasks.Task.Factory.StartNew(() => storage.UploadLocalDirectory(path).Wait());
                uploadTaask.Wait();

                Logger.Log($"[BunnyCDNBuildStep] Upload Complete");
            }
            catch (Exception ex)
            {
                Logger.Log($"[BunnyCDNBuildStep] Upload Failed!");
                Logger.LogException(ex);
                throw new UnityEditor.Build.BuildFailedException("[BunnyCDNBuildStep] Failed to upload output to BunnyCDN");
            }
#else
            throw new UnityEditor.Build.BuildFailedException("Trying to use BunnyCDN Storage when package is missing!");
#endif
        }
    }
}
