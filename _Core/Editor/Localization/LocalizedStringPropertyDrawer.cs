//-----------------------------------------------------------------------
// <copyright file="LocalizedStringPropertyDrawer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(LocalizedString))]
    public class LocalizedStringPropertyDrawer : PropertyDrawer
    {
        private const float ButtonWidth = 22f;
        private const float Spacing = 2f;
        private const float IndentWidth = 15f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
            {
                return lineHeight * 3 + Spacing * 2;
            }
            return lineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var tableIdProp = property.FindPropertyRelative("tableId");
            var keyProp = property.FindPropertyRelative("localizedStringId");

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float labelWidth = EditorGUIUtility.labelWidth;

            // Get table and localized value for display
            string currentTableId = tableIdProp.stringValue;
            var table = LocalizationTable.GetTableById(currentTableId);
            string localizedValue = "";
            if (table != null && !string.IsNullOrEmpty(keyProp.stringValue))
            {
                localizedValue = table.GetValue(keyProp.stringValue);
            }

            // Check if value is not set (auto-expand when clicked)
            bool isNotSet = string.IsNullOrEmpty(currentTableId) || string.IsNullOrEmpty(keyProp.stringValue);

            // Line 1: Foldout + Variable name label + read-only localized value preview
            Rect line1 = new Rect(position.x, position.y, position.width, lineHeight);

            // Foldout arrow area
            Rect foldoutRect = new Rect(line1.x, line1.y, IndentWidth, lineHeight);
            Rect labelRect = new Rect(line1.x, line1.y, labelWidth, lineHeight);
            Rect valueRect = new Rect(labelRect.xMax, line1.y, position.width - labelWidth, lineHeight);

            // Draw foldout
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none, true);

            // Draw label
            EditorGUI.LabelField(labelRect, label);

            // Draw value field - if clicked and not set, auto-expand
            GUI.enabled = false;
            EditorGUI.TextField(valueRect, localizedValue);
            GUI.enabled = true;

            // Handle click on value field to auto-expand if not set
            if (isNotSet && Event.current.type == EventType.MouseDown && valueRect.Contains(Event.current.mousePosition))
            {
                property.isExpanded = true;
                Event.current.Use();
            }

            // Only draw Table and Key lines if expanded
            if (property.isExpanded)
            {
                // Line 2: Indented Table label + table dropdown + popout button
                Rect line2 = new Rect(position.x, position.y + lineHeight + Spacing, position.width, lineHeight);
                float indentedLabelWidth = labelWidth - IndentWidth;
                float popoutWidth = ButtonWidth;
                float tableFieldWidth = position.width - labelWidth - popoutWidth - Spacing;

                Rect tableLabelRect = new Rect(line2.x + IndentWidth, line2.y, indentedLabelWidth, lineHeight);
                Rect tableFieldRect = new Rect(tableLabelRect.xMax, line2.y, tableFieldWidth, lineHeight);
                Rect popoutRect = new Rect(tableFieldRect.xMax + Spacing, line2.y, popoutWidth, lineHeight);

                EditorGUI.LabelField(tableLabelRect, "Table");

                // Draw dropdown for table selection
                string displayName = "(None)";
                if (!string.IsNullOrEmpty(currentTableId))
                {
                    displayName = table != null ? table.name : $"[Missing: {currentTableId}]";
                }

                if (EditorGUI.DropdownButton(tableFieldRect, new GUIContent(displayName), FocusType.Keyboard))
                {
                    ShowTableSelectionMenu(tableFieldRect, tableIdProp);
                }

                // Popout button
                GUI.enabled = table != null;
                if (GUI.Button(popoutRect, EditorGUIUtility.IconContent("d_ScaleTool", "Open Table Window")))
                {
                    LocalizationTableWindow.OpenWindow(table);
                }
                GUI.enabled = true;

                // Line 3: Indented Key label + key dropdown + add button
                Rect line3 = new Rect(position.x, position.y + (lineHeight + Spacing) * 2, position.width, lineHeight);
                float addButtonWidth = ButtonWidth;
                float keyFieldWidth = position.width - labelWidth - addButtonWidth - Spacing;

                Rect keyLabelRect = new Rect(line3.x + IndentWidth, line3.y, indentedLabelWidth, lineHeight);
                Rect keyFieldRect = new Rect(keyLabelRect.xMax, line3.y, keyFieldWidth, lineHeight);
                Rect addButtonRect = new Rect(keyFieldRect.xMax + Spacing, line3.y, addButtonWidth, lineHeight);

                EditorGUI.LabelField(keyLabelRect, "Key");

                if (table != null)
                {
                    // Draw dropdown for key selection
                    if (EditorGUI.DropdownButton(keyFieldRect, new GUIContent(keyProp.stringValue ?? "(None)"), FocusType.Keyboard))
                    {
                        ShowKeySelectionMenu(keyFieldRect, table, keyProp);
                    }

                    // Add button
                    if (GUI.Button(addButtonRect, EditorGUIUtility.IconContent("d_Toolbar Plus", "Add New Entry")))
                    {
                        AddNewEntryPopup.Show(addButtonRect, table, keyProp);
                    }
                }
                else
                {
                    GUI.enabled = false;
                    EditorGUI.TextField(keyFieldRect, keyProp.stringValue);
                    GUI.Button(addButtonRect, EditorGUIUtility.IconContent("d_Toolbar Plus"));
                    GUI.enabled = true;
                }
            }

            EditorGUI.EndProperty();
        }

        private void ShowTableSelectionMenu(Rect buttonRect, SerializedProperty tableIdProp)
        {
            var popup = new TableSelectionPopup(tableIdProp);
            PopupWindow.Show(buttonRect, popup);
        }

        private void ShowKeySelectionMenu(Rect buttonRect, LocalizationTable table, SerializedProperty keyProp)
        {
            var popup = new KeySelectionPopup(table, keyProp);
            PopupWindow.Show(buttonRect, popup);
        }
    }

    public class TableSelectionPopup : PopupWindowContent
    {
        private readonly SerializedProperty tableIdProp;
        private string searchFilter = "";
        private Vector2 scrollPosition;
        private List<(string tableId, string friendlyName)> allTables;

        public TableSelectionPopup(SerializedProperty tableIdProp)
        {
            this.tableIdProp = tableIdProp;
            this.allTables = new List<(string tableId, string friendlyName)>();

            // Get all registered tables with their friendly names
            foreach (var table in LocalizationTable.GetAllRegisteredTables())
            {
                this.allTables.Add((table.EditorTableId, table.name));
            }

            this.allTables.Sort((a, b) => string.Compare(a.friendlyName, b.friendlyName, System.StringComparison.OrdinalIgnoreCase));
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(300, 250);
        }

        public override void OnGUI(Rect rect)
        {
            // Search field
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            GUI.SetNextControlName("TableSearchField");
            this.searchFilter = EditorGUILayout.TextField(this.searchFilter);
            EditorGUILayout.EndHorizontal();

            // Focus search field on open
            if (Event.current.type == EventType.Repaint && string.IsNullOrEmpty(this.searchFilter))
            {
                EditorGUI.FocusTextInControl("TableSearchField");
            }

            EditorGUILayout.Space(5);

            // None option
            if (GUILayout.Button("(None)", EditorStyles.miniButton))
            {
                this.tableIdProp.stringValue = "";
                this.tableIdProp.serializedObject.ApplyModifiedProperties();
                editorWindow.Close();
            }

            EditorGUILayout.Space(2);

            if (this.allTables.Count == 0)
            {
                EditorGUILayout.HelpBox("No localization tables registered. Create a LocalizationTable asset and assign it a Table ID.", MessageType.Info);
                return;
            }

            // Filtered list
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);

            string filterLower = this.searchFilter?.ToLowerInvariant() ?? "";

            foreach (var(tableId, friendlyName) in this.allTables)
            {
                // Filter by friendly name
                if (!string.IsNullOrEmpty(filterLower) && !friendlyName.ToLowerInvariant().Contains(filterLower))
                    continue;

                bool isSelected = tableId == this.tableIdProp.stringValue;
                var style = isSelected ? EditorStyles.boldLabel : EditorStyles.label;

                EditorGUILayout.BeginHorizontal();

                if (isSelected)
                {
                    EditorGUILayout.LabelField("►", GUILayout.Width(15));
                }
                else
                {
                    EditorGUILayout.LabelField("", GUILayout.Width(15));
                }

                if (GUILayout.Button(friendlyName, style))
                {
                    this.tableIdProp.stringValue = tableId;
                    this.tableIdProp.serializedObject.ApplyModifiedProperties();
                    editorWindow.Close();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
    }

    public class KeySelectionPopup : PopupWindowContent
    {
        private readonly LocalizationTable table;
        private readonly SerializedProperty keyProp;
        private string searchFilter = "";
        private Vector2 scrollPosition;
        private List<string> allKeys;

        public KeySelectionPopup(LocalizationTable table, SerializedProperty keyProp)
        {
            this.table = table;
            this.keyProp = keyProp;
            this.allKeys = new List<string>();

            foreach (var key in table.GetAllKeys())
            {
                this.allKeys.Add(key);
            }

            this.allKeys.Sort();
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(300, 250);
        }

        public override void OnGUI(Rect rect)
        {
            // Search field
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            GUI.SetNextControlName("SearchField");
            this.searchFilter = EditorGUILayout.TextField(this.searchFilter);
            EditorGUILayout.EndHorizontal();

            // Focus search field on open
            if (Event.current.type == EventType.Repaint && string.IsNullOrEmpty(this.searchFilter))
            {
                EditorGUI.FocusTextInControl("SearchField");
            }

            EditorGUILayout.Space(5);

            // None option
            if (GUILayout.Button("(None)", EditorStyles.miniButton))
            {
                this.keyProp.stringValue = "";
                this.keyProp.serializedObject.ApplyModifiedProperties();
                editorWindow.Close();
            }

            EditorGUILayout.Space(2);

            // Filtered list
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);

            string filterLower = this.searchFilter?.ToLowerInvariant() ?? "";

            foreach (var key in this.allKeys)
            {
                if (!string.IsNullOrEmpty(filterLower) && !key.ToLowerInvariant().Contains(filterLower))
                    continue;

                bool isSelected = key == this.keyProp.stringValue;
                var style = isSelected ? EditorStyles.boldLabel : EditorStyles.label;

                EditorGUILayout.BeginHorizontal();

                if (isSelected)
                {
                    EditorGUILayout.LabelField("►", GUILayout.Width(15));
                }
                else
                {
                    EditorGUILayout.LabelField("", GUILayout.Width(15));
                }

                if (GUILayout.Button(key, style))
                {
                    this.keyProp.stringValue = key;
                    this.keyProp.serializedObject.ApplyModifiedProperties();
                    editorWindow.Close();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
    }

    public class AddNewEntryPopup : PopupWindowContent
    {
        private LocalizationTable table;
        private SerializedProperty keyProp;
        private string newKey = "";
        private string newValue = "";

        public static void Show(Rect buttonRect, LocalizationTable table, SerializedProperty keyProp)
        {
            var popup = new AddNewEntryPopup
            {
                table = table,
                keyProp = keyProp
            };
            PopupWindow.Show(buttonRect, popup);
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(300, 120);
        }

        public override void OnGUI(Rect rect)
        {
            EditorGUILayout.LabelField("Add New Entry", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Key:", GUILayout.Width(40));
            GUI.SetNextControlName("NewKeyField");
            this.newKey = EditorGUILayout.TextField(this.newKey);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Value:", GUILayout.Width(40));
            this.newValue = EditorGUILayout.TextField(this.newValue);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUI.enabled = !string.IsNullOrEmpty(this.newKey);
            if (GUILayout.Button("Add & Select", GUILayout.Width(100)))
            {
                Undo.RecordObject(this.table, "Add Localization Entry");
                this.table.AddEntry(this.newKey, this.newValue);
                EditorUtility.SetDirty(this.table);

                this.keyProp.stringValue = this.newKey;
                this.keyProp.serializedObject.ApplyModifiedProperties();

                editorWindow.Close();
            }
            GUI.enabled = true;

            if (GUILayout.Button("Cancel", GUILayout.Width(60)))
            {
                editorWindow.Close();
            }

            EditorGUILayout.EndHorizontal();

            // Focus key field on open
            if (Event.current.type == EventType.Repaint && string.IsNullOrEmpty(this.newKey))
            {
                EditorGUI.FocusTextInControl("NewKeyField");
            }
        }
    }
}
