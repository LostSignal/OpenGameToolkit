//-----------------------------------------------------------------------
// <copyright file="ShowIfBasePropertyDrawer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

////
//// Thanks https://gist.github.com/deebrol/02f61b7611fd4eca923776077b92dfc2 for the starting point for this file.
////

namespace OGT
{
    using UnityEditor;
    using UnityEngine;

    public abstract class ShowIfBasePropertyDrawer : PropertyDrawer
    {
        private bool showField = true;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty conditionSerializedProperty = property.serializedObject.FindProperty(this.GetConditionFieldName());

            if (conditionSerializedProperty == null)
            {
                var propertyPath = property.propertyPath.Replace(property.name, this.GetConditionFieldName());
                conditionSerializedProperty = property.serializedObject.FindProperty(propertyPath);
            }

            // Make sure we can find the condition field
            if (conditionSerializedProperty == null)
            {
                this.ShowError(position, label, "Error getting the condition Field. Check the name.");
                return;
            }

            switch (conditionSerializedProperty.propertyType)
            {
                case SerializedPropertyType.Boolean:
                {
                    if (this.GetCompareValue() is bool boolValue)
                    {
                        this.showField = this.CompareBool(conditionSerializedProperty.boolValue, boolValue);
                    }
                    else
                    {
                        this.ShowError(position, label, "Error, the supplied condition value is not a bool.");
                    }

                    break;
                }

                case SerializedPropertyType.Enum:
                {
                    var conditionValue = conditionSerializedProperty.enumNames[conditionSerializedProperty.enumValueIndex];
                    var compareValue = this.GetCompareValue()?.ToString();
                    this.showField = this.CompareString(conditionValue, compareValue);

                    break;
                }

                default:
                {
                    this.ShowError(position, label, "This type has not supported.");
                    break;
                }
            }

            if (this.showField)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (this.showField)
            {
                return EditorGUI.GetPropertyHeight(property, label);
            }
            else
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }
        }

        protected abstract string GetConditionFieldName();

        protected abstract object GetCompareValue();

        protected abstract bool CompareBool(bool b1, bool b2);

        protected abstract bool CompareString(string str1, string str2);

        private void ShowError(Rect position, GUIContent label, string errorText)
        {
            EditorGUI.LabelField(position, label, new GUIContent(errorText));
            this.showField = true;
        }
    }
}
