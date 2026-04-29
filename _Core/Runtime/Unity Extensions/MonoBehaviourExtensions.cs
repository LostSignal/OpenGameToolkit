//-----------------------------------------------------------------------
// <copyright file="MonoBehaviourExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections;
    using System.Linq;
    using UnityEngine;

    public static class MonoBehaviourExtensions
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        public static void EditorSetValue<T>(this MonoBehaviour monoBehaviour, ref T memberVariable, T newValue)
            where T : IEquatable<T>
        {
#if UNITY_EDITOR
            if (memberVariable?.Equals(newValue) == false)
            {
                memberVariable = newValue;
                EditorUtil.SetDirty(monoBehaviour);
            }
#endif
        }

        public static void EditorSetValueIfNullOrEmpty(this MonoBehaviour monoBehaviour, ref string memberVariable, string newValue)
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(memberVariable) == false)
            {
                return;
            }

            if (memberVariable != newValue)
            {
                memberVariable = newValue;
                EditorUtil.SetDirty(monoBehaviour);
            }
#endif
        }

        public static void EditorGetFirstComponentInChildren<T>(this MonoBehaviour monoBehaviour, ref T memberVariable, bool ignoreIfNotNull = false)
            where T : Component
        {
#if UNITY_EDITOR
            if (ignoreIfNotNull && memberVariable != null)
            {
                return;
            }

            T component = monoBehaviour.GetComponentsInChildren<T>().FirstOrDefault();

            if (memberVariable != component)
            {
                memberVariable = component;
                EditorUtil.SetDirty(monoBehaviour);
            }
#endif
        }

        public static void EditorGetComponent<T>(this MonoBehaviour monoBehaviour, ref T memberVariable, bool ignoreIfNotNull = false)
            where T : Component
        {
#if UNITY_EDITOR
            if (ignoreIfNotNull && memberVariable == null)
            {
                return;
            }

            T component = monoBehaviour.GetComponent<T>();

            if (memberVariable != component)
            {
                memberVariable = component;
                EditorUtil.SetDirty(monoBehaviour);

                if (memberVariable == null)
                {
                    Logger.LogErrorFormat(monoBehaviour.gameObject, "{0} {1} couldn't find {2} component", monoBehaviour.GetType().Name, GetFullName(monoBehaviour), typeof(T).Name);
                }
            }
#endif
        }

        public static void EditorGetComponentInParent<T>(this MonoBehaviour monoBehaviour, ref T memberVariable)
            where T : Component
        {
#if UNITY_EDITOR
            // Making sure we're in a valid scene (and not just a prefab) before looking upwards
            if (monoBehaviour.gameObject.scene.IsValid() == false || UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                return;
            }

            T component = monoBehaviour.GetComponentInParent<T>();

            if (memberVariable != component)
            {
                memberVariable = component;
                EditorUtil.SetDirty(monoBehaviour);

                if (memberVariable == null)
                {
                    Logger.LogErrorFormat(monoBehaviour.gameObject, "{0} {1} couldn't find {2} component in parent.", monoBehaviour.GetType().Name, GetFullName(monoBehaviour), typeof(T).Name);
                }
            }
#endif
        }

        public static Coroutine ExecuteAtEndOfFrame(this MonoBehaviour lhs, Action action)
        {
            return CoroutineRunner.Instance.StartCoroutine(DelayTillEndOfFrameCoroutine());

            IEnumerator DelayTillEndOfFrameCoroutine()
            {
                yield return WaitForUtil.EndOfFrame;
                action?.Invoke();
            }
        }

        public static Coroutine ExecuteDelayed(this MonoBehaviour lhs, float delayInSeconds, Action action)
        {
            if (delayInSeconds <= 0)
            {
                action?.Invoke();
                return null;
            }

            return CoroutineRunner.Instance.StartCoroutine(DelayInSecondsCoroutine());

            IEnumerator DelayInSecondsCoroutine()
            {
                yield return WaitForUtil.Seconds(delayInSeconds);
                action?.Invoke();
            }
        }

        public static Coroutine ExecuteDelayedRealtime(this MonoBehaviour lhs, float delayInRealtimeSeconds, Action action)
        {
            if (delayInRealtimeSeconds <= 0)
            {
                action?.Invoke();
                return null;
            }

            return CoroutineRunner.Instance.StartCoroutine(DelayExecuteRealtimeCoroutine());

            IEnumerator DelayExecuteRealtimeCoroutine()
            {
                yield return WaitForUtil.RealtimeSeconds(delayInRealtimeSeconds);
                action?.Invoke();
            }
        }

        public static void Destroy(this MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null)
            {
                return;
            }
            else if (Application.isPlaying)
            {
                GameObject.Destroy(monoBehaviour);
            }
            else
            {
                GameObject.DestroyImmediate(monoBehaviour);
            }
        }

        private static string GetFullName(MonoBehaviour monoBehaviour)
        {
            return GetFullName(monoBehaviour.gameObject);
        }

        private static string GetFullName(GameObject gameObject)
        {
            if (gameObject.transform.parent == null)
            {
                return string.Empty;
            }
            else
            {
                string parentName = GetFullName(gameObject.transform.parent.gameObject);

                return string.IsNullOrEmpty(parentName) ? gameObject.name : parentName + "/" + gameObject.name;
            }
        }
    }
}
