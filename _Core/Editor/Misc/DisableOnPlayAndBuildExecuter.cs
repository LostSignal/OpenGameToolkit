//-----------------------------------------------------------------------
// <copyright file="DisableOnPlayAndBuildExecuter.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using UnityEngine;

    public static class DisableOnPlayAndBuildExecuter
    {
        private const string EditorPrefKey = "DisableOnPlay_DisabledInstanceIds";

        [EditorEvents.OnProcessSceneBuild]
        private static void OnProcessSceneBuild()
        {
            var componentsToDestroy = new List<DisableOnPlayAndBuild>();

            foreach (var disableOnPlay in GameObject.FindObjectsByType<DisableOnPlayAndBuild>(FindObjectsInactive.Include))
            {
                disableOnPlay.gameObject.SetActive(false);
                componentsToDestroy.Add(disableOnPlay);
            }

            foreach (var component in componentsToDestroy)
            {
                if (component)
                {
                    Object.DestroyImmediate(component);
                }
            }
        }

        [EditorEvents.OnEnterPlayMode]
        private static void OnEnterPlayMode()
        {
            UnityEditor.EditorPrefs.DeleteKey(EditorPrefKey);

            var objectToDisable = GameObject.FindObjectsByType<DisableOnPlayAndBuild>(FindObjectsInactive.Include);
            var disabledEntityIds = new List<long>();

            foreach (var obj in objectToDisable)
            {
                if (obj.gameObject.activeSelf)
                {
                    disabledEntityIds.Add(obj.gameObject.GetEntityId());
                    obj.gameObject.SetActive(false);
                }
            }

            if (disabledEntityIds.Count > 0)
            {
                UnityEditor.EditorPrefs.SetString(EditorPrefKey, string.Join(",", disabledEntityIds));
            }
        }

        [EditorEvents.OnExitPlayMode]
        private static void OnExitPlayMode()
        {
            var objectToEnable = GameObject.FindObjectsByType<DisableOnPlayAndBuild>(FindObjectsInactive.Include);
            var disabledEntityIdsString = UnityEditor.EditorPrefs.GetString(EditorPrefKey, null);
            var disabledEntityIds = new HashSet<long>();

            if (disabledEntityIdsString != null)
            {
                foreach (var idStr in disabledEntityIdsString.Split(','))
                {
                    if (long.TryParse(idStr, out long entityId))
                    {
                        disabledEntityIds.Add(entityId);
                    }
                }
            }

            foreach (var obj in objectToEnable)
            {
                if (disabledEntityIds.Contains(obj.gameObject.GetEntityId()))
                {
                    obj.gameObject.SetActive(true);
                }
            }

            UnityEditor.EditorPrefs.DeleteKey(EditorPrefKey);
        }
    }
}
