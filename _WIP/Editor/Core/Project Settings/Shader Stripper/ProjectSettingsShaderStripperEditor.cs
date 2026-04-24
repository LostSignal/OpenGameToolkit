//-----------------------------------------------------------------------
// <copyright file="ProjectSettingsShaderStripperEditor.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(ProjectSettingsShaderStripper))]
    public class ProjectSettingsShaderStripperEditor : OGT.Editor
    {
        protected override void NewOnInspectorGUI()
        {
            this.DrawMember("enableShaderStripping");
            this.DrawMember("isForwardRednderer");
            this.DrawMember("approvedFolders");
            this.DrawMember("essentialShadersStartWith");
            this.DrawMember("essentialShadersEqual");
            this.DrawMember("varientsToSkip");

            EditorGUILayout.Space(20);

            if (GUILayout.Button("Reset To Defaults"))
            {
                ProjectSettingsShaderStripper.Instance.LoadDefaults();
            }
        }

        protected override void OnGUIChanged()
        {
            base.OnGUIChanged();

            ProjectSettingsShaderStripper.Instance.Save();
        }

        [SettingsProvider]
        private static SettingsProvider CreateLostLibrarySettingsProvider() =>
            SettingsProviderUtil.GetSettingsProvider(ProjectSettingsShaderStripper.Instance, "Project/Open Game Toolkit/WIP - Shader Stripper");
    }
}
