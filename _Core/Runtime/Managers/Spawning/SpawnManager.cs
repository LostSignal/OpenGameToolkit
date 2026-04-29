namespace OGT
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;

    ////
    //// NEEDS TO BE ASYNC AND USE ADDRESSABLES FOR SPAWNING!!!
    ////
    //// NEEDS TO REGISTER WITH LEVEL MANAGER TO POSTPONE LEVEL LOADING FINISHING IF ALL POOLS ARE NOT FINISHED LOADING
    ////
    public class SpawnManager : Manager, IValidate
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        [SerializeField]
        private List<SpawnPool> pools;

        private Dictionary<string, SpawnPool> poolDictionaryCache;

        public void CreatePool(Spawnable spawnable, int initialCount)
        {
            // TODO [bgish]: Eventually use SpawnPool.CreateRuntimePool(spawnable.Guid);
            throw new NotImplementedException();
        }

        public void DestroyPool(Spawnable spawnable)
        {
            throw new NotImplementedException();
        }

        public GameObject Spawn(GameObject gameObject) => Spawn(gameObject, Vector3.zero, Quaternion.identity, null);

        public GameObject Spawn(GameObject gameObject, Vector3 position) => Spawn(gameObject, position, Quaternion.identity, null);

        public GameObject Spawn(GameObject gameObject, Vector3 position, Quaternion rotation) => Spawn(gameObject, position, rotation, null);

        public GameObject Spawn(GameObject gameObject, Transform parent) => Spawn(gameObject, Vector3.zero, Quaternion.identity, parent);

        public GameObject Spawn(GameObject gameObject, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (gameObject == null)
            {
                Logger.LogError("Tried to spawn a null GameObject.");
                return null;
            }
            else if (gameObject.TryGetComponent(out Spawnable spawnablePrefab))
            {
                return Spawn<Spawnable>(spawnablePrefab, position, rotation, parent).gameObject;
            }
            else
            {
                Logger.LogError($"Spawning GameObject {gameObject.name} without a Spawnable component. No pooling will occur");
                return GameObject.Instantiate(gameObject, position, rotation, parent);
            }
        }

        public T Spawn<T>(GameObject gameObject)
            where T : Component => Spawn<T>(gameObject, Vector3.zero, Quaternion.identity, null);

        public T Spawn<T>(GameObject gameObject, Vector3 position)
            where T : Component => Spawn<T>(gameObject, position, Quaternion.identity, null);

        public T Spawn<T>(GameObject gameObject, Vector3 position, Quaternion rotation)
            where T : Component => Spawn<T>(gameObject, position, rotation, null);

        public T Spawn<T>(GameObject gameObject, Transform parent)
            where T : Component => Spawn<T>(gameObject, Vector3.zero, Quaternion.identity, parent);

        public T Spawn<T>(GameObject gameObject, Vector3 position, Quaternion rotationm, Transform parent)
            where T : Component
        {
            var result = Spawn(gameObject, position, rotationm, parent);

            if (result == null)
            {
                Logger.LogError("Unable to spawn null gameObject!");
                return null;
            }

            if (result.TryGetComponent<T>(out T component))
            {
                return component;
            }
            else
            {
                Logger.LogError($"GameObject {gameObject.name} does not contain component {typeof(T).Name}");
                return null;
            }
        }

        public T Spawn<T>(Spawnable spawnable)
            where T : Component => Spawn<T>(spawnable, Vector3.zero, Quaternion.identity, null);

        public T Spawn<T>(Spawnable spawnable, Vector3 position)
            where T : Component => Spawn<T>(spawnable, position, Quaternion.identity, null);

        public T Spawn<T>(Spawnable spawnable, Vector3 position, Quaternion rotation)
            where T : Component => Spawn<T>(spawnable, position, rotation, null);

        public T Spawn<T>(Spawnable spawnable, Transform parent)
            where T : Component => Spawn<T>(spawnable, Vector3.zero, Quaternion.identity, parent);

        public T Spawn<T>(Spawnable spawnablePrefab, Vector3 position, Quaternion rotation, Transform parent)
            where T : Component
        {
            var pool = this.GetOrCreatePool(spawnablePrefab);
            var spawnableInstance = pool.TakeFromPool(spawnablePrefab, position, rotation, parent);

            if (spawnableInstance.TryGetComponent(out T component) == false)
            {
                Logger.LogError($"Spawnable Object {spawnablePrefab.name} does not have component a component of {typeof(T).Name}");
                return null;
            }

            return component;
        }

        public void Despawn(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return;
            }

            if (gameObject.TryGetComponent<Spawnable>(out Spawnable component))
            {
                if (component.IsSpawned)
                {
                    Despawn(component);
                }
            }
            else
            {
                // LOG ERROR: Tried to destroy spawn that wasn't created through the SpawnManger
                GameObject.Destroy(gameObject);
            }
        }

        public void Despawn(Spawnable spawnable)
        {
            var pool = this.GetPool(spawnable);

            if (pool == null)
            {
                // LOG ERROR: Tried to destroy spawn that wasn't created through the SpawnManger
                GameObject.Destroy(spawnable.gameObject);
            }
            else
            {
                pool.AddToPool(spawnable);
            }
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
#if UNITY_EDITOR
            //// TODO [bgish]: Check every pool for initialPoolSize > 0

            if (this.pools == null)
            {
                this.pools = new List<SpawnPool>();
                EditorUtil.SetDirty(this);
            }

            for (int i = 0; i < this.pools.Count; i++)
            {
                var pool = this.pools[i];
                var gameObject = pool.Spawnable.editorAsset as GameObject;

                if (gameObject == null)
                {
                    report.ReportError(this, "Assert Not Null", $"Pool has null Spawnable reference at index {i}");
                    continue;
                }

                var spawnable = gameObject.GetComponent<Spawnable>();

                if (spawnable == null)
                {
                    report.ReportError(this, "Assert Has Spawnable", $"Pool has Spawnable reference without a Spawnable Component at index {i}");
                    continue;
                }
            }
#endif
        }

        private void OnDestroy()
        {
            // TODO [bgish]: Iterate over every Pool in the this.GetPoolDictionaryCache().Values
            //     If (pool.WasCreatedAtRuntime) Print Warning: Add Spawnable to SpawnManager
            //     If (pool.InitialCount < pool.MaxCreatedCount) Print Warning: Increase Pool Size
        }

        private SpawnPool GetPool(Spawnable spawnable)
        {
            return this.GetPoolDictionaryCache().TryGetValue(spawnable.Guid, out SpawnPool pool) ? pool : null;
        }

        private SpawnPool GetOrCreatePool(Spawnable spawnable)
        {
            var poolCache = this.GetPoolDictionaryCache();

            if (poolCache.TryGetValue(spawnable.Guid, out SpawnPool pool) == false)
            {
                pool = SpawnPool.CreateRuntimePool(spawnable.Guid);
                poolCache.Add(spawnable.Guid, pool);
            }

            return pool;
        }

        private Dictionary<string, SpawnPool> GetPoolDictionaryCache()
        {
            if (this.poolDictionaryCache == null)
            {
                this.poolDictionaryCache = new Dictionary<string, SpawnPool>();

                foreach (var pool in this.pools)
                {
                    this.poolDictionaryCache.Add(pool.Guid, pool);
                }
            }

            return this.poolDictionaryCache;
        }

        protected override Task InitializeManager(Bootloader bootloader)
        {
            if (this.pools.IsNullOrEmpty() == false)
            {
                foreach (var pool in this.pools)
                {
                    pool.Initialize();
                }
            }

            return Task.CompletedTask;
        }
    }
}
