//-----------------------------------------------------------------------
// <copyright file="FoldoutHelper.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using UnityEditor;
    using UnityEngine;

    public class BoxAreaScope : IDisposable
    {
        private static GUIStyle titleGuiStyle = null;

        public BoxAreaScope(string title, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical("box", options);

            if (string.IsNullOrEmpty(title) == false)
            {
                EditorGUILayout.LabelField(title, TitleGuiStyle);
            }

            EditorGUILayout.Space(5);
        }

        private static GUIStyle TitleGuiStyle
        {
            get
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

                return titleGuiStyle;
            }
        }

        public void Dispose()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }
    }
}
