//-----------------------------------------------------------------------
// <copyright file="BunnyDeliveryEditorWindow.Builds.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using OGT.BuildProfile;
    using UnityEditor;
    using UnityEngine;

    //// Builds tab: Lists everything in builds.json, lets you mark builds as saved, and deletes builds older
    ////             than a given age (skipping saved builds unless told otherwise).
    public sealed partial class BunnyDeliveryEditorWindow
    {
        private List<BunnyCDNBuildInfo> builds;
        private DateTime buildsFetchedAt;
        private string buildsFromFilter = string.Empty;
        private string buildsToFilter = string.Empty;
        private bool savedBuildsOnly;
        private int deleteOlderThanAmount = 3;
        private AgeUnit deleteOlderThanUnit = AgeUnit.Months;
        private bool keepSavedBuilds = true;

        private enum AgeUnit
        {
            Days,
            Weeks,
            Months,
        }

        private static bool IsStorageConfigured =>
            BunnyDeliverySecrets.Has(BunnyDeliverySecrets.StorageZoneName) &&
            BunnyDeliverySecrets.Has(BunnyDeliverySecrets.StorageZoneApiAccessKey);

        private void OnBuildsTabSelected()
        {
#if USING_BUNNY_CDN
            if (this.builds == null && IsStorageConfigured && this.isRunning == false)
            {
                this.RunAsync(this.LoadBuilds);
            }
#endif
        }

        private void DrawBuildsTab()
        {
#if USING_BUNNY_CDN
            if (IsStorageConfigured == false)
            {
                EditorGUILayout.HelpBox("Pick a pull zone in the Config tab first.", MessageType.Warning);
                return;
            }

            this.DrawBuildsList();
            EditorGUILayout.Space(8);
            this.DrawDeleteOldBuilds();
#else
            EditorGUILayout.HelpBox("The BunnyCDN Storage package (net.bunnycdn.storage) is missing.", MessageType.Warning);
#endif
        }

#if USING_BUNNY_CDN

        private void DrawBuildsList()
        {
            using (new EditorGUILayout.VerticalScope(this.sectionStyle))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Builds", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();

                    var summary = this.builds == null
                        ? "Not loaded"
                        : $"{this.builds.Count} builds · {FormatSize(this.builds.Sum(x => x.totalSize))} · Loaded {this.buildsFetchedAt:t}";

                    GUILayout.Label(summary, EditorStyles.miniLabel);

                    if (GUILayout.Button("Refresh", EditorStyles.miniButton, GUILayout.Width(70)))
                    {
                        this.RunAsync(this.LoadBuilds);
                    }
                }

                EditorGUILayout.Space(4);

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("From", GUILayout.Width(34));
                    this.buildsFromFilter = EditorGUILayout.TextField(this.buildsFromFilter, GUILayout.Width(90));
                    GUILayout.Label("To", GUILayout.Width(20));
                    this.buildsToFilter = EditorGUILayout.TextField(this.buildsToFilter, GUILayout.Width(90));
                    GUILayout.Space(8);
                    this.savedBuildsOnly = EditorGUILayout.ToggleLeft("Saved only", this.savedBuildsOnly, GUILayout.Width(90));
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Clear", EditorStyles.miniButton, GUILayout.Width(50)))
                    {
                        GUI.FocusControl(null);
                        this.buildsFromFilter = string.Empty;
                        this.buildsToFilter = string.Empty;
                        this.savedBuildsOnly = false;
                    }
                }

                var hasFrom = TryParseFilterDate(this.buildsFromFilter, out var from);
                var hasTo = TryParseFilterDate(this.buildsToFilter, out var to);

                if ((hasFrom == false && string.IsNullOrWhiteSpace(this.buildsFromFilter) == false) ||
                    (hasTo == false && string.IsNullOrWhiteSpace(this.buildsToFilter) == false))
                {
                    EditorGUILayout.LabelField("Dates should look like 2026-10-16.", this.detailStyle);
                }

                if (this.builds == null)
                {
                    return;
                }

                var filtered = this.builds
                    .Where(x => this.savedBuildsOnly == false || x.saved)
                    .Where(x =>
                    {
                        if (hasFrom == false && hasTo == false)
                        {
                            return true;
                        }

                        if (TryGetBuildTime(x, out var time) == false)
                        {
                            return false;
                        }

                        return (hasFrom == false || time.Date >= from.Date) && (hasTo == false || time.Date <= to.Date);
                    })
                    .OrderByDescending(x => x.buildStartTime, StringComparer.Ordinal)
                    .ToList();

                EditorGUILayout.Space(6);

                if (filtered.Count == 0)
                {
                    EditorGUILayout.LabelField(this.builds.Count == 0 ? "No builds yet." : "No builds match the filter.", this.detailStyle);
                    return;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(24);
                    GUILayout.Label($"Build ({filtered.Count})", EditorStyles.miniBoldLabel);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Saved", EditorStyles.miniBoldLabel, GUILayout.Width(70));
                }

                for (int i = 0; i < filtered.Count; i++)
                {
                    this.DrawBuildRow(filtered[i], i);
                }
            }
        }

        private void DrawBuildRow(BunnyCDNBuildInfo build, int index)
        {
            var rect = EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(34));

            if (Event.current.type == EventType.Repaint && index % 2 == 0)
            {
                EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 0.03f) : new Color(0, 0, 0, 0.04f));
            }

            GUILayout.Label(EditorGUIUtility.IconContent(GetPlatformIcon(build.buildTarget)), GUILayout.Width(20), GUILayout.Height(30));

            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Label(GetBuildName(build), EditorStyles.label);
                GUILayout.Label(GetBuildDetail(build), this.detailStyle);
            }

            GUILayout.FlexibleSpace();

            using (new EditorGUILayout.VerticalScope(GUILayout.Width(64)))
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Delete", EditorStyles.miniButton, GUILayout.Width(60)))
                {
                    this.ConfirmDeleteBuild(build);
                }

                GUILayout.FlexibleSpace();
            }

            using (new EditorGUILayout.VerticalScope(GUILayout.Width(70)))
            {
                GUILayout.FlexibleSpace();

                var saved = EditorGUILayout.ToggleLeft("Saved", build.saved, GUILayout.Width(66));

                if (saved != build.saved)
                {
                    this.RunAsync(() => this.SetBuildSaved(build, saved));
                }

                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void ConfirmDeleteBuild(BunnyCDNBuildInfo build)
        {
            var savedWarning = build.saved ? "\n\nThis build is marked as saved." : string.Empty;

            var confirmed = EditorUtility.DisplayDialog(
                "Delete Build",
                $"Are you sure you want to permanently delete this build from Bunny?\n\n{GetBuildName(build)}\n{GetBuildDetail(build)}{savedWarning}\n\nThis can't be undone.",
                "Delete",
                "Cancel");

            if (confirmed)
            {
                this.RunAsync(() => this.DeleteBuilds(new List<BunnyCDNBuildInfo> { build }));
            }
        }

        private void DrawDeleteOldBuilds()
        {
            using (new EditorGUILayout.VerticalScope(this.sectionStyle))
            {
                EditorGUILayout.LabelField("Delete Old Builds", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Older than", GUILayout.Width(70));
                    this.deleteOlderThanAmount = Mathf.Max(1, EditorGUILayout.IntField(this.deleteOlderThanAmount, GUILayout.Width(50)));
                    this.deleteOlderThanUnit = (AgeUnit)EditorGUILayout.EnumPopup(this.deleteOlderThanUnit, GUILayout.Width(80));
                    GUILayout.FlexibleSpace();
                }

                this.keepSavedBuilds = EditorGUILayout.ToggleLeft("Keep saved builds", this.keepSavedBuilds);

                if (this.builds == null)
                {
                    EditorGUILayout.LabelField("Load the builds first.", this.detailStyle);
                    return;
                }

                var cutoff = this.GetDeleteCutoff();
                var buildsToDelete = this.GetBuildsToDelete(cutoff);
                var totalSize = FormatSize(buildsToDelete.Sum(x => x.totalSize));

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField($"{buildsToDelete.Count} builds started before {cutoff:d} · {totalSize}", this.detailStyle);

                for (int i = 0; i < buildsToDelete.Count; i++)
                {
                    var build = buildsToDelete[i];
                    this.DrawRow(i, GetPlatformIcon(build.buildTarget), GetBuildName(build), GetBuildDetail(build), "Will be deleted", DeleteColor);
                }

                EditorGUILayout.Space(6);

                using (new EditorGUI.DisabledScope(buildsToDelete.Count == 0))
                {
                    if (GUILayout.Button($"Delete {buildsToDelete.Count} Builds ({totalSize})", GUILayout.Height(32)))
                    {
                        var confirmed = EditorUtility.DisplayDialog(
                            "Delete Builds",
                            $"Permanently delete {buildsToDelete.Count} builds ({totalSize}) from Bunny?\n\nThis can't be undone.",
                            "Delete",
                            "Cancel");

                        if (confirmed)
                        {
                            this.RunAsync(() => this.DeleteBuilds(buildsToDelete));
                        }
                    }
                }
            }
        }

        private DateTime GetDeleteCutoff()
        {
            return this.deleteOlderThanUnit switch
            {
                AgeUnit.Days => DateTime.Now.AddDays(-this.deleteOlderThanAmount),
                AgeUnit.Weeks => DateTime.Now.AddDays(-7 * this.deleteOlderThanAmount),
                _ => DateTime.Now.AddMonths(-this.deleteOlderThanAmount),
            };
        }

        // Builds without a readable start time are never deleted
        private List<BunnyCDNBuildInfo> GetBuildsToDelete(DateTime cutoff)
        {
            return this.builds
                .Where(x => this.keepSavedBuilds == false || x.saved == false)
                .Where(x => TryGetBuildTime(x, out var time) && time < cutoff)
                .OrderBy(x => x.buildStartTime, StringComparer.Ordinal)
                .ToList();
        }

        private BunnyBuildsStore CreateBuildsStore()
        {
            return new BunnyBuildsStore(
                BunnyDeliverySecrets.Get(BunnyDeliverySecrets.StorageZoneName),
                BunnyDeliverySecrets.Get(BunnyDeliverySecrets.StorageZoneApiAccessKey),
                BunnyDeliverySecrets.Get(BunnyDeliverySecrets.StorageZoneMainReplicationRegion));
        }

        private async Task LoadBuilds()
        {
            this.SetStatus($"Loading {BunnyCDNBuildStep.BuildsJsonFileName}...");

            var buildList = await this.CreateBuildsStore().ReadBuildListAsync();

            this.builds = buildList.builds;
            this.buildsFetchedAt = DateTime.Now;
            this.SetStatus($"Loaded {this.builds.Count} builds.");
        }

        private async Task SetBuildSaved(BunnyCDNBuildInfo build, bool saved)
        {
            var store = this.CreateBuildsStore();
            var guid = build.guid;
            var name = GetBuildName(build);

            this.SetStatus($"{(saved ? "Saving" : "Unsaving")} {name}...");

            // build_info.json first, so if builds.json ever has to be rebuilt the build comes back with the right flag
            try
            {
                var buildInfo = await store.ReadBuildInfoAsync(guid);

                if (buildInfo != null)
                {
                    buildInfo.saved = saved;
                    await store.WriteBuildInfoAsync(buildInfo);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Bunny Delivery] Couldn't update {BunnyCDNBuildStep.BuildInfoFileName} for {guid}: {ex.Message}");
            }

            await store.UpdateBuildListAsync(
                $"{(saved ? "saving" : "unsaving")} build {guid}",
                buildList => buildList.builds.Where(x => x.guid == guid).ToList().ForEach(x => x.saved = saved),
                buildList => buildList.builds.Where(x => x.guid == guid).All(x => x.saved == saved));

            await this.LoadBuilds();
            this.SetStatus($"{(saved ? "Saved" : "Unsaved")} {name}.");
        }

        private async Task DeleteBuilds(List<BunnyCDNBuildInfo> buildsToDelete)
        {
            var store = this.CreateBuildsStore();
            var existingFolders = await store.GetBuildFolderGuidsAsync();
            var deletedGuids = new HashSet<string>();
            long deletedSize = 0;

            for (int i = 0; i < buildsToDelete.Count; i++)
            {
                var build = buildsToDelete[i];
                this.SetStatus($"Deleting build {i + 1}/{buildsToDelete.Count}\n\n{GetBuildName(build)}");

                try
                {
                    // Bunny's delete only reports success / failure, so a folder that's already gone is checked up front
                    if (existingFolders.Contains(build.guid))
                    {
                        Debug.Log($"[Bunny Delivery] Deleting build folder {build.guid} ({GetBuildName(build)})");
                        await store.DeleteBuildFolderAsync(build.guid);
                    }

                    deletedGuids.Add(build.guid);
                    deletedSize += build.totalSize;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Bunny Delivery] Failed to delete build {build.guid}: {ex.Message}");
                }
            }

            if (deletedGuids.Count > 0)
            {
                // Folders are deleted first, so self-healing can't add these builds back
                await store.UpdateBuildListAsync(
                    $"deleting {deletedGuids.Count} builds",
                    buildList => buildList.builds.RemoveAll(x => deletedGuids.Contains(x.guid)),
                    buildList => buildList.builds.Any(x => deletedGuids.Contains(x.guid)) == false);
            }

            await this.LoadBuilds();

            var failedCount = buildsToDelete.Count - deletedGuids.Count;
            var result = $"Deleted {deletedGuids.Count} builds ({FormatSize(deletedSize)})." + (failedCount > 0 ? $" {failedCount} failed, see the console." : string.Empty);

            this.SetStatus(result);
            Debug.Log("[Bunny Delivery] " + result);
        }

        private static string GetBuildName(BunnyCDNBuildInfo build)
        {
            return $"{build.gameName} {build.version}".Trim();
        }

        private static string GetBuildDetail(BunnyCDNBuildInfo build)
        {
            var date = TryGetBuildTime(build, out var time) ? time.ToString("g") : "Unknown date";
            var parts = new[] { date, build.platform, build.buildTarget, build.channel, FormatSize(build.totalSize), build.guid };
            return string.Join("  ·  ", parts.Where(x => string.IsNullOrEmpty(x) == false));
        }

        private static string GetPlatformIcon(string buildTarget)
        {
            if (buildTarget == "Android")
            {
                return "BuildSettings.Android.Small";
            }

            if (buildTarget == "WebGL")
            {
                return "BuildSettings.WebGL.Small";
            }

            if (buildTarget == "iOS")
            {
                return "BuildSettings.iPhone.Small";
            }

            if (buildTarget != null && buildTarget.StartsWith("Standalone", StringComparison.Ordinal))
            {
                return "BuildSettings.Standalone.Small";
            }

            return "BuildSettings.Editor.Small";
        }

        private static bool TryGetBuildTime(BunnyCDNBuildInfo build, out DateTime localTime)
        {
            if (DateTime.TryParse(build.buildStartTime, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var time))
            {
                localTime = time.ToLocalTime();
                return true;
            }

            localTime = default;
            return false;
        }

        private static bool TryParseFilterDate(string text, out DateTime date)
        {
            date = default;
            return string.IsNullOrWhiteSpace(text) == false && DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out date);
        }

        private static string FormatSize(long bytes)
        {
            if (bytes <= 0)
            {
                return "0 B";
            }

            string[] units = { "B", "KB", "MB", "GB", "TB" };
            var unit = Math.Min(units.Length - 1, (int)Math.Floor(Math.Log(bytes, 1024)));
            return $"{bytes / Math.Pow(1024, unit):0.#} {units[unit]}";
        }

#endif
    }
}
