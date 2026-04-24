//-----------------------------------------------------------------------
// <copyright file="EditorEvents.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;

#if UNITY_EDITOR
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
#endif

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Nesting for better discoverability.")]
    public static class EditorEvents
    {
        public class EditorEvent : Attribute { }
        public sealed class InitializeOnLoad : EditorEvent { }
        public sealed class OnPreprocessBuildAttribute : EditorEvent { }
        public sealed class OnPostprocessBuildAttribute : EditorEvent { }
        public sealed class OnPostGenerateGradleAndroidProjectAttribute : EditorEvent { }
        public sealed class OnProcessSceneAttribute : EditorEvent { }
        public sealed class OnProcessSceneBuildAttribute : EditorEvent { }
        public sealed class OnExitingPlayModeAttribute : EditorEvent { }
        public sealed class OnExitPlayModeAttribute : EditorEvent { }
        public sealed class OnEnterPlayModeAttribute : EditorEvent { }
        public sealed class OnExitEditor : EditorEvent { }

#if UNITY_EDITOR

        [UnityEditor.InitializeOnLoad]
        public class EditorEventsExecutor :
#if UNITY_ANDROID
            UnityEditor.Android.IPostGenerateGradleAndroidProject,
#endif
            IPreprocessBuildWithReport,
            IPostprocessBuildWithReport,
            IProcessSceneWithReport
        {
            private static readonly StringBuilder stringBuilderCache = new StringBuilder();
            private static List<MethodInfo> EditorEventMethods = null;

            private static bool DisableLogging
            {
                get => EditorPrefs.GetBool("OGT.DisableEditorEventsLogging", false);
                set => EditorPrefs.SetBool("OGT.DisableEditorEventsLogging", value);
            }

            [MenuItem("Tools/OGT/Editor Events/Disable Logging", priority = MenuItemPriorities.EditorEvents + 0)]
            private static void DisableLoggingMenuItem()
            {
                DisableLogging = !DisableLogging;
            }

            [MenuItem("Tools/OGT/Editor Events/Disable Logging", true, priority = MenuItemPriorities.EditorEvents + 0)]
            private static bool DisableLoggingMenuItemValidate()
            {
                Menu.SetChecked("Tools/OGT/Editor Events/Disable Logging", DisableLogging);
                return true;
            }

            static EditorEventsExecutor()
            {
                // Special case to make sure Logging and Platform are always initialized in the editor
                UnityPlatformInitializer.RegisterProvider();
                UnityLoggingInitializer.RegisterProvider();

                EditorApplication.delayCall += () => ExecuteAttribute<EditorEvents.InitializeOnLoad>();
                EditorApplication.quitting += () => ExecuteAttribute<EditorEvents.OnExitEditor>();

                EditorApplication.playModeStateChanged += PlayModeStateChanged;
            }

            int IOrderedCallback.callbackOrder => 10;

            public static void ExecuteAttribute<T>(params object[] parameters)
                where T : Attribute
            {
                // Collecting all Methods that contain EditorEvent Attributes
                EditorEventMethods ??= FindEditorEventMethods();

                // Going through all the EditorEvent Methods and only firiing the ones of type T
                var executeAttributeStart = DateTime.Now;
                stringBuilderCache.Clear();

                foreach (var methodInfo in EditorEventMethods.Where(x => x.GetCustomAttribute<T>() != null))
                {
                    try
                    {
                        var executeMethodStart = DateTime.Now;
                        ExecuteMethod(methodInfo);
                        var executeMethodTotalMillis = DateTime.Now.Subtract(executeMethodStart).TotalMilliseconds;

                        stringBuilderCache.AppendLine($"    {methodInfo.DeclaringType.Name}.{methodInfo.Name} = {executeMethodTotalMillis} milliseconds");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Exception Executing Editor Event {typeof(T).Name}: {methodInfo.DeclaringType.Name}.{methodInfo.Name}");
                        Debug.LogException(ex);
                    }
                }

                var executeAttributeTotalMillis = DateTime.Now.Subtract(executeAttributeStart).TotalMilliseconds;

                LogFormat(
                    "Executing Attribute {0} took {1} milliseconds\n{2}\n",
                    typeof(T).Name,
                    executeAttributeTotalMillis,
                    stringBuilderCache);

                void ExecuteMethod(MethodInfo method)
                {
                    var methodParameters = method.GetParameters();

                    // Special Case for Android Gradle builds
                    if (typeof(T) == typeof(EditorEvents.OnPostGenerateGradleAndroidProjectAttribute) &&
                        methodParameters?.Length == 1 &&
                        methodParameters[0].ParameterType == typeof(string))
                    {
                        method.Invoke(null, parameters);
                        return;
                    }

                    // Special Case for Pre/Post Process Build
                    if ((typeof(T) == typeof(EditorEvents.OnPreprocessBuildAttribute) ||
                         typeof(T) == typeof(EditorEvents.OnPostprocessBuildAttribute)) &&
                        methodParameters?.Length == 1 &&
                        methodParameters[0].ParameterType == typeof(BuildReport))
                    {
                        method.Invoke(null, parameters);
                        return;
                    }

                    // Special Case for Process Scene
                    if (typeof(T) == typeof(EditorEvents.OnProcessSceneAttribute) ||
                        typeof(T) == typeof(EditorEvents.OnProcessSceneBuildAttribute))
                    {
                        var scene = parameters[0];
                        var buildReport = parameters[1];

                        if (methodParameters?.Length == 1 &&
                            methodParameters[0].ParameterType == typeof(Scene))
                        {
                            // Scene
                            method.Invoke(null, new object[] { scene });
                            return;
                        }
                        else if (methodParameters?.Length == 1 &&
                                 methodParameters[0].ParameterType == typeof(BuildReport))
                        {
                            // BuildReport
                            method.Invoke(null, new object[] { buildReport });
                            return;
                        }
                        else if (methodParameters?.Length == 2 &&
                                 methodParameters[0].ParameterType == typeof(BuildReport) &&
                                 methodParameters[1].ParameterType == typeof(Scene))
                        {
                            // BuildReport, Scene
                            method.Invoke(null, new object[] { buildReport, scene });
                            return;
                        }
                        else if (methodParameters?.Length == 2 &&
                                 methodParameters[0].ParameterType == typeof(Scene) &&
                                 methodParameters[1].ParameterType == typeof(BuildReport))
                        {
                            // Scene, BuildReport
                            method.Invoke(null, new object[] { scene, buildReport });
                            return;
                        }
                    }

                    method.Invoke(null, null);
                }
            }

            void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
            {
                Log("EditorEventsExecutor.OnPreprocessBuild");
                ExecuteAttribute<EditorEvents.OnPreprocessBuildAttribute>(report);
            }

            void IPostprocessBuildWithReport.OnPostprocessBuild(BuildReport report)
            {
                Log("EditorEventsExecutor.OnPostprocessBuild");
                ExecuteAttribute<EditorEvents.OnPostprocessBuildAttribute>(report);
            }

            void IProcessSceneWithReport.OnProcessScene(Scene scene, BuildReport report)
            {
                ExecuteAttribute<EditorEvents.OnProcessSceneAttribute>(scene, report);

                if (Application.isPlaying == false && (Application.isBatchMode || BuildPipeline.isBuildingPlayer))
                {
                    ExecuteAttribute<EditorEvents.OnProcessSceneBuildAttribute>(scene, report);
                }
            }

#if UNITY_ANDROID
            void UnityEditor.Android.IPostGenerateGradleAndroidProject.OnPostGenerateGradleAndroidProject(string gradlePath)
            {
                ExecuteAttribute<EditorEvents.OnPostGenerateGradleAndroidProjectAttribute>(gradlePath);
            }
#endif

            private static void PlayModeStateChanged(PlayModeStateChange state)
            {
                if (EditorApplication.isPlaying == false && EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    ExecuteAttribute<EditorEvents.OnEnterPlayModeAttribute>();
                }
                else if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    ExecuteAttribute<EditorEvents.OnExitingPlayModeAttribute>();
                    EditorApplication.delayCall += WaitForPlayModeExit;
                }
            }

            private static void WaitForPlayModeExit()
            {
                if (Application.isPlaying)
                {
                    EditorApplication.delayCall += WaitForPlayModeExit;
                }
                else
                {
                    ExecuteAttribute<EditorEvents.OnExitPlayModeAttribute>();
                }
            }

            private static List<MethodInfo> FindEditorEventMethods()
            {
                var results = new List<MethodInfo>();
                var typeCacheStart = DateTime.Now;

                foreach (var method in TypeCache.GetMethodsWithAttribute<EditorEvents.EditorEvent>())
                {
                    results.Add(method);
                }

                LogFormat(
                    "Searching all DLLs for EditorEvent Attributes found {0} attributes and took {1} milliseconds",
                    results.Count,
                    DateTime.Now.Subtract(typeCacheStart).TotalMilliseconds);

                return results;
            }

            private static void Log(string message) => LogFormat(message, null);

            private static void LogFormat(string format, params object[] args)
            {
                if (DisableLogging == false || Application.isBatchMode || BuildPipeline.isBuildingPlayer)
                {
                    Debug.LogFormat(format, args);
                }
            }
        }
#endif
    }
}
