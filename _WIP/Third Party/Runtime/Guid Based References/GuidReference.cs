//-----------------------------------------------------------------------
// <copyright file="GuidReference.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    // This call is the type used by any other code to hold a reference to an object by GUID
    // If the target object is loaded, it will be returned, otherwise, NULL will be returned
    // This always works in Game Objects, so calling code will need to use GetComponent<>
    // or other methods to track down the specific objects need by any given system

    // Ideally this would be a struct, but we need the ISerializationCallbackReciever
    [System.Serializable]
    public class GuidReference : ISerializationCallbackReceiver
    {
        // Cache the referenced Game Object if we find one for performance
        [NonSerialized] private GameObject cachedReference;
        [NonSerialized] private bool isCacheSet;
        [NonSerialized] private System.Guid guid;

        // store our GUID in a form that Unity can save
        [SerializeField]
        private byte[] serializedGuid;

#if UNITY_EDITOR
        // decorate with some extra info in Editor so we can inform a user of what that GUID means
        [SerializeField]
        private string cachedName;

        [SerializeField]
        private string cachedGuidString;

        [SerializeField]
        private SceneAsset cachedScene;
#endif

        // create concrete delegates to avoid boxing.
        // When called 10,000 times, boxing would allocate ~1MB of GC Memory
        private Action<GameObject> addDelegate;
        private Action removeDelegate;

        public GuidReference()
        {
        }

        public GuidReference(GuidComponent target)
        {
            this.guid = target.GetGuid();
        }

        // Set up events to let users register to cleanup their own cached references on destroy or to cache off values
        public event Action<GameObject> OnGuidAdded = (GameObject go) => {};

        public event Action OnGuidRemoved = () => {};

        // optimized accessor, and ideally the only code you ever call on this class
        public GameObject GameObject
        {
            get
            {
                if (this.isCacheSet)
                {
                    return this.cachedReference;
                }

                this.cachedReference = GuidManager.ResolveGuid(this.guid, this.addDelegate, this.removeDelegate);
                this.isCacheSet = true;
                return this.cachedReference;
            }

            private set
            {
            }
        }

        // Convert system guid to a format unity likes to work with
        public void OnBeforeSerialize()
        {
            this.serializedGuid = this.guid.ToByteArray();
        }

        // Convert from byte array to system guid and reset state
        public void OnAfterDeserialize()
        {
            this.cachedReference = null;
            this.isCacheSet = false;

            if (this.serializedGuid == null || this.serializedGuid.Length != 16)
            {
                this.serializedGuid = new byte[16];
            }

            this.guid = new System.Guid(this.serializedGuid);
            this.addDelegate = this.GuidAdded;
            this.removeDelegate = this.GuidRemoved;
        }

        private void GuidAdded(GameObject go)
        {
            this.cachedReference = go;
            this.OnGuidAdded(go);
        }

        private void GuidRemoved()
        {
            this.cachedReference = null;
            this.isCacheSet = false;
            this.OnGuidRemoved();
        }
    }
}
