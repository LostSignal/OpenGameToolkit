//-----------------------------------------------------------------------
// <copyright file="RuntimeSettings.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public static class RuntimeSettings
    {
        private static readonly string SettingsFileName = "ogt-runtime-settings.json";
        private static readonly OGTLogger Logger = OGTLogger.OGT;
        private static Dictionary<string, object> Settings = null;

#if UNITY_6000_0_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Settings = null;
#endif

        public static bool IsOgtDevelopmentBuild
        {
            get => GetSetting("OGT.IsOgtDevelopmentBuild", true);
            set => SetSetting("OGT.IsOgtDevelopmentBuild", value);
        }

        public static string SourceControlBranchName
        {
            get => GetSetting("OGT.SourceControlBranchName", string.Empty);
            set => SetSetting("OGT.SourceControlBranchName", value);
        }

        public static string SourceControlChangeset
        {
            get => GetSetting("OGT.SourceControlChangeset", string.Empty);
            set => SetSetting("OGT.SourceControlChangeset", value);
        }

        public static string BuildProfileName
        {
            get => GetSetting("OGT.BuildProfileName", string.Empty);
            set => SetSetting("OGT.BuildProfileName", value);
        }

        public static T GetSetting<T>(string settingsName, T defaultValue = default)
        {
            LoadSettings();

            if (Settings.TryGetValue(settingsName, out object value))
            {
                if (value is T)
                {
                    return (T)value;
                }
                else
                {
                    Logger.LogError($"RuntimeSettings Type Mismatch for '{settingsName}'!  Found '{value?.GetType()?.Name}' and expected '{typeof(T).Name}'. Returning default value.");
                    return defaultValue;
                }
            }

            return defaultValue;
        }

        public static void SetSetting<T>(string settingsName, T value)
        {
            if (Application.isEditor == false)
            {
                Logger.LogError($"RuntimeSettings.SetSettings {settingsName} being called when not in Editor!");
                return;
            }

            LoadSettings();
            Settings.AddOrOverwrite(settingsName, value);
            SaveSettings();
        }

        private static void LoadSettings()
        {
            if (Settings != null)
            {
                return;
            }

            string fileText = null;

            if (Application.isEditor)
            {
                var editorFilePath = GetRuntimeSettingsEditorFilePath();

                if (System.IO.File.Exists(editorFilePath))
                {
                    fileText = System.IO.File.ReadAllText(editorFilePath);
                }
            }
            else
            {
                var settingsFileResourcePath = System.IO.Path.GetFileNameWithoutExtension(SettingsFileName);
                fileText = Resources.Load<TextAsset>(settingsFileResourcePath)?.text;
            }

            if (string.IsNullOrEmpty(fileText) == false)
            {
                Settings = JsonUtil.Deserialize<Dictionary<string, object>>(fileText, true);

                if (Settings == null)
                {
                    Logger.LogError($"Unable to Deserialize RuntimeSettings json!\n{fileText}");
                }
            }

            Settings ??= new Dictionary<string, object>();
        }

        private static void SaveSettings()
        {
            var outputFilePath = GetRuntimeSettingsEditorFilePath();
            var outputDirectory = System.IO.Path.GetDirectoryName(outputFilePath);

            if (System.IO.Directory.Exists(outputDirectory) == false)
            {
                System.IO.Directory.CreateDirectory(outputDirectory);
            }

            System.IO.File.WriteAllText(GetRuntimeSettingsEditorFilePath(), JsonUtil.Serialize(Settings, true));
        }

        private static string GetRuntimeSettingsEditorFilePath()
        {
            var projectSettingsGeneral = System.IO.File.ReadAllText("ProjectSettings/ProjectSettingsGeneral.asset");
            var searchString = "\"generatedOutputDirectory\": \"";
            var startIndex = projectSettingsGeneral.IndexOf(searchString) + searchString.Length;
            var endIndex = projectSettingsGeneral.IndexOf("\"", startIndex);
            var generatedOutputDirectory = projectSettingsGeneral.Substring(startIndex, endIndex - startIndex);
            var fullPath = System.IO.Path.Combine(generatedOutputDirectory, "Resources", SettingsFileName);
            fullPath = fullPath.Replace("\\", "/");

            if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(fullPath)) == false)
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullPath));
            }

            return fullPath;
        }

#if UNITY_EDITOR

        [UnityEditor.MenuItem("Tools/OGT/Is Development Build", true)]
        private static bool IsOgtDevelopmentBuildValidate()
        {
            Menu.SetChecked("Tools/OGT/Is Development Build", RuntimeSettings.IsOgtDevelopmentBuild);
            return true;
        }

        [UnityEditor.MenuItem("Tools/OGT/Is Development Build", false)]
        private static void IsOgtDevelopmentBuildMenuItem()
        {
            RuntimeSettings.IsOgtDevelopmentBuild = !RuntimeSettings.IsOgtDevelopmentBuild;
        }

#endif
    }
}
