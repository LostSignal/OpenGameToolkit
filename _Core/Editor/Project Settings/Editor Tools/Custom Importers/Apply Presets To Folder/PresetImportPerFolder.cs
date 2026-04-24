//-----------------------------------------------------------------------
// <copyright file="PresetImportPerFolder.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

////
//// This file taken from Unity docs - https://docs.unity3d.com/Manual//DefaultPresetsByFolder.html
////

namespace OGT
{
    using System.IO;
    using UnityEditor;
    using UnityEditor.Presets;

    public class PresetImportPerFolder : AssetPostprocessor
    {
        private static bool isEditorReady = false;

        static PresetImportPerFolder()
        {
            EditorApplication.delayCall += MarkEditorReady;
        }

        private static void MarkEditorReady()
        {
            EditorApplication.delayCall -= MarkEditorReady;
            isEditorReady = true;
        }

        private void OnPreprocessAsset()
        {
            bool useApplyFolderPresetsImporter = false;

            try
            {
                if (isEditorReady)
                {
                    useApplyFolderPresetsImporter = ProjectSettingsEditorTools.Instance.UseApplyFolderPresetsImporter;
                }
                else
                {
                    var settingsFileContents = File.ReadAllText("./ProjectSettings/ProjectSettingsEditorTools.asset");
                    useApplyFolderPresetsImporter = settingsFileContents.Contains("\"useApplyFolderPresetsImporter\": true");
                }
            }
            catch
            {
                var settingsFileContents = File.ReadAllText("./ProjectSettings/ProjectSettingsEditorTools.asset");
                useApplyFolderPresetsImporter = settingsFileContents.Contains("\"useApplyFolderPresetsImporter\": true");
            }

            if (useApplyFolderPresetsImporter)
            {
                PresetImportPerFolder.Process(this.assetImporter, this.assetPath);
            }
        }

        private static void Process(AssetImporter assetImporter, string assetPath)
        {
            // Make sure we are applying presets the first time an asset is imported.
            if (assetImporter.importSettingsMissing)
            {
                // Get the current imported asset folder.
                var path = Path.GetDirectoryName(assetPath).Replace("\\", "/");

                if (path == "ProjectSettings" || path.StartsWith("Packages/"))
                {
                    return;
                }

                while (string.IsNullOrEmpty(path) == false)
                {
                    // Find all Preset assets in this folder.
                    var presetGuids = AssetDatabase.FindAssets("t:Preset", new[] { path });

                    foreach (var presetGuid in presetGuids)
                    {
                        // Make sure we are not testing Presets in a subfolder.
                        string presetPath = AssetDatabase.GUIDToAssetPath(presetGuid);

                        if (Path.GetDirectoryName(presetPath) == path)
                        {
                            // Load the Preset and try to apply it to the importer.
                            var preset = AssetDatabase.LoadAssetAtPath<Preset>(presetPath);

                            if (preset.ApplyTo(assetImporter))
                            {
                                return;
                            }
                        }
                    }

                    // Try again in the parent folder.
                    path = Path.GetDirectoryName(path);
                }
            }
        }
    }
}
