//-----------------------------------------------------------------------
// <copyright file="EditorConfigTools.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.IO;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    public class EditorConfigTools : AssetPostprocessor
    {
        private static bool UseEditorConfig => ProjectSettingsEditorTools.Instance.UseEditorConfig;

        private static string EditorConfigFileName => ProjectSettingsEditorTools.Instance.EditorConfigFileName;

        private static TextAsset EditorConfigFileTempate => ProjectSettingsEditorTools.Instance.EditorConfigTempate;

        public static string OnGeneratedSlnSolution(string path, string content)
        {
            if (UseEditorConfig)
            {
                CreateEditorconfigFile();
                content = AddEditorConfigToSolution(path, content);
            }

            return content;
        }

        public static string OnGeneratedCSProject(string path, string content)
        {
            if (UseEditorConfig)
            {
                CreateEditorconfigFile();
                content = AddEditorConfigToCSProj(content);
            }

            return content;
        }

        private static void CreateEditorconfigFile()
        {
            if (EditorConfigFileName.IsNullOrWhitespace() == false &&
                File.Exists(EditorConfigFileName) == false &&
                EditorConfigFileTempate != null &&
                string.IsNullOrWhiteSpace(EditorConfigFileTempate.text) == false)
            {
                File.WriteAllText(EditorConfigFileName, EditorConfigFileTempate.text);
            }
        }

        private static string AddEditorConfigToSolution(string path, string solutionContents)
        {
            if (UseEditorConfig && solutionContents.Contains(EditorConfigFileName) == false)
            {
                if (path.ToLower().EndsWith(".sln"))
                {
                    int globalIndex = solutionContents.IndexOf("Global");

                    if (globalIndex == -1)
                    {
                        Debug.LogError("Failed to find Global section of the solution file. Cannot add .editorconfig to solution items.");
                    }
                    else
                    {
                        return solutionContents.Insert(globalIndex, GetEditorconfigString());
                    }
                }
                else if (path.ToLower().EndsWith(".slnx"))
                {
                    int solutionIndex = solutionContents.IndexOf("<Solution>");

                    if (solutionIndex == -1)
                    {
                        Debug.LogError("Failed to find Solution section of the solution file. Cannot add .editorconfig to solution items.");
                    }
                    else
                    {
                        return solutionContents.Insert(
                            solutionIndex + "<Solution>".Length,
                            "\r\n  <Folder Name=\"/Solution Items/\">\r\n    <File Path=\".editorconfig\" />\r\n  </Folder>");
                    }
                }
                else
                {
                    Debug.LogError($"Unknown Solution Type '{path}' found!");
                }
            }

            return solutionContents;

            static string GetEditorconfigString()
            {
                var builder = new StringBuilder();
                builder.AppendLine("Project(\"{2150E333-8FDC-42A3-9474-1A3956D46DE8}\") = \"Solution Items\", \"Solution Items\", \"{NEW_GUID}\"");
                builder.AppendLine("\tProjectSection(SolutionItems) = preProject");
                builder.AppendLine("\t\t.editorconfig = .editorconfig");
                builder.AppendLine("\tEndProjectSection");
                builder.AppendLine("EndProject");

                return builder.ToString().Replace("NEW_GUID", Guid.NewGuid().ToString().ToUpper());
            }
        }

        private static string AddEditorConfigToCSProj(string csProjContents)
        {
            var editorconfigInclude = $"<None Include=\"{EditorConfigFileName}\" />";

            if (UseEditorConfig && csProjContents.Contains(editorconfigInclude) == false)
            {
                var itemGroup = new StringBuilder();
                itemGroup.AppendLine($"  <ItemGroup>");
                itemGroup.AppendLine($"    {editorconfigInclude}");
                itemGroup.AppendLine($"  </ItemGroup>");

                int firstItemGroupIndex = csProjContents.IndexOf("  <ItemGroup>");
                return csProjContents.Insert(firstItemGroupIndex, itemGroup.ToString());
            }
            else
            {
                return csProjContents;
            }
        }
    }
}
