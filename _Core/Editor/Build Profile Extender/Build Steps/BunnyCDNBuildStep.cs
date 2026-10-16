//-----------------------------------------------------------------------
// <copyright file="BunnyCDNBuildStep.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BuildProfile
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;
    using UnityEngine.Serialization;

#if USING_BUNNY_CDN
    using BunnyCDN.Net.Storage;
#endif

    //// Storage Layout:
    ////   builds.json                      <- BunnyCDNBuildList of every build
    ////   builds/{guid}/output/...         <- The raw build output
    ////   builds/{guid}/{ExeName}.zip      <- Windows only
    ////   builds/{guid}/index.html
    ////   builds/{guid}/build_info.json
    ////   builds/{guid}/build_report.json
    ////   builds/{guid}/Editor.log
    ////   builds/{guid}/icon.png
    [CreateAssetMenu(fileName = "Bunny CDN Upload", menuName = "OGT/Build Steps/Upload to BunnyCDN")]
    public class BunnyCDNBuildStep : BuildStep
    {
        public const string BuildsFolderName = "builds";
        public const string BuildsJsonFileName = "builds.json";
        public const string OutputFolderName = "output";
        public const string IndexHtmlFileName = "index.html";
        public const string BuildInfoFileName = "build_info.json";
        public const string BuildReportFileName = "build_report.json";
        public const string EditorLogFileName = "Editor.log";
        public const string IconFileName = "icon.png";

        private const int MaxUploadAttempts = 3;
        private const int MaxConcurrentUploads = 8;

        //// NOTE: Defaults point at the secrets written by Tools/OGT/Tools/Bunny Delivery (Config tab)
        [Header("Storage Zone")]
        [SerializeField] private SecretString storageZoneName = new SecretString(BunnyDeliverySecrets.StorageZoneName);
        [SerializeField] private SecretString storageZoneApiAccessKey = new SecretString(BunnyDeliverySecrets.StorageZoneApiAccessKey);
        [SerializeField] private SecretString mainReplicationRegion = new SecretString(BunnyDeliverySecrets.StorageZoneMainReplicationRegion);
        [SerializeField] private SecretString storagePullZoneUrl = new SecretString(BunnyDeliverySecrets.StoragePullZoneUrl);

        [Header("File Templates")]
        [FormerlySerializedAs("androidIndexHtml")]
        [SerializeField] private TextAsset indexHtml;

        [Header("Discord Info")]
        [SerializeField] private SecretString discordWebhookUrl;

        // Resolved from the secrets above at the start of each upload
        [NonSerialized] private string storageZone;

        [Header("Game Info")]
        [SerializeField] private string gameId;
        [SerializeField] private string gameName;
        [SerializeField] private Texture2D gameIcon;

        [Header("Publish Info")]
        [SerializeField] private string channel = "dev";
        [Tooltip("Where this build is going (ie. Website, Discord), not the BuildTarget. The BuildTarget is recorded separately.")]
        [SerializeField] private string platform;



        public override void OnPostprocessBuild(BuildProfileExtender.Environment environment, BuildReport report)
        {
#if USING_BUNNY_CDN
            var summary = report.summary;
            var guid = summary.guid.ToString();
            var target = summary.platform;

            Logger.Log($"[BunnyCDNBuildStep] OnPostprocessBuild({environment}, {target}, '{summary.outputPath}', {guid})");

            if (this.indexHtml == null)
            {
                throw new BuildFailedException("[BunnyCDNBuildStep] index.html TextAsset is not set!");
            }

            this.storageZone = GetRequiredSecret(this.storageZoneName, "Storage Zone Name");
            var storageZoneAccessKey = GetRequiredSecret(this.storageZoneApiAccessKey, "Storage Zone API Access Key");
            var pullZoneUrl = GetRequiredSecret(this.storagePullZoneUrl, "Storage Pull Zone URL").TrimEnd('/');

            // An empty region is valid (the library treats it as the default "de" region)
            var region = this.mainReplicationRegion.TryGetValue(out var regionValue) ? regionValue ?? string.Empty : string.Empty;

            var buildFolder = $"{BuildsFolderName}/{guid}";
            var buildUrl = $"{pullZoneUrl}/{buildFolder}";

            var outputFiles = GetOutputFiles(target, summary.outputPath);
            Logger.Log($"[BunnyCDNBuildStep] Found {outputFiles.Count} output files");

            var stagingDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Temp", "BunnyCDN", guid));

            if (Directory.Exists(stagingDirectory))
            {
                Directory.Delete(stagingDirectory, true);
            }

            Directory.CreateDirectory(stagingDirectory);

            var buildInfo = this.CreateBuildInfo(environment, report, outputFiles);

            // Primary download / play link
            if (target == BuildTarget.WebGL)
            {
                buildInfo.primaryPath = $"{OutputFolderName}/index.html";
            }
            else if (target == BuildTarget.Android)
            {
                buildInfo.primaryPath = $"{OutputFolderName}/{Path.GetFileName(summary.outputPath)}";
            }
            else if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
            {
                // {ExeName}.{Version}.zip so downloaded builds are easy to tell apart
                var version = string.Concat((buildInfo.version ?? string.Empty).Split(Path.GetInvalidFileNameChars())).Trim();
                var zipFileName = Path.GetFileNameWithoutExtension(summary.outputPath) + (string.IsNullOrEmpty(version) ? string.Empty : "." + version) + ".zip";
                Logger.Log($"[BunnyCDNBuildStep] Zipping build to '{zipFileName}'");
                CreateZip(Path.Combine(stagingDirectory, zipFileName), outputFiles);
                buildInfo.primaryPath = zipFileName;
            }

            // Build artifacts
            File.WriteAllText(Path.Combine(stagingDirectory, BuildReportFileName), JsonUtility.ToJson(BuildReportData.Create(report), true));
            CopyEditorLog(Path.Combine(stagingDirectory, EditorLogFileName));
            var hasIcon = this.WriteIcon(Path.Combine(stagingDirectory, IconFileName));

            buildInfo.buildUrl = buildUrl;
            buildInfo.indexUrl = $"{buildUrl}/{IndexHtmlFileName}";
            buildInfo.buildInfoUrl = $"{buildUrl}/{BuildInfoFileName}";
            buildInfo.iconUrl = hasIcon ? $"{buildUrl}/{IconFileName}" : string.Empty;
            buildInfo.primaryUrl = string.IsNullOrEmpty(buildInfo.primaryPath) ? string.Empty : $"{buildUrl}/{EscapeUrl(buildInfo.primaryPath)}";
            buildInfo.outputFiles = outputFiles.Select(x => CreateArtifact($"{OutputFolderName}/{x.RelativePath}", x.Size, buildUrl)).ToArray();

            // index.html and build_info.json are listed before being written so they show up in the artifact list
            var stagedArtifacts = Directory.GetFiles(stagingDirectory)
                .Select(x => CreateArtifact(Path.GetFileName(x), new FileInfo(x).Length, buildUrl))
                .ToList();

            stagedArtifacts.Add(CreateArtifact(IndexHtmlFileName, 0, buildUrl));
            stagedArtifacts.Add(CreateArtifact(BuildInfoFileName, 0, buildUrl));
            buildInfo.artifacts = stagedArtifacts.OrderBy(x => x.path, StringComparer.OrdinalIgnoreCase).ToArray();

            File.WriteAllText(Path.Combine(stagingDirectory, BuildInfoFileName), JsonUtility.ToJson(buildInfo, true));
            this.WriteIndexHtmlFile(Path.Combine(stagingDirectory, IndexHtmlFileName), buildInfo);

            var stagedFiles = GetDirectoryFiles(stagingDirectory);

            try
            {
                var buildsStore = new BunnyBuildsStore(this.storageZone, storageZoneAccessKey, region);
                var storage = buildsStore.Storage;
                var discordWebhookUrl = this.discordWebhookUrl.HasKey ? this.discordWebhookUrl.Value : null;

                // NOTE: Must happen on a separate thread in the Unity Editor or else it will hang
                Task.Run(async () =>
                {
                    Logger.Log($"[BunnyCDNBuildStep] Uploading {outputFiles.Count} output files to '{buildFolder}/{OutputFolderName}'");
                    await this.UploadFilesAsync(storage, $"{buildFolder}/{OutputFolderName}", outputFiles);

                    // index.html last so the link never points at a partially uploaded build
                    Logger.Log($"[BunnyCDNBuildStep] Uploading {stagedFiles.Count} build artifacts to '{buildFolder}'");
                    await this.UploadFilesAsync(storage, buildFolder, stagedFiles.Where(x => x.RelativePath != IndexHtmlFileName).ToList());
                    await this.UploadFilesAsync(storage, buildFolder, stagedFiles.Where(x => x.RelativePath == IndexHtmlFileName).ToList());

                    await buildsStore.UpdateBuildListAsync(
                        $"adding build {buildInfo.guid}",
                        buildList =>
                        {
                            buildList.builds.RemoveAll(x => x.guid == buildInfo.guid);
                            buildList.builds.Add(JsonUtility.FromJson<BunnyCDNBuildInfo>(JsonUtility.ToJson(buildInfo)));
                        },
                        buildList => buildList.builds.Any(x => x.guid == buildInfo.guid));

                    if (discordWebhookUrl != null)
                    {
                        await this.PostToDiscordAsync(discordWebhookUrl, buildInfo);
                    }
                }).GetAwaiter().GetResult();

                Logger.Log($"[BunnyCDNBuildStep] Build Step Complete: {buildInfo.indexUrl}");
            }
            catch (Exception ex)
            {
                Logger.Log($"[BunnyCDNBuildStep] Upload Failed!");
                Logger.LogException(ex);
                throw new BuildFailedException("[BunnyCDNBuildStep] Failed to upload output to BunnyCDN");
            }
#else
            throw new BuildFailedException("Trying to use BunnyCDN Storage when package is missing!");
#endif
        }

        private void OnValidate()
        {
            if (this.indexHtml == null)
            {
                this.indexHtml = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath("61a3e6b998b5d3247a395d42e259e4e2"));
                EditorUtil.SetDirty(this);
            }
        }

