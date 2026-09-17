using OGT;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;

public class AppendBuildNumber : PreBuildStep
{
    [SerializeField] private bool appendToPatchVersion = false;
    [SerializeField] private bool appendToBuildVersion = true;
    [SerializeField] private bool appendToAndroidBundleVersionCode = true;
    [SerializeField] private bool appendToIosBuildNumber = true;

    public override string Name => "Append Build Number (UCB)";

    public override void Run(BuildProfile buildProfile)
    {
        if (IsUnityCloudBuild() == false)
        {
            return;
        }

        if (TryGetCloudBuildNumber(out int buildNumber) == false)
        {
            Debug.LogWarning("[AppendBuildNumber] Unity Cloud Build detected, but BUILD_NUMBER was invalid.");
            return;
        }

        var versionString = OGTVersionString.Parse(PlayerSettings.bundleVersion);

        if (this.appendToPatchVersion)
        {
            versionString.UpdatePatch(buildNumber);
        }

        if (this.appendToBuildVersion)
        {
            versionString.UpdateBuild(buildNumber);
        }

        if (this.appendToBuildVersion || this.appendToPatchVersion)
        {
            PlayerSettings.bundleVersion = versionString.GetVersionString();
        }

        if (this.appendToAndroidBundleVersionCode)
        {
            PlayerSettings.Android.bundleVersionCode = buildNumber;
        }

        if (this.appendToIosBuildNumber)
        {
            PlayerSettings.iOS.buildNumber = buildNumber.ToString();
        }

        Debug.Log($"[AppendBuildNumber] Applied build #{buildNumber}. Version={PlayerSettings.bundleVersion}, AndroidCode={PlayerSettings.Android.bundleVersionCode}, iOSBuild={PlayerSettings.iOS.buildNumber}");
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
        string buildNumberRaw = Environment.GetEnvironmentVariable("UCB_BUILD_NUMBER");
        return int.TryParse(buildNumberRaw, out buildNumber);
    }

    public class OGTVersionString
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
