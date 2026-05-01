//-----------------------------------------------------------------------
// <copyright file="GuidManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    // Class to handle registering and accessing objects by GUID
    public class GuidManager
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        // Singleton interface
        private static GuidManager instance;

        // Instance data
        private readonly Dictionary<System.Guid, GuidInfo> guidToObjectMap = new();

        //// NOTE [bgish]: This class needs to be reset eventually, just not sure when
        ////               Try with Reload Domain and Reload Scene off
        ////               Also check if gameobejcts in scene get destroyed when entering play mode.
        ////
        //// [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        //// private static void RuntimeInit()
        //// {
        ////     OGT.Logger.Log("RuntimeInit");
        //// }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        // All the public API is static so you need not worry about creating an instance
        public static bool Add(GuidComponent guidComponent)
        {
            if (instance == null)
            {
                instance = new GuidManager();
            }

            return instance.InternalAdd(guidComponent);
        }

        public static void Remove(System.Guid guid)
        {
            if (instance == null)
            {
                instance = new GuidManager();
            }

            instance.InternalRemove(guid);
        }

        public static GameObject ResolveGuid(System.Guid guid, Action<GameObject> onAddCallback, Action onRemoveCallback)
        {
            if (instance == null)
            {
                instance = new GuidManager();
            }

            return instance.ResolveGuidInternal(guid, onAddCallback, onRemoveCallback);
        }

        public static GameObject ResolveGuid(System.Guid guid, Action onDestroyCallback)
        {
            if (instance == null)
            {
                instance = new GuidManager();
            }

            return instance.ResolveGuidInternal(guid, null, onDestroyCallback);
        }

        public static GameObject ResolveGuid(System.Guid guid)
        {
            if (instance == null)
            {
                instance = new GuidManager();
            }

            return instance.ResolveGuidInternal(guid, null, null);
        }

        private bool InternalAdd(GuidComponent guidComponent)
        {
            Guid guid = guidComponent.GetGuid();

            var info = new GuidInfo(guidComponent);

            if (this.guidToObjectMap.ContainsKey(guid) == false)
            {
                this.guidToObjectMap.Add(guid, info);
                return true;
            }

            GuidInfo existingInfo = this.guidToObjectMap[guid];
            if (existingInfo.GO != null && existingInfo.GO != guidComponent.gameObject)
            {
                // normally, a duplicate GUID is a big problem, means you won't necessarily be referencing what you expect
                if (Application.isPlaying)
                {
                    Logger.AssertFormat(
                        false,
                        guidComponent,
                        "Guid Collision Detected between {0} and {1}.\nAssigning new Guid. Consider tracking runtime instances using a direct reference or other method.",
                        this.guidToObjectMap[guid].GO != null ? this.guidToObjectMap[guid].GO.name : "NULL",
                        guidComponent != null ? guidComponent.name : "NULL");
                }
                else
                {
                    // however, at editor time, copying an object with a GUID will duplicate the GUID resulting in a collision and repair.
                    // we warn about this just for pedantry reasons, and so you can detect if you are unexpectedly copying these components
                    Logger.LogWarningFormat(guidComponent, "Guid Collision Detected while creating {0}.\nAssigning new Guid.", guidComponent != null ? guidComponent.name : "NULL");
                }

                return false;
            }

            // if we already tried to find this GUID, but haven't set the game object to anything specific, copy any OnAdd callbacks then call them
            existingInfo.GO = info.GO;
            existingInfo.HandleAddCallback();

            this.guidToObjectMap[guid] = existingInfo;

            return true;
        }

        private void InternalRemove(System.Guid guid)
        {
            if (this.guidToObjectMap.TryGetValue(guid, out GuidInfo info))
            {
                // trigger all the destroy delegates that have registered
                info.HandleRemoveCallback();
            }

            this.guidToObjectMap.Remove(guid);
        }

        // Nice easy api to find a GUID, and if it works, register an on destroy callback
        // this should be used to register functions to cleanup any data you cache on finding
        // your target. Otherwise, you might keep components in memory by referencing them
        private GameObject ResolveGuidInternal(System.Guid guid, Action<GameObject> onAddCallback, Action onRemoveCallback)
        {
            if (this.guidToObjectMap.TryGetValue(guid, out GuidInfo info))
            {
                if (onAddCallback != null)
                {
                    info.OnAdd += onAddCallback;
                }

                if (onRemoveCallback != null)
                {
                    info.OnRemove += onRemoveCallback;
                }

                this.guidToObjectMap[guid] = info;
                return info.GO;
            }

            if (onAddCallback != null)
            {
                info.OnAdd += onAddCallback;
            }

            if (onRemoveCallback != null)
            {
                info.OnRemove += onRemoveCallback;
            }

            this.guidToObjectMap.Add(guid, info);

            return null;
        }

        // For each GUID we need to know the Game Object it references
        // and an event to store all the callbacks that need to know when it is destroyed
        private struct GuidInfo
        {
            public GameObject GO;

            public GuidInfo(GuidComponent comp)
            {
                this.GO = comp.gameObject;
                this.OnRemove = null;
                this.OnAdd = null;
            }

            public event Action<GameObject> OnAdd;

            public event Action OnRemove;

            public void HandleAddCallback()
            {
                this.OnAdd?.Invoke(this.GO);
            }

            public void HandleRemoveCallback()
            {
                this.OnRemove?.Invoke();
            }
        }
    }
}