#if USING_BUNNY_CDN

        private BunnyCDNBuildInfo CreateBuildInfo(BuildProfileExtender.Environment environment, BuildReport report, List<LocalFile> outputFiles)
        {
            var summary = report.summary;
            var startTime = ToUtc(summary.buildStartedAt);

            // NOTE: This step runs during post process, so the report isn't finalized yet (no end time / result)
            var endTime = summary.buildEndedAt > summary.buildStartedAt ? ToUtc(summary.buildEndedAt) : DateTime.UtcNow;
            var totalSize = summary.totalSize > 0 ? (long)summary.totalSize : outputFiles.Sum(x => x.Size);
            var totalTime = summary.totalTime > TimeSpan.Zero ? summary.totalTime : endTime - startTime;

            return new BunnyCDNBuildInfo
            {
                guid = summary.guid.ToString(),
                gameId = this.gameId,
                gameName = string.IsNullOrWhiteSpace(this.gameName) ? PlayerSettings.productName : this.gameName,
                channel = this.channel,
                platform = this.platform,
                buildTarget = summary.platform.ToString(),
                environment = environment.ToString(),
                version = Platform.VersionString ?? PlayerSettings.bundleVersion,
                unityVersion = Application.unityVersion,
                result = summary.result.ToString(),
                buildStartTime = startTime.ToString("o"),
                buildEndTime = endTime.ToString("o"),
                totalTimeSeconds = totalTime.TotalSeconds,
                totalSize = totalSize,
                totalWarnings = summary.totalWarnings,
                totalErrors = summary.totalErrors,
            };
        }

        private void WriteIndexHtmlFile(string indexHtmlPath, BunnyCDNBuildInfo buildInfo)
        {
            // Escaping "</" so the json can never close the <script> tag it's embedded in
            var buildInfoJson = JsonUtility.ToJson(buildInfo).Replace("</", "<\\/");

            var html = this.indexHtml.text
                .Replace("{appName}", WebUtility.HtmlEncode(buildInfo.gameName))
                .Replace("{buildInfoJson}", buildInfoJson);

            File.WriteAllText(indexHtmlPath, html);
        }

        private bool WriteIcon(string iconPath)
        {
            var icon = this.gameIcon != null ? this.gameIcon : PlayerSettings.GetIcons(NamedBuildTarget.Unknown, IconKind.Any).FirstOrDefault(x => x != null);

            if (icon == null)
            {
                Logger.LogWarning("[BunnyCDNBuildStep] No game icon found, skipping icon.png");
                return false;
            }

            try
            {
                var assetPath = AssetDatabase.GetAssetPath(icon);

                if (string.IsNullOrEmpty(assetPath) == false && assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    File.Copy(FileUtil.GetPhysicalPath(assetPath), iconPath, true);
                    return true;
                }

                // Blitting so this works even if the texture isn't readable or isn't a png
                var renderTexture = RenderTexture.GetTemporary(icon.width, icon.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                var previousRenderTexture = RenderTexture.active;
                var readableIcon = new Texture2D(icon.width, icon.height, TextureFormat.RGBA32, false);

                try
                {
                    Graphics.Blit(icon, renderTexture);
                    RenderTexture.active = renderTexture;
                    readableIcon.ReadPixels(new Rect(0, 0, icon.width, icon.height), 0, 0);
                    readableIcon.Apply();
                    File.WriteAllBytes(iconPath, readableIcon.EncodeToPNG());
                }
                finally
                {
                    RenderTexture.active = previousRenderTexture;
                    RenderTexture.ReleaseTemporary(renderTexture);
                    DestroyImmediate(readableIcon);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"[BunnyCDNBuildStep] Failed to write icon.png: {ex.Message}");
                return false;
            }
        }

        private static void CopyEditorLog(string destinationPath)
        {
            var editorLogPath = Application.consoleLogPath;

            if (string.IsNullOrEmpty(editorLogPath) || File.Exists(editorLogPath) == false)
            {
                Logger.LogWarning($"[BunnyCDNBuildStep] Editor log not found at '{editorLogPath}', skipping");
                return;
            }

            // The editor still has the log open, so it must be opened with sharing
            using var source = new FileStream(editorLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var destination = File.Create(destinationPath);
            source.CopyTo(destination);
        }

        private static void CreateZip(string zipPath, List<LocalFile> files)
        {
            using var zipStream = File.Create(zipPath);
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);

            foreach (var file in files)
            {
                var entry = archive.CreateEntry(file.RelativePath, System.IO.Compression.CompressionLevel.Optimal);
                using var entryStream = entry.Open();
                using var fileStream = File.OpenRead(file.FullPath);
                fileStream.CopyTo(entryStream);
            }
        }

        private static List<LocalFile> GetOutputFiles(BuildTarget target, string outputPath)
        {
            if (Directory.Exists(outputPath))
            {
                return GetDirectoryFiles(outputPath);
            }

            if (File.Exists(outputPath) == false)
            {
                throw new BuildFailedException($"[BunnyCDNBuildStep] Build output '{outputPath}' does not exist");
            }

            // Android builds sit next to previous builds, so only take the package itself
            if (target == BuildTarget.Android)
            {
                return new List<LocalFile> { new LocalFile(outputPath, Path.GetFileName(outputPath)) };
            }

            return GetDirectoryFiles(Path.GetDirectoryName(outputPath));
        }

        private static List<LocalFile> GetDirectoryFiles(string directory)
        {
            var root = Path.GetFullPath(directory).TrimEnd('/', '\\');

            return Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                .Select(x => new LocalFile(x, x.Substring(root.Length + 1).Replace('\\', '/')))
                .ToList();
        }

        private static BunnyCDNBuildArtifact CreateArtifact(string path, long size, string buildUrl)
        {
            return new BunnyCDNBuildArtifact
            {
                name = Path.GetFileName(path),
                path = path,
                url = $"{buildUrl}/{EscapeUrl(path)}",
                size = size,
            };
        }

        private static string GetRequiredSecret(SecretString secret, string displayName)
        {
            if (secret == null || secret.HasKey == false)
            {
                throw new BuildFailedException($"[BunnyCDNBuildStep] {displayName} secret is not set!");
            }

            if (secret.TryGetValue(out var value) == false || string.IsNullOrWhiteSpace(value))
            {
                throw new BuildFailedException($"[BunnyCDNBuildStep] {displayName} secret '{secret.SecretKey}' has no value. Run Tools/OGT/Tools/Bunny Delivery to set it up.");
            }

            return value;
        }

        private static DateTime ToUtc(DateTime dateTime)
        {
            return dateTime.Kind == DateTimeKind.Local ? dateTime.ToUniversalTime() : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }

        private static string EscapeUrl(string url)
        {
            var escapedSegments = Array.ConvertAll(url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries), Uri.EscapeDataString);
            return string.Join("/", escapedSegments);
        }

        private async Task UploadFilesAsync(BunnyCDNStorage storage, string destinationFolder, List<LocalFile> files)
        {
            using var throttle = new SemaphoreSlim(MaxConcurrentUploads);

            await Task.WhenAll(files.Select(async file =>
            {
                await throttle.WaitAsync();

                try
                {
                    await UploadFileAsync(storage, file.FullPath, $"{this.storageZone}/{destinationFolder}/{file.RelativePath}");
                }
                finally
                {
                    throttle.Release();
                }
            }));
        }

        private static async Task UploadFileAsync(BunnyCDNStorage storage, string localFilePath, string storagePath)
        {
            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    await storage.UploadAsync(localFilePath, storagePath);
                    return;
                }
                catch (Exception ex) when (attempt < MaxUploadAttempts)
                {
                    Logger.LogWarning($"[BunnyCDNBuildStep] Upload of '{storagePath}' failed (attempt {attempt}), retrying: {ex.Message}");
                    await Task.Delay(1000 * attempt);
                }
            }
        }

        private async Task PostToDiscordAsync(string webhookUrl, BunnyCDNBuildInfo buildInfo)
        {
            var title = $"Build Uploaded to BunnyCDN";

            var description =
                $"**Game ID:** {buildInfo.gameId}\n" +
                $"**Channel:** {buildInfo.channel}\n" +
                $"**Platform:** {buildInfo.platform}\n" +
                $"**Build Target:** {buildInfo.buildTarget}\n" +
                $"**Version:** {buildInfo.version}\n" +
                $"**Build ID:** {buildInfo.guid}\n";

            Logger.Log($"[BunnyCDNBuildStep] Posting to Discord webhook '{title}', '{description}', '{buildInfo.indexUrl}'");

            try
            {
                await DiscordWebhook.PostAsync(Logger, webhookUrl, title, description, buildInfo.indexUrl);
                Logger.Log($"[BunnyCDNBuildStep] Posting to Discord Complete");
            }
            catch (Exception ex)
            {
                Logger.Log($"[BunnyCDNBuildStep] Discord Webhook Post Failed: {ex.Message}");
            }
        }

        private class LocalFile
        {
            public LocalFile(string fullPath, string relativePath)
            {
                this.FullPath = fullPath;
                this.RelativePath = relativePath;
                this.Size = new FileInfo(fullPath).Length;
            }

            public string FullPath { get; }

            public string RelativePath { get; }

            public long Size { get; }
        }

        [Serializable]
        private class BuildReportData
        {
            public SummaryData summary;
            public StepData[] steps;
            public FileData[] files;

            public static BuildReportData Create(BuildReport report)
            {
                var summary = report.summary;

                return new BuildReportData
                {
                    summary = new SummaryData
                    {
                        guid = summary.guid.ToString(),
                        platform = summary.platform.ToString(),
                        platformGroup = summary.platformGroup.ToString(),
                        options = summary.options.ToString(),
                        outputPath = summary.outputPath,
                        result = summary.result.ToString(),
                        buildStartedAt = ToUtc(summary.buildStartedAt).ToString("o"),
                        buildEndedAt = ToUtc(summary.buildEndedAt).ToString("o"),
                        totalTimeSeconds = summary.totalTime.TotalSeconds,
                        totalSize = (long)summary.totalSize,
                        totalWarnings = summary.totalWarnings,
                        totalErrors = summary.totalErrors,
                    },
                    steps = report.steps.Select(x => new StepData
                    {
                        name = x.name,
                        depth = x.depth,
                        durationSeconds = x.duration.TotalSeconds,
                        messages = x.messages.Select(m => new MessageData { type = m.type.ToString(), content = m.content }).ToArray(),
                    }).ToArray(),
                    files = report.GetFiles().Select(x => new FileData { path = x.path, role = x.role, size = (long)x.size }).ToArray(),
                };
            }
        }

        [Serializable]
        private class SummaryData
        {
            public string guid;
            public string platform;
            public string platformGroup;
            public string options;
            public string outputPath;
            public string result;
            public string buildStartedAt;
            public string buildEndedAt;
            public double totalTimeSeconds;
            public long totalSize;
            public int totalWarnings;
            public int totalErrors;
        }

        [Serializable]
        private class StepData
        {
            public string name;
            public int depth;
            public double durationSeconds;
            public MessageData[] messages;
        }

        [Serializable]
        private class MessageData
        {
            public string type;
            public string content;
        }

        [Serializable]
        private class FileData
        {
            public string path;
            public string role;
            public long size;
        }

