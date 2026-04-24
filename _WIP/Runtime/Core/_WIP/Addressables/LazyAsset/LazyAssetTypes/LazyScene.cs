//-----------------------------------------------------------------------
// <copyright file="LazyScene.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using OGT;
    using UnityEngine;

    #if UNITY
    using UnityEngine.ResourceManagement.AsyncOperations;
    using UnityEngine.SceneManagement;
    #endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2235:Mark all non-serializable fields", Justification = "Using Unity Serialization")]
    [Serializable]
    public class LazyScene : LazyAsset, ILazyScene
    {
        #if UNITY
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        private AsyncOperationHandle operation;

        public AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance> LoadScene(LoadSceneMode loadSceneMode)
        {
            var sceneOperation = UnityEngine.AddressableAssets.Addressables.LoadSceneAsync(this.RuntimeKey, loadSceneMode);
            this.operation = sceneOperation;
            return sceneOperation;
        }

        public void Release()
        {
            if (this.operation.IsValid() == false)
            {
                Logger.LogWarning("Cannot release a null or unloaded asset.");
                return;
            }

            UnityEngine.AddressableAssets.Addressables.Release(this.operation);
            this.operation = default;
        }

        #endif
    }
}
