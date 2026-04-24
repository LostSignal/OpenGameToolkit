//-----------------------------------------------------------------------
// <copyright file="DataStoreObject.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public abstract class DataStoreObject : ScriptableObject
    {
        private static readonly List<DataStoreObject> DataStoreObjects = new ();

        private readonly DataStore dataStore = new ();

#pragma warning disable 0649
        [SerializeField][ReadOnly] private int id;
        [SerializeField] private bool saveOnUnload;
#pragma warning restore 0649

        public int Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.id;
        }

        public DataStore DataStore
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.dataStore;
        }

        public abstract void Load();

        public abstract void Save();

        protected virtual void OnEnable()
        {
            if (UnityPlatformProvider.IsPlayingOrEnteringPlaymode)
            {
                DataStoreObjects.AddIfUnique(this);
                this.Load();
            }
        }

        protected virtual void OnDisable()
        {
            if (DataStoreObjects.Contains(this))
            {
                DataStoreObjects.Remove(this);
                this.SaveOnUnload();
            }
        }

        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            var path = UnityEditor.AssetDatabase.GetAssetPath(this);
            var guid = UnityEditor.AssetDatabase.GUIDFromAssetPath(path);
            int hash = Math.Abs(guid.GetHashCode());

            if (this.id != hash)
            {
                this.id = hash;
                EditorUtil.SetDirty(this);
            }
#endif
        }

        [EditorEvents.OnEnterPlayMode]
        private static void OnEnterPlayMode()
        {
            DataStoreObjects.Clear();
        }

        [EditorEvents.OnExitingPlayMode]
        private static void OnExitingPlayMode()
        {
            foreach (var dataStoreObject in DataStoreObjects)
            {
                if (dataStoreObject)
                {
                    dataStoreObject.SaveOnUnload();
                }
            }

            DataStoreObjects.Clear();
        }

        private void SaveOnUnload()
        {
            if (this.saveOnUnload)
            {
                this.Save();
            }
        }
    }
}

#endif
