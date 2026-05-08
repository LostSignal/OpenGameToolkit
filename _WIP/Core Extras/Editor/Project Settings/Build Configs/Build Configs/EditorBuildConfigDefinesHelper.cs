//-----------------------------------------------------------------------
// <copyright file="EditorBuildConfigDefinesHelper.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Build;

    public static class EditorBuildConfigDefinesHelper
    {
        public static void UpdateProjectDefines()
        {
            //// if (ProjectSettingsBuildConfigs.Instance.UsingBuildConfigs == false ||
            ////     ProjectSettingsBuildConfigs.Instance.BuildConfigs == null ||
            ////     ProjectSettingsBuildConfigs.Instance.ActiveBuildConfig == null)
            //// {
            ////     return;
            //// }
            ////
            //// HashSet<string> activeDefines = new();
            //// HashSet<string> definesToRemove = new();
            ////
            //// GetActiveDefines(ProjectSettingsBuildConfigs.Instance.ActiveBuildConfig, activeDefines);
            //// GetAllDefines(ProjectSettingsBuildConfigs.Instance.BuildConfigs, definesToRemove);
            ////
            //// foreach (var define in activeDefines)
            //// {
            ////     definesToRemove.Remove(define);
            //// }
            ////
            //// UpdateProjectDefines(activeDefines, definesToRemove);
        }

        public static void UpdateProjectDefines(HashSet<string> definesToAdd, HashSet<string> definesToRemove)
        {
            //// foreach (var namedBuildTarget in BuildTargetGroupUtil.GetValid())
            //// {
            ////     string currentDefinesString = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            ////     string definesString = GetDefinesString(namedBuildTarget, definesToAdd, definesToRemove);
            ////
            ////     if (currentDefinesString != definesString)
            ////     {
            ////         PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, definesString);
            ////     }
            //// }
        }

        //// private static void GetActiveDefines(BuildConfig buildConfig, HashSet<string> defines)
        //// {
        ////     if (buildConfig != null)
        ////     {
        ////         if (buildConfig.Defines != null)
        ////         {
        ////             foreach (var define in buildConfig.Defines)
        ////             {
        ////                 defines.Add(define);
        ////             }
        ////         }
        ////
        ////         GetActiveDefines(buildConfig.Parent, defines);
        ////     }
        //// }
        ////
        //// private static void GetAllDefines(List<BuildConfig> buildConfigs, HashSet<string> defines)
        //// {
        ////     foreach (var buildConfig in buildConfigs)
        ////     {
        ////         if (buildConfig != null && buildConfig.Defines != null)
        ////         {
        ////             foreach (var define in buildConfig.Defines)
        ////             {
        ////                 defines.Add(define);
        ////             }
        ////         }
        ////     }
        //// }
        ////
        //// private static string GetDefinesString(NamedBuildTarget namedBuildTarget, HashSet<string> definesToAdd, HashSet<string> definesToRemove)
        //// {
        ////     var currentDefines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget).Split(';').ToList();
        ////
        ////     foreach (var define in definesToAdd)
        ////     {
        ////         if (currentDefines.Contains(define) == false)
        ////         {
        ////             currentDefines.Add(define);
        ////         }
        ////     }
        ////
        ////     foreach (var define in definesToRemove)
        ////     {
        ////         if (currentDefines.Contains(define))
        ////         {
        ////             currentDefines.Remove(define);
        ////         }
        ////     }
        ////
        ////     currentDefines.Sort();
        ////
        ////     return string.Join(";", currentDefines);
        //// }
    }
}
