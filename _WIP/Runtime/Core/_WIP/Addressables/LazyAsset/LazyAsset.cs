//-----------------------------------------------------------------------
// <copyright file="LazyAsset.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using OGT;
    using UnityEngine;

    [Serializable]
    public class LazyAsset
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

#pragma warning disable 0649, CA2235
        [SerializeField] private string assetGuid;
#pragma warning restore 0649, CA2235

        public LazyAsset()
        {
        }

        public LazyAsset(string guid)
        {
            this.assetGuid = guid;
        }

        public object RuntimeKey => this.assetGuid;

        public string AssetGuid
        {
            get
            {
                return this.assetGuid;
            }

            set
            {
#if UNITY
                if (this.assetGuid != null && this.assetGuid != value && Application.isPlaying)
                {
                    Logger.LogError("Changing a LazyAsset's Guid after it has been set will cause issues!");
                }
#endif

                this.assetGuid = value;
            }
        }

        public virtual System.Type Type
        {
            #if UNITY
            get => typeof(UnityEngine.Object);
            #else
            get => null;
            #endif
        }

        #if UNITY
        public UnityTask<GameObject> InstantiateGameObject(Transform parent = null, bool resetTransform = true)
        {
            return UnityTask<GameObject>.Run(Coroutine());

            System.Collections.Generic.IEnumerator<GameObject> Coroutine()
            {
                var instantiateOperation = UnityEngine.AddressableAssets.Addressables.InstantiateAsync(this.RuntimeKey, parent);

                while (instantiateOperation.IsDone == false && instantiateOperation.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Failed)
                {
                    yield return default;
                }

                if (instantiateOperation.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Failed)
                {
                    Logger.LogErrorFormat("Unable to successfully instantiate asset {0} of type {1}", (object)this.AssetGuid, (object)nameof(GameObject));
                    yield return default;
                    yield break;
                }

                var gameObject = instantiateOperation.Result;

                if (gameObject != null)
                {
                    if (parent != null)
                    {
                        gameObject.transform.SetParent(parent);
                    }

                    if (resetTransform)
                    {
                        gameObject.transform.Reset();
                    }
                }

                yield return gameObject;
            }
        }

        public UnityTask<T> InstantiateGameObject<T>(Transform parent = null, bool resetTransform = true)
            where T : Component
        {
            #if UNITY_EDITOR
            if (typeof(T).IsSubclassOf(typeof(Component)) == false && typeof(T) != typeof(GameObject))
            {
                Logger.LogWarningFormat("You are Instantiating LastAsset<{0}> as if it were a GameObject, you should instead use Load instead of Instantiate.", typeof(T).Name);
            }
            #endif

            return UnityTask<T>.Run(Coroutine());

            System.Collections.Generic.IEnumerator<T> Coroutine()
            {
                var instantiateOperation = UnityEngine.AddressableAssets.Addressables.InstantiateAsync(this.RuntimeKey, parent);

                while (instantiateOperation.IsDone == false && instantiateOperation.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Failed)
                {
                    yield return default;
                }

                if (instantiateOperation.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Failed)
                {
                    Logger.LogErrorFormat("Unable to successfully instantiate asset {0} of type {1}", (object)this.AssetGuid, (object)typeof(T).Name);
                    yield return default;
                    yield break;
                }

                var gameObject = instantiateOperation.Result;

                if (parent != null && gameObject == null)
                {
                    Logger.LogError($"Can not set parent on Non-GameObject:  Runtime Key = {this.RuntimeKey}.");
                }

                if (gameObject != null)
                {
                    if (parent != null)
                    {
                        gameObject.transform.SetParent(parent);
                    }

                    if (resetTransform)
                    {
                        gameObject.transform.Reset();
                    }
                }

                if (typeof(T) == typeof(GameObject))
                {
                    yield return gameObject as T;
                }
                else if (typeof(T).IsSubclassOf(typeof(Component)))
                {
                    if (gameObject == null)
                    {
                        Logger.LogErrorFormat("LazyAsset {0} is not of type GameObject, so can't get Component {1} from it.", (object)this.AssetGuid, (object)typeof(T).Name);
                        yield break;
                    }

                    if (gameObject.TryGetComponent(out T component))
                    {
                        yield return component;
                    }
                    else
                    {
                        Logger.LogErrorFormat("LazyAsset {0} does not have Component {1} on it.", (object)this.AssetGuid, (object)typeof(T).Name);
                        yield break;
                    }
                }
                else
                {
                    Logger.LogError("LazyAssetT hit unknown if/else situtation.");
                }
            }
        }

        #endif
    }
}
