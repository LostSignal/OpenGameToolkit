//-----------------------------------------------------------------------
// <copyright file="SourceControlUtils.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using SourceControlType = ProjectSettingsEditorTools.SourceControlType;

    public static class SourceControlUtils
    {
        private static readonly OGTLogger Logger = OGTLogger.OGTEditor;

        [EditorEvents.InitializeOnLoad]
        private static void AutoApplySourceControlSettings()
        {
            EditorApplication.delayCall += ApplyAutoSettings;
        }

        private static void ApplyAutoSettings()
        {
            AutoSetP4IgnoreEnvironmentVariable();
            AutoSetPlasticSCMSettings();
        }

        public static void GenerateSourceControlIgnoreFile()
        {
            Generate(
                ProjectSettingsEditorTools.Instance.SourceControl,
                ProjectSettingsEditorTools.Instance.IgnoreTemplatePlastic,
                ProjectSettingsEditorTools.Instance.IgnoreTemplateGit,
                ProjectSettingsEditorTools.Instance.IgnoreTemplateP4,
                ProjectSettingsEditorTools.Instance.P4IgnoreFileName);

            static void Generate(SourceControlType sourceControlType,
                TextAsset ignoreTemplatePlastic,
                TextAsset ignoreTemplateGit,
                TextAsset ignoreTemplateP4,
                string p4IgnoreFileName)
            {
                if (sourceControlType == SourceControlType.Plastic)
                {
                    var currentUnityDirectoryInfo = new DirectoryInfo(".");
                    var currentUnityDirectoryPath = currentUnityDirectoryInfo.FullName.Replace("\\", "/");
                    var plasticDirectoryPath = FindPlasticRootDirectoryPath(currentUnityDirectoryInfo);

                    if (string.IsNullOrEmpty(plasticDirectoryPath))
                    {
                        Logger.LogError("Unable to find the root of the Plastic repository.  File was not created.");
                        return;
                    }

                    string relativeUnityDirectory = currentUnityDirectoryPath != plasticDirectoryPath ?
                        "/" + currentUnityDirectoryPath.Substring(plasticDirectoryPath.Length + 1).Replace("\\", "/") :
                        string.Empty;

                    File.WriteAllText(
                        Path.Combine(plasticDirectoryPath, "ignore.conf"),
                        ignoreTemplatePlastic.text.Replace("{UNITY_PROJECT_DIRECTORY}", relativeUnityDirectory));
                }
                else if (sourceControlType == SourceControlType.Perforce)
                {
                    File.WriteAllText(p4IgnoreFileName, ignoreTemplateP4.text);
                }
                else if (sourceControlType == SourceControlType.Git)
                {
                    File.WriteAllText(".gitignore", ignoreTemplateGit.text);
                }
            }
        }

        private static void AutoSetP4IgnoreEnvironmentVariable()
        {
            Apply(
                ProjectSettingsEditorTools.Instance.SourceControl,
                ProjectSettingsEditorTools.Instance.AutosetP4IgnoreEnvironmentVariable,
                ProjectSettingsEditorTools.Instance.P4IgnoreFileName);

            static void Apply(SourceControlType sourceControl, bool autosetP4IgnoreEnvironmentVariable, string p4IgnoreFileName)
            {
                if (Application.platform == RuntimePlatform.WindowsEditor &&
                    sourceControl == SourceControlType.Perforce &&
                    autosetP4IgnoreEnvironmentVariable &&
                    p4IgnoreFileName != GetCurrentP4IgnoreVariableWindows())
                {
                    SetP4IgnoreVariableForWindows(p4IgnoreFileName);
                }
            }

            static string GetCurrentP4IgnoreVariableWindows()
            {
                try
                {
                    var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "p4",
                        Arguments = "set P4IGNORE",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                    });

                    return process.StandardOutput.ReadToEnd().Replace("P4IGNORE=", string.Empty).Replace("(set)", string.Empty).Trim();
                }
                catch
                {
                    return null;
                }
            }

            static void SetP4IgnoreVariableForWindows(string p4ignoreFileName)
            {
                try
                {
                    System.Diagnostics.Process.Start("p4", "set P4IGNORE=" + p4ignoreFileName);
                }
                catch
                {
                    Logger.LogError("Unable To Set P4IGNORE Variable.  Is P4 installed?");
                }
            }
        }

        private static void AutoSetPlasticSCMSettings()
        {
            Apply(
                ProjectSettingsEditorTools.Instance.SourceControl,
                ProjectSettingsEditorTools.Instance.PlasticAutoSetFileCasingError,
                ProjectSettingsEditorTools.Instance.PlasticAutoSetYamlMergeToolPath);

            static void Apply(SourceControlType sourceControl, bool plasticAutoSetFileCasingError, bool plasticAutoSetYamlMergeToolPath)
            {
                if (sourceControl != SourceControlType.Plastic)
                {
                    return;
                }

                PlasticSCM.UpdateClientConfigSettings(PlasticSCM.GetClientConfigPath(), plasticAutoSetFileCasingError, plasticAutoSetYamlMergeToolPath);
            }
        }

        private static string FindPlasticRootDirectoryPath(DirectoryInfo directory)
        {
            string directoryPath = directory.FullName.Replace("\\", "/");

            if (Directory.Exists(Path.Combine(directoryPath, ".plastic")))
            {
                return directoryPath;
            }
            else if (string.IsNullOrEmpty(directory.Parent?.FullName) == false)
            {
                return FindPlasticRootDirectoryPath(directory.Parent);
            }
            else
            {
                return null;
            }
        }
    }
}
