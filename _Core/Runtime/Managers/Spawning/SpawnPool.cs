
namespace OGT
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    [Serializable]
    public class SpawnPool
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        [SerializeField] private AssetReference spawnable;
        [SerializeField] private int initialPoolSize = 1;

        private List<Spawnable> spawnableObjectsPool;
        private bool wasCreatedAtRuntime;
        private int currentSpawnCount;
        private int maxSpawnedCount;

        public AssetReference Spawnable => this.spawnable;

        public string Guid => this.spawnable.AssetGUID;

        public bool WasCreatedAtRuntime => this.wasCreatedAtRuntime;

        public bool IsEmpty => this.spawnableObjectsPool.Count == 0;

        private SpawnPool()
        {
        }

        public static SpawnPool CreateRuntimePool(string guid, int initialPoolSize = 0)
        {
            var newPool = new SpawnPool();
            newPool.spawnable = new AssetReferenceT<Spawnable>(guid);
            newPool.wasCreatedAtRuntime = true;
            newPool.initialPoolSize = initialPoolSize;
            newPool.Initialize();

            return newPool;
        }

        public void Initialize()
        {
            this.spawnableObjectsPool = this.initialPoolSize > 0 ?
                new List<Spawnable>(this.initialPoolSize) :
                new List<Spawnable>();

            // TODO [bgish]: Need to handle the async nature better!
            for (int i = 0; i < this.initialPoolSize; i++)
            {
                this.spawnable.InstantiateAsync().Completed += (result) =>
                {
                    var gameObject = result.Result;
                    var newSpawnable = gameObject.GetComponent<Spawnable>();
                    this.AddToPool(newSpawnable);
                };
            }
        }

        public Spawnable TakeFromPool(Spawnable spawnablePrefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            // Make sure there is something in the pool
            if (this.spawnableObjectsPool.Count == 0)
            {
                Logger.LogError($"Spawnable '{spawnablePrefab.name}' pool was empty, consider increasing the initial pool size.");

                // Create instance and set pos/rot/parent
                var spawnable = parent != null ?
                    GameObject.Instantiate(spawnablePrefab, position, rotation, parent) :
                    GameObject.Instantiate(spawnablePrefab, position, rotation);

                this.AddToPool(spawnable);
            }

            int lastIndex = this.spawnableObjectsPool.Count - 1;
            Spawnable spawnableInstance = this.spawnableObjectsPool[lastIndex];
            this.spawnableObjectsPool.RemoveAt(lastIndex);

            // Get instance and set pos/rot/parent
            spawnableInstance.transform.SetParent(parent);
            spawnableInstance.transform.SetPositionAndRotation(position, rotation);

            // Activating and Spawning the object
            spawnableInstance.gameObject.SetActive(true);
            spawnableInstance.OnSpawn();

            this.currentSpawnCount++;
            this.maxSpawnedCount = Math.Max(this.currentSpawnCount, this.maxSpawnedCount);

            return spawnableInstance;
        }

        public void AddToPool(Spawnable spawnable)
        {
            this.spawnableObjectsPool.Add(spawnable);

            // Deactivating / Despawning the object
            spawnable.OnDespawn();
            spawnable.gameObject.SetActive(false);
            spawnable.gameObject.transform.SetParent(null);
            GameObject.DontDestroyOnLoad(spawnable);

            this.currentSpawnCount--;
        }
    }
}
