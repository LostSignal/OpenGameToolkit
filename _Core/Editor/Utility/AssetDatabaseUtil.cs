//-----------------------------------------------------------------------
// <copyright file="AssetDatabaseUtil.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public static class AssetDatabaseUtil
    {
        public static IEnumerable<GameObject> GetAllProjectPrefabs()
        {
            return AssetDatabase.GetAllAssetPaths()
                .Where(x => x.ToLower().EndsWith(".prefab"))
                .Select(x => AssetDatabase.LoadAssetAtPath<GameObject>(x));
        }

        public static IEnumerable<ScriptableObject> GetAllScriptableObjects(string title)
        {
            string[] scriptableObjectGuids = AssetDatabase.FindAssets("t:scriptableobject");

            int count = 0;
            foreach (var scriptableObjectGuid in scriptableObjectGuids)
            {
                var scriptableObjectPath = AssetDatabase.GUIDToAssetPath(scriptableObjectGuid);
                var scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(scriptableObjectPath);

                EditorUtility.DisplayProgressBar(title, scriptableObjectPath, count++ / (float)scriptableObjectGuids.Length);

                yield return scriptableObject;
            }

            EditorUtility.ClearProgressBar();
        }

        public static IEnumerable<GameObject> GetAllPrefabs(string title)
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:prefab");

            int count = 0;
            foreach (var prefabGuid in prefabGuids)
            {
                var prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                var gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                EditorUtility.DisplayProgressBar(title, prefabPath, count++ / (float)prefabGuids.Length);

                yield return gameObject;
            }

            EditorUtility.ClearProgressBar();
        }

        public static IEnumerable<T> GetAllPrefabsOfType<T>(string title)
        {
            foreach (var prefab in GetAllPrefabs(title))
            {
                if (prefab.TryGetComponent<T>(out T component))
                {
                    yield return component;
                }
            }
        }
    }
}
