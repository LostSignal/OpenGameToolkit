//-----------------------------------------------------------------------
// <copyright file="Player.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Settings
{
    using UnityEditor;

    public class Player : Settings
    {
        public enum Backend
        {
            Mono,
            IL2CPP,
        }

        public string BundleId { get; set; }
        public Backend? ScriptingBackend { get; set; }
        public string Version { get; set; }
        public bool? AppendBuildRevision { get; set; }

        public override void ApplySettings()
        {
            this.SetBundleId();
            this.SetScriptingBackend();
            this.SetVersion();
        }

        private void SetBundleId()
        {
            if (string.IsNullOrWhiteSpace(this.BundleId))
            {
                return;
            }

            foreach (var targetGroup in BuildTargetGroupUtil.GetValid())
            {
                var currentIdntifier = PlayerSettings.GetApplicationIdentifier(targetGroup);

                if (currentIdntifier != this.BundleId)
                {
                    PlayerSettings.SetApplicationIdentifier(targetGroup, this.BundleId);
                }
            }
        }

        private void SetScriptingBackend()
        {
            if (this.ScriptingBackend == null)
            {
                return;
            }

            if (this.ScriptingBackend == Backend.Mono)
            {
                foreach (var namedBuildTarget in BuildTargetGroupUtil.GetValid())
                {
                    PlayerSettings.SetScriptingBackend(namedBuildTarget, ScriptingImplementation.Mono2x);
                }
            }
            else if (this.ScriptingBackend == Backend.IL2CPP)
            {
                foreach (var namedBuildTarget in BuildTargetGroupUtil.GetValid())
                {
                    PlayerSettings.SetScriptingBackend(namedBuildTarget, ScriptingImplementation.IL2CPP);
                }
            }
            else
            {
                Settings.Logger.LogError($"Unknown PlayerSettings.ScriptingBackend Found {this.ScriptingBackend}");
            }
        }

        private void SetVersion()
        {
            if (string.IsNullOrWhiteSpace(this.Version))
            {
                return;
            }

            if (this.AppendBuildRevision == true)
            {
                PlayerSettings.bundleVersion = this.Version;
            }
            else
            {
                PlayerSettings.bundleVersion = this.Version;
            }
        }
    }
}

/*
 [BuildConfigSettingsOrder(275)]
    public class CloudBuildSetBuildNumber : BuildConfigSettings
    {
        private static readonly OGTLogger Logger = OGTLogger.LostEditor;

#pragma warning disable 0649
        [Tooltip("SCM Commit Numer only works for Perfoce and PlasticSCM, and Cloud Build Number works for all source control types.")]
        [SerializeField] private BuildNumberType buildNumberType;
        [SerializeField] private int incrementBuildNumberBy;
#pragma warning restore 0649

        public enum BuildNumberType
        {
            ScmCommitNumber,
            CloudBuildNumber,
        }

        public override string DisplayName => "CloudBuild - Set Build Number";

        public override bool IsInline => false;

        [EditorEvents.OnPreprocessBuild]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called via reflection.")]
        private static void OnPreproccessBuild()
        {
            var settings = ProjectSettingsBuildConfigs.GetActiveSettings<CloudBuildSetBuildNumber>();

            if (settings == null)
            {
                return;
            }

            int buildNumber = GetBuildNumber(settings);

            if (buildNumber != -1)
            {
                PlayerSettings.iOS.buildNumber = buildNumber.ToString();
                PlayerSettings.Android.bundleVersionCode = buildNumber;
            }
        }

        private static int GetBuildNumber(CloudBuildSetBuildNumber settings)
        {
            if (UnityPlatform.IsUnityCloudBuild == false)
            {
                // NOTE [bgish]: Gradle Build will fail if build number is 0, so returning 1
                // android.defaultConfig.versionCode is set to 0, but it should be a positive integer.
                return 1;
            }

            var cloudBuildManifest = CloudBuildManifest.Find();

            if (cloudBuildManifest == null)
            {
                Logger.LogError("CloudBuildSetBuildNumber couldn't find CloudBuildManifest!");
            }
            else if (settings.buildNumberType == BuildNumberType.CloudBuildNumber)
            {
                Logger.LogFormat("CloudBuildSetBuildNumber setting application build number to unity cloud CloudBuildNumber {0}!", cloudBuildManifest.BuildNumber);
                return cloudBuildManifest.BuildNumber + settings.incrementBuildNumberBy;
            }
            else if (settings.buildNumberType == BuildNumberType.ScmCommitNumber)
            {
                string commitId = cloudBuildManifest.ScmCommitId;

                if (int.TryParse(commitId, out int commitNumber))
                {
                    Logger.LogFormat("CloudBuildSetBuildNumber setting application build number to ScmCommitId {0}!", commitId);
                    return commitNumber + settings.incrementBuildNumberBy;
                }
                else
                {
                    Logger.LogErrorFormat("CloudBuildSetBuildNumber couldn't parse ScmCommitId {0}.  It is not a valid integer!", commitId);
                }
            }
            else
            {
                Logger.LogErrorFormat("Found unknown BuildNumberType {0}", settings.buildNumberType);
            }

            return -1;
        }
    }
*/
