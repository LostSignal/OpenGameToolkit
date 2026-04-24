//-----------------------------------------------------------------------
// <copyright file="SpriteAtlasLoadingManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.U2D;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2235:Mark all non-serializable fields", Justification = "Using Unity Serialization")]
    [Serializable]
    public class Atlas
    {
#pragma warning disable 0649
        [SerializeField] private string tag;
        [SerializeField] private LazySpriteAtlas spriteAtlas;
#pragma warning restore 0649

        public Atlas(string tag, string guid)
        {
            this.tag = tag;
            this.spriteAtlas = new LazySpriteAtlas(guid);
        }

        public LazySpriteAtlas SpriteAtlas
        {
            get { return this.spriteAtlas; }
            set { this.spriteAtlas = value; }
        }

        public string Tag
        {
            get { return this.tag; }
            set { this.tag = value; }
        }
    }

    public sealed class SpriteAtlasLoadingManager : Manager
    {
#pragma warning disable 0649
        [SerializeField] private List<Atlas> atlases = new List<Atlas>();
#pragma warning restore 0649

        private readonly Dictionary<string, Action<SpriteAtlas>> unknownAtlasRequests = new Dictionary<string, Action<SpriteAtlas>>();
        private Dictionary<string, Atlas> atlasesMap = null;

        public static SpriteAtlasLoadingManager Instance
        {
            get
            {
                Debug.LogError("SpriteAtlaseLoadingManager.Instance no longer supported");
                return GameObject.FindAnyObjectByType<Bootloader>().FindManager<SpriteAtlasLoadingManager>();
            }
        }

        protected override Task InitializeManager(Bootloader bootloader)
        {
            SpriteAtlasManager.atlasRequested += this.RequestAtlas;
            return Task.CompletedTask;
        }

        private void OnDestroy()
        {
            SpriteAtlasManager.atlasRequested -= this.RequestAtlas;
        }

        public void RegisterAtlas(string tag, string guid)
        {
            Dictionary<string, Atlas> atlasMap = this.GetAtlasMap();
            atlasMap.Add(tag, new Atlas(tag, guid));

            if (this.unknownAtlasRequests.ContainsKey(tag))
            {
                this.RequestAtlas(tag, this.unknownAtlasRequests[tag]);
                this.unknownAtlasRequests.Remove(tag);
            }
        }

        public bool IsAtlasTagLoaded(string tag)
        {
            if (this.GetAtlasMap().TryGetValue(tag, out Atlas atlas))
            {
                return atlas.SpriteAtlas.IsLoaded;
            }

            return false;
        }

        public UnityTask<SpriteAtlas> LoadAtlasTag(string tag)
        {
            if (this.GetAtlasMap().TryGetValue(tag, out Atlas atlas))
            {
                return atlas.SpriteAtlas.Load();
            }

            return null;
        }

        public void UnloadAtlas(string tag)
        {
            if (this.GetAtlasMap().TryGetValue(tag, out Atlas atlas))
            {
                if (atlas.SpriteAtlas.IsLoaded)
                {
                    atlas.SpriteAtlas.Release();
                }
            }
        }

        private Dictionary<string, Atlas> GetAtlasMap()
        {
            if (this.atlasesMap == null)
            {
                this.atlasesMap = new Dictionary<string, Atlas>();

                foreach (var atlas in this.atlases)
                {
                    this.atlasesMap.Add(atlas.Tag, atlas);
                }
            }

            return this.atlasesMap;
        }

        private void RequestAtlas(string tag, Action<SpriteAtlas> callback)
        {
            Dictionary<string, Atlas> atlasMap = this.GetAtlasMap();

            if (atlasMap.TryGetValue(tag, out Atlas atlas))
            {
                if (atlas.SpriteAtlas.IsLoaded)
                {
                    callback?.Invoke(atlas.SpriteAtlas.Load().Value);
                }
                else
                {
                    CoroutineRunner.Instance.StartCoroutine(LoadSpriteAtlasCoroutine());
                }
            }
            else
            {
                if (this.unknownAtlasRequests.ContainsKey(tag) == false)
                {
                    this.unknownAtlasRequests.Add(tag, callback);
                }
            }

            IEnumerator LoadSpriteAtlasCoroutine()
            {
                var loadAtlas = atlas.SpriteAtlas.Load();
                yield return loadAtlas;
                callback?.Invoke(loadAtlas.Value);
            }
        }
    }
}

#endif
