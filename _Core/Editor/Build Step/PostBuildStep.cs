//-----------------------------------------------------------------------
// <copyright file="PostBuildStep.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Build.Reporting;

    public abstract class PostBuildStep : BuildStep
    {
        public const string PostBuildStepMenuPath = BuildStepMenuPath + "Add Post Build Step/";

        [EditorEvents.OnPostprocessBuild]
        public static void OnPostprocessBuild(BuildReport report)
        {
            Logger.Log($"PostBuildStep OnPostprocessBuild Started...");
            var buildProfile = UnityEditor.Build.Profile.BuildProfile.GetActiveBuildProfile();

            if (buildProfile == null)
            {
                Logger.Log($"PostBuildStep OnPostprocessBuild Early Exit: No Active Build Profile Found.");
                return;
            }

            var path = AssetDatabase.GetAssetPath(buildProfile);
            var postBuildSteps = AssetDatabase.LoadAllAssetsAtPath(path).OfType<PostBuildStep>().OrderBy(x => x.Order).ToList();

            if (postBuildSteps.Count == 0)
            {
                Logger.Log($"PostBuildStep OnPostprocessBuild Early Exit: No Post-Build Steps Found.");
                return;
            }

            foreach (var postBuildStep in postBuildSteps)
            {
                Logger.Log($"PostBuildStep Running Step {postBuildStep.Name}...");

                var startTime = System.DateTime.UtcNow;

                postBuildStep.Run(buildProfile, report);

                var totalTime = System.DateTime.UtcNow.Subtract(startTime).TotalSeconds;

                Logger.Log($"PostBuildStep Step {postBuildStep.Name} took {totalTime} seconds");
            }
        }

        public abstract void Run(UnityEditor.Build.Profile.BuildProfile buildProfile, BuildReport report);
    }
}
