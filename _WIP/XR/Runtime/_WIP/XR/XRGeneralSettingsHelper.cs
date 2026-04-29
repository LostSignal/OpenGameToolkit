#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="XRGeneralSettingsHelper.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_MANAGEMENT

namespace OGT.XR
{
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.XR.Management;

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class XRGeneralSettingsHelper
    {
#if UNITY_EDITOR
        //// NOTE [bgish]: The sole purpose of this package is to handle what xr plugins are initialize.  For this
        ////               to work, we can't have InitManagerOnStart on, so this code forces it off.
        static XRGeneralSettingsHelper()
        {
            //// UnityEditor.EditorApplication.delayCall += () =>
            //// {
            ////     string searchText = "t:XRGeneralSettings";
            ////     string[] assets = UnityEditor.AssetDatabase.FindAssets(searchText);
            ////
            ////     if (assets.Length > 0)
            ////     {
            ////         string path = UnityEditor.AssetDatabase.GUIDToAssetPath(assets[0]);
            ////
            ////         foreach (var asset in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path))
            ////         {
            ////             if (asset is XRGeneralSettings settings)
            ////             {
            ////                 if (settings.InitManagerOnStart)
            ////                 {
            ////                     settings.InitManagerOnStart = false;
            ////                 }
            ////
            ////                 UnityEditor.EditorUtility.SetDirty(settings);
            ////             }
            ////         }
            ////     }
            //// };
        }

#endif

        // NOTE [bgish]: There is a bug in XRManagerSettings where TrySetLoader is broken, but this hack fixes it
        //   https://forum.unity.com/threads/xrmanagersettings-trysetloaders-doesnt-work-at-runtime.1210000/#post-7941664
        //   https://fogbugz.unity3d.com/default.asp?1408435_6emra084bpmhk9lg
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void UpdateRegisteredLoadersListHack()
        {
            if (XRGeneralSettings.Instance == null || XRGeneralSettings.Instance.Manager == null)
            {
                return;
            }

            var xrSettingsManager = XRGeneralSettings.Instance.Manager;
            var registeredLoadersField = xrSettingsManager.GetType().GetField("m_RegisteredLoaders", BindingFlags.Instance | BindingFlags.NonPublic);
            var registeredLoaders = registeredLoadersField.GetValue(xrSettingsManager) as HashSet<UnityEngine.XR.Management.XRLoader>;

            if (registeredLoaders.Count == 0)
            {
                foreach (var activeLoader in xrSettingsManager.activeLoaders)
                {
                    registeredLoaders.Add(activeLoader);
                }
            }
        }
    }
}

#endif
