//-----------------------------------------------------------------------
// <copyright file="PropertyGroupEditor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Properties
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(PropertyGroup))]
    internal class PropertiesEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            using (new UnityEditor.EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Bool"))
                {
                    (this.target as PropertyGroup).AddProperty<PropertyGroup.BoolProperty>();
                    UnityEditor.EditorUtility.SetDirty(this);
                }

                if (GUILayout.Button("Int"))
                {
                    var property = (this.target as PropertyGroup).AddProperty<PropertyGroup.IntProperty>();
                    property.Min = int.MinValue;
                    property.Max = int.MaxValue;
                    UnityEditor.EditorUtility.SetDirty(this);
                }

                if (GUILayout.Button("Float"))
                {
                    var property = (this.target as PropertyGroup).AddProperty<PropertyGroup.FloatProperty>();
                    property.Min = float.MinValue;
                    property.Max = float.MaxValue;
                    UnityEditor.EditorUtility.SetDirty(this);
                }

                if (GUILayout.Button("String"))
                {
                    (this.target as PropertyGroup).AddProperty<PropertyGroup.StringProperty>();
                    UnityEditor.EditorUtility.SetDirty(this);
                }

                if (GUILayout.Button("Enum"))
                {
                    (this.target as PropertyGroup).AddProperty<PropertyGroup.EnumProperty>();
                    UnityEditor.EditorUtility.SetDirty(this);
                }
            }
        }

        [CustomPropertyDrawer(typeof(PropertyGroup.BoolProperty))]
        [CustomPropertyDrawer(typeof(PropertyGroup.IntProperty))]
        [CustomPropertyDrawer(typeof(PropertyGroup.FloatProperty))]
        [CustomPropertyDrawer(typeof(PropertyGroup.StringProperty))]
        [CustomPropertyDrawer(typeof(PropertyGroup.EnumProperty))]
        private class PropertyGroupPropertyDrawer : PropertyDrawer
        {
            private static readonly Dictionary<string, string> typeNameCache = new();
            private static readonly Dictionary<ulong, string[]> enumNamesCache = new();

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                bool isIntOrFloatProperty = GetTypeName(property.type) == nameof(IntProperty) || GetTypeName(property.type) == nameof(FloatProperty);
                bool isEnumProperty = GetTypeName(property.type) == nameof(EnumProperty);

                // Don't make child fields be indented
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                EditorGUI.BeginProperty(position, label, property);

                float rowHeight = base.GetPropertyHeight(property, label);
                position.height = rowHeight;

                // Draw foldout for int, float, and enum properties
                if (isIntOrFloatProperty || isEnumProperty)
                {
                    position.x += 5;
                    property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none);
                    position.x += 5;
                }
                else
                {
                    position.x += 10;
                }

                // Draw label
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), GUIContent.none);
                float totalWidth = position.width;
                float leftSideWidth = totalWidth * 0.55f;
                float rightSideWidth = totalWidth - leftSideWidth;

                // Calculate Rects
                var column0Rect = new Rect(position.x + 0, position.y, 40, rowHeight);
                var column1Rect = new Rect(column0Rect.x + 40, position.y, leftSideWidth - 45, rowHeight);
                var column2Rect = new Rect(column1Rect.x + (leftSideWidth - 40), position.y, 45, rowHeight);
                var column3Rect = new Rect(column2Rect.x + 50, position.y, rightSideWidth - 60, rowHeight);

                DrawNameAndDefault();

                // Move down to next row
                column0Rect.y += rowHeight + 2;
                column1Rect.y += rowHeight + 2;
                column2Rect.y += rowHeight + 2;
                column3Rect.y += rowHeight + 2;

                if (isIntOrFloatProperty)
                {
                    DrawMinMax();
                }
                else if (isEnumProperty)
                {
                    DrawEnum();
                }

                // Set indent back to what it was
                EditorGUI.indentLevel = indent;
                EditorGUI.EndProperty();

                void DrawNameAndDefault()
                {
                    EditorGUI.LabelField(column0Rect, GetTypeName(property.type).Replace("Property", string.Empty));
                    EditorGUI.PropertyField(column1Rect, property.FindPropertyRelative("name"), GUIContent.none);
                    EditorGUI.LabelField(column2Rect, "Default");

                    if (isEnumProperty)
                    {
                        var enumType = property.FindPropertyRelative("enumType").objectReferenceValue as Enum;

                        if (enumType == null)
                        {
                            EditorGUI.HelpBox(column3Rect, "Assign Enum Type", UnityEditor.MessageType.Warning);
                        }
                        else
                        {
                            int index = property.FindPropertyRelative("defaultIndex").intValue;
                            int newIndex = EditorGUI.Popup(column3Rect, index, GetEnumNames(enumType));

                            if (index != newIndex && newIndex >= 0)
                            {
                                property.FindPropertyRelative("defaultIndex").intValue = newIndex;
                            }
                        }
                    }
                    else
                    {
                        EditorGUI.PropertyField(column3Rect, property.FindPropertyRelative("defaultValue"), GUIContent.none);
                    }
                }

                void DrawMinMax()
                {
                    if (property.isExpanded == false)
                    {
                        return;
                    }

                    EditorGUI.LabelField(column0Rect, "Min");
                    EditorGUI.PropertyField(column1Rect, property.FindPropertyRelative("min"), GUIContent.none);
                    EditorGUI.LabelField(column2Rect, "Max");
                    EditorGUI.PropertyField(column3Rect, property.FindPropertyRelative("max"), GUIContent.none);
                }

                void DrawEnum()
                {
                    if (property.isExpanded == false)
                    {
                        return;
                    }

                    EditorGUI.LabelField(column0Rect, "Enum");
                    EditorGUI.PropertyField(column1Rect, property.FindPropertyRelative("enumType"), GUIContent.none);
                }
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return base.GetPropertyHeight(property, label) * (property.isExpanded ? 2 : 1) + 2;
            }

            private static string GetTypeName(string fullTypeName)
            {
                if (typeNameCache.TryGetValue(fullTypeName, out var typeName) == false)
                {
                    typeName = fullTypeName.Replace("managedReference<", string.Empty).Replace(">", string.Empty);
                    typeNameCache.Add(fullTypeName, typeName);
                }

                return typeName;
            }

            private static string[] GetEnumNames(Enum enumType)
            {
                if (enumType == null)
                {
                    return Array.Empty<string>();
                }

                if (enumNamesCache.TryGetValue(EntityId.ToULong(enumType.GetEntityId()), out var enumNames) == false)
                {
                    enumNames = enumType.EnumValues.Select(x => x.Name).ToArray();
                    enumNamesCache.Add(EntityId.ToULong(enumType.GetEntityId()), enumNames);
                }

                return enumNames;
            }
        }
    }
}
