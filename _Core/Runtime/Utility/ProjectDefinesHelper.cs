#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="ProjectDefinesHelper.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using OGT;

    #if UNITY_EDITOR
    using UnityEditor;
    #endif

    public static class ProjectDefinesHelper
    {
        private static readonly HashSet<int> BuildTargetsToIgnore = new HashSet<int>
        {
            0,  // Unknown
            2,  // WebPlayer
            5,  // PS3
            6,  // XBOX360
            15, // WP8
            16, // BlackBerry
            17, // Tizen
            18, // PSP2
            20, // PSM
            22, // SamsungTV
            23, // N3DS
            24, // WiiU
            26, // Facebook
            28, // Lumin
            29, // Stadia
        };

        public static void AddOrRemoveDefine(string assemblyName, string define)
        {
            #if UNITY_EDITOR

            bool typeExists = AppDomain.CurrentDomain.GetAssemblies().Any(assembly =>
            {
                int endIndex = assembly.FullName.IndexOf(", Version=");
                return assembly.FullName.Substring(0, endIndex) == assemblyName;
            });

            if (typeExists)
            {
                ProjectDefinesHelper.AddDefineToProject(define);
            }
            else
            {
                ProjectDefinesHelper.RemoveDefineFromProject(define);
            }

            #endif
        }

        public static void AddDefineToProject(string defineToAdd)
        {
            #if UNITY_EDITOR

            var buildTargetGroups = (BuildTargetGroup[])Enum.GetValues(typeof(BuildTargetGroup));

            foreach (var buildTargetGroup in buildTargetGroups)
            {
                if (BuildTargetsToIgnore.Contains((int)buildTargetGroup))
                {
                    continue;
                }

                string currentDefinesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
                string newDefinesString = GetNewDefinesString(buildTargetGroup);

                if (currentDefinesString != newDefinesString)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefinesString);
                }
            }

            EditorUtil.SaveProject();

            string GetNewDefinesString(BuildTargetGroup buildTargetGroup)
            {
                var defines = GetBuildTargetDefines(buildTargetGroup);
                defines.AddIfUnique(defineToAdd);
                return string.Join(";", defines);
            }

            #endif
        }

        public static void RemoveDefineFromProject(string defineToRemove)
        {
            #if UNITY_EDITOR

            var buildTargetGroups = (BuildTargetGroup[])Enum.GetValues(typeof(BuildTargetGroup));
            bool saveNeeded = false;

            foreach (var buildTargetGroup in buildTargetGroups)
            {
                if (BuildTargetsToIgnore.Contains((int)buildTargetGroup))
                {
                    continue;
                }

                string currentDefinesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
                string newDefinesString = GetNewDefinesString(buildTargetGroup);

                if (currentDefinesString != newDefinesString)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefinesString);
                    saveNeeded = true;
                }
            }

            if (saveNeeded)
            {
                AssetDatabase.SaveAssets();
                EditorApplication.ExecuteMenuItem("File/Save Project");
            }

            string GetNewDefinesString(BuildTargetGroup buildTargetGroup)
            {
                var defines = GetBuildTargetDefines(buildTargetGroup);
                defines.Remove(defineToRemove);

                return defines.Count == 0 ? string.Empty :
                    defines.Count == 1 ? defines[0] :
                    string.Join(";", defines);
            }

            #endif
        }

        #if UNITY_EDITOR

        private static List<string> GetBuildTargetDefines(BuildTargetGroup buildTargetGroup)
        {
            var currentDefinesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(currentDefinesString))
            {
                // Do nothing, there are no defines
            }
            else if (currentDefinesString.Contains(';') == false)
            {
                result.Add(currentDefinesString);
            }
            else
            {
                result.AddRange(currentDefinesString
                    .Split(';')
                    .Where(x => string.IsNullOrWhiteSpace(x) == false)
                    .Select(x => x.Trim()));
            }

            return result;
        }

        #endif
    }
}
