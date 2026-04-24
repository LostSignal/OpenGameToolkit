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
            var buildProfile = BuildProfile.GetActiveBuildProfile();

            if (buildProfile == null)
            {
                return;
            }

            var path = AssetDatabase.GetAssetPath(buildProfile);

            foreach (var preBuildStep in AssetDatabase.LoadAllAssetsAtPath(path).OfType<PreBuildStep>().OrderBy(x => x.Order))
            {
                Logger.Log($"Running Pre-Build Step {preBuildStep.Name}...");

                var startTime = System.DateTime.UtcNow;

                preBuildStep.Run(buildProfile);

                var totalTime = System.DateTime.UtcNow.Subtract(startTime).TotalSeconds;

                Logger.Log($"Pre-Build Step {preBuildStep.Name} took {totalTime} seconds");
            }
        }

        public abstract void Run(BuildProfile buildProfile);
    }
}
