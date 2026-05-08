//-----------------------------------------------------------------------
// <copyright file=" ProjectSettingsRosylnAnalyzers.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(ProjectSettingsRosylnAnalyzers))]
    public class ProjectSettingsRosylnAnalyzersEditor : OGT.Editor
    {
        protected override void NewOnInspectorGUI()
        {
            this.DrawMember("applyRosylnAnalyzers");
            this.DrawMember("analyzers");

            EditorGUILayout.Space(20);

            if (GUILayout.Button("Reset To Defaults"))
            {
                ProjectSettingsRosylnAnalyzers.Instance.LoadDefaults();
            }
        }

        protected override void OnGUIChanged()
        {
            base.OnGUIChanged();

            ProjectSettingsRosylnAnalyzers.Instance.Save();
        }

        [SettingsProvider]
        private static SettingsProvider CreateLostLibrarySettingsProvider() =>
            SettingsProviderUtil.GetSettingsProvider(ProjectSettingsRosylnAnalyzers.Instance, "Project/Open Game Toolkit/WIP - Rosyln Analyzers");
    }
}
