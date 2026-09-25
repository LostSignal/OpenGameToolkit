//-----------------------------------------------------------------------
// <copyright file="SecretsEditorWindow.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public class SecretsEditorWindow : EditorWindow
    {
        private const string WindowTitle = "OGT Secrets";
        private const float RemoveButtonWidth = 22f;
        private const float KeyColumnWidth = 220f;

        private readonly List<Row> rows = new List<Row>();
        private Vector2 scrollPosition;
        private bool showValues;
        private bool isDirty;

        [MenuItem("Tools/OGT/Edit Secrets", priority = MenuItemPriorities.Tools + 10)]
        public static void Open()
        {
            var window = GetWindow<SecretsEditorWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(450, 200);
            window.Show();
        }

        private void OnEnable()
        {
            this.Reload();
        }

        private void OnGUI()
        {
            this.UpdateTitle();
            this.DrawToolbar();

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField(SecretsStore.SecretsFilePath, EditorStyles.miniLabel);

            this.DrawHeader();

            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);

            for (int i = 0; i < this.rows.Count; i++)
            {
                this.DrawRow(i);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(4);

            if (GUILayout.Button("Add Secret"))
            {
                this.rows.Add(new Row());
                this.isDirty = true;
                GUI.FocusControl(null);
            }

            this.DrawValidation();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            this.showValues = GUILayout.Toggle(this.showValues, "Show Values", EditorStyles.toolbarButton, GUILayout.Width(90));

            GUILayout.FlexibleSpace();

            using (new EditorGUI.DisabledScope(this.isDirty == false))
            {
                if (GUILayout.Button("Revert", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    GUI.FocusControl(null);
                    this.Reload();
                }

                using (new EditorGUI.DisabledScope(this.GetValidationError() != null))
                {
                    if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(60)))
                    {
                        GUI.FocusControl(null);
                        this.Save();
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Key", EditorStyles.boldLabel, GUILayout.Width(KeyColumnWidth));
            EditorGUILayout.LabelField("Value", EditorStyles.boldLabel);
            GUILayout.Space(RemoveButtonWidth + 4);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRow(int index)
        {
            var row = this.rows[index];

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();

            row.Key = EditorGUILayout.TextField(row.Key, GUILayout.Width(KeyColumnWidth));

            row.Value = this.showValues
                ? EditorGUILayout.TextField(row.Value)
                : EditorGUILayout.PasswordField(row.Value);

            if (EditorGUI.EndChangeCheck())
            {
                this.isDirty = true;
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Toolbar Minus", "Remove Secret"), GUILayout.Width(RemoveButtonWidth)))
            {
                this.rows.RemoveAt(index);
                this.isDirty = true;
                GUI.FocusControl(null);
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawValidation()
        {
            var error = this.GetValidationError();

            if (error != null)
            {
                EditorGUILayout.HelpBox(error, MessageType.Error);
            }
            else if (this.isDirty)
            {
                EditorGUILayout.HelpBox("You have unsaved changes.", MessageType.Warning);
            }
        }

        private string GetValidationError()
        {
            if (this.rows.Any(r => string.IsNullOrWhiteSpace(r.Key)))
            {
                return "Every secret needs a key.";
            }

            var duplicateKey = this.rows
                .GroupBy(r => r.Key.Trim())
                .FirstOrDefault(g => g.Count() > 1)?
                .Key;

            if (duplicateKey != null)
            {
                return $"Duplicate key '{duplicateKey}'.";
            }

            return null;
        }

        private void Reload()
        {
            this.rows.Clear();

            foreach (var key in SecretsStore.GetKeys())
            {
                this.rows.Add(new Row { Key = key, Value = SecretsStore.GetValue(key) });
            }

            this.isDirty = false;
        }

        private void Save()
        {
            SecretsStore.ReplaceAll(this.rows.Select(r => new KeyValuePair<string, string>(r.Key.Trim(), r.Value)));
            SecretsStore.Save();

            this.isDirty = false;
        }

        private void UpdateTitle()
        {
            var title = this.isDirty ? $"{WindowTitle}*" : WindowTitle;

            if (this.titleContent.text != title)
            {
                this.titleContent = new GUIContent(title);
            }
        }

        private class Row
        {
            public string Key = string.Empty;
            public string Value = string.Empty;
        }
    }
}
