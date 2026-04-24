//-----------------------------------------------------------------------
// <copyright file="LostSettingsLogListener.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.IO;
    using UnityEngine;

    public static class AutoFixLineEndings
    {
        private static readonly OGTLogger Logger = OGTLogger.OGTEditor;

        [EditorEvents.InitializeOnLoad]
        private static void RegisterForMessagedReceivedCallback()
        {
            // Listen for logs about inconsistent line endings
            Application.logMessageReceived += OnLogMessageReceived;
        }

        private static void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (condition.StartsWith("There are inconsistent line endings in the") && ProjectSettingsEditorTools.Instance.AutomaticallyFixLineEndingMismatches)
            {
                FixFile(condition);
            }
        }

        private static void FixFile(string condition)
        {
            if (ProjectSettingsEditorTools.Instance.AutomaticallyFixLineEndingMismatches == false)
            {
                return;
            }

            int startIndex = condition.IndexOf("'") + 1;
            int endIndex = condition.IndexOf("' script. Some are");

            if (startIndex > 1 && endIndex > 0)
            {
                string filePath = condition.Substring(startIndex, endIndex - startIndex);
                string fullFilePath = Path.GetFullPath(filePath).Replace("\\", "/");

                if (PackageCacheUtil.IsInPackageCache(fullFilePath) == false)
                {
                    string fileText = File.ReadAllText(fullFilePath);
                    Logger.Log($"Fixed line endings for file {fullFilePath}");
                    FileUtil.UpdateFile(FileUtil.ConvertLineEndings(fileText), fullFilePath, true);
                }
            }
        }
    }
}
