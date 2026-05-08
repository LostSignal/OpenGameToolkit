//-----------------------------------------------------------------------
// <copyright file="ConfigSettingsExecuter.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Settings
{
    using System.Collections.Generic;
    using UnityEditor.Build.Reporting;
    using UnityEditor;

    public static class ConfigSettingsExecuter
    {
        public static void ApplySettings()
        {
            // Applying Settings
            foreach (var setting in GetEditorSettings())
            {
                setting.ApplySettings();
            }
        }

        [EditorEvents.InitializeOnLoad]
        private static void InitializeOnLoad()
        {
            // Registering Build Player Options
            BuildPlayerWindow.RegisterGetBuildPlayerOptionsHandler(BuildPlayerOptionsHandler);

            ApplySettings();

            static BuildPlayerOptions BuildPlayerOptionsHandler(BuildPlayerOptions options)
            {
                options = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(options);

                foreach (var setting in GetEditorSettings())
                {
                    options = setting.ApplyBuildPlayerOptions(options);
                }

                return options;
            }
        }

        [EditorEvents.OnPostGenerateGradleAndroidProject]
        private static void OnPostGenerateGradleAndroidProject(string gradlePath)
        {
            foreach (var setting in GetEditorSettings())
            {
                setting.ApplySettingsPostAndroidBuild(gradlePath);
            }
        }

        [EditorEvents.OnPostprocessBuild]
        private static void OnPostprocessBuild(BuildReport buildReport)
        {
            foreach (var setting in GetEditorSettings())
            {
                setting.ApplySettingsPostBuild(buildReport);
            }
        }

        [EditorEvents.OnEnterPlayMode]
        private static void OnEnterPlayMode()
        {
            foreach (var setting in GetEditorSettings())
            {
                setting.ApplySettingsOnEnterPlayMode();
            }
        }

        [EditorEvents.OnPreprocessBuild]
        private static void OnBuildStared()
        {
            foreach (var setting in GetEditorSettings())
            {
                setting.ApplySettingOnBuildStarted();
            }
        }

        private static IEnumerable<Settings> GetEditorSettings()
        {
            var activeConfigSettings = ConfigSettings.Instance.GetActiveSettingsFileObjects();

            if (activeConfigSettings == null || activeConfigSettings.Count == 0)
            {
                yield break;
            }

            foreach (var settings in activeConfigSettings)
            {
                if (settings is Settings editorSettings)
                {
                    yield return editorSettings;
                }
            }
        }
    }
}
