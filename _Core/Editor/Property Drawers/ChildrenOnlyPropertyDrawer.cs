//-----------------------------------------------------------------------
// <copyright file="ChildrenOnlyPropertyDrawer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(ChildrenOnlyAttribute))]
    public class ChildrenOnlyPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded == false)
            {
                property.isExpanded = true;
            }

            float x = position.x;
            float y = position.y;
            float width = position.width;
            int childDepth = property.depth + 1;

            foreach (SerializedProperty child in property)
            {
                float propHeight = EditorGUI.GetPropertyHeight(child, label, includeChildren: false);

                if (child.depth == childDepth)
                {
                    EditorGUI.PropertyField(new Rect(x, y, width, propHeight), child, false);
                }

                y += propHeight;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label) - EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
