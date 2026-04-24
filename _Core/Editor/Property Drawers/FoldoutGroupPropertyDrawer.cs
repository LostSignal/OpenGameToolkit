//-----------------------------------------------------------------------
// <copyright file="FoldoutGroupPropertyDrawer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//------------------------

namespace OGT
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(FoldoutGroupAttribute))]
    public class FoldoutGroupPropertyDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, bool> FoldoutExpanded = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            OnGUI(position, property, label, (FoldoutGroupAttribute)this.attribute);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetPropertyHeight(property, label, (FoldoutGroupAttribute)this.attribute, base.GetPropertyHeight(property, label));
        }

        private static void OnGUI(Rect position, SerializedProperty property, GUIContent label, FoldoutGroupAttribute attribute)
        {
            //// NOTE [bgish]: Old debug code that can be removed
            //// Logger.Log("OnGUI = " + label.text + ", " + property.propertyPath);

            var foldoutKey = GetFoldoutKey(property, attribute);
            var expanded = IsExpanded(foldoutKey);
            var originalLabel = new GUIContent(label);

            if (attribute.IsFirstItem)
            {
                Rect foldoutRect = position;
                foldoutRect.height = 20.0f;

                bool newExpanded = EditorGUI.Foldout(foldoutRect, expanded, attribute.FoldoutGroupName);

                if (newExpanded != expanded)
                {
                    SetExpanded(foldoutKey, newExpanded);
                    return;
                }
            }

            if (expanded && attribute.IsFirstItem)
            {
                Rect propertyPosition = position;
                propertyPosition.height -= 20.0f;
                propertyPosition.y += 20.0f;

                EditorGUI.PropertyField(propertyPosition, property, originalLabel, true);
            }
            else if (expanded)
            {
                EditorGUI.PropertyField(position, property, originalLabel, true);
            }
        }

        private static float GetPropertyHeight(SerializedProperty property, GUIContent label, FoldoutGroupAttribute attribute, float baseHeight)
        {
            var foldoutKey = GetFoldoutKey(property, attribute);
            var expanded = IsExpanded(foldoutKey);

            if (attribute.IsFirstItem)
            {
                return expanded ? baseHeight + 20.0f : 20.0f;
            }
            else
            {
                return expanded ? baseHeight : -EditorGUIUtility.standardVerticalSpacing;
            }
        }

        private static bool IsExpanded(string key)
        {
            if (FoldoutExpanded.TryGetValue(key, out bool value))
            {
                return value;
            }

            return false;
        }

        private static void SetExpanded(string key, bool expanded)
        {
            if (FoldoutExpanded.ContainsKey(key))
            {
                FoldoutExpanded[key] = expanded;
            }
            else
            {
                FoldoutExpanded.Add(key, expanded);
            }
        }

        private static string GetFoldoutKey(SerializedProperty property, FoldoutGroupAttribute attribute)
        {
            //// property.serializedObject.GetHashCode()
            //// property.GetHashCode();
            //// attribute.FoldoutGroupName

            return $"{attribute.FoldoutGroupName}";
        }
    }
}
