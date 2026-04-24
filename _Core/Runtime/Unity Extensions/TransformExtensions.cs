//-----------------------------------------------------------------------
// <copyright file="TransformExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using UnityEngine;

    public static partial class TransformExtensions
    {
        public static void Reset(this Transform transform)
        {
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        public static Transform FindOrCreateChild(this Transform transform, string childName, params System.Type[] types)
        {
            var child = transform.Find(childName);

            if (child == null)
            {
                child = new GameObject(childName, types).transform;
                child.SetParent(transform);
                child.Reset();
            }

            return child;
        }

        public static List<Transform> GetChildrenRecursively(this Transform transform)
        {
            var children = new List<Transform>();
            GetChildrenRecursively(transform, children);
            return children;
        }

        public static void GetChildrenRecursively(this Transform transform, List<Transform> results)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                results.Add(child);
                GetChildrenRecursively(child, results);
            }
        }

        public static string GetFullPath(this Transform transform)
        {
            return CalculateFullPath(transform);

            static string CalculateFullPath(Transform transform)
            {
                string parentName = transform.parent != null ? CalculateFullPath(transform.parent) : null;
                return parentName != null ? parentName + "/" + transform.name : transform.name;
            }
        }

        public static string GetFullPathWithSceneName(this Transform transform)
        {
            return CalculateFullPath(transform);

            static string CalculateFullPath(Transform transform)
            {
                string parentName = transform.parent == null ?
                    transform.gameObject.scene.name :
                    CalculateFullPath(transform.parent);

                return parentName + "/" + transform.name;
            }
        }

        public static Transform FindChildRecursive(this Transform transform, string childName)
        {
            if (transform)
            {
                if (transform.name == childName)
                {
                    return transform;
                }

                for (int i = 0; i < transform.childCount; i++)
                {
                    var childTransform = FindChildRecursive(transform.GetChild(i), childName);

                    if (childTransform != null)
                    {
                        return childTransform;
                    }
                }
            }

            return null;
        }

        public static void DestroyAllChildren(this Transform transform)
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                for (int i = transform.childCount - 1; i >= 0; i--)
                {
                    GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
                }

                return;
            }
#endif

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }
        }

        public static void SafeSetActive(this Transform transform, bool active)
        {
            if (transform != null)
            {
                transform.gameObject.SafeSetActive(active);
            }
        }
    }
}
