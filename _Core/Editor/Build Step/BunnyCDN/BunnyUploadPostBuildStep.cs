//-----------------------------------------------------------------------
// <copyright file="BunnyUploadPostBuildStep.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using UnityEngine;
    using UnityEditor.Build.Profile;
    using UnityEditor.Build.Reporting;

    public class BunnyUploadPostBuildStep : PostBuildStep
    {
        [SerializeField] private string storageZoneName;
        [SerializeField] private string apiAccessKey;
        [SerializeField] private string mainReplicationRegion;

        public override string Name => "BunnyCDN Upload";

        public override void Run(BuildProfile buildProfile, BuildReport report)
        {
#if USING_BUNNY_CDN
            var outputPath = report.summary.outputPath;

            Logger.Log($"Uploading '{outputPath}' Build to `{this.storageZoneName}`");

            try
            {
                var storage = new BunnyCDN.Net.Storage.BunnyCDNStorage(this.storageZoneName, this.apiAccessKey, this.mainReplicationRegion);

                // NOTE: Must happen on a seperate thread in the Unity Editor or else it will hang
                var uploadTaask = System.Threading.Tasks.Task.Factory.StartNew(() => storage.UploadLocalDirectory(outputPath).Wait());
                uploadTaask.Wait();

                Logger.Log($"Upload Complete");
            }
            catch (Exception ex)
            {
                Logger.Log($"Upload Failed!");
                Logger.LogException(ex);
                throw new UnityEditor.Build.BuildFailedException("Failed to upload output to BunnyCDN");
            }
#else
            throw new UnityEditor.Build.BuildFailedException("Trying to use BunnyCDN Storage when package is missing!");
#endif
        }
    }
}
