//-----------------------------------------------------------------------
// <copyright file="PreBuildStep.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Build.Profile;
    using UnityEditor.Build.Reporting;

    public abstract class PreBuildStep : BuildStep
    {
        public const string PreBuildStepMenuPath = BuildStepMenuPath + "Add Pre Build Step/";

        [EditorEvents.OnPreprocessBuild]
        public static void OnPreprocessBuild(BuildReport report)
        {
            Logger.Log($"PreBuildStep OnPreprocessBuild Started...");
            var buildProfile = BuildProfile.GetActiveBuildProfile();

            if (buildProfile == null)
            {
                Logger.Log($"PreBuildStep OnPreprocessBuild Early Exit: No Active Build Profile Found.");
                return;
            }

            var path = AssetDatabase.GetAssetPath(buildProfile);
            var preBuildSteps = AssetDatabase.LoadAllAssetsAtPath(path).OfType<PreBuildStep>().OrderBy(x => x.Order).ToList();

            if (preBuildSteps.Count == 0)
            {
                Logger.Log($"PreBuildStep OnPreprocessBuild Early Exit: No Pre-Build Steps Found.");
                return;
            }

            foreach (var preBuildStep in preBuildSteps)
            {
                Logger.Log($"PreBuildStep Running Step {preBuildStep.Name}...");

                var startTime = System.DateTime.UtcNow;

                preBuildStep.Run(buildProfile);

                var totalTime = System.DateTime.UtcNow.Subtract(startTime).TotalSeconds;

                Logger.Log($"PreBuildStep Step {preBuildStep.Name} took {totalTime} seconds");
            }
        }

        public abstract void Run(BuildProfile buildProfile);
    }
}
