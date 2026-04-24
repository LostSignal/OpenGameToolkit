//-----------------------------------------------------------------------
// <copyright file="GameObjectExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using UnityEngine;

    public static class GameObjectExtensions
    {
        public static string GetFullName(this GameObject gameObject)
        {
            return GetFullNameRecursive(gameObject.transform);

            static string GetFullNameRecursive(Transform transform)
            {
                if (transform.parent != null)
                {
                    return $"{GetFullNameRecursive(transform.parent)}/{transform.name}";
                }
                else
                {
                    return transform.name;
                }
            }
        }

        public static void SafeSetActive(this GameObject gameObject, bool active)
        {
            if (gameObject && gameObject.activeSelf != active)
            {
                gameObject.SetActive(active);
            }
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject)
            where T : Component
        {
            if (gameObject.TryGetComponent<T>(out T component))
            {
                return component;
            }

            return gameObject.AddComponent<T>();
        }

        public static Component GetOrAddComponent(this GameObject gameObject, System.Type componentType)
        {
            var component = gameObject.GetComponent(componentType);

            if (component == null)
            {
                component = gameObject.AddComponent(componentType);
            }

            return component;
        }

        public static List<T> GetOrAddComponents<T>(this GameObject gameObject, int count)
            where T : Component
        {
            var results = new List<T>(gameObject.GetComponents<T>());

            int needed = count - results.Count;
            for (int i = 0; i < needed; i++)
            {
                T component = gameObject.AddComponent<T>();
                results.Add(component);
            }

            return results;
        }

        public static GameObject GetChild(this GameObject gameObject, string name)
        {
            Transform childTransform = gameObject.transform.Find(name);
            return childTransform == null ? null : childTransform.gameObject;
        }

        public static void SetLayerRecursively(this GameObject gameObject, string layerName)
        {
            PrivateSetLayerRecursively(gameObject, LayerMask.NameToLayer(layerName));
        }

        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            PrivateSetLayerRecursively(gameObject, layer);
        }

        public static void Destroy(this GameObject gameObject)
        {
            DestroyInternal(gameObject);
        }

        public static void DestroyImmediate(this GameObject gameObject)
        {
            GameObject.DestroyImmediate(gameObject);
        }

        public static void DestroyChildren(this GameObject gameObject)
        {
            for (int i = gameObject.transform.childCount - 1; i >= 0; i--)
            {
                DestroyInternal(gameObject.transform.GetChild(i).gameObject);
            }
        }

        public static void DestroyAllChildrenRecursively(this GameObject gameObject)
        {
            for (int i = gameObject.transform.childCount - 1; i >= 0; i--)
            {
                PrivateDestroyAllRecursively(gameObject.transform.GetChild(i).gameObject);
            }
        }

        public static void DestroyAllRecursively(this GameObject gameObject)
        {
            PrivateDestroyAllRecursively(gameObject);
        }

        public static List<GameObject> GetChildrenRecursively(this GameObject gameObject)
        {
            List<GameObject> children = new();
            GetChildrenRecursively(gameObject, children);
            return children;
        }

        public static void GetChildrenRecursively(this GameObject gameObject, List<GameObject> results)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                var child = gameObject.transform.GetChild(i).gameObject;
                results.Add(child);
                GetChildrenRecursively(child, results);
            }
        }

        private static void PrivateSetLayerRecursively(GameObject gameObject, int layer)
        {
            if (gameObject.layer != layer)
            {
                gameObject.layer = layer;
            }

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                PrivateSetLayerRecursively(gameObject.transform.GetChild(i).gameObject, layer);
            }
        }

        private static void PrivateDestroyAllRecursively(GameObject gameObject)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                PrivateDestroyAllRecursively(gameObject.transform.GetChild(i).gameObject);
            }

            DestroyInternal(gameObject);
        }

        private static void DestroyInternal(GameObject gameObject)
        {
            if (Application.isPlaying)
            {
                GameObject.Destroy(gameObject);
            }
            else
            {
                GameObject.DestroyImmediate(gameObject);
            }
        }
    }
}
