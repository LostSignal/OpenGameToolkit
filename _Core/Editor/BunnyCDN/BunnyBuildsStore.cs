//-----------------------------------------------------------------------
// <copyright file="BunnyBuildsStore.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_BUNNY_CDN

namespace OGT.BuildProfile
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using BunnyCDN.Net.Storage;
    using UnityEngine;

    //// NOTE [bgish]: Shared access to builds.json and the builds folder of a Bunny Storage Zone.  Used by the
    ////               BunnyCDNBuildStep (adding builds) and the BunnyDeliveryEditorWindow (saving / deleting builds).
    public class BunnyBuildsStore
    {
        private const int MaxUpdateAttempts = 10;

        private static readonly OGTLogger Logger = BuildStep.Logger;

        private readonly string storageZoneName;

        public BunnyBuildsStore(string storageZoneName, string accessKey, string mainReplicationRegion)
        {
            this.storageZoneName = storageZoneName;
            this.Storage = new BunnyCDNStorage(storageZoneName, accessKey, mainReplicationRegion ?? string.Empty);
        }

        public BunnyCDNStorage Storage { get; }

        public string StorageZoneName => this.storageZoneName;

        //// NOTE: Bunny Storage has no ETags, so this does a read / modify / write and then re-reads to make sure
        ////       another writer didn't overwrite the change, re-applying it until it sticks.
        ////
        ////       modify:    Applied to the freshly read list on every attempt
        ////       isApplied: Checked against the list that's read back after writing
        public async Task UpdateBuildListAsync(string description, Action<BunnyCDNBuildList> modify, Func<BunnyCDNBuildList, bool> isApplied)
        {
            var buildsJsonPath = BunnyCDNBuildStep.BuildsJsonFileName;
            var random = new System.Random();

            for (int attempt = 1; attempt <= MaxUpdateAttempts; attempt++)
            {
                Logger.Log($"[BunnyBuildsStore] Updating {BunnyCDNBuildStep.BuildsJsonFileName}, {description} (attempt {attempt})");

                var buildList = await this.ReadBuildListAsync();
                await this.AddMissingBuildsAsync(buildList);

                modify(buildList);
                buildList.builds.Sort((a, b) => string.CompareOrdinal(a.buildStartTime, b.buildStartTime));

                // Keeping builds.json small, the full file lists live in each build's build_info.json
                foreach (var build in buildList.builds)
                {
                    build.artifacts = Array.Empty<BunnyCDNBuildArtifact>();
                    build.outputFiles = Array.Empty<BunnyCDNBuildArtifact>();
                }

                await this.UploadTextAsync(buildsJsonPath, JsonUtility.ToJson(buildList, true));
                await Task.Delay(1000);

                var verifyList = await this.ReadBuildListAsync();

                if (isApplied(verifyList))
                {
                    Logger.Log($"[BunnyBuildsStore] {BunnyCDNBuildStep.BuildsJsonFileName} updated and verified ({verifyList.builds.Count} builds)");
                    return;
                }

                Logger.LogWarning($"[BunnyBuildsStore] Change ({description}) missing from {BunnyCDNBuildStep.BuildsJsonFileName} after write, another writer likely overwrote it. Retrying...");
                await Task.Delay(random.Next(500, 2000));
            }

            throw new Exception($"[BunnyBuildsStore] Failed to update {BunnyCDNBuildStep.BuildsJsonFileName} ({description}) after {MaxUpdateAttempts} attempts");
        }

        public async Task<BunnyCDNBuildList> ReadBuildListAsync()
        {
            // NOTE: Checking existence via listing, since a failed download can't be told apart from a missing file
            //       and treating a network error as "missing" would wipe out builds.json
            var rootObjects = await this.Storage.GetStorageObjectsAsync($"{this.storageZoneName}/");

            if (rootObjects.Any(x => x.IsDirectory == false && x.ObjectName == BunnyCDNBuildStep.BuildsJsonFileName) == false)
            {
                return new BunnyCDNBuildList();
            }

            var json = await this.DownloadTextAsync(BunnyCDNBuildStep.BuildsJsonFileName);

            try
            {
                var buildList = JsonUtility.FromJson<BunnyCDNBuildList>(json) ?? new BunnyCDNBuildList();
                buildList.builds ??= new List<BunnyCDNBuildInfo>();
                buildList.builds.RemoveAll(x => x == null || string.IsNullOrEmpty(x.guid));
                return buildList;
            }
            catch (Exception ex)
            {
                // Safe since AddMissingBuildsAsync rebuilds the list from the build folders
                Logger.LogWarning($"[BunnyBuildsStore] {BunnyCDNBuildStep.BuildsJsonFileName} is unreadable, rebuilding it from the build folders: {ex.Message}");
                return new BunnyCDNBuildList();
            }
        }

        //// Self healing: any build folder whose build_info.json isn't in the list gets added back
        public async Task AddMissingBuildsAsync(BunnyCDNBuildList buildList)
        {
            List<BunnyCDN.Net.Storage.Models.StorageObject> buildFolders;

            try
            {
                buildFolders = await this.Storage.GetStorageObjectsAsync($"{this.storageZoneName}/{BunnyCDNBuildStep.BuildsFolderName}/");
            }
            catch (BunnyCDNStorageFileNotFoundException)
            {
                return;
            }

            var knownGuids = new HashSet<string>(buildList.builds.Select(x => x.guid));

            foreach (var buildFolder in buildFolders.Where(x => x.IsDirectory && knownGuids.Contains(x.ObjectName) == false))
            {
                try
                {
                    var buildInfo = await this.ReadBuildInfoAsync(buildFolder.ObjectName);

                    if (buildInfo != null && string.IsNullOrEmpty(buildInfo.guid) == false && knownGuids.Add(buildInfo.guid))
                    {
                        Logger.Log($"[BunnyBuildsStore] Restoring missing build {buildInfo.guid} to {BunnyCDNBuildStep.BuildsJsonFileName}");
                        buildList.builds.Add(buildInfo);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"[BunnyBuildsStore] Skipping build folder '{buildFolder.ObjectName}', no readable {BunnyCDNBuildStep.BuildInfoFileName}: {ex.Message}");
                }
            }
        }

        public async Task<BunnyCDNBuildInfo> ReadBuildInfoAsync(string guid)
        {
            var json = await this.DownloadTextAsync(GetBuildInfoPath(guid));
            return JsonUtility.FromJson<BunnyCDNBuildInfo>(json);
        }

        public Task WriteBuildInfoAsync(BunnyCDNBuildInfo buildInfo)
        {
            return this.UploadTextAsync(GetBuildInfoPath(buildInfo.guid), JsonUtility.ToJson(buildInfo, true));
        }

        // Guids of every folder currently in builds/
        public async Task<HashSet<string>> GetBuildFolderGuidsAsync()
        {
            try
            {
                var objects = await this.Storage.GetStorageObjectsAsync($"{this.storageZoneName}/{BunnyCDNBuildStep.BuildsFolderName}/");
                return new HashSet<string>(objects.Where(x => x.IsDirectory).Select(x => x.ObjectName));
            }
            catch (BunnyCDNStorageFileNotFoundException)
            {
                return new HashSet<string>();
            }
        }

        // Deletes builds/{guid}/ and everything in it (it does not touch builds.json)
        public async Task DeleteBuildFolderAsync(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid) || guid.Contains("/") || guid.Contains(".."))
            {
                throw new ArgumentException($"Invalid build guid '{guid}'", nameof(guid));
            }

            var success = await this.Storage.DeleteObjectAsync($"{this.storageZoneName}/{BunnyCDNBuildStep.BuildsFolderName}/{guid}/");

            if (success == false)
            {
                throw new Exception($"[BunnyBuildsStore] Bunny refused to delete build folder '{guid}'");
            }
        }

        // Path is relative to the storage zone root
        public async Task<string> DownloadTextAsync(string path)
        {
            using var stream = await this.Storage.DownloadObjectAsStreamAsync($"{this.storageZoneName}/{path}");
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }

        // Path is relative to the storage zone root
        public async Task UploadTextAsync(string path, string text)
        {
            using var stream = new MemoryStream(new UTF8Encoding(false).GetBytes(text));
            await this.Storage.UploadAsync(stream, $"{this.storageZoneName}/{path}");
        }

        private static string GetBuildInfoPath(string guid)
        {
            return $"{BunnyCDNBuildStep.BuildsFolderName}/{guid}/{BunnyCDNBuildStep.BuildInfoFileName}";
        }
    }
}

#endif
