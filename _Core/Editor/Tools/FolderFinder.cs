//-----------------------------------------------------------------------
// <copyright file="FolderFinder.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public static class FolderFinder
    {
        private static readonly OGTLogger Logger = OGTLogger.OGTEditor;

        [MenuItem("Tools/OGT/Folders/Show Editor Logs", priority = MenuItemPriorities.Folders + 0)]
        public static void OpenEditorLogs()
        {
            if (UnityPlatformProvider.CurrentEditorPlatform == EditorPlatform.Windows)
            {
                EditorUtility.RevealInFinder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Unity", "Editor", "Editor.log"));
            }
            else if (UnityPlatformProvider.CurrentEditorPlatform == EditorPlatform.Mac)
            {
                EditorUtility.RevealInFinder("~/Library/Logs/Unity/Editor.log");
            }
            else
            {
                Logger.LogError("Unable to open Editor Log...  Unknown Platform.");
            }
        }

        [MenuItem("Tools/OGT/Folders/Show Player Log", priority = MenuItemPriorities.Folders + 1)]
        public static void OpenPlayerLog()
        {
            if (UnityPlatformProvider.CurrentEditorPlatform == EditorPlatform.Windows)
            {
                string appDataRootPath = Path.GetDirectoryName(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)));
                EditorUtility.RevealInFinder(Path.Combine(appDataRootPath, "LocalLow", PlayerSettings.companyName, PlayerSettings.productName, "Player.log"));
            }
            else if (UnityPlatformProvider.CurrentEditorPlatform == EditorPlatform.Mac)
            {
                EditorUtility.RevealInFinder("~/Library/Logs/Unity/Player.log");
            }
            else if (UnityPlatformProvider.CurrentEditorPlatform == EditorPlatform.Linux)
            {
                EditorUtility.RevealInFinder(Path.Combine("~", ".config", "unity3d", PlayerSettings.companyName, PlayerSettings.productName, "Player.log"));
            }
            else
            {
                Logger.LogError("Unable to open Editor Log...  Unknown Platform.");
            }
        }

        [MenuItem("Tools/OGT/Folders/Show Persistent Data Path", priority = MenuItemPriorities.Folders + 2)]
        public static void OpenPersistentDataPath()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }
    }
}
