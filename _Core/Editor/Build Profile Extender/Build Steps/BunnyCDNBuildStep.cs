//-----------------------------------------------------------------------
// <copyright file="BunnyCDNBuildStep.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BuildProfile
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Bunny CDN Upload", menuName = "OGT/Build Steps/Upload to BunnyCDN")]
    public class BunnyCDNBuildStep : BuildStep
    {
        [SerializeField] private string storageZoneName;
        [SerializeField] private SecretString apiAccessKey;
        [SerializeField] private string mainReplicationRegion;
        [SerializeField] private string storagePullZoneUrl;

        [Header("Upload Path")]
        [SerializeField] private string uploadPathFormat = "{gameId}/{channel}/{platform}/{version}";
        [SerializeField] private string gameId;
        [SerializeField] private string channel = "dev";
        [SerializeField] private string platform;

        [Header("Discord Info")]
        [SerializeField] private SecretString discordWebhookUrl;

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
                var uploadPath = this.uploadPathFormat
                    .Replace("{gameId}", this.gameId)
                    .Replace("{channel}", this.channel)
                    .Replace("{platform}", this.platform)
                    .Replace("{version}", Platform.VersionString);

                // Removing any double forward slashes from the buildPath string
                while (uploadPath.Contains("//"))
                {
                    uploadPath = uploadPath.Replace("//", "/");
                }

                var storage = new BunnyCDN.Net.Storage.BunnyCDNStorage(this.storageZoneName, this.apiAccessKey.Value, this.mainReplicationRegion);

                // NOTE: Must happen on a separate thread in the Unity Editor or else it will hang
                var uploadTaask = System.Threading.Tasks.Task.Factory.StartNew(() => storage.UploadLocalDirectory(path, uploadPath).Wait());
                uploadTaask.Wait();

                if (this.discordWebhookUrl.HasKey)
                {
                    var title = $"Build Uploaded to BunnyCDN";

                    var description =
                                  $"**Game ID:** {this.gameId}\n" +
                                  $"**Channel:** {this.channel}\n" +
                                  $"**Platform:** {this.platform}\n" +
                                  $"**Version:** {Platform.VersionString}\n";

                    string url = this.storagePullZoneUrl.EndsWith("/") ?
                        $"{this.storagePullZoneUrl}{uploadPath}/index.html" :
                        $"{this.storagePullZoneUrl}/{uploadPath}/index.html";

                    // NOTE: Must happen on a separate thread in the Unity Editor or else it will hang
                    var discordTask = Task.Factory.StartNew(() => DiscordWebhook.PostAsync(Logger, this.discordWebhookUrl.Value, title, description, url).Wait());
                    discordTask.Wait();
                }

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

    public static class DiscordWebhook
    {
        [System.Serializable]
        private class Payload
        {
            public Embed[] embeds;
        }

        [System.Serializable]
        private class Embed
        {
            public string title;
            public string description;
            public string url;
        }

        public static async Task PostAsync(OGTLogger logger, string webhookUrl, string title, string description, string url)
        {
            var payload = new Payload
            {
                embeds = new[] { new Embed { title = title, description = description, url = url } }
            };

            var response = await HttpUtil.SendJsonPost(webhookUrl, JsonUtility.ToJson(payload));


            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent)
            {
                logger.LogError($"[BunnyCDNBuildStep] Discord webhook failed: {response.StatusCode}");
            }
        }
    }
}
