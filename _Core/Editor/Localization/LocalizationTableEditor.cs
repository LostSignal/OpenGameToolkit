//-----------------------------------------------------------------------
// <copyright file="LocalizationTableEditor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(LocalizationTable))]
    public class LocalizationTableEditor : UnityEditor.Editor
    {
        private string searchFilter = "";
        private string newKey = "";
        private string newValue = "";
        private Vector2 scrollPosition;
        private bool showAddSection = false;

        public override void OnInspectorGUI()
        {
            LocalizationTable table = (LocalizationTable)target;

            // Table ID field
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Table ID:", GUILayout.Width(60));
            string newTableId = EditorGUILayout.TextField(table.EditorTableId);
            if (newTableId != table.EditorTableId)
            {
                Undo.RecordObject(table, "Change Table ID");
                table.EditorTableId = newTableId;
                EditorUtility.SetDirty(table);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Search box
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            this.searchFilter = EditorGUILayout.TextField(this.searchFilter);
            if (GUILayout.Button("Clear", GUILayout.Width(50)))
            {
                this.searchFilter = "";
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Add new entry section
            this.showAddSection = EditorGUILayout.Foldout(this.showAddSection, "Add New Entry", true);
            if (this.showAddSection)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                this.newKey = EditorGUILayout.TextField("Key:", this.newKey);
                EditorGUILayout.LabelField("Value:");
                this.newValue = EditorGUILayout.TextArea(this.newValue, GUILayout.MinHeight(40));

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUI.enabled = !string.IsNullOrEmpty(this.newKey);
                if (GUILayout.Button("Add Entry", GUILayout.Width(100)))
                {
                    Undo.RecordObject(table, "Add Localization Entry");
                    table.AddEntry(this.newKey, this.newValue);
                    EditorUtility.SetDirty(table);
                    this.newKey = "";
                    this.newValue = "";
                    GUI.FocusControl(null);
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(10);

            // Entries list header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Entries ({table.Entries.Count})", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open in Window", GUILayout.Width(120)))
            {
                LocalizationTableWindow.OpenWindow(table);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Entries list
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition, GUILayout.MaxHeight(400));

            string filterLower = this.searchFilter?.ToLowerInvariant() ?? "";
            string keyToRemove = null;

            foreach (var entry in table.Entries)
            {
                // Apply filter
                if (!string.IsNullOrEmpty(filterLower))
                {
                    bool matchesKey = entry.Key?.ToLowerInvariant().Contains(filterLower) ?? false;
                    bool matchesValue = entry.Value?.ToLowerInvariant().Contains(filterLower) ?? false;
                    if (!matchesKey && !matchesValue)
                        continue;
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Key row
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Key:", GUILayout.Width(40));
                EditorGUILayout.SelectableLabel(entry.Key, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));

                if (GUILayout.Button("×", GUILayout.Width(22), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                {
                    if (EditorUtility.DisplayDialog("Remove Entry", $"Remove '{entry.Key}'?", "Remove", "Cancel"))
                    {
                        keyToRemove = entry.Key;
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Value row
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Value:", GUILayout.Width(40));
                string newVal = EditorGUILayout.TextArea(entry.Value ?? "", GUILayout.MinHeight(20));
                if (newVal != entry.Value)
                {
                    Undo.RecordObject(table, "Edit Localization Value");
                    table.UpdateEntry(entry.Key, newVal);
                    EditorUtility.SetDirty(table);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();

            // Handle removal after iteration
            if (keyToRemove != null)
            {
                Undo.RecordObject(table, "Remove Localization Entry");
                table.RemoveEntry(keyToRemove);
                EditorUtility.SetDirty(table);
            }
        }
    }

    public class LocalizationTableWindow : EditorWindow
    {
        private LocalizationTable table;
        private string searchFilter = "";
        private string newKey = "";
        private string newValue = "";
        private Vector2 scrollPosition;

        public static void OpenWindow(LocalizationTable table)
        {
            var window = GetWindow<LocalizationTableWindow>("Localization Table");
            window.table = table;
            window.Show();
        }

        public static void OpenWindowByTableId(string tableId)
        {
            var table = LocalizationTable.GetTableById(tableId);
            if (table != null)
            {
                OpenWindow(table);
            }
        }

        private void OnGUI()
        {
            if (this.table == null)
            {
                EditorGUILayout.HelpBox("No Localization Table selected.", MessageType.Info);

                this.table = EditorGUILayout.ObjectField("Table:", this.table, typeof(LocalizationTable), false) as LocalizationTable;
                return;
            }

            // Table selector
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Table:", GUILayout.Width(40));
            this.table = EditorGUILayout.ObjectField(this.table, typeof(LocalizationTable), false) as LocalizationTable;
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeObject = this.table;
                EditorGUIUtility.PingObject(this.table);
            }
            EditorGUILayout.EndHorizontal();

            // Table ID display/edit
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Table ID:", GUILayout.Width(60));
            string newTableId = EditorGUILayout.TextField(this.table.EditorTableId);
            if (newTableId != this.table.EditorTableId)
            {
                Undo.RecordObject(this.table, "Change Table ID");
                this.table.EditorTableId = newTableId;
                EditorUtility.SetDirty(this.table);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Search box
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            this.searchFilter = EditorGUILayout.TextField(this.searchFilter);
            if (GUILayout.Button("Clear", GUILayout.Width(50)))
            {
                this.searchFilter = "";
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Add new entry
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Add New Entry", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Key:", GUILayout.Width(40));
            this.newKey = EditorGUILayout.TextField(this.newKey);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Value:", GUILayout.Width(40));
            this.newValue = EditorGUILayout.TextField(this.newValue);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.enabled = !string.IsNullOrEmpty(this.newKey);
            if (GUILayout.Button("+ Add", GUILayout.Width(80)))
            {
                Undo.RecordObject(this.table, "Add Localization Entry");
                this.table.AddEntry(this.newKey, this.newValue);
                EditorUtility.SetDirty(this.table);
                this.newKey = "";
                this.newValue = "";
                GUI.FocusControl(null);
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Entries header
            EditorGUILayout.LabelField($"Entries ({this.table.Entries.Count})", EditorStyles.boldLabel);

            // Entries list
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);

            string filterLower = this.searchFilter?.ToLowerInvariant() ?? "";
            string keyToRemove = null;

            foreach (var entry in this.table.Entries)
            {
                // Apply filter
                if (!string.IsNullOrEmpty(filterLower))
                {
                    bool matchesKey = entry.Key?.ToLowerInvariant().Contains(filterLower) ?? false;
                    bool matchesValue = entry.Value?.ToLowerInvariant().Contains(filterLower) ?? false;
                    if (!matchesKey && !matchesValue)
                        continue;
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Key:", GUILayout.Width(40));
                EditorGUILayout.SelectableLabel(entry.Key, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                if (GUILayout.Button("×", GUILayout.Width(22)))
                {
                    if (EditorUtility.DisplayDialog("Remove Entry", $"Remove '{entry.Key}'?", "Remove", "Cancel"))
                    {
                        keyToRemove = entry.Key;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Value:", GUILayout.Width(40));
                string newVal = EditorGUILayout.TextArea(entry.Value ?? "", GUILayout.MinHeight(20));
                if (newVal != entry.Value)
                {
                    Undo.RecordObject(this.table, "Edit Localization Value");
                    this.table.UpdateEntry(entry.Key, newVal);
                    EditorUtility.SetDirty(this.table);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();

            if (keyToRemove != null)
            {
                Undo.RecordObject(this.table, "Remove Localization Entry");
                this.table.RemoveEntry(keyToRemove);
                EditorUtility.SetDirty(this.table);
            }
        }
    }
}
