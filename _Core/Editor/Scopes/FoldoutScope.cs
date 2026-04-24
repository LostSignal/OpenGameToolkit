//-----------------------------------------------------------------------
// <copyright file="FoldoutHelper.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class FoldoutScope : IDisposable
    {
        private static readonly Dictionary<long, bool> Foldouts = new();

        private static GUIStyle titleGuiStyle = null;

        public FoldoutScope(long foldoutId, string title, float width, out bool visible, bool defaultVisible = false)
        {
            // making sure this foldoutId is in the list
            if (Foldouts.ContainsKey(foldoutId) == false)
            {
                Foldouts.Add(foldoutId, defaultVisible);
            }

            Rect position = EditorGUILayout.BeginVertical("box", GUILayout.Width(width));

            this.DrawTitle(position, foldoutId, title);

            visible = Foldouts[foldoutId];
        }

        public FoldoutScope(long foldoutId, string title, out bool visible, bool defaultVisible = false)
        {
            // making sure this foldoutId is in the list
            if (Foldouts.ContainsKey(foldoutId) == false)
            {
                Foldouts.Add(foldoutId, defaultVisible);
            }

            Rect position = EditorGUILayout.BeginVertical("box");

            this.DrawTitle(position, foldoutId, title);

            visible = Foldouts[foldoutId];
        }

        public FoldoutScope(long foldoutId, string title, out bool visible, out Rect position, bool defaultVisible = false)
        {
            // making sure this foldoutId is in the list
            if (Foldouts.ContainsKey(foldoutId) == false)
            {
                Foldouts.Add(foldoutId, defaultVisible);
            }

            position = EditorGUILayout.BeginVertical("box");

            this.DrawTitle(position, foldoutId, title);

            visible = Foldouts[foldoutId];
        }

        public void Dispose()
        {
            EditorGUILayout.EndVertical();
        }

        private void DrawTitle(Rect position, long foldoutId, string title)
        {
            if (titleGuiStyle == null)
            {
                titleGuiStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.LowerCenter,
                    stretchWidth = true,
                    border = new RectOffset(),
                };

                if (EditorColorUtil.IsProTheme())
                {
                    titleGuiStyle.normal.textColor = Color.white;
                }
                else
                {
                    titleGuiStyle.normal.textColor = Color.black;
                }
            }

            if (string.IsNullOrEmpty(title) == false)
            {
                EditorGUILayout.LabelField(title, titleGuiStyle);
            }
            else
            {
                EditorGUILayout.LabelField(string.Empty, titleGuiStyle);
            }

            // drawing the foldout
            Rect foldoutPosition = position;
            foldoutPosition.x += 15;
            foldoutPosition.y += 3;
            foldoutPosition.width = 15;
            foldoutPosition.height = 15;

            Foldouts[foldoutId] = EditorGUI.Foldout(foldoutPosition, Foldouts[foldoutId], GUIContent.none, false);
        }
    }
}
