//-----------------------------------------------------------------------
// <copyright file="BuildStepExecutor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BuildProfile
{
    using System;
    using System.Linq;
    using Unity.Scripting.LifecycleManagement;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Profile;

    [AutoStaticsCleanup]
    [InitializeOnLoad]
    public partial class BuildStepExecutor : BuildPlayerProcessor
    {
        public static readonly OGTLogger Logger = new OGTLogger("Build Step");

        private static BuildProfile activeBuildProfile;

        static BuildStepExecutor()
        {
            EditorApplication.update -= CheckSelectedBuildProfile;
            EditorApplication.update += CheckSelectedBuildProfile;
        }

        public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
        {
            Logger.Log($"BuildStepExecutor PrepareForBuild Started...");
            var buildProfileExtender = GetBuildProfileExtenderGetActiveBuildProfileBuildSteps();
            var buildStepCount = buildProfileExtender?.BuildSteps?.Count ?? 0;

            for (int i = 0; i < buildStepCount; i++)
            {
                buildProfileExtender = GetBuildProfileExtenderGetActiveBuildProfileBuildSteps();
                var buildStep = buildProfileExtender.BuildSteps[i];
                var buildStepName = buildStep.name;
                Logger.Log($"BuildStepExecutor Running Step {buildStepName}...");
                var startTime = System.DateTime.UtcNow;
                buildStep.OnPrepareForBuild(buildPlayerContext);
                var totalTime = System.DateTime.UtcNow.Subtract(startTime).TotalSeconds;
                Logger.Log($"BuildStepExecutor Step {buildStepName} took {totalTime} seconds");
            }
        }

        [EditorEvents.OnPreprocessBuild]
        public static void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            Logger.Log($"BuildStepExecutor OnPreprocessBuild Started...");
            var buildProfileExtender = GetBuildProfileExtenderGetActiveBuildProfileBuildSteps();

            if (buildProfileExtender == null)
            {
                return;
            }
        
            foreach (var buildStep in buildProfileExtender.BuildSteps)
            {
                Logger.Log($"BuildStepExecutor Running Step {buildStep.name}...");
                var startTime = System.DateTime.UtcNow;
                buildStep.OnPreprocessBuild(buildProfileExtender.CurrentEnvironment, report.summary.platform);
                var totalTime = System.DateTime.UtcNow.Subtract(startTime).TotalSeconds;
                Logger.Log($"BuildStepExecutor Step {buildStep.name} took {totalTime} seconds");
            }
        }

        [EditorEvents.OnPostprocessBuild]
        public static void OnPostprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            Logger.Log($"BuildStepExecutor OnPostprocessBuild Started...");
            var buildProfileExtender = GetBuildProfileExtenderGetActiveBuildProfileBuildSteps();

            if (buildProfileExtender == null)
            {
                return;
            }

            foreach (var buildStep in buildProfileExtender.BuildSteps)
            {
                Logger.Log($"BuildStepExecutor Running Step {buildStep.name}...");
                var startTime = System.DateTime.UtcNow;
                buildStep.OnPostprocessBuild(buildProfileExtender.CurrentEnvironment, report.summary.platform, report.summary.outputPath);
                var totalTime = System.DateTime.UtcNow.Subtract(startTime).TotalSeconds;
                Logger.Log($"BuildStepExecutor Step {buildStep.name} took {totalTime} seconds");
            }
        }

        private static BuildProfileExtender GetBuildProfileExtenderGetActiveBuildProfileBuildSteps()
        {
            var buildProfile = UnityEditor.Build.Profile.BuildProfile.GetActiveBuildProfile();

            if (buildProfile == null)
            {
                Logger.Log($"BuildStepExecutor Early Exit: No Active Build Profile Found.");
                return null;
            }

            var buildProfileExtender = GetBuildProfileExtender(buildProfile);

            if (buildProfileExtender == null)
            {
                Logger.Log($"BuildStepExecutor Early Exit: No BuildProfileExtender Found.");
                return null;
            }

            return buildProfileExtender;
        }

        private static BuildProfileExtender GetBuildProfileExtender(UnityEditor.Build.Profile.BuildProfile buildProfile)
        {
            var path = AssetDatabase.GetAssetPath(buildProfile);
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<BuildProfileExtender>().FirstOrDefault();
        }

        private static void CheckSelectedBuildProfile()
        {
            if (activeBuildProfile == UnityEditor.Build.Profile.BuildProfile.GetActiveBuildProfile())
            {
                return;
            }

            activeBuildProfile = UnityEditor.Build.Profile.BuildProfile.GetActiveBuildProfile();

            var buildProfileExtender = GetBuildProfileExtender(activeBuildProfile);

            if (buildProfileExtender?.BuildSteps == null)
            {
                return;
            }

            foreach (var buildStep in buildProfileExtender.BuildSteps)
            {
                if (buildStep == null)
                {
                    continue;
                }

                try
                {
                    buildStep.OnBuildProfileSelected();
                }
                catch (Exception ex)
                {
                    Logger.LogError($"BuildStepExecutor OnBuildProfileSelected failed for '{buildStep.name}': {ex}");
                }
            }
        }
    }
}
