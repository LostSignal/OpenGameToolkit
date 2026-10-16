//-----------------------------------------------------------------------
// <copyright file="BunnyDeliveryEditorWindow.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEngine;

    //// Config tab: Picks a Pull Zone with your Bunny API key, pulls the Storage Zone info underneath it and
    ////             stores everything in the OGT Secrets (BunnyDeliverySecrets).
    //// Rules tab:  Creates / updates the Pull Zone Edge Rules needed for Unity builds (WebGL compression,
    ////             content types, never caching builds.json, and the builds folder password).
    //// Builds tab: Lists the builds in builds.json, marks builds as saved and deletes old builds
    ////             (see BunnyDeliveryEditorWindow.Builds.cs).
    public sealed partial class BunnyDeliveryEditorWindow : EditorWindow
    {
        private const string ApiBase = "https://api.bunny.net";

        // Every rule owned by this tool begins with this (kept from the old Unity WebGL configurator so
        // previously created rules are still recognized)
        private const string RulePrefix = "[Unity WebGL] ";

        // Edge Rule Action Types
        private const int ActionOverrideCacheTime = 3;
        private const int ActionBlockRequest = 4;
        private const int ActionSetResponseHeader = 5;
        private const int ActionBypassPermaCache = 15;
        private const int ActionOverrideBrowserCacheResponseHeader = 25;

        // Edge Rule Trigger Types
        private const int TriggerTypeUrl = 0;
        private const int TriggerTypeRequestHeader = 1;

        // Trigger / Pattern Matching Types
        private const int MatchAny = 0;
        private const int MatchAll = 1;
        private const int MatchNone = 2;

        // Bunny Origin Type for Pull Zones backed by a Storage Zone
        private const int OriginTypeStorageZone = 2;

        private const int GeneratedPasswordLength = 32;
        private const string PasswordCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        // builds.json is polled by external tools to detect new builds, so it must never be cached
        private static readonly string[] NeverCachedPatterns = { "*/builds.json", "*/builds.json?*" };
        private static readonly string[] BuildsFolderPatterns = { "*/builds/*" };

        private static readonly Color InstalledColor = new Color(0.35f, 0.8f, 0.45f);
        private static readonly Color OutOfDateColor = new Color(1.0f, 0.7f, 0.25f);
        private static readonly Color MissingColor = new Color(0.6f, 0.6f, 0.6f);
        private static readonly Color DeleteColor = new Color(0.95f, 0.4f, 0.4f);

        private Tab tab;
        private Vector2 scrollPosition;
        private bool isRunning;
        private string status = "Ready.";

        // Config
        private string apiKey = string.Empty;
        private string pullZoneId = string.Empty;
        private string storageZoneName = string.Empty;
        private string storageZoneApiAccessKey = string.Empty;
        private string storageZoneRegion = string.Empty;
        private string storagePullZoneUrl = string.Empty;
        private string buildsPassword = string.Empty;
        private bool showBuildsPassword;
        private PullZone[] pullZones = Array.Empty<PullZone>();

        // Rules
        private bool deleteObsoleteRules = true;
        private EdgeRule[] installedRules;
        private DateTime installedRulesFetchedAt;

        private GUIStyle sectionStyle;
        private GUIStyle statusStyle;
        private GUIStyle detailStyle;

        private enum Tab
        {
            Config,
            Rules,
            Builds,
        }

        private enum RuleState
        {
            Unknown,
            Installed,
            OutOfDate,
            Missing,
            Skipped,
        }

        private bool IsConfigured => string.IsNullOrWhiteSpace(this.apiKey) == false && long.TryParse(this.pullZoneId, out _);

        [MenuItem("Tools/OGT/Tools/Bunny Delivery", priority = MenuItemPriorities.Tools + 2)]
        public static void Open()
        {
            var window = GetWindow<BunnyDeliveryEditorWindow>();
            window.titleContent = new GUIContent("Bunny Delivery");
            window.minSize = new Vector2(560, 600);
            window.Show();
        }

        private void OnEnable()
        {
            this.LoadFromSecrets();
        }

        private void OnGUI()
        {
            this.InitializeStyles();

            EditorGUILayout.Space(6);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                var newTab = (Tab)GUILayout.Toolbar((int)this.tab, new[] { "Config", "Rules", "Builds" }, GUILayout.Width(360), GUILayout.Height(24));

                if (newTab != this.tab)
                {
                    this.tab = newTab;

                    if (this.tab == Tab.Rules && this.installedRules == null && this.IsConfigured && this.isRunning == false)
                    {
                        this.RunAsync(this.RefreshInstalledRules);
                    }
                    else if (this.tab == Tab.Builds)
                    {
                        this.OnBuildsTabSelected();
                    }
                }

                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.Space(6);

            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);

            using (new EditorGUI.DisabledScope(this.isRunning))
            {
                if (this.tab == Tab.Config)
                {
                    this.DrawConfigTab();
                }
                else if (this.tab == Tab.Rules)
                {
                    this.DrawRulesTab();
                }
                else
                {
                    this.DrawBuildsTab();
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(this.status, this.isRunning ? MessageType.Info : MessageType.None);
        }

        private void InitializeStyles()
        {
            if (this.sectionStyle != null)
            {
                return;
            }

            this.sectionStyle = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(10, 10, 8, 10) };
            this.statusStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleRight };
            this.detailStyle = new GUIStyle(EditorStyles.miniLabel) { wordWrap = true };
        }

        //// ===================================================================================================
        //// Config Tab
        //// ===================================================================================================

        private void DrawConfigTab()
        {
            using (new EditorGUILayout.VerticalScope(this.sectionStyle))
            {
                EditorGUILayout.LabelField("Bunny Account", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                this.apiKey = EditorGUILayout.PasswordField(new GUIContent("API Key", "Your Bunny account API key (Account Settings → API)."), this.apiKey);

                if (EditorGUI.EndChangeCheck())
                {
                    this.pullZones = Array.Empty<PullZone>();
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(this.apiKey)))
                    {
                        if (GUILayout.Button(this.pullZones.Length == 0 ? "Load Pull Zones" : "Reload Pull Zones", GUILayout.Width(140)))
                        {
                            this.RunAsync(this.LoadPullZones);
                        }
                    }
                }
            }

            EditorGUILayout.Space(8);

            using (new EditorGUILayout.VerticalScope(this.sectionStyle))
            {
                EditorGUILayout.LabelField("Pull Zone", EditorStyles.boldLabel);

                if (this.pullZones.Length == 0)
                {
                    EditorGUILayout.HelpBox("Load your pull zones to pick one.", MessageType.None);
                }
                else
                {
                    var names = this.pullZones.Select(x => x.Name).ToArray();
                    var selectedIndex = Array.FindIndex(this.pullZones, x => x.Id.ToString() == this.pullZoneId);
                    var newIndex = EditorGUILayout.Popup("Pull Zone", selectedIndex, names);

                    if (newIndex != selectedIndex && newIndex >= 0)
                    {
                        var pullZone = this.pullZones[newIndex];
                        this.RunAsync(() => this.SelectPullZone(pullZone.Id));
                    }
                }

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.TextField("Pull Zone ID", this.pullZoneId);
                }
            }

            EditorGUILayout.Space(8);

            using (new EditorGUILayout.VerticalScope(this.sectionStyle))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Storage Zone", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();

                    using (new EditorGUI.DisabledScope(this.IsConfigured == false))
                    {
                        if (GUILayout.Button("Refresh", EditorStyles.miniButton, GUILayout.Width(70)))
                        {
                            this.RunAsync(() => this.SelectPullZone(long.Parse(this.pullZoneId)));
                        }
                    }
                }

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.TextField("Name", this.storageZoneName);
                    EditorGUILayout.PasswordField("API Access Key", this.storageZoneApiAccessKey);
                    EditorGUILayout.TextField("Main Replication Region", this.storageZoneRegion);
                    EditorGUILayout.TextField("Pull Zone URL", this.storagePullZoneUrl);
                }
            }

            EditorGUILayout.Space(8);

            using (new EditorGUILayout.VerticalScope(this.sectionStyle))
            {
                EditorGUILayout.LabelField("Builds Password", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();

                    this.buildsPassword = this.showBuildsPassword
                        ? EditorGUILayout.TextField("Password", this.buildsPassword)
                        : EditorGUILayout.PasswordField("Password", this.buildsPassword);

                    if (EditorGUI.EndChangeCheck())
                    {
                        this.SaveSecret(BunnyDeliverySecrets.BuildsPassword, this.buildsPassword);
                    }

                    this.showBuildsPassword = GUILayout.Toggle(this.showBuildsPassword, EditorGUIUtility.IconContent("animationvisibilitytoggleon", "Show Password"), EditorStyles.miniButton, GUILayout.Width(28));

                    if (string.IsNullOrEmpty(this.buildsPassword))
                    {
                        if (GUILayout.Button("Generate", GUILayout.Width(80)))
                        {
                            GUI.FocusControl(null);
                            this.buildsPassword = GeneratePassword();
                            this.SaveSecret(BunnyDeliverySecrets.BuildsPassword, this.buildsPassword);
                            this.SetStatus("Generated a new builds password.");
                        }
                    }
                }

                EditorGUILayout.LabelField(
                    $"Requests to /builds/* must send the header '{BunnyDeliverySecrets.BuildsPasswordHeader}: <password>' once the password rule is installed.",
                    this.detailStyle);

                if (Regex.IsMatch(this.buildsPassword ?? string.Empty, @"[*?,\s]"))
                {
                    EditorGUILayout.HelpBox("Edge rule patterns treat * ? , and whitespace specially, so the password shouldn't contain them.", MessageType.Warning);
                }
            }

            EditorGUILayout.Space(8);

            using (new EditorGUILayout.VerticalScope(this.sectionStyle))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("OGT Secrets", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Edit Secrets", EditorStyles.miniButton, GUILayout.Width(90)))
                    {
                        SecretsEditorWindow.Open();
                    }
                }

                foreach (var key in BunnyDeliverySecrets.All)
                {
                    var exists = BunnyDeliverySecrets.Has(key);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label(EditorGUIUtility.IconContent(exists ? "TestPassed" : "TestNormal"), GUILayout.Width(20), GUILayout.Height(18));
                        EditorGUILayout.LabelField(key);
                        GUILayout.FlexibleSpace();
                        this.DrawColoredLabel(exists ? "Saved" : "Missing", exists ? InstalledColor : MissingColor, 80);
                    }
                }
            }
        }

        private async Task LoadPullZones()
        {
            this.SetStatus("Loading pull zones...");

            using var client = new BunnyClient(this.apiKey);
            this.pullZones = (await client.GetPullZones()).OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToArray();

            // The key worked, so it's safe to keep
            this.SaveSecret(BunnyDeliverySecrets.ApiKey, this.apiKey);
            this.SetStatus($"Found {this.pullZones.Length} pull zones.");
        }

        private async Task SelectPullZone(long id)
        {
            this.SetStatus($"Reading pull zone {id}...");

            using var client = new BunnyClient(this.apiKey);
            var pullZone = await client.GetPullZone(id);

            if (pullZone.OriginType != OriginTypeStorageZone || pullZone.StorageZoneId <= 0)
            {
                throw new Exception($"Pull zone '{pullZone.Name}' isn't backed by a Storage Zone.");
            }

            this.SetStatus($"Reading storage zone {pullZone.StorageZoneId}...");
            var storageZone = await client.GetStorageZone(pullZone.StorageZoneId);

            this.pullZoneId = pullZone.Id.ToString();
            this.storageZoneName = storageZone.Name ?? string.Empty;
            this.storageZoneApiAccessKey = storageZone.Password ?? string.Empty;
            this.storageZoneRegion = (storageZone.Region ?? string.Empty).ToLowerInvariant();
            this.storagePullZoneUrl = GetPullZoneUrl(pullZone);
            this.installedRules = pullZone.EdgeRules ?? Array.Empty<EdgeRule>();
            this.installedRulesFetchedAt = DateTime.Now;

            SecretsStore.Set(BunnyDeliverySecrets.ApiKey, this.apiKey);
            SecretsStore.Set(BunnyDeliverySecrets.PullZoneId, this.pullZoneId);
            SecretsStore.Set(BunnyDeliverySecrets.StorageZoneName, this.storageZoneName);
            SecretsStore.Set(BunnyDeliverySecrets.StorageZoneApiAccessKey, this.storageZoneApiAccessKey);
            SecretsStore.Set(BunnyDeliverySecrets.StorageZoneMainReplicationRegion, this.storageZoneRegion);
            SecretsStore.Set(BunnyDeliverySecrets.StoragePullZoneUrl, this.storagePullZoneUrl);
            SecretsStore.Save();

            this.SetStatus($"Saved '{pullZone.Name}' (storage zone '{this.storageZoneName}') to the OGT Secrets.");
        }

        private static string GetPullZoneUrl(PullZone pullZone)
        {
            var hostnames = pullZone.Hostnames ?? Array.Empty<Hostname>();

            // Prefer a custom domain with a certificate, otherwise fall back to the b-cdn.net hostname
            var hostname =
                hostnames.FirstOrDefault(x => x.IsSystemHostname == false && x.HasCertificate) ??
                hostnames.FirstOrDefault(x => x.IsSystemHostname) ??
                hostnames.FirstOrDefault();

            return hostname == null ? $"https://{pullZone.Name}.b-cdn.net" : $"https://{hostname.Value}";
        }

        private static string GeneratePassword()
        {
            var result = new StringBuilder(GeneratedPasswordLength);
            var buffer = new byte[1];

            using var random = RandomNumberGenerator.Create();

            // Rejection sampling so every character is equally likely
            int limit = 256 - (256 % PasswordCharacters.Length);

            while (result.Length < GeneratedPasswordLength)
            {
                random.GetBytes(buffer);

                if (buffer[0] < limit)
                {
                    result.Append(PasswordCharacters[buffer[0] % PasswordCharacters.Length]);
                }
            }

            return result.ToString();
        }

        private void LoadFromSecrets()
        {
            this.apiKey = BunnyDeliverySecrets.Get(BunnyDeliverySecrets.ApiKey);
            this.pullZoneId = BunnyDeliverySecrets.Get(BunnyDeliverySecrets.PullZoneId);
            this.storageZoneName = BunnyDeliverySecrets.Get(BunnyDeliverySecrets.StorageZoneName);
            this.storageZoneApiAccessKey = BunnyDeliverySecrets.Get(BunnyDeliverySecrets.StorageZoneApiAccessKey);
            this.storageZoneRegion = BunnyDeliverySecrets.Get(BunnyDeliverySecrets.StorageZoneMainReplicationRegion);
            this.storagePullZoneUrl = BunnyDeliverySecrets.Get(BunnyDeliverySecrets.StoragePullZoneUrl);
            this.buildsPassword = BunnyDeliverySecrets.Get(BunnyDeliverySecrets.BuildsPassword);
        }

        private void SaveSecret(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                SecretsStore.Remove(key);
            }
            else
            {
                SecretsStore.Set(key, value);
            }

            SecretsStore.Save();
        }

        //// ===================================================================================================
        //// Rules Tab
        //// ===================================================================================================

        private void DrawRulesTab()
        {
            if (this.IsConfigured == false)
            {
                EditorGUILayout.HelpBox("Pick a pull zone in the Config tab first.", MessageType.Warning);
                return;
            }

            var desiredRules = this.BuildRules();

            using (new EditorGUILayout.VerticalScope(this.sectionStyle))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Rules", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();

                    var fetchedLabel = this.installedRules == null ? "Not checked" : $"Checked {this.installedRulesFetchedAt:t}";
                    GUILayout.Label(fetchedLabel, EditorStyles.miniLabel);

                    if (GUILayout.Button("Refresh", EditorStyles.miniButton, GUILayout.Width(70)))
                    {
                        this.RunAsync(this.RefreshInstalledRules);
                    }
                }

                EditorGUILayout.Space(4);
                this.DrawRuleHeader();

                foreach (var group in desiredRules.GroupBy(x => x.Group))
                {
                    var rules = group.ToList();
                    var installedCount = rules.Count(x => this.GetRuleState(x) == RuleState.Installed);

                    EditorGUILayout.Space(6);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(group.Key, EditorStyles.boldLabel);
                        GUILayout.FlexibleSpace();
                        GUILayout.Label($"{installedCount}/{rules.Count}", EditorStyles.miniLabel);
                    }

                    for (int i = 0; i < rules.Count; i++)
                    {
                        this.DrawRuleRow(rules[i], i);
                    }
                }

                var obsoleteRules = this.GetObsoleteRules(desiredRules);

                if (obsoleteRules.Count > 0)
                {
                    EditorGUILayout.Space(6);
                    EditorGUILayout.LabelField("No Longer Needed", EditorStyles.boldLabel);

                    for (int i = 0; i < obsoleteRules.Count; i++)
                    {
                        this.DrawObsoleteRow(obsoleteRules[i], i);
                    }
                }
            }

            EditorGUILayout.Space(8);

            using (new EditorGUILayout.VerticalScope(this.sectionStyle))
            {
                this.deleteObsoleteRules = EditorGUILayout.ToggleLeft(
                    new GUIContent("Delete rules that are no longer needed", "Cache rules are always reconciled, this controls everything else."),
                    this.deleteObsoleteRules);

                EditorGUILayout.Space(4);

                var pendingCount = desiredRules.Count(x => this.GetRuleState(x) is RuleState.Missing or RuleState.OutOfDate or RuleState.Unknown);

                if (GUILayout.Button(this.isRunning ? "Applying..." : $"Apply Rules ({pendingCount} to install)", GUILayout.Height(36)))
                {
                    this.RunAsync(() => this.ApplyRules(desiredRules));
                }

                EditorGUILayout.LabelField("Purge the pull zone after changing rules so cached responses pick them up.", this.detailStyle);
            }
        }

        private void DrawRuleHeader()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(24);
                GUILayout.Label("Rule", EditorStyles.miniBoldLabel);
                GUILayout.FlexibleSpace();
                GUILayout.Label("Status", EditorStyles.miniBoldLabel, GUILayout.Width(130));
            }
        }

        private void DrawRuleRow(DesiredRule rule, int index)
        {
            var state = this.GetRuleState(rule);

            var (icon, label, color) = state switch
            {
                RuleState.Installed => ("TestPassed", "Installed", InstalledColor),
                RuleState.OutOfDate => ("console.warnicon.sml", "Out of date", OutOfDateColor),
                RuleState.Missing => ("TestNormal", "Not installed", MissingColor),
                RuleState.Skipped => ("console.warnicon.sml", rule.SkipReason, OutOfDateColor),
                _ => ("TestNormal", "Unknown", MissingColor),
            };

            var canInstall = state is RuleState.Missing or RuleState.OutOfDate;

            this.DrawRow(index, icon, rule.Name, rule.Detail, label, color, canInstall ? "Install" : null, () => this.RunAsync(() => this.InstallRule(rule)));
        }

        private void DrawObsoleteRow(EdgeRule rule, int index)
        {
            var willDelete = this.deleteObsoleteRules || IsCacheRule(rule);

            this.DrawRow(
                index,
                willDelete ? "TestFailed" : "TestNormal",
                rule.Description.Substring(RulePrefix.Length),
                willDelete ? "Will be deleted on apply." : "Kept on apply (deleting is turned off).",
                willDelete ? "Will be deleted" : "Kept",
                willDelete ? DeleteColor : MissingColor,
                "Delete",
                () => this.RunAsync(() => this.DeleteRule(rule)));
        }

        private void DrawRow(int index, string icon, string name, string detail, string statusLabel, Color statusColor, string buttonLabel = null, Action buttonAction = null)
        {
            var rect = EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(34));

            if (Event.current.type == EventType.Repaint && index % 2 == 0)
            {
                EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 0.03f) : new Color(0, 0, 0, 0.04f));
            }

            GUILayout.Label(EditorGUIUtility.IconContent(icon), GUILayout.Width(20), GUILayout.Height(30));

            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Label(name, EditorStyles.label);
                GUILayout.Label(detail, this.detailStyle);
            }

            GUILayout.FlexibleSpace();

            using (new EditorGUILayout.VerticalScope(GUILayout.Width(64)))
            {
                GUILayout.FlexibleSpace();

                if (buttonLabel != null && GUILayout.Button(buttonLabel, EditorStyles.miniButton, GUILayout.Width(60)))
                {
                    buttonAction?.Invoke();
                }

                GUILayout.FlexibleSpace();
            }

            using (new EditorGUILayout.VerticalScope(GUILayout.Width(130)))
            {
                GUILayout.FlexibleSpace();
                this.DrawColoredLabel(statusLabel, statusColor, 130);
                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawColoredLabel(string text, Color color, float width)
        {
            var previousColor = this.statusStyle.normal.textColor;
            this.statusStyle.normal.textColor = color;
            GUILayout.Label(text, this.statusStyle, GUILayout.Width(width));
            this.statusStyle.normal.textColor = previousColor;
        }

        private RuleState GetRuleState(DesiredRule rule)
        {
            if (rule.SkipReason != null)
            {
                return RuleState.Skipped;
            }

            if (this.installedRules == null)
            {
                return RuleState.Unknown;
            }

            var description = GetDescription(rule);
            var existing = this.installedRules.FirstOrDefault(x => x.Description == description);

            if (existing == null)
            {
                return RuleState.Missing;
            }

            return IsInstalled(existing, CreateRequest(rule, existing.Guid)) ? RuleState.Installed : RuleState.OutOfDate;
        }

        private List<EdgeRule> GetObsoleteRules(List<DesiredRule> desiredRules)
        {
            if (this.installedRules == null)
            {
                return new List<EdgeRule>();
            }

            // Skipped rules (ie. the password rule without a password) are left alone rather than deleted
            var desiredDescriptions = new HashSet<string>(desiredRules.Select(GetDescription), StringComparer.Ordinal);

            return this.installedRules
                .Where(x => IsManagedRule(x) && desiredDescriptions.Contains(x.Description) == false)
                .ToList();
        }

        private async Task RefreshInstalledRules()
        {
            this.SetStatus("Reading pull zone edge rules...");

            using var client = new BunnyClient(this.apiKey);
            var pullZone = await client.GetPullZone(long.Parse(this.pullZoneId));

            this.installedRules = pullZone.EdgeRules ?? Array.Empty<EdgeRule>();
            this.installedRulesFetchedAt = DateTime.Now;
            this.SetStatus($"Found {this.installedRules.Count(IsManagedRule)} rules managed by this tool on '{pullZone.Name}'.");
        }

        private async Task InstallRule(DesiredRule rule)
        {
            using var client = new BunnyClient(this.apiKey);

            // Working off the latest rules so an existing rule is updated rather than duplicated
            await this.RefreshInstalledRules();

            var description = GetDescription(rule);
            var existing = this.installedRules.FirstOrDefault(x => x.Description == description);

            this.SetStatus($"{(existing != null ? "Updating" : "Installing")} {rule.Name}...");
            Debug.Log($"[Bunny Delivery] {(existing != null ? "Updating" : "Creating")}: {description}");
            await client.AddOrUpdateEdgeRule(long.Parse(this.pullZoneId), CreateRequest(rule, existing?.Guid));

            await this.RefreshInstalledRules();
            this.SetStatus($"Installed {rule.Name}.");
        }

        private async Task DeleteRule(EdgeRule rule)
        {
            if (string.IsNullOrWhiteSpace(rule.Guid))
            {
                throw new Exception($"Cannot delete rule without GUID: {rule.Description}");
            }

            using var client = new BunnyClient(this.apiKey);

            this.SetStatus($"Deleting {rule.Description}...");
            Debug.Log($"[Bunny Delivery] Deleting: {rule.Description} ({rule.Guid})");
            await client.DeleteEdgeRule(long.Parse(this.pullZoneId), rule.Guid);

            await this.RefreshInstalledRules();
            this.SetStatus($"Deleted {rule.Description}.");
        }

        private async Task ApplyRules(List<DesiredRule> desiredRules)
        {
            long id = long.Parse(this.pullZoneId);

            using var client = new BunnyClient(this.apiKey);

            // Always working off the latest rules, in case they were changed on the dashboard
            await this.RefreshInstalledRules();

            int created = 0;
            int updated = 0;
            int deleted = 0;

            var rulesToSend = desiredRules.Where(x => this.GetRuleState(x) is RuleState.Missing or RuleState.OutOfDate).ToList();

            for (int i = 0; i < rulesToSend.Count; i++)
            {
                var rule = rulesToSend[i];
                var description = GetDescription(rule);
                var existing = this.installedRules.FirstOrDefault(x => x.Description == description);

                this.SetStatus($"Rule {i + 1}/{rulesToSend.Count}\n\n{rule.Name}");
                Debug.Log($"[Bunny Delivery] {(existing != null ? "Updating" : "Creating")}: {description}");

                await client.AddOrUpdateEdgeRule(id, CreateRequest(rule, existing?.Guid));

                if (existing != null)
                {
                    updated++;
                }
                else
                {
                    created++;
                }
            }

            foreach (var obsolete in this.GetObsoleteRules(desiredRules).Where(x => this.deleteObsoleteRules || IsCacheRule(x)))
            {
                if (string.IsNullOrWhiteSpace(obsolete.Guid))
                {
                    Debug.LogWarning($"[Bunny Delivery] Cannot delete rule without GUID: {obsolete.Description}");
                    continue;
                }

                this.SetStatus($"Deleting obsolete rule...\n\n{obsolete.Description}");
                Debug.Log($"[Bunny Delivery] Deleting: {obsolete.Description} ({obsolete.Guid})");
                await client.DeleteEdgeRule(id, obsolete.Guid);
                deleted++;
            }

            await this.RefreshInstalledRules();

            var skipped = desiredRules.Count(x => x.SkipReason != null);
            var result = $"Rules applied.  Created: {created}  Updated: {updated}  Deleted: {deleted}" + (skipped > 0 ? $"  Skipped: {skipped}" : string.Empty);

            this.SetStatus(result);
            Debug.Log("[Bunny Delivery] " + result);
        }

        //// ===================================================================================================
        //// Rule Generation
        //// ===================================================================================================

        private List<DesiredRule> BuildRules()
        {
            var rules = new List<DesiredRule>();

            // Compression (Unity WebGL Brotli and Gzip builds)
            AddCompressedRules(rules, "br", "br");
            AddCompressedRules(rules, "gz", "gzip");

            // Content Types
            AddContentTypeRule(rules, "*.data", "application/octet-stream");
            AddContentTypeRule(rules, "*.bundle", "application/octet-stream");
            AddContentTypeRule(rules, "*.symbols.json", "application/octet-stream");
            AddContentTypeRule(rules, "*.js", "application/javascript");
            AddContentTypeRule(rules, "*.wasm", "application/wasm");
            AddContentTypeRule(rules, "*.apk", "application/vnd.android.package-archive");

            // Never Cached (builds.json is the only file with caching rules, everything else uses the pull zone defaults)
            rules.Add(new DesiredRule
            {
                Group = "Never Cached",
                Pattern = "Never Cached",
                Patterns = NeverCachedPatterns,
                Header = "CDN Cache Time",
                Detail = "builds.json · CDN cache time → 0",
                ActionType = ActionOverrideCacheTime,
                ActionParameter1 = "0",
            });

            rules.Add(new DesiredRule
            {
                Group = "Never Cached",
                Pattern = "Never Cached",
                Patterns = NeverCachedPatterns,
                Header = "Bypass Perma-Cache",
                Detail = "builds.json · Bypass Perma-Cache",
                ActionType = ActionBypassPermaCache,
                ActionParameter1 = string.Empty,
            });

            rules.Add(new DesiredRule
            {
                Group = "Never Cached",
                Pattern = "Never Cached",
                Patterns = NeverCachedPatterns,
                Header = "Browser Cache-Control",
                Detail = "builds.json · Cache-Control: no-store, max-age=0",
                ActionType = ActionOverrideBrowserCacheResponseHeader,
                ActionParameter1 = "no-store, max-age=0",
            });

            // Security
            var hasPassword = BunnyDeliverySecrets.Has(BunnyDeliverySecrets.BuildsPassword);

            rules.Add(new DesiredRule
            {
                Group = "Security",
                Pattern = "Builds Folder",
                Patterns = BuildsFolderPatterns,
                Header = "Require Password",
                Detail = $"Blocks /builds/* unless the '{BunnyDeliverySecrets.BuildsPasswordHeader}' header matches the builds password",
                ActionType = ActionBlockRequest,
                ActionParameter1 = string.Empty,
                RequiredHeaderName = BunnyDeliverySecrets.BuildsPasswordHeader,
                RequiredHeaderValue = hasPassword ? BunnyDeliverySecrets.Get(BunnyDeliverySecrets.BuildsPassword) : null,
                SkipReason = hasPassword ? null : "Secret does not exist",
            });

            return rules;
        }

        private static void AddCompressedRules(List<DesiredRule> rules, string extension, string encoding)
        {
            var group = encoding == "br" ? "Brotli" : "Gzip";
            AddCompressedRules(rules, group, $"*.wasm.{extension}", "application/wasm", encoding);
            AddCompressedRules(rules, group, $"*.js.{extension}", "application/javascript", encoding);
            AddCompressedRules(rules, group, $"*.data.{extension}", "application/octet-stream", encoding);
            AddCompressedRules(rules, group, $"*.json.{extension}", "application/json", encoding);
        }

        private static void AddCompressedRules(List<DesiredRule> rules, string group, string pattern, string contentType, string encoding)
        {
            rules.Add(CreateResponseHeaderRule(group, pattern, "Content-Type", contentType));
            rules.Add(CreateResponseHeaderRule(group, pattern, "Content-Encoding", encoding));
        }

        private static void AddContentTypeRule(List<DesiredRule> rules, string pattern, string contentType)
        {
            rules.Add(CreateResponseHeaderRule("Content Types", pattern, "Content-Type", contentType));
        }

        private static DesiredRule CreateResponseHeaderRule(string group, string pattern, string header, string value)
        {
            return new DesiredRule
            {
                Group = group,
                Pattern = pattern,
                Header = header,
                Detail = $"{header}: {value}",
                ActionType = ActionSetResponseHeader,
                ActionParameter1 = header,
                Value = value,
            };
        }

        private static EdgeRuleRequest CreateRequest(DesiredRule rule, string guid)
        {
            var triggers = new List<EdgeRuleTrigger>
            {
                new EdgeRuleTrigger
                {
                    Type = TriggerTypeUrl,
                    PatternMatchingType = MatchAny,
                    PatternMatches = rule.Patterns ?? new[] { rule.Pattern },
                },
            };

            if (rule.ExcludedPatterns.Length > 0)
            {
                triggers.Add(new EdgeRuleTrigger
                {
                    Type = TriggerTypeUrl,
                    PatternMatchingType = MatchNone,
                    PatternMatches = rule.ExcludedPatterns,
                });
            }

            // Triggers when the header is missing or doesn't match
            if (rule.RequiredHeaderName != null)
            {
                triggers.Add(new EdgeRuleTrigger
                {
                    Type = TriggerTypeRequestHeader,
                    Parameter1 = rule.RequiredHeaderName,
                    PatternMatchingType = MatchNone,
                    PatternMatches = new[] { rule.RequiredHeaderValue ?? string.Empty },
                });
            }

            return new EdgeRuleRequest
            {
                Guid = guid,
                ActionType = rule.ActionType,
                ActionParameter1 = rule.ActionParameter1 ?? rule.Header,
                ActionParameter2 = rule.Value,
                Enabled = true,
                Description = GetDescription(rule),
                TriggerMatchingType = triggers.Count > 1 ? MatchAll : MatchAny,
                Triggers = triggers.ToArray(),
            };
        }

        private static bool IsInstalled(EdgeRule existing, EdgeRuleRequest desired)
        {
            if (existing.Enabled == false ||
                existing.ActionType != desired.ActionType ||
                Normalize(existing.ActionParameter1) != Normalize(desired.ActionParameter1) ||
                Normalize(existing.ActionParameter2) != Normalize(desired.ActionParameter2) ||
                existing.TriggerMatchingType != desired.TriggerMatchingType)
            {
                return false;
            }

            var existingTriggers = existing.Triggers ?? Array.Empty<EdgeRuleTrigger>();

            if (existingTriggers.Length != desired.Triggers.Length)
            {
                return false;
            }

            for (int i = 0; i < existingTriggers.Length; i++)
            {
                var a = existingTriggers[i];
                var b = desired.Triggers[i];

                if (a.Type != b.Type ||
                    a.PatternMatchingType != b.PatternMatchingType ||
                    Normalize(a.Parameter1) != Normalize(b.Parameter1) ||
                    (a.PatternMatches ?? Array.Empty<string>()).SequenceEqual(b.PatternMatches ?? Array.Empty<string>()) == false)
                {
                    return false;
                }
            }

            return true;

            static string Normalize(string value) => value ?? string.Empty;
        }

        //// NOTE: The description is deterministic and acts as the persistent identifier between Unity and Bunny
        private static string GetDescription(DesiredRule rule)
        {
            return RulePrefix + rule.Pattern + " " + rule.Header;
        }

        private static bool IsManagedRule(EdgeRule rule)
        {
            return rule != null && string.IsNullOrEmpty(rule.Description) == false && rule.Description.StartsWith(RulePrefix, StringComparison.Ordinal);
        }

        private static bool IsCacheRule(EdgeRule rule)
        {
            return IsManagedRule(rule) &&
                (rule.ActionType == ActionOverrideCacheTime ||
                 rule.ActionType == ActionBypassPermaCache ||
                 rule.ActionType == ActionOverrideBrowserCacheResponseHeader);
        }

        //// ===================================================================================================
        //// Helpers
        //// ===================================================================================================

        private async void RunAsync(Func<Task> action)
        {
            if (this.isRunning)
            {
                return;
            }

            this.isRunning = true;

            try
            {
                await action();
            }
            catch (Exception exception)
            {
                this.SetStatus("Failed:\n\n" + exception.Message);
                Debug.LogException(exception);
            }
            finally
            {
                this.isRunning = false;
                this.Repaint();
            }
        }

        private void SetStatus(string newStatus)
        {
            this.status = newStatus;
            this.Repaint();
        }

        //// ===================================================================================================
        //// Bunny Client
        //// ===================================================================================================

        private sealed class BunnyClient : IDisposable
        {
            private readonly HttpClient client;

            public BunnyClient(string apiKey)
            {
                this.client = new HttpClient
                {
                    BaseAddress = new Uri(ApiBase),
                    Timeout = TimeSpan.FromSeconds(30),
                };

                this.client.DefaultRequestHeaders.Add("AccessKey", apiKey);
                this.client.DefaultRequestHeaders.Add("Accept", "application/json");
            }

            public async Task<PullZone[]> GetPullZones()
            {
                var json = await this.GetJson("/pullzone?page=0&perPage=1000&includeCertificate=false");

                // Bunny returns either a plain array or a paginated { Items: [...] } object
                var list = json.TrimStart().StartsWith("[")
                    ? JsonUtility.FromJson<PullZoneList>("{\"Items\":" + json + "}")
                    : JsonUtility.FromJson<PullZoneList>(json);

                return list?.Items ?? Array.Empty<PullZone>();
            }

            public async Task<PullZone> GetPullZone(long pullZoneId)
            {
                var pullZone = JsonUtility.FromJson<PullZone>(await this.GetJson($"/pullzone/{pullZoneId}"));
                return pullZone ?? throw new Exception("Bunny returned an empty Pull Zone.");
            }

            public async Task<StorageZone> GetStorageZone(long storageZoneId)
            {
                var storageZone = JsonUtility.FromJson<StorageZone>(await this.GetJson($"/storagezone/{storageZoneId}"));
                return storageZone ?? throw new Exception("Bunny returned an empty Storage Zone.");
            }

            // Bunny uses the same endpoint for creating and updating, supplying a Guid updates the existing rule
            public async Task AddOrUpdateEdgeRule(long pullZoneId, EdgeRuleRequest request)
            {
                using var content = new StringContent(JsonUtility.ToJson(request), Encoding.UTF8, "application/json");
                using var response = await this.client.PostAsync($"/pullzone/{pullZoneId}/edgerules/addOrUpdate", content);
                EnsureSuccess(response, await response.Content.ReadAsStringAsync());
            }

            public async Task DeleteEdgeRule(long pullZoneId, string guid)
            {
                using var response = await this.client.DeleteAsync($"/pullzone/{pullZoneId}/edgerules/{guid}");
                EnsureSuccess(response, await response.Content.ReadAsStringAsync());
            }

            public void Dispose()
            {
                this.client?.Dispose();
            }

            private static void EnsureSuccess(HttpResponseMessage response, string responseBody)
            {
                if (response.IsSuccessStatusCode)
                {
                    return;
                }

                throw new Exception($"Bunny API request failed.\n\nHTTP {(int)response.StatusCode} {response.ReasonPhrase}\n\n{responseBody}");
            }

            private async Task<string> GetJson(string path)
            {
                using var response = await this.client.GetAsync(path);
                var json = await response.Content.ReadAsStringAsync();
                EnsureSuccess(response, json);
                return json;
            }
        }

        //// ===================================================================================================
        //// Models
        //// ===================================================================================================

        private sealed class DesiredRule
        {
            public string Group;
            public string Pattern;
            public string[] Patterns;
            public string[] ExcludedPatterns = Array.Empty<string>();
            public string Header;
            public string Detail;
            public int ActionType;
            public string ActionParameter1;
            public string Value;
            public string RequiredHeaderName;
            public string RequiredHeaderValue;

            // Non-null when the rule can't be sent yet (shown in the status column)
            public string SkipReason;

            public string Name => this.Pattern + "  ·  " + this.Header;
        }

        [Serializable]
        private sealed class PullZoneList
        {
            public PullZone[] Items;
        }

        [Serializable]
        private sealed class PullZone
        {
            public long Id;
            public string Name;
            public int OriginType;
            public long StorageZoneId;
            public Hostname[] Hostnames;
            public EdgeRule[] EdgeRules;
        }

        [Serializable]
        private sealed class Hostname
        {
            public string Value;
            public bool IsSystemHostname;
            public bool HasCertificate;
        }

        [Serializable]
        private sealed class StorageZone
        {
            public long Id;
            public string Name;
            public string Password;
            public string Region;
        }

        [Serializable]
        private sealed class EdgeRule
        {
            public string Guid;
            public int ActionType;
            public string ActionParameter1;
            public string ActionParameter2;
            public bool Enabled;
            public string Description;
            public int TriggerMatchingType;
            public EdgeRuleTrigger[] Triggers;
        }

        [Serializable]
        private sealed class EdgeRuleRequest
        {
            // Null when creating, existing Bunny GUID when updating
            public string Guid;
            public int ActionType;
            public string ActionParameter1;
            public string ActionParameter2;
            public bool Enabled;
            public string Description;
            public int TriggerMatchingType;
            public EdgeRuleTrigger[] Triggers;
        }

        [Serializable]
        private sealed class EdgeRuleTrigger
        {
            public int Type;
            public string[] PatternMatches;
            public int PatternMatchingType;
            public string Parameter1;
        }
    }
}
