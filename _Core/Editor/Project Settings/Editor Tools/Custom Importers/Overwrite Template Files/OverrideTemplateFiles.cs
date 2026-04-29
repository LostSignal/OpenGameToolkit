//-----------------------------------------------------------------------
// <copyright file="ProjectSettingsTemplateFiles.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public class OverrideTemplateFiles : AssetModificationProcessor
    {
        public static void OnWillCreateAsset(string assetPath)
        {
            if (assetPath.EndsWith(".cs") && ProjectSettingsEditorTools.Instance.OverrideTemplateFiles)
            {
                OverrideCSharpTemplateFiles(assetPath);
            }
        }

        private static void OverrideCSharpTemplateFiles(string csharpAssetPath)
        {
            EditorApplication.delayCall += () =>
            {
                // Getting the new template files
                TextAsset templateFile = GetTemplateTextAsset(csharpAssetPath);

                if (templateFile == null)
                {
                    Debug.Log("OGT: Override Template File: No template file found for: " + csharpAssetPath);
                    return;
                }

                // Determining the company name and namespace
                bool isLostFolder = csharpAssetPath.StartsWith("Packages/com.lostsignal.");
                string companyName = "Lost Signal LLC";
                string nameSpace = "Lost";

                if (isLostFolder == false)
                {
                    companyName = string.IsNullOrWhiteSpace(PlayerSettings.companyName) ? "Player Settings Company Not Defined" : PlayerSettings.companyName;
                    nameSpace = string.IsNullOrWhiteSpace(EditorSettings.projectGenerationRootNamespace) ? "Editor Settings RootNamespace Not Defined" : EditorSettings.projectGenerationRootNamespace;
                }

                // Getting the script name and the template file to use
                string scriptName = Path.GetFileNameWithoutExtension(csharpAssetPath);

                // Writing the C# File
                string fileContents = templateFile == null ? File.ReadAllText(csharpAssetPath) : templateFile.text;

                fileContents = fileContents.Replace("#COMPANY_NAME#", companyName)
                    .Replace("#ROOTNAMESPACE#", nameSpace)
                    .Replace("#SCRIPTNAME#", scriptName)
                    .Replace("#NOTRIM#", string.Empty);

                File.WriteAllText(csharpAssetPath, FileUtil.ConvertLineEndings(fileContents));
                AssetDatabase.Refresh();
            };
        }

        private static TextAsset GetTemplateTextAsset(string assetPath)
        {
            string fileContents = File.ReadAllText(assetPath);

            if (fileContents.Contains(": PlayableAsset"))
            {
                return ProjectSettingsEditorTools.Instance.TemplatePlayableAsset;
            }
            else if (fileContents.Contains(": PlayableBehaviour"))
            {
                return ProjectSettingsEditorTools.Instance.TemplatePlayableBehaviour;
            }
            else if (fileContents.Contains(": StateMachineBehaviour"))
            {
                if (fileContents.Contains("OnStateMachineEnter"))
                {
                    return ProjectSettingsEditorTools.Instance.TemplateSubStateMachineBehaviour;
                }
                else
                {
                    return ProjectSettingsEditorTools.Instance.TemplateStateMachineBehaviour;
                }
            }
            else if (fileContents.Contains("[Test]"))
            {
                return ProjectSettingsEditorTools.Instance.TemplateEditorTestScript;
            }
            else if (fileContents.Contains(": MonoBehaviour"))
            {
                return ProjectSettingsEditorTools.Instance.TemplateMonoBehaviour;
            }
            else if (fileContents.Contains(": ScriptableObject"))
            {
                return ProjectSettingsEditorTools.Instance.TemplateScriptableObject;
            }
            else if (fileContents.Contains(":") == false)
            {
                return ProjectSettingsEditorTools.Instance.TemplateEmptyCSharp;
            }

            return null;
        }
    }
}