#endif
    }

    //// NOTE: These are read by external tools (index.html, the build browser app), so field names are part of the format
    [Serializable]
    public class BunnyCDNBuildList
    {
        public List<BunnyCDNBuildInfo> builds = new List<BunnyCDNBuildInfo>();
    }

    [Serializable]
    public class BunnyCDNBuildInfo
    {
        public string guid;
        public string gameId;
        public string gameName;
        public string channel;
        public string platform;
        public string buildTarget;
        public string environment;
        public string version;
        public string unityVersion;
        public string result;
        public string buildStartTime;
        public string buildEndTime;
        public double totalTimeSeconds;
        public long totalSize;
        public int totalWarnings;
        public int totalErrors;

        // Set from the Bunny Delivery window (Builds tab), saved builds are skipped when deleting old builds
        public bool saved;

        public string buildUrl;
        public string indexUrl;
        public string buildInfoUrl;
        public string iconUrl;

        //// Android = APK, Windows = Zip, WebGL = output/index.html, everything else = empty
        public string primaryPath;
        public string primaryUrl;

        public BunnyCDNBuildArtifact[] artifacts;
        public BunnyCDNBuildArtifact[] outputFiles;
    }

    [Serializable]
    public class BunnyCDNBuildArtifact
    {
        public string name;
        public string path;
        public string url;
        public long size;
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
