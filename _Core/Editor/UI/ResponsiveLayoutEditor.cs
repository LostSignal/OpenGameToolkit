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

        [MenuItem("CONTEXT/RectTransform/Add to Parent ResponsiveLayout")]
        private static void AddToParentResponsiveLayout(MenuCommand command)
        {
            RectTransform rectTransform = command.context as RectTransform;
            if (rectTransform == null)
                return;

            // Search for ResponsiveLayout in parent hierarchy
            ResponsiveLayout responsiveLayout = rectTransform.GetComponentInParent<ResponsiveLayout>();

            if (responsiveLayout == null)
            {
                Debug.LogWarning($"No ResponsiveLayout found in parent hierarchy of '{rectTransform.name}'");
                return;
            }

            Undo.RecordObject(responsiveLayout, "Add to ResponsiveLayout");

            if (responsiveLayout.AddTarget(rectTransform))
            {
                EditorUtility.SetDirty(responsiveLayout);
                Debug.Log($"Added '{rectTransform.name}' to ResponsiveLayout on '{responsiveLayout.name}'");
            }
            else
            {
                Debug.LogWarning($"'{rectTransform.name}' is already in the ResponsiveLayout targets or is null");
            }
        }

        [MenuItem("CONTEXT/RectTransform/Add to Parent ResponsiveLayout", true)]
        private static bool ValidateAddToParentResponsiveLayout(MenuCommand command)
        {
            RectTransform rectTransform = command.context as RectTransform;
            if (rectTransform == null)
                return false;

            return rectTransform.GetComponentInParent<ResponsiveLayout>() != null;
        }
    }
}
