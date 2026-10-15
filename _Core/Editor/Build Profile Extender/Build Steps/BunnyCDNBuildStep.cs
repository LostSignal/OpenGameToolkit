//-----------------------------------------------------------------------
// <copyright file="BunnyCDNBuildStep.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BuildProfile
{
    using System;
    using System.Net;
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

        [Header("File Templates")]
        [SerializeField] private TextAsset androidIndexHtml;

        public override void OnPostprocessBuild(BuildProfileExtender.Environment environment, BuildTarget target, string path)
        {
#if USING_BUNNY_CDN

            Logger.Log($"[BunnyCDNBuildStep] OnPostprocessBuild({environment}, {target}, '{path}')");

            // Remove trailing slash from storagePullZoneUrl if it exists
            if (this.storagePullZoneUrl.EndsWith("/"))
            {
                this.storagePullZoneUrl = this.storagePullZoneUrl.Substring(0, this.storagePullZoneUrl.Length - 1);
            }

            // Generating the Upload Path
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

            // Determining the file name to upload based on the build target
            string fileName = string.Empty;

            if (target == BuildTarget.WebGL)
            {
                fileName = "index.html";
            }
            else if (target == BuildTarget.Android)
            {
                string apkName = System.IO.Path.GetFileName(path);
                Logger.Log($"[BunnyCDNBuildStep] apkName = '{apkName}'");

                path = System.IO.Path.GetDirectoryName(path);
                Logger.Log($"[BunnyCDNBuildStep] path = '{path}'");

                Logger.Log($"[BunnyCDNBuildStep] Writing index.html");
                this.WriteIndexHtmlFile(this.androidIndexHtml, path, uploadPath, apkName);

                fileName = "index.html";
            }
            else if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
            {
                fileName = System.IO.Path.GetFileName(path);
                path = System.IO.Path.GetDirectoryName(path);
            }
            else
            {
                throw new UnityEditor.Build.BuildFailedException($"[BunnyCDNBuildStep] Unsupported BuildTarget {target}");
            }

            Logger.Log($"[BunnyCDNBuildStep] fileName = '{fileName}'");
            Logger.Log($"[BunnyCDNBuildStep] Uploading '{path}' Build to `{this.storageZoneName}`");

            if (this.apiAccessKey.HasKey == false)
            {
                throw new UnityEditor.Build.BuildFailedException("BunnyCDN API Access Key is not set!");
            }

            try
            {
                var storage = new BunnyCDN.Net.Storage.BunnyCDNStorage(this.storageZoneName, this.apiAccessKey.Value, this.mainReplicationRegion);

                //// NOTE: Must happen on a separate thread in the Unity Editor or else it will hang
                Logger.Log($"[BunnyCDNBuildStep] UploadLocalDirectory('{path}', '{uploadPath}')");
                var uploadTask = System.Threading.Tasks.Task.Factory.StartNew(() => storage.UploadLocalDirectory(path, uploadPath).Wait());
                uploadTask.Wait();

                Logger.Log($"[BunnyCDNBuildStep] UploadLocalDirectory Task Complete");

                if (this.discordWebhookUrl.HasKey)
                {
                    var title = $"Build Uploaded to BunnyCDN";

                    var description =
                        $"**Game ID:** {this.gameId}\n" +
                        $"**Channel:** {this.channel}\n" +
                        $"**Platform:** {this.platform}\n" +
                        $"**Version:** {Platform.VersionString}\n";

                    string url = $"{this.storagePullZoneUrl}/{EscapeUrl(uploadPath)}/{EscapeUrl(fileName)}";

                    Logger.Log($"[BunnyCDNBuildStep] Posting to Discord webhook '{title}', '{description}', '{url}'");

                    // NOTE: Must happen on a separate thread in the Unity Editor or else it will hang
                    var discordTask = System.Threading.Tasks.Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            DiscordWebhook.PostAsync(Logger, this.discordWebhookUrl.Value, title, description, url).Wait();
                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"[BunnyCDNBuildStep] Discord Webhook Post Failed: {ex.Message}");
                        }
                    });

                    discordTask.Wait();

                    Logger.Log($"[BunnyCDNBuildStep] Posting to Discord Complete");
                }

                Logger.Log($"[BunnyCDNBuildStep] Build Step Complete");
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

        private void WriteIndexHtmlFile(TextAsset indexHtmlAsset, string outputDirectoryPath, string uploadPath, string apkName)
        {
            if (indexHtmlAsset == null)
            {
                throw new UnityEditor.Build.BuildFailedException("BunnyCDNBuildStep: index.html TextAsset is not set!");
            }

            var indexHtmlPath = System.IO.Path.Combine(outputDirectoryPath, "index.html");
            var apkUrl = $"{this.storagePullZoneUrl}/{EscapeUrl(uploadPath)}/{EscapeUrl(apkName)}";

            var html = indexHtmlAsset.text
                .Replace("{gameId}", this.gameId)
                .Replace("{channel}", this.channel)
                .Replace("{platform}", this.platform)
                .Replace("{version}", Platform.VersionString)
                .Replace("{appName}", System.IO.Path.GetFileNameWithoutExtension(apkName))
                .Replace("{apkUrl}", apkUrl);

            System.IO.File.WriteAllText(indexHtmlPath, html);
        }

        private static string EscapeUrl(string url)
        {
            var escapedSegments = Array.ConvertAll(url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries), Uri.EscapeDataString);
            return string.Join("/", escapedSegments);
        }

        private void OnValidate()
        {
            if (androidIndexHtml == null)
            {
                androidIndexHtml = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath("61a3e6b998b5d3247a395d42e259e4e2"));
                EditorUtil.SetDirty(this);
            }
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

        public static async System.Threading.Tasks.Task PostAsync(OGTLogger logger, string webhookUrl, string title, string description, string url)
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
