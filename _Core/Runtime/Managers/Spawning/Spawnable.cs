//-----------------------------------------------------------------------
// <copyright file="Spawnable.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public class Spawnable : GameBehavior, IValidate
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        [ReadOnly][SerializeField] private string guid;
        [ReadOnly][SerializeField] private List<MonoBehaviour> spawnComponents;
        [ReadOnly][SerializeField] private bool isSpawned;

        public string Guid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.guid;
        }

        public bool IsSpawned
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.isSpawned;
        }

        public void OnSpawn()
        {
            this.isSpawned = true;

            if (this.spawnComponents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var component in this.spawnComponents)
            {
                if (component is ISpawn spawnComponent)
                {
                    spawnComponent.OnSpawn();
                }
                else
                {
                    Logger.LogError("Spawnable.OnSpawn found spawnComponents that did not implement ISpawn!", this);
                }
            }
        }

        public void OnDespawn()
        {
            this.isSpawned = false;

            if (this.spawnComponents.IsNullOrEmpty())
            {
                return;
            }

            foreach (var component in this.spawnComponents)
            {
                if (component is ISpawn spawnComponent)
                {
                    spawnComponent.OnDespawn();
                }
                else
                {
                    Logger.LogError("Spawnable.OnDespawn found spawnComponents that did not implement ISpawn!", this);
                }
            }
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
#if UNITY_EDITOR
            if (isSceneObject || this.EditorIsPrefab() == false)
            {
                return;
            }

            var childSpawnComponents = this.GetComponentsInChildren<MonoBehaviour>()
                .Where(x => x is ISpawn)
                .ToList();

            if (childSpawnComponents.Count == 0)
            {
                if (this.spawnComponents != null)
                {
                    this.spawnComponents = null;
                    EditorUtil.SetDirty(this);
                }
            }
            else if (this.spawnComponents == null || this.spawnComponents.SequenceEqual(childSpawnComponents) == false)
            {
                this.spawnComponents = childSpawnComponents;
                EditorUtil.SetDirty(this);
            }

            var guid = this.EditorGetGuid();

            if (guid != null)
            {
                this.EditorSetValue(ref this.guid, guid);
            }

            report.AssertNotNull(this, this.guid, nameof(this.guid));
            report.AssertAddressable(this);
#endif
        }
    }
}
