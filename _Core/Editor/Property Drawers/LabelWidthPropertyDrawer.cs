//-----------------------------------------------------------------------
// <copyright file="LabelWidthPropertyDrawer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(LabelWidthAttribute))]
    public class LabelWidthPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            LabelWidthAttribute attribute = (LabelWidthAttribute)this.attribute;
            float previousLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = attribute.LabelWidth;

            try
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
            finally
            {
                EditorGUIUtility.labelWidth = previousLabelWidth;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}
