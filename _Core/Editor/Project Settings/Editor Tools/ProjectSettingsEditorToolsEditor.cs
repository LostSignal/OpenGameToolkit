//-----------------------------------------------------------------------
// <copyright file="ProjectSettingsEditorToolsEditor.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using SourceControlType = ProjectSettingsEditorTools.SourceControlType;

    [CustomEditor(typeof(ProjectSettingsEditorTools))]
    public class ProjectSettingsEditorToolsEditor : OGT.Editor
    {
        protected override void NewOnInspectorGUI()
        {
            using (new LabelWidthScope(290))
            {
                DrawCustomImporters();
                DrawWarpedImagination();
                DrawOverwriteTemplateFiles();
            }

            DrawGenerateIgnoreFiles();
            DrawGenerateEditorConfig();
            DrawEditorEventsConfig();

            if (GUILayout.Button("Reset To Defaults"))
            {
                ProjectSettingsEditorTools.Instance.LoadDefaults();
            }

            void DrawCustomImporters()
            {
                this.BoxArea("Custom Importers", () =>
                {
                    this.DrawMember("useUnrealNamingCollisionImporter");
                    this.DrawMember("useApplyFolderPresetsImporter");
                    this.DrawMember("automaticallyFixLineEndingsMismatch");
                });
            }

            void DrawWarpedImagination()
            {
                this.BoxArea("Warped Imagination Hierarchy Icons", () =>
                {
                    this.DrawMember("useWarpedImaginationNextLevelHierarchy");
                });
            }

            void DrawOverwriteTemplateFiles()
            {
                this.BoxArea("Override Template File", () =>
                {
                    this.DrawMember("overrideTemplateFiles");
                    this.DrawMember("templateMonoBehaviour");
                    this.DrawMember("templatePlayableAsset");
                    this.DrawMember("templatePlayableBehaviour");
                    this.DrawMember("templateStateMachineBehaviour");
                    this.DrawMember("templateSubStateMachineBehaviour");
                    this.DrawMember("templateEditorTestScript");
                });
            }

            void DrawGenerateIgnoreFiles()
            {
                this.BoxArea("Generate Source Control Ignore File", () =>
                {
                    this.DrawMember("sourceControlType");

                    EditorGUILayout.Space(5);

                    var sourceControlType = this.GetProperty("sourceControlType");
                    var sourceControl = (SourceControlType)sourceControlType.intValue;

                    if (sourceControl == SourceControlType.Git)
                    {
                        this.DrawMember("ignoreTemplateGit");
                    }
                    else if (sourceControl == SourceControlType.Plastic)
                    {
                        this.DrawMember("ignoreTemplatePlastic");
                        this.DrawMember("plasticAutoSetFileCasingError");
                        this.DrawMember("plasticAutoSetYamlMergeToolPath");
                    }
                    else if (sourceControl == SourceControlType.Perforce)
                    {
                        this.DrawMember("ignoreTemplateP4");
                        this.DrawMember("p4IgnoreFileName");
                        this.DrawMember("autosetP4IgnoreEnvironmentVariable");
                    }

                    using (new EditorGUILayout.HorizontalScope(GUILayout.Width(210)))
                    {
                        EditorGUILayout.Space(10);

                        if (GUILayout.Button("Generate Ignore File", GUILayout.Width(200)))
                        {
                            SourceControlUtils.GenerateSourceControlIgnoreFile();
                        }
                    }

                    EditorGUILayout.Space(5);
                });
            }

            void DrawGenerateEditorConfig()
            {
                this.BoxArea("Generate \".editorconfig\" File", () =>
                {
                    this.DrawMember("useEditorConfig");
                    this.DrawMember("editorConfigFileName");
                    this.DrawMember("editorConfigTemplate");

                    var editorConfigTemplate = this.GetProperty("editorConfigTemplate");

                    if (editorConfigTemplate.objectReferenceValue != null)
                    {
                        using (new EditorGUILayout.HorizontalScope(GUILayout.Width(210)))
                        {
                            EditorGUILayout.Space(10);

                            if (GUILayout.Button("Generate .editorconfig File", GUILayout.Width(200)))
                            {
                                MenuItemTools.GenerateFileFromTextAsset("editor config", ".editorconfig", EditorUtil.GetGuid(editorConfigTemplate.objectReferenceValue));
                            }
                        }

                        EditorGUILayout.Space(5);
                    }
                });
            }

            void DrawEditorEventsConfig()
            {
                this.BoxArea("Editor Events", () =>
                {
                    // Special case for setting editor events to always print at the Info level
                    if (OGTLogger.GetLoggingLevel("Editor Events") != LoggingLevel.Info)
                    {
                        GUILayout.Space(10);

                        if (GUILayout.Button("Set 'Editor Event' Logging Level to Info"))
                        {
                            OGTLogger.SetLoggingLevel("Editor Events", LoggingLevel.Info);
                            UnityLoggingProvider.SaveChannels();
                        }

                        GUILayout.Space(20);
                    }

                    this.DrawMember("editorEventsIgnoreAssemblies", "Ignore Assemblies");
                });
            }
        }

        protected override void OnGUIChanged()
        {
            base.OnGUIChanged();

            ProjectSettingsEditorTools.Instance.Save();
        }

        [SettingsProvider]
        private static SettingsProvider CreateSettingsProvider() =>
            SettingsProviderUtil.GetSettingsProvider(ProjectSettingsEditorTools.Instance, "Project/Open Game Toolkit/Editor Tools");
    }
}
