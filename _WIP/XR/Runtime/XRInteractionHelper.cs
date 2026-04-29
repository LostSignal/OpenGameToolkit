#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="XRInteractionHelper.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace OGT.Haven
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Unity.XR.CoreUtils;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.SceneManagement;
    using UnityEngine.XR.Interaction.Toolkit;
    using UnityEngine.XR.Interaction.Toolkit.Inputs;
    using UnityEngine.XR.Interaction.Toolkit.Locomotion;
    using UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing;
    using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;
    using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
    using UnityEngine.XR.Interaction.Toolkit.UI;

    public static class XRInteractionHelper
    {
        private static XRInteractionManager xrInteractionManagerInstance;

        public static XRInteractionManager XRInteractionManagerInstance => xrInteractionManagerInstance;

        [EditorEvents.OnExitPlayMode]
        private static void OnExitPlayMode()
        {
            xrInteractionManagerInstance = null;

            ResetLastFindFrame<TeleportationProvider>();
            ResetLastFindFrame<EventSystem>();
            ResetLastFindFrame<CanvasOptimizer>();
            ResetLastFindFrame<TeleportationProvider>();
            ResetLastFindFrame<LocomotionMediator>();
            ResetLastFindFrame<GravityProvider>();
            ResetLastFindFrame<ClimbProvider>();
            ResetLastFindFrame<XROrigin>();
            ResetLastFindFrame<XRInputModalityManager>();
            ResetLastFindFrame<XRInteractionManager>();
            ResetLastFindFrame<ClimbTeleportInteractor>();

#if AR_FOUNDATION_PRESENT
            // ResetLastFindFrame<ARRaycastManager>();
            // ResetLastFindFrame<ARPlaneManager>();
            // ResetLastFindFrame<ARSessionOrigin>();
#endif

            static void ResetLastFindFrame<T>()
            {
                try
                {
                    Assembly asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Unity.XR.Interaction.Toolkit");
                    Type genericType = asm.GetType("UnityEngine.XR.Interaction.Toolkit.Utilities.ComponentLocatorUtility`1");
                    Type closedType = genericType.MakeGenericType(typeof(TeleportationProvider));
                    FieldInfo lastTryFindFrameField = closedType.GetField("s_LastTryFindFrame", BindingFlags.Static | BindingFlags.NonPublic);
                    lastTryFindFrameField.SetValue(null, -1);
                }
                catch (Exception)
                {
                    // Ignore
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            FindOrCreateXRInteractionManager();
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private static void FindOrCreateXRInteractionManager()
        {
            if (xrInteractionManagerInstance != null)
            {
                return;
            }

            xrInteractionManagerInstance = GameObject.FindAnyObjectByType<XRInteractionManager>();

            if (xrInteractionManagerInstance == null)
            {
                xrInteractionManagerInstance = new GameObject("XRInteractionManager", typeof(XRInteractionManager)).GetComponent<XRInteractionManager>();
                GameObject.DontDestroyOnLoad(xrInteractionManagerInstance.gameObject);
            }
        }

        private static void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            FindOrCreateXRInteractionManager();
            FixTeleports();
        }

        private static void FixTeleports()
        {
            // xrInteractionManagerInstance.StartCoroutine(Coroutine());
            //
            // IEnumerator Coroutine()
            // {
            //     yield return HavenRig.WaitForRig();
            //
            //     var rig = HavenRig.GetRig();
            //     var teleportProvider = rig.GetComponentInChildren<TeleportationProvider>();
            //
            //     foreach (var teleport in GameObject.FindObjectsOfType<BaseTeleportationInteractable>(true))
            //     {
            //         teleport.teleportationProvider = teleportProvider;
            //         teleport.interactionManager = xrInteractionManagerInstance;
            //     }
            // }
        }
    }
}

#endif
