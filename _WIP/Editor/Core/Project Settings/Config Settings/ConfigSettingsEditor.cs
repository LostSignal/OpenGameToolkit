//-----------------------------------------------------------------------
// <copyright file="ConfigSettingsEditor.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Settings
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(ConfigSettings))]
    public class ConfigSettingsEditor : OGT.Editor
    {
        private List<string> parentNames = new List<string>();
        private ISettingsFile selectedSettingsFile;
        private float totalWidth;

        private SettingsFileCollection SettingsFiles => ConfigSettings.Instance.SettingsFiles;

        protected override void OnEnable()
        {
            base.OnEnable();

            this.selectedSettingsFile = SettingsFiles.GetDefaultSettingFile();
        }

        protected override void NewOnInspectorGUI()
        {
            var settings = this.target as ConfigSettings;
            var configs = SettingsFiles.Files;
            var width = EditorGUILayout.GetControlRect().width;

            // HACK [bgish]: For some reason, EditorGUILayout.GetControlRect().width sometimes returns 1, so ignore it!
            if (width != 1)
            {
                this.totalWidth = width;
            }

            var columnPadding = 5;
            var leftColumnWidth = 120;
            var rightColumnWidth = this.totalWidth - leftColumnWidth - columnPadding;
            var height = Screen.height;

            using (new GUILayout.HorizontalScope())
            {
                // ------------------------ Left Side ------------------------
                using (new GUILayout.VerticalScope(GUILayout.Width(leftColumnWidth), GUILayout.Height(height)))
                {
                    GUILayout.Space(4);

                    // https://forum.unity.com/threads/how-to-make-own-list-ui-in-editor-window.461428/
                    Color color_default = GUI.backgroundColor;
                    Color color_selected = Color.gray;
                    GUIStyle itemStyle = new(GUI.skin.button)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        margin = new RectOffset(0, 0, 0, 0),
                    };

                    itemStyle.active.background = itemStyle.normal.background;

                    foreach (var config in configs.OrderBy(x => SettingsFiles.GetFullName(x)))
                    {
                        GUI.backgroundColor = (config == selectedSettingsFile) ? color_selected : Color.clear;

                        StringBuilder depthString = new();

                        for (int i = 0; i < 1 + SettingsFiles.GetDepth(config) * 3; i++)
                        {
                            depthString.Append(" ");
                        }

                        if (GUILayout.Button(depthString + config.Name + (SettingsFiles.IsDefaultSettingsFile(config) ? " (Default)" : string.Empty), itemStyle))
                        {
                            selectedSettingsFile = config;

                            // NOTE [bgish]: If the user has the content text focuses, it will not update when switching configs, so this fixes that
                            GUI.FocusControl(null);
                        }

                        // This is to avoid affecting other GUIs outside of the list
                        GUI.backgroundColor = color_default;
                    }
                }

                // ------------------------ Right Side ------------------------
                this.BoxArea(() =>
                {
                    GUILayout.Space(columnPadding);

                    using (new GUILayout.VerticalScope())
                    {
                        using (new LabelWidthScope(220))
                        {
                            DrawConfig(selectedSettingsFile);
                        }
                    }
                }, GUILayout.Height(height - 120));
            }
        }

        protected override void OnGUIChanged()
        {
            base.OnGUIChanged();

            ConfigSettings.Instance.Save();
        }

        private void DrawConfig(ISettingsFile settingsFile)
        {
            if (settingsFile == null)
            {
                return;
            }

            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.TextField("Id", settingsFile.Id);
            }

            settingsFile.Name = EditorGUILayout.TextField("Name", settingsFile.Name);

            DrawParentDropdown();

            settingsFile.IsSelectable = EditorGUILayout.Toggle("Is Selectable", settingsFile.IsSelectable);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(20);

                if (GUILayout.Button("Set As Default", GUILayout.Width(120)))
                {
                    ConfigSettings.Instance.SettingsFiles.SetDefaultSettingFile(settingsFile);
                }
            }


            EditorGUILayout.Space();
            EditorGUILayout.Space();

            settingsFile.Content = EditorGUILayout.TextArea(settingsFile.Content);

            if (GUILayout.Button("Update"))
            {
                ConfigSettingsExecuter.ApplySettings();
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            void DrawParentDropdown()
            {
                // Showing the Parent Drop Down
                this.SettingsFiles.GetAvailableParentNames(settingsFile, this.parentNames);

                for (int i = 0; i < this.parentNames.Count; i++)
                {
                    this.parentNames[i] = this.parentNames[i].Replace("/", " \u2215 ");
                }

                var newParentIndex = EditorGUILayout.Popup("Parent", -1, parentNames.ToArray());

                if (newParentIndex != -1)
                {
                    Debug.Log("PARENT CHANGE!");

                    // this.SettingsFiles.
                }

                // settingsFile.ParentId = EditorGUILayout.TextField("Parent Id", settingsFile.ParentId);
            }
        }

        [SettingsProvider]
        private static SettingsProvider CreateLostLibrarySettingsProvider() =>
            SettingsProviderUtil.GetSettingsProvider(ConfigSettings.Instance, "Project/Open Game Toolkit/Config Settings");
    }
}
