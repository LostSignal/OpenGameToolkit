//-----------------------------------------------------------------------
// <copyright file="ResponsiveLayoutEditor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(ResponsiveLayout))]
    public class ResponsiveLayoutEditor : UnityEditor.Editor
    {
        // Game View resolution names
        private const string PortraitResolution = "Generic Phone";
        private const string LandscapeResolution = "1080p";

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ResponsiveLayout layout = (ResponsiveLayout)target;

            EditorGUILayout.Space(10);

            var orientation = OrientationManager.GetCurrentOrientation();
            Vector2 gameViewSize = Handles.GetMainGameViewSize();

            EditorGUILayout.HelpBox(
                $"Current Orientation: {orientation}\n" +
                $"Game View: {gameViewSize.x} x {gameViewSize.y}",
                MessageType.Info);

            EditorGUILayout.Space(5);

            if (GUILayout.Button($"Save {orientation} Layout", GUILayout.Height(32)))
            {
                Undo.RecordObject(layout, $"Save {orientation} Layout");
                layout.RecordCurrentLayout();
                EditorUtility.SetDirty(layout);
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Switch Orientation", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Switch to Portrait", GUILayout.Height(28)))
            {
                layout.RecordCurrentLayout();
                SetGameViewResolution(PortraitResolution);
            }

            if (GUILayout.Button("Switch to Landscape", GUILayout.Height(28)))
            {
                layout.RecordCurrentLayout();
                SetGameViewResolution(LandscapeResolution);
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void SetGameViewResolution(string resolutionName)
        {
            GameViewUtils.SetResolutionByName(resolutionName);
        }
    }
}
