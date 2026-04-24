//-----------------------------------------------------------------------
// <copyright file="OptimizationCleanUp.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Linq;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public static class OptimizationCleanUp
    {
        private static readonly OGTLogger Logger = OGTLogger.OGTEditor;

        [EditorEvents.OnProcessScene]
        public static void CleanUp(Scene scene)
        {
            bool shouldCleanup = Application.isEditor && Application.isPlaying == false && UnityEditor.BuildPipeline.isBuildingPlayer;

            if (shouldCleanup == false)
            {
                return;
            }

            Logger.Log($"OptimizationCleanUp.CleanUp({scene.name}) Started...");

            foreach (var objectOptimizer in GameObject.FindObjectsByType<ObjectOptimizer>(FindObjectsSortMode.None).Where(x => x.gameObject.scene == scene))
            {
                Logger.Log($"OptimizationCleanUp Cleaning Up ObjectOptimizer {objectOptimizer.name}...");
                objectOptimizer.CleanUp();
            }

            foreach (var sceneOptimizer in GameObject.FindObjectsByType<SceneOptimizer>(FindObjectsSortMode.None).Where(x => x.gameObject.scene == scene))
            {
                Logger.Log($"OptimizationCleanUp Cleaning Up SceneOptimizer {sceneOptimizer.name}...");
                sceneOptimizer.CleanUp();
            }

            foreach (var volumeOptimizer in GameObject.FindObjectsByType<VolumeOptimizer>(FindObjectsSortMode.None).Where(x => x.gameObject.scene == scene))
            {
                Logger.Log($"OptimizationCleanUp Cleaning Up VolumeOptimizer {volumeOptimizer.name}...");
                volumeOptimizer.CleanUp();
            }
        }
    }
}
