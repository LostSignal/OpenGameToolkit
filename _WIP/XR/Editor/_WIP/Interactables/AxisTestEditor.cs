#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="AxisTestEditor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace OGT.XR
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(AxisTest))]
    public class AxisTestEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var axisTest = this.target as AxisTest;

            GUILayout.Space(10);

            if (GUILayout.Button("Set Start"))
            {
                axisTest.SetStartPosition();
                EditorUtil.SetDirty(axisTest);
            }

            if (GUILayout.Button("Set End"))
            {
                axisTest.SetEndPosition();
                EditorUtil.SetDirty(axisTest);
            }

            float percentage = axisTest.Percentage;
            float newPercentage = EditorGUILayout.Slider(percentage, 0.0f, 1.0f);

            if (percentage != newPercentage)
            {
                axisTest.SetPercentage(newPercentage);
                // axisTest.ObjectTransform.localPosition = Vector3.Lerp(axisTest.StartPosition, axisTest.EndPosition, axisTest.Percentage);
                EditorUtil.SetDirty(axisTest.ObjectTransform);
                EditorUtil.SetDirty(axisTest);
            }
        }
    }
}

#endif
