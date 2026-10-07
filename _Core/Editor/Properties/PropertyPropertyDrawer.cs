//-----------------------------------------------------------------------
// <copyright file="PropertyPropertyDrawer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Properties
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(BoolProperty))]
    [CustomPropertyDrawer(typeof(IntProperty))]
    [CustomPropertyDrawer(typeof(FloatProperty))]
    [CustomPropertyDrawer(typeof(StringProperty))]
    [CustomPropertyDrawer(typeof(EnumProperty))]
    public class PropertyPropertyDrawer : PropertyDrawer
    {
        private const string PropertyGroupField = "propertyGroup";
        private const string PropertyIdField = "propertyId";
        private const string MixedValueText = "—";

        private static readonly int GroupDropdownHint = "OGT.PropertyGroupDropdown".GetHashCode();
        private static readonly int PropertyDropdownHint = "OGT.PropertyDropdown".GetHashCode();
        private static readonly Dictionary<Type, Type> valueTypeCache = new();

        // All PropertyGroup assets in the project, rebuilt whenever the project changes
        private static List<PropertyGroup> propertyGroups;
        private static string[] propertyGroupMenuNames;

        // GenericMenu callbacks happen outside of OnGUI, so the selection is stored here
        // and picked up by the matching control on the next repaint
        private static int pendingControlId;
        private static object pendingValue;
        private static bool hasPendingValue;

        static PropertyPropertyDrawer()
        {
            EditorApplication.projectChanged += () => propertyGroups = null;
        }

        private static float LineHeight => EditorGUIUtility.singleLineHeight;

        private static float LineStep => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var groupProperty = property.FindPropertyRelative(PropertyGroupField);
            bool showPropertyLine = groupProperty.hasMultipleDifferentValues || groupProperty.objectReferenceValue != null;
            return showPropertyLine ? LineHeight + LineStep : LineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var groupProperty = property.FindPropertyRelative(PropertyGroupField);
            var idProperty = property.FindPropertyRelative(PropertyIdField);

            label = EditorGUI.BeginProperty(position, label, property);

            var fieldRect = EditorGUI.PrefixLabel(new Rect(position.x, position.y, position.width, LineHeight), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var valueType = this.GetValueType(property);

            if (valueType == null)
            {
                EditorGUI.HelpBox(fieldRect, $"Unable to determine property type for '{property.type}'.", MessageType.Error);
            }
            else
            {
                var currentGroup = groupProperty.objectReferenceValue as PropertyGroup;

                EditorGUI.showMixedValue = groupProperty.hasMultipleDifferentValues;

                if (GroupDropdown(fieldRect, currentGroup, out var newGroup))
                {
                    groupProperty.objectReferenceValue = newGroup;
                    idProperty.intValue = 0;
                    currentGroup = newGroup;
                }

                EditorGUI.showMixedValue = false;

                fieldRect.y += LineStep;

                if (groupProperty.hasMultipleDifferentValues)
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUI.LabelField(fieldRect, "Multiple property groups selected");
                    }
                }
                else if (currentGroup != null)
                {
                    EditorGUI.showMixedValue = idProperty.hasMultipleDifferentValues;

                    if (PropertyDropdown(fieldRect, currentGroup, valueType, (uint)idProperty.intValue, out uint newId))
                    {
                        idProperty.intValue = (int)newId;
                    }

                    EditorGUI.showMixedValue = false;
                }
            }

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        // Used by the Visual Scripting inspectors, which work on the Property instance directly
        public static void Draw(Property property, Rect position)
        {
            var groupRect = new Rect(position.x, position.y, position.width, LineHeight);
            var propertyRect = new Rect(position.x, position.y + LineStep, position.width, LineHeight);

            groupRect = EditorGUI.PrefixLabel(groupRect, new GUIContent("Group"));

            if (GroupDropdown(groupRect, property.PropertyGroup, out var newGroup))
            {
                property.PropertyGroup = newGroup;
                property.PropertyId = 0;
            }

            if (property.PropertyGroup == null)
            {
                EditorGUI.HelpBox(propertyRect, "Please select a Property Group.", MessageType.Warning);
                return;
            }

            propertyRect = EditorGUI.PrefixLabel(propertyRect, new GUIContent("Property"));

            if (PropertyDropdown(propertyRect, property.PropertyGroup, property.Type, property.PropertyId, out uint newId))
            {
                property.PropertyId = newId;
            }
        }

        private static bool GroupDropdown(Rect rect, PropertyGroup current, out PropertyGroup selected)
        {
            int controlId = GUIUtility.GetControlID(GroupDropdownHint, FocusType.Keyboard, rect);

            if (TryConsumePendingValue(controlId, out object value))
            {
                selected = value as PropertyGroup;
                return selected != current;
            }

            selected = current;

            string text = EditorGUI.showMixedValue ? MixedValueText : current == null ? "None" : current.name;

            if (EditorGUI.DropdownButton(rect, new GUIContent(text), FocusType.Keyboard))
            {
                RefreshPropertyGroups();

                var window = EditorWindow.mouseOverWindow;
                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("None"), current == null, () => SetPendingValue(controlId, null, window));
                menu.AddSeparator(string.Empty);

                if (propertyGroups.Count == 0)
                {
                    menu.AddDisabledItem(new GUIContent("No Property Groups found in project"));
                }

                for (int i = 0; i < propertyGroups.Count; i++)
                {
                    var group = propertyGroups[i];
                    menu.AddItem(new GUIContent(propertyGroupMenuNames[i]), group == current, () => SetPendingValue(controlId, group, window));
                }

                menu.DropDown(rect);
            }

            return false;
        }

        private static bool PropertyDropdown(Rect rect, PropertyGroup group, Type valueType, uint currentId, out uint selectedId)
        {
            int controlId = GUIUtility.GetControlID(PropertyDropdownHint, FocusType.Keyboard, rect);

            if (TryConsumePendingValue(controlId, out object value))
            {
                selectedId = (uint)value;
                return selectedId != currentId;
            }

            selectedId = currentId;

            var entries = group.GetProperties(valueType)
                .OrderBy(p => p.Name)
                .ToList();

            var currentEntry = entries.FirstOrDefault(p => p.Id == currentId);
            bool isMissing = currentId != 0 && currentEntry == null && EditorGUI.showMixedValue == false;

            string text = EditorGUI.showMixedValue ? MixedValueText :
                currentId == 0 ? "None" :
                currentEntry != null ? GetDisplayName(currentEntry) :
                $"Missing (Id {currentId})";

            var oldColor = GUI.color;

            if (isMissing)
            {
                GUI.color = new Color(1.0f, 0.5f, 0.5f);
            }

            bool clicked = EditorGUI.DropdownButton(rect, new GUIContent(text), FocusType.Keyboard);
            GUI.color = oldColor;

            if (clicked)
            {
                var window = EditorWindow.mouseOverWindow;
                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("None"), currentId == 0, () => SetPendingValue(controlId, 0, window));
                menu.AddSeparator(string.Empty);

                if (entries.Count == 0)
                {
                    menu.AddDisabledItem(new GUIContent($"No {GetTypeName(valueType)} properties in {group.name}"));
                }

                foreach (var entry in entries)
                {
                    uint id = entry.Id;
                    string menuPath = GetDisplayName(entry).Replace('.', '/');
                    menu.AddItem(new GUIContent(menuPath), id == currentId, () => SetPendingValue(controlId, id, window));
                }

                menu.DropDown(rect);
            }

            return false;
        }

        private static void SetPendingValue(int controlId, object value, EditorWindow window)
        {
            pendingControlId = controlId;
            pendingValue = value;
            hasPendingValue = true;

            if (window != null)
            {
                window.Repaint();
            }
        }

        private static bool TryConsumePendingValue(int controlId, out object value)
        {
            if (hasPendingValue && pendingControlId == controlId)
            {
                value = pendingValue;
                pendingValue = null;
                hasPendingValue = false;
                GUI.changed = true;
                return true;
            }

            value = null;
            return false;
        }

        private static void RefreshPropertyGroups()
        {
            if (propertyGroups != null)
            {
                return;
            }

            var paths = new Dictionary<PropertyGroup, string>();

            foreach (string path in AssetDatabase.FindAssets($"t:{nameof(PropertyGroup)}").Select(AssetDatabase.GUIDToAssetPath).Distinct())
            {
                var group = AssetDatabase.LoadAssetAtPath<PropertyGroup>(path);

                if (group != null)
                {
                    paths[group] = path;
                }
            }

            propertyGroups = paths.Keys.OrderBy(g => g.name).ToList();

            // Append the asset path to any groups that share a name so they can be told apart
            var duplicateNames = new HashSet<string>(propertyGroups.GroupBy(g => g.name).Where(g => g.Count() > 1).Select(g => g.Key));

            propertyGroupMenuNames = propertyGroups
                .Select(g => duplicateNames.Contains(g.name) ? $"{g.name} ({paths[g].Replace('/', '\\')})" : g.name)
                .ToArray();
        }

        private static string GetDisplayName(PropertyGroup.Property property)
        {
            return string.IsNullOrEmpty(property.Name) ? $"Unnamed (Id {property.Id})" : property.Name;
        }

        private static string GetTypeName(Type valueType)
        {
            return valueType == typeof(bool) ? "Bool" :
                valueType == typeof(int) ? "Int" :
                valueType == typeof(float) ? "Float" :
                valueType == typeof(string) ? "String" :
                valueType == typeof(Enum) ? "Enum" : valueType.Name;
        }

        // Figures out which value type (bool, int, float, string, Enum) the drawn Property wraps
        private Type GetValueType(SerializedProperty property)
        {
            var propertyType = this.fieldInfo?.FieldType;

            if (propertyType != null && propertyType.IsArray)
            {
                propertyType = propertyType.GetElementType();
            }
            else if (propertyType != null && propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                propertyType = propertyType.GetGenericArguments()[0];
            }

            if (propertyType == null || propertyType.IsAbstract || typeof(Property).IsAssignableFrom(propertyType) == false)
            {
                propertyType = TypeCache.GetTypesDerivedFrom<Property>().FirstOrDefault(t => t.Name == property.type && t.IsAbstract == false);
            }

            if (propertyType == null)
            {
                return null;
            }

            if (valueTypeCache.TryGetValue(propertyType, out var valueType) == false)
            {
                valueType = ((Property)Activator.CreateInstance(propertyType)).Type;
                valueTypeCache[propertyType] = valueType;
            }

            return valueType;
        }
    }
}
