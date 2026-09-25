//-----------------------------------------------------------------------
// <copyright file="AppInfoBuildStep.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BuildProfile
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.AddressableAssets.Settings;
    using UnityEditor.Build;
    using UnityEngine;
    using UnityEngine.Assemblies;

    [CreateAssetMenu(fileName = "App Info", menuName = "OGT/Build Steps/App Info")]
    public class AppInfoBuildStep : BuildStep
    {
        private const int LabelWidth = 300;

        [SerializeField] private string productName;
        [SerializeField] private string bundleIdentifier;
        [SerializeField] private string version;
        [SerializeField] private Bootloader bootloader;
        [SerializeField] private bool buildAddressables;

        [Header("Project Auditor (Editor Hang Post Build Fix)")]
        [LabelWidth(LabelWidth)]
        [SerializeField] private bool disableProjectAuditorPostBuild = true;

        [Header("Unity Cloud Build (iOS)")]
        [LabelWidth(LabelWidth)]
        [SerializeField] private bool setBuildNumberToIosBuildNumber = true;

        [Header("Unity Cloud Build (Android)")]
        [LabelWidth(LabelWidth)]
        [SerializeField] private bool setBuildNumberToAndroidBundleVersionCode = true;

        [Header("Unity Cloud Build (Other)")]
        [LabelWidth(LabelWidth)]
        [SerializeField]
        private bool setBuildNumberToBuildVersion = true;

        [LabelWidth(LabelWidth)]
        [SerializeField]
        private bool setBuildNumberToPatchVersion = false;

        [Header("iOS")]
        [LabelWidth(LabelWidth)]
        [SerializeField]
        private bool appUsesNonExemptEncryption = false;

        [Header("WebGL")]
        [LabelWidth(LabelWidth)]
        [SerializeField]
        private bool fixRequestInstancesParameters = true;

        public override void OnBuildProfileSelected()
        {
            this.SetBootloaderGuid();
        }

        public override void OnPrepareForBuild(BuildPlayerContext buildPlayerContext)
        {
            this.DisableProjectAuditorPostBuild();
            this.BuildAddressables();
        }

        public override void OnPreprocessBuild(BuildProfileExtender.Environment environment, BuildTarget target)
        {
            this.SetProductName();
            this.SetBundleIdentifier(target);
            this.SetVersion();
            this.SetBootloaderGuid();
            this.AppendBuildNumberOnUnityCloudBuild();
        }

        public override void OnPostprocessBuild(BuildProfileExtender.Environment environment, BuildTarget target, string path)
        {
            this.FixWebGLRequestInstancesParameters(target, path);
            this.SetAppUsesNonExemptEncryption(target, path);
        }

        private static bool IsUnityCloudBuild()
        {
#if UNITY_CLOUD_BUILD
            return true;
#else
            return false;
#endif
        }

        private static bool TryGetCloudBuildNumber(out int buildNumber)
        {
            string buildNumberRaw = System.Environment.GetEnvironmentVariable("UCB_BUILD_NUMBER");
            return int.TryParse(buildNumberRaw, out buildNumber);
        }

        private void BuildAddressables()
        {
            if (this.buildAddressables == false)
            {
                return;
            }

            Logger.Log("[AppInfoBuildStep] Building Addressables...");
            AddressableAssetSettings.BuildPlayerContent();
        }

        private void SetProductName()
        {
            if (string.IsNullOrWhiteSpace(this.productName))
            {
                return;
            }

            Logger.Log($"[AppInfoBuildStep] Setting Product Name to '{this.productName}'");

            if (PlayerSettings.productName != this.productName)
            {
                PlayerSettings.productName = this.productName;
            }
        }

        private void SetBundleIdentifier(BuildTarget target)
        {
            if (string.IsNullOrWhiteSpace(this.bundleIdentifier))
            {
                return;
            }

            Logger.Log($"[AppInfoBuildStep] Setting Bundle Identifier to '{this.bundleIdentifier}'");

            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(target);
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);

            if (PlayerSettings.GetApplicationIdentifier(namedBuildTarget) != this.bundleIdentifier)
            {
                PlayerSettings.SetApplicationIdentifier(namedBuildTarget, this.bundleIdentifier);
            }
        }

        private void SetVersion()
        {
            if (string.IsNullOrWhiteSpace(this.version))
            {
                return;
            }

            Logger.Log($"[AppInfoBuildStep] Setting Application Version to '{this.version}'");

            if (PlayerSettings.bundleVersion != this.version)
            {
                PlayerSettings.bundleVersion = this.version;
            }
        }

        private void SetBootloaderGuid()
        {
            if (this.bootloader == null)
            {
                return;
            }

            var bootloaderGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(this.bootloader));
            Logger.Log($"[AppInfoBuildStep] Setting Bootloader Guid to '{bootloaderGuid}'");

            if (RuntimeSettings.GetSetting<string>("OGT.Bootloader") != bootloaderGuid)
            {
                RuntimeSettings.SetSetting("OGT.Bootloader", bootloaderGuid);
            }
        }

        private void AppendBuildNumberOnUnityCloudBuild()
        {
            if (IsUnityCloudBuild() == false)
            {
                Logger.Log("[AppInfoBuildStep] Unity Cloud Build not detected.");
                return;
            }

            if (TryGetCloudBuildNumber(out int buildNumber) == false)
            {
                Logger.LogWarning("[AppInfoBuildStep] Unity Cloud Build detected, but BUILD_NUMBER was invalid.");
                return;
            }

            var versionString = OGTVersionString.Parse(PlayerSettings.bundleVersion);

            if (this.setBuildNumberToPatchVersion)
            {
                versionString.UpdatePatch(buildNumber);
            }

            if (this.setBuildNumberToBuildVersion)
            {
                versionString.UpdateBuild(buildNumber);
            }

            if (this.setBuildNumberToBuildVersion || this.setBuildNumberToPatchVersion)
            {
                var originalVersion = PlayerSettings.bundleVersion;
                PlayerSettings.bundleVersion = versionString.GetVersionString();
                Logger.Log($"[AppInfoBuildStep] Updated version from {originalVersion} to {PlayerSettings.bundleVersion}");
            }

            if (this.setBuildNumberToAndroidBundleVersionCode)
            {
                var originalAndroidCode = PlayerSettings.Android.bundleVersionCode;
                PlayerSettings.Android.bundleVersionCode = buildNumber;
                Logger.Log($"[AppInfoBuildStep] Updated Android bundle version code from {originalAndroidCode} to {PlayerSettings.Android.bundleVersionCode}");
            }

            if (this.setBuildNumberToIosBuildNumber)
            {
                var originalIosBuildNumber = PlayerSettings.iOS.buildNumber;
                PlayerSettings.iOS.buildNumber = buildNumber.ToString();
                Logger.Log($"[AppInfoBuildStep] Updated iOS build number from {originalIosBuildNumber} to {PlayerSettings.iOS.buildNumber}");
            }

            Logger.Log($"[AppInfoBuildStep] Applied build #{buildNumber}. Version={PlayerSettings.bundleVersion}, AndroidCode={PlayerSettings.Android.bundleVersionCode}, iOSBuild={PlayerSettings.iOS.buildNumber}");
        }

        private void FixWebGLRequestInstancesParameters(BuildTarget target, string path)
        {
            if (target != BuildTarget.WebGL || this.fixRequestInstancesParameters == false)
            {
                return;
            }

            ////
            //// https://discussions.unity.com/t/cannot-set-properties-of-undefined-setting-1-when-running-a-unitywebrequest/873817
            ////
            var buildFolder = Path.Combine(path, "Build");

            if (Directory.Exists(buildFolder) == false)
            {
                Logger.LogWarning($"[AppInfoBuildStep] WebGL Build folder not found at '{buildFolder}'");
                return;
            }

            var frameworkJsFile = Directory.EnumerateFiles(buildFolder, "*.framework.js", SearchOption.TopDirectoryOnly).FirstOrDefault();

            if (frameworkJsFile != null)
            {
                Logger.Log($"[AppInfoBuildStep] Updating File '{frameworkJsFile}' to fix WebGL Bug");

                var fileContents = File.ReadAllText(frameworkJsFile);
                var oldLine = "var wr = {requestInstances:{},nextRequestId:1,loglevel:2};";
                var newLine = "var wr = {requestInstances:{},nextRequestId:1,loglevel:2, abortControllers:[], requests:[], timer:[], responses:[]};";

                if (fileContents.Contains(oldLine) == false)
                {
                    Logger.Log($"[AppInfoBuildStep] WebGL requestInstances bug not found.");
                    return;
                }

                try
                {
                    File.WriteAllText(frameworkJsFile, fileContents.Replace(oldLine, newLine));
                }
                catch
                {
                    throw new BuildFailedException("Unable to fix WebGL WebRequest bug!");
                }
            }
        }

        private void SetAppUsesNonExemptEncryption(BuildTarget target, string path)
        {
            if (target != BuildTarget.iOS)
            {
                return;
            }

#if UNITY_IOS && UNITY_EDITOR
            var plist = new UnityEditor.iOS.Xcode.PlistDocument();
            var plistPath = path + "/Info.plist";
            plist.ReadFromFile(plistPath);
            plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", this.appUsesNonExemptEncryption);
            plist.WriteToFile(plistPath);

            Logger.Log($"[AppInfoBuildStep] Setting ITSAppUsesNonExemptEncryption = {this.appUsesNonExemptEncryption}");
#else
            Logger.Log($"[AppInfoBuildStep] Skiping ITSAppUsesNonExemptEncryption = {this.appUsesNonExemptEncryption}, not an iOS build");
#endif
        }

        private void DisableProjectAuditorPostBuild()
        {
            if (this.disableProjectAuditorPostBuild == false)
            {
                return;
            }

            var userPreferencesType = Type.GetType("Unity.ProjectAuditor.Editor.UserPreferences, Unity.ProjectAuditor.Editor")
                ?? CurrentAssemblies.GetLoadedAssemblies()
                    .Select(a => a.GetType("Unity.ProjectAuditor.Editor.UserPreferences"))
                    .FirstOrDefault(t => t != null);

            if (userPreferencesType == null)
            {
                Logger.LogWarning("[AppInfoBuildStep] Could not find Unity.ProjectAuditor.Editor.UserPreferences via reflection.");
                return;
            }

            var analyzeAfterBuildField = userPreferencesType.GetField("AnalyzeAfterBuild", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            if (analyzeAfterBuildField == null)
            {
                Logger.LogWarning("[AppInfoBuildStep] Could not find AnalyzeAfterBuild field via reflection.");
                return;
            }

            var analyzeAfterBuildSetting = analyzeAfterBuildField.GetValue(null);

            if (analyzeAfterBuildSetting == null)
            {
                Logger.LogWarning("[AppInfoBuildStep] AnalyzeAfterBuild field value is null.");
                return;
            }

            var setMethod = analyzeAfterBuildSetting.GetType().GetMethod("Set", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(bool) }, null);

            if (setMethod == null)
            {
                Logger.LogWarning("[AppInfoBuildStep] Could not find AnalyzeAfterBuild.Set(bool) via reflection.");
                return;
            }

            setMethod.Invoke(analyzeAfterBuildSetting, new object[] { false });
            Logger.Log("[AppInfoBuildStep] Set AnalyzeAfterBuild to false via reflection.");
        }

        private class OGTVersionString
        {
            public int? Major { get; private set; }
            public int? Minor { get; private set; }
            public int? Patch { get; private set; }
            public int? Build { get; private set; }

            public static OGTVersionString Parse(string version)
            {
                var parts = string.IsNullOrWhiteSpace(version) ? Array.Empty<string>() : version.Split('.');

                return new OGTVersionString
                {
                    Major = ParsePart(parts, 0),
                    Minor = ParsePart(parts, 1),
                    Patch = ParsePart(parts, 2),
                    Build = ParsePart(parts, 3),
                };
            }

            public string GetVersionString()
            {
                return ToVersionString(this.Major, this.Minor, this.Patch, this.Build);

                string ToVersionString(params int?[] parts)
                {
                    int lastNonNullIndex = Array.FindLastIndex(parts, p => p.HasValue);

                    if (lastNonNullIndex < 0)
                    {
                        return string.Empty;
                    }

                    return string.Join('.', parts
                        .Take(lastNonNullIndex + 1)
                        .Select(x => x?.ToString() ?? string.Empty));
                }
            }

            public void UpdateMajor(int? value) => this.Major = SanitizePart(value);

            public void UpdateMinor(int? value) => this.Minor = SanitizePart(value);

            public void UpdatePatch(int? value) => this.Patch = SanitizePart(value);

            public void UpdateBuild(int? value) => this.Build = SanitizePart(value);

            private static int? ParsePart(string[] parts, int index)
            {
                if (parts.Length <= index)
                {
                    return null;
                }

                if (string.IsNullOrWhiteSpace(parts[index]))
                {
                    return null;
                }

                return int.TryParse(parts[index], out int parsed) ? Math.Max(0, parsed) : null;
            }

            private static int? SanitizePart(int? value)
            {
                if (value.HasValue == false)
                {
                    return null;
                }

                return Math.Max(0, value.Value);
            }
        }
    }
}
