//-----------------------------------------------------------------------
// <copyright file="ProjectSettingsGeneralEditor.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(ProjectSettingsGeneral))]
    public class ProjectSettingsGeneralEditor : OGT.Editor
    {
        private static readonly OGTLogger Logger = OGTLogger.OGTEditor;

        protected override void NewOnInspectorGUI()
        {
            try
            {
                DrawProjectSettingsProxies(labelWidth: 170);

                using (new LabelWidthScope(180))
                {
                    DrawLineEndings();
                    DrawSerializationMode();
                }

                using (new LabelWidthScope(300))
                {
                    DrawAssetImporter();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }

            EditorGUILayout.Space(20);

            if (GUILayout.Button("Reset To Defaults"))
            {
                ProjectSettingsGeneral.Instance.LoadDefaults();
            }

            void DrawProjectSettingsProxies(int labelWidth)
            {
                using (new BoxAreaScope("Project Settings"))
                using (new IndentLevelScope(1))
                {
                    // Product Name
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Product Name", GUILayout.Width(labelWidth));

                        var productName = EditorGUILayout.TextField(PlayerSettings.productName);

                        if (PlayerSettings.productName != productName)
                        {
                            PlayerSettings.productName = productName;
                        }
                    }

                    // Company Name
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Company Name", GUILayout.Width(labelWidth));

                        var companyName = EditorGUILayout.TextField(PlayerSettings.companyName);

                        if (PlayerSettings.companyName != companyName)
                        {
                            PlayerSettings.companyName = companyName;
                        }
                    }

                    // Root Namespace
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Root Namespace", GUILayout.Width(labelWidth));

                        var rootNamespace = EditorGUILayout.TextField(EditorSettings.projectGenerationRootNamespace);

                        if (EditorSettings.projectGenerationRootNamespace != rootNamespace)
                        {
                            EditorSettings.projectGenerationRootNamespace = rootNamespace;
                        }
                    }

                    // HACK [bgish]: Not sure why the label width is 15 off here, but this fixes it for now
                    EditorGUIUtility.labelWidth = labelWidth + 15;
                    this.DrawMember("generatedOutputDirectory");
                }
            }

            void DrawLineEndings()
            {
                this.BoxArea("Project Line Endings Settings", () =>
                {
                    this.DrawMember("forceProjectLineEndings");
                    this.DrawMember("projectLineEndings");
                });
            }

            void DrawSerializationMode()
            {
                this.BoxArea("Project Serialization Mode", () =>
                {
                    this.DrawMember("forceSerializationMode");
                    this.DrawMember("serializationMode");
                });
            }

            void DrawAssetImporter()
            {
                this.BoxArea("Asset Importing", () =>
                {
                    this.DrawMember("forceParallelImport");
                    this.DrawMember("desiredImportWorkerCount");
                    this.DrawMember("standbyImportWorkerCount");
                    this.DrawMember("idleImportWorkerShutdownDelayInSeconds");
                });
            }
        }

        protected override void OnGUIChanged()
        {
            base.OnGUIChanged();

            ProjectSettingsGeneral.Instance.ApplyAllSettings();
            ProjectSettingsGeneral.Instance.Save();
        }

        [SettingsProvider]
        private static SettingsProvider CreateSettingsProvider() =>
            SettingsProviderUtil.GetSettingsProvider(ProjectSettingsGeneral.Instance, "Project/Open Game Toolkit");
    }
}
