//-----------------------------------------------------------------------
// <copyright file="LostSettingsRosylnAnalyzers.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    public class ProjectSettingsRosylnAnalyzers : ProjectSettingsBase<ProjectSettingsRosylnAnalyzers>
    {
#pragma warning disable 0649
        [SerializeField] private bool applyRosylnAnalyzers;
        [SerializeField] private List<Analyzer> analyzers;
#pragma warning restore 0649

        public override string AssetName => nameof(ProjectSettingsRosylnAnalyzers);

        public override void LoadDefaults()
        {
            this.applyRosylnAnalyzers = true;

            this.analyzers = new List<Analyzer>
            {
                new Analyzer()
                {
                    Name = "StyleCop",
                    Ruleset = EditorUtil.GetAssetByGuid<TextAsset>("6d22bf8a5b4217246a8bd27939b3a093"),
                    Config = EditorUtil.GetAssetByGuid<TextAsset>("447a0d2defa062a4cb1ab9f0a161d7f7"),
                    DLLs = new List<TextAsset>
                    {
                        EditorUtil.GetAssetByGuid<TextAsset>("34b2bcdbab6772c43803d97146553550"),
                        EditorUtil.GetAssetByGuid<TextAsset>("fdf22cdd44a87ed4f9ae0c0d6e685ae6"),
                        EditorUtil.GetAssetByGuid<TextAsset>("d86a7268d4b5874478f3bf9019de4dd3"),
                    },
                    CSProjects = new List<string>
                    {
                        "LostCore",
                        "LostCore.Editor",
                        "LostCore",
                        "LostCore.Editor",
                        "LostCore.Tests",
                    },
                },
            };
        }

        public string AddAnalyzersToCSProjects(string csProjFilePath, string csprojFileContents)
        {
            if (this.applyRosylnAnalyzers == false)
            {
                return csprojFileContents;
            }

            var fileName = Path.GetFileNameWithoutExtension(csProjFilePath);

            if (this.analyzers?.Count > 0)
            {
                for (int i = 0; i < this.analyzers.Count; i++)
                {
                    var analyzer = this.analyzers[i];

                    if (analyzer.CSProjects.Contains(fileName))
                    {
                        csprojFileContents = AddAnalyzerToCSProj(csprojFileContents, analyzer, i);
                    }
                }
            }

            return csprojFileContents;
        }

        private static string AddAnalyzerToCSProj(string contents, Analyzer analyzer, int analyzerIndex)
        {
            var additionalFiles = new List<string>();
            var ruleSets = new List<string>();
            var analyzers = new List<string>();

            if (analyzer.Config != null)
            {
                additionalFiles.Add(FullPath(analyzer.Config));
            }

            if (analyzer.Ruleset != null)
            {
                ruleSets.Add(FullPath(analyzer.Ruleset));
            }

            if (analyzer.DLLs != null)
            {
                for (int i = 0; i < analyzer.DLLs.Count; i++)
                {
                    if (analyzer.DLLs[i] != null)
                    {
                        analyzers.Add(CreateDLL(analyzer.DLLs[i]));
                    }
                }
            }

            return UpdateCSProjFile(contents, additionalFiles, ruleSets, analyzers);

            string CreateDLL(TextAsset dllAsset)
            {
                var sourceFilePath = FullPath(dllAsset);
                var sourceFileBytes = File.ReadAllBytes(sourceFilePath);

                string dllDirectory = $"./Library/Analyzers/{analyzerIndex}_{analyzer.Name}";

                if (Directory.Exists(dllDirectory) == false)
                {
                    Directory.CreateDirectory(dllDirectory);
                }

                string dllName = Path.GetFileNameWithoutExtension(sourceFilePath);
                string fullDllPath = Path.GetFullPath(Path.Combine(dllDirectory, dllName));

                if (File.Exists(fullDllPath))
                {
                    File.Delete(fullDllPath);
                }

                File.WriteAllBytes(fullDllPath, sourceFileBytes);

                return fullDllPath;
            }

            string FullPath(UnityEngine.Object obj) => Path.GetFullPath(AssetDatabase.GetAssetPath(obj));
        }

        private static string UpdateCSProjFile(string csprojFileContents, List<string> additionalFiles, List<string> rulesetFiles, List<string> analyzerDlls)
        {
            var lines = csprojFileContents.Replace("\r\n", "\n").Split("\n").ToList();

            AddRulesets(rulesetFiles, lines);
            AddAnalyzerDLLs(analyzerDlls, lines);
            AddAdditionalFiles(additionalFiles, lines);

            return GetFileContents(lines);

            void AddRulesets(List<string> files, List<string> lines)
            {
                if (files == null || files.Count == 0)
                {
                    return;
                }

                int rulesetIndex = GetLineIndex("<CodeAnalysisRuleSet>", lines);
                int firstItemGroupIndex = GetLineIndex("<ItemGroup>", lines);
                var newLines = new List<string>();
                var newLinesAdded = false;

                if (rulesetIndex == -1)
                {
                    newLines.Add("  <PropertyGroup>");
                }

                foreach (var file in files)
                {
                    string newLine = $"   <CodeAnalysisRuleSet>{file}</CodeAnalysisRuleSet>";

                    if (lines.Contains(newLine) == false)
                    {
                        newLinesAdded = true;
                        newLines.Add(newLine);
                    }
                }

                if (rulesetIndex == -1)
                {
                    newLines.Add("  </PropertyGroup>");
                }

                if (newLinesAdded)
                {
                    lines.InsertRange(rulesetIndex != -1 ? rulesetIndex + 1 : firstItemGroupIndex, newLines);
                }
            }

            void AddAdditionalFiles(List<string> files, List<string> lines)
            {
                if (files == null || files.Count == 0)
                {
                    return;
                }

                int additionalFilesIndex = GetLineIndex("<AdditionalFiles ", lines);
                int firstItemGroupIndex = GetLineIndex("<ItemGroup>", lines);
                var newLines = new List<string>();
                var newLinesAdded = false;

                if (additionalFilesIndex == -1)
                {
                    newLines.Add("  <ItemGroup>");
                }

                foreach (var file in files)
                {
                    string newLine = $"    <AdditionalFiles Include=\"{file}\" />";

                    if (lines.Contains(newLine) == false)
                    {
                        newLinesAdded = true;
                        newLines.Add(newLine);
                    }
                }

                if (additionalFilesIndex == -1)
                {
                    newLines.Add("  </ItemGroup>");
                }

                if (newLinesAdded)
                {
                    lines.InsertRange(additionalFilesIndex != -1 ? additionalFilesIndex + 1 : firstItemGroupIndex, newLines);
                }
            }

            void AddAnalyzerDLLs(List<string> files, List<string> lines)
            {
                if (files == null || files.Count == 0)
                {
                    return;
                }

                int analyzerFilesIndex = GetLineIndex("<Analyzer ", lines);
                int firstItemGroupIndex = GetLineIndex("<ItemGroup>", lines);
                var newLines = new List<string>();
                var newLinesAdded = false;

                if (analyzerFilesIndex == -1)
                {
                    newLines.Add("  <ItemGroup>");
                }

                foreach (var file in files)
                {
                    string newLine = $"    <Analyzer Include=\"{file}\" />";

                    if (lines.Contains(newLine) == false)
                    {
                        newLinesAdded = true;
                        newLines.Add(newLine);
                    }
                }

                if (analyzerFilesIndex == -1)
                {
                    newLines.Add("  </ItemGroup>");
                }

                if (newLinesAdded)
                {
                    lines.InsertRange(analyzerFilesIndex != -1 ? analyzerFilesIndex + 1 : firstItemGroupIndex, newLines);
                }
            }

            int GetLineIndex(string startsWith, List<string> lines)
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i] != null && lines[i].Trim().StartsWith(startsWith))
                    {
                        return i;
                    }
                }

                return -1;
            }

            string GetFileContents(List<string> lines)
            {
                var result = new StringBuilder();

                foreach (var line in lines)
                {
                    result.AppendLine(line);
                }

                return result.ToString();
            }
        }
    }
}
