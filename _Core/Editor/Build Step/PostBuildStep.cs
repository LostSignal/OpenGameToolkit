//-----------------------------------------------------------------------
// <copyright file="PostBuildStep.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Build.Profile;
    using UnityEditor.Build.Reporting;

    public abstract class PostBuildStep : BuildStep
    {
        public const string PostBuildStepMenuPath = BuildStepMenuPath + "Add Post Build Step/";

        [EditorEvents.OnPostprocessBuild]
        public static void OnPostprocessBuild(BuildReport report)
        {
            var buildProfile = BuildProfile.GetActiveBuildProfile();

            if (buildProfile == null)
            {
                return;
            }

            var path = AssetDatabase.GetAssetPath(buildProfile);

            foreach (var postBuildStep in AssetDatabase.LoadAllAssetsAtPath(path).OfType<PostBuildStep>().OrderBy(x => x.Order))
            {
                Logger.Log($"Running Post-Build Step {postBuildStep.Name}...");

                var startTime = System.DateTime.UtcNow;

                postBuildStep.Run(buildProfile, report);

                var totalTime = System.DateTime.UtcNow.Subtract(startTime).TotalSeconds;

                Logger.Log($"Post-Build Step {postBuildStep.Name} took {totalTime} seconds");
            }
        }

        public abstract void Run(BuildProfile buildProfile, BuildReport report);
    }
}
