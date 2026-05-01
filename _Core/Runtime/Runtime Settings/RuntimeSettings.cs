//-----------------------------------------------------------------------
// <copyright file="RuntimeSettings.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
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

            string fileText = Application.isEditor ?
                System.IO.File.ReadAllText(GetRuntimeSettingsEditorFilePath()) :
                Resources.Load<TextAsset>(SettingsFileName).text;

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

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Test/Show Runtime Settings File Path")]
#endif
        private static void PrintRuntimeSettingsEditorFilePath()
        {
            Debug.Log(GetRuntimeSettingsEditorFilePath());
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

            return fullPath;
        }
    }
}
