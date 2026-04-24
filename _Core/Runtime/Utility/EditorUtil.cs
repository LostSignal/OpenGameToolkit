//-----------------------------------------------------------------------
// <copyright file="EditorUtil.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Diagnostics;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public static class EditorUtil
    {
        private static readonly OGTLogger Logger = OGTLogger.OGTEditor;

        public static void SetIfNull<T>(MonoBehaviour context, ref T field)
            where T : Component
        {
            if (field == null)
            {
                field = context.GetComponent<T>();

                if (field == null)
                {
                    Logger.LogError($"EditorUtil.SetIfNull<{typeof(T).Name}> failed to get component of that type.", context);
                    return;
                }

                SetDirty(context);
            }
        }

        public static void SetIfNull<T>(UnityEngine.Object context, ref T field, string guid)
            where T : UnityEngine.Object
        {
            if (field == null)
            {
                field = GetAssetByGuid<T>(guid);

                if (field == null)
                {
                    Logger.LogError($"EditorUtil.SetIfNull<{typeof(T).Name}> failed to find asset with guid {guid} for {context.name}", context);
                    return;
                }

                SetDirty(context);
            }
        }

        public static string GetPath(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetAssetPath(obj.GetEntityId());
#else
            Logger.LogError("EditorUtil.GetPath called at runtime!");
            return null;
#endif
        }

        public static string GetGuid(UnityEngine.Object obj)
        {
#if UNITY_EDITOR

            if (obj == null)
            {
                return null;
            }

            string path = GetPath(obj);

            return string.IsNullOrEmpty(path) ? null : UnityEditor.AssetDatabase.AssetPathToGUID(path);

#else

            Logger.LogError("EditorUtil.GetGuid called at runtime!");
            return null;

#endif
        }

        /// <summary>
        /// Creates a child game object with the given name attached to this GameObject, or finds the game object
        /// if it already exists.  It will make sure that one of each of the given component types exist on the child.
        /// It only makes sure 1 component of that type exists though.  If you need multiple components then use the
        /// GetOrAddComponents method on the returned game object.
        /// </summary>
        /// <param name="gameObject">The game object to search.</param>
        /// <param name="name">The name of the child to search for.</param>
        /// <param name="components">The list of components to add ensure exist on the child.</param>
        /// <returns>The newly created or the found child.</returns>
        public static GameObject GetOrCreateChild(this GameObject gameObject, string name, params System.Type[] components)
        {
            if (Application.isPlaying)
            {
                Logger.LogError("Should NOT be calling GetOrCreateChild at runtime");
            }

            Transform childTransform = gameObject.transform.Find(name);

            // getting/creating a ball object
            if (childTransform == null)
            {
                var childGameObject = components == null || components.Length == 0 ? new GameObject(name) : new GameObject(name, components);
                childGameObject.transform.SetParent(gameObject.transform);
                childGameObject.transform.Reset();

                return childGameObject;
            }
            else
            {
                var childGameObject = childTransform.gameObject;

                if (components != null)
                {
                    foreach (var component in components)
                    {
                        EditorUtil.GetOrAddComponent(childGameObject, component);
                    }
                }

                return childGameObject;
            }
        }

        public static Component GetOrAddComponent(GameObject gameObject, Type componentType)
        {
            if (Application.isPlaying)
            {
                Logger.LogError($"EditorUtil.GetOrAddComponent({gameObject.name}, {componentType.Name}>() called at runtime!");
            }

            var component = gameObject.GetComponent(componentType);

            if (component)
            {
                return component;
            }

            var result = gameObject.AddComponent(componentType);

            EditorUtil.SetDirty(gameObject);

            return result;
        }

        public static Component GetOrAddComponent(MonoBehaviour behaviour, Type componentType)
        {
            if (Application.isPlaying)
            {
                Logger.LogError($"EditorUtil.GetOrAddComponent({behaviour.GetType().Name}, {componentType.Name}>() called at runtime!");
            }

            var component = behaviour.GetComponent(componentType);

            if (component)
            {
                return component;
            }

            var result = behaviour.gameObject.AddComponent(componentType);

            EditorUtil.SetDirty(behaviour);

            return result;
        }

        public static T GetOrAddComponent<T>(MonoBehaviour behaviour)
            where T : Component
        {
            return GetOrAddComponent<T>(behaviour.gameObject);
        }

        public static T GetOrAddComponent<T>(Transform transform)
            where T : Component
        {
            return GetOrAddComponent<T>(transform.gameObject);
        }

        public static T GetOrAddComponent<T>(GameObject gameObject)
            where T : Component
        {
            if (Application.isPlaying)
            {
                Logger.LogError($"EditorUtil.GetOrAddComponent<{typeof(T).Name}>() called at runtime!");
            }

            var component = gameObject.GetComponent<T>();

            if (component)
            {
                return component;
            }

            var result = gameObject.AddComponent<T>();

            EditorUtil.SetDirty(gameObject);

            return result;
        }

        public static T GetAssetByGuid<T>(string guid)
            where T : UnityEngine.Object
        {
            if (Application.isEditor == false)
            {
                Logger.LogError("Trying to call EditorUtil.GetAssetByGuid from a build!");
            }
            else
            {
#if UNITY_EDITOR
                if (string.IsNullOrEmpty(guid))
                {
                    return null;
                }

                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

                if (string.IsNullOrEmpty(path))
                {
                    return null;
                }

                return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
#endif
            }

            return null;
        }

        [Conditional("UNITY_EDITOR")]
        public static void SetDirty(UnityEngine.Object target)
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(target);
#endif
        }

        public static void SaveAll()
        {
            SaveProject();
            SaveScenes();
        }

        public static void SaveProjectAndScene(Component component)
        {
            SaveProject();
            SaveScene(component);
        }

        public static void SaveProjectAndScene(GameObject gameObject)
        {
            SaveProject();
            SaveScene(gameObject);
        }

        public static void SaveProjectAndScene(Scene scene)
        {
            SaveProject();
            SaveScene(scene);
        }

        public static void SaveProject()
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }

        public static void SaveScenes()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExecuteMenuItem("File/Save");
#endif
        }

        public static void SaveScene(Component component)
        {
            SaveScene(component.gameObject.scene);
        }

        public static void SaveScene(GameObject gameObject)
        {
            SaveScene(gameObject.scene);
        }

        public static void SaveScene(Scene scene)
        {
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
#endif
        }
    }
}
