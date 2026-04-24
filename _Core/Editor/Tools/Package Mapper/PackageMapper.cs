//-----------------------------------------------------------------------
// <copyright file="PackageMapper.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using UnityEditor;

    public static class PackageMapper
    {
        private const string MapToLocalSource = "Assets/OGT/Package Mapper/Map Package(s) To Local Source";
        private const string MapToGithubSource = "Assets/OGT/Package Mapper/Map Package(s) To Latest GitHub";

        private const string PackageMappingsPath = ".packagemappings";
        private const string ManifestPath = "./Packages/manifest.json";
        private const string PackagesPath = "Packages/";

        private static readonly OGTLogger Logger = OGTLogger.OGTEditor;

        public enum Mode
        {
            None,
            Git,
            Folder,
        }

        [MenuItem(MapToLocalSource, false, priority = -100)]
        public static void UpdateToLocalSource()
        {
            foreach (var selectedObject in Selection.objects)
            {
                SwitchRepositoryTo(selectedObject, Mode.Folder);
            }
        }

        [MenuItem(MapToLocalSource, true, priority = -100)]
        public static bool UpdateToLocalSourceValidate()
        {
            return IsSelectionAllValidPackages();
        }

        [MenuItem(MapToGithubSource, false, priority = -100)]
        public static void UpdateToLatestGitHub()
        {
            foreach (var selectedObject in Selection.objects)
            {
                SwitchRepositoryTo(selectedObject, Mode.Git);
            }
        }

        [MenuItem(MapToGithubSource, true, priority = -100)]
        public static bool UpdateToLatestGitHubValidate()
        {
            return IsSelectionAllValidPackages();
        }

        private static bool IsSelectionAllValidPackages()
        {
            if (Selection.objects == null || Selection.objects.Length == 0)
            {
                return false;
            }

            var mappings = GetMappings();
            var manifest = GetManifest();

            foreach (var selectedObject in Selection.objects)
            {
                var packageName = GetPackageName(selectedObject);

                bool isPackage = string.IsNullOrWhiteSpace(packageName) == false;
                bool hasMapping = mappings.Any(x => x.PackageIdentifier == packageName);
                bool isGit = GetPackageMode(manifest, packageName) == Mode.Git;

                if (isPackage == false || (hasMapping == false && isGit == false))
                {
                    return false;
                }
            }

            return true;
        }

        private static List<PackageMapping> GetMappings()
        {
            List<PackageMapping> result = null;

            if (File.Exists(PackageMappingsPath))
            {
                string json = File.ReadAllText(PackageMappingsPath);
                result = JsonUtil.Deserialize<List<PackageMapping>>(json);
            }

            return result ?? new List<PackageMapping>();
        }

        private static string[] GetManifest()
        {
            return File.ReadAllLines(ManifestPath).Where(x => string.IsNullOrEmpty(x) == false).ToArray();
        }

        private static void SaveMappings(List<PackageMapping> mappings)
        {
            File.WriteAllText(PackageMappingsPath, JsonUtil.Serialize(mappings));
        }

        private static void SwitchRepositoryTo(UnityEngine.Object selectedObject, Mode newMode)
        {
            var manifest = GetManifest();
            var packageName = GetPackageName(selectedObject);
            var currentMode = GetPackageMode(manifest, packageName);

            var mappings = GetMappings();
            var mapping = mappings.FirstOrDefault(x => x.PackageIdentifier == packageName);

            // Making a new mapping and saving it
            if (mapping == null)
            {
                mapping = new PackageMapping
                {
                    PackageIdentifier = packageName,
                    GitUrl = GetGitPath(manifest, packageName),
                    LocalPath = GetFilePath(manifest, packageName),
                };

                if (string.IsNullOrWhiteSpace(mapping.GitUrl))
                {
                    // TODO [bgish]: Prompt user for a Git Url
                }

                if (string.IsNullOrWhiteSpace(mapping.LocalPath))
                {
                    string folder = mappings.Count > 0 ? new DirectoryInfo(mappings[0].LocalPath).Parent.FullName : string.Empty;
                    mapping.LocalPath = EditorUtility.OpenFolderPanel("Local Git Repository Directory", folder, string.Empty);
                }

                mappings.Add(mapping);
                SaveMappings(mappings);
            }

            // Making sure we have valid values
            if (string.IsNullOrWhiteSpace(mapping.LocalPath))
            {
                Logger.LogError($"Skipping mapping package {mapping.PackageIdentifier}, no local path found.");
                return;
            }

            if (string.IsNullOrWhiteSpace(mapping.GitUrl))
            {
                Logger.LogError($"Skipping mapping package {mapping.PackageIdentifier}, no git url found.");
                return;
            }

            // Updating the manifest
            var newFileContents = new StringBuilder();

            foreach (var line in File.ReadAllLines(ManifestPath))
            {
                if (line.Contains($"\"{mapping.PackageIdentifier}\""))
                {
                    int colonIndex = line.IndexOf(":");
                    newFileContents.Append(line.Substring(0, colonIndex + 1));

                    if (newMode == Mode.Folder)
                    {
                        newFileContents.AppendLine($" \"file:{mapping.LocalPath}\",");
                    }
                    else
                    {
                        newFileContents.AppendLine($" \"{mapping.GitUrl}#{GetLastestGitHash(mapping.LocalPath)}\",");
                    }
                }
                else
                {
                    newFileContents.AppendLine(line);
                }
            }

            File.WriteAllText(ManifestPath, newFileContents.ToString());
            UnityEditor.PackageManager.Client.Resolve();
        }

        private static string GetLastestGitHash(string gitPath)
        {
            var gitProcess = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "log",
                    WorkingDirectory = gitPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                },
            };

            gitProcess.Start();
            while (gitProcess.StandardOutput.EndOfStream == false)
            {
                return gitProcess.StandardOutput.ReadLine()
                    .Replace("commit ", string.Empty)
                    .Substring(0, 40);
            }

            return null;
        }

        private static string GetPackageName(UnityEngine.Object selectedObject)
        {
            string path = AssetDatabase.GetAssetPath(selectedObject);
            string[] split = path?.Split('/');

            return (path != null && path.StartsWith(PackagesPath) && Directory.Exists(path) && split?.Length == 2) ? split[1] : null;
        }

        private static Mode GetPackageMode(string[] manifest, string packageName)
        {
            return GetFilePath(manifest, packageName) != null ? Mode.Folder :
                GetGitPath(manifest, packageName) != null ? Mode.Git :
                Mode.None;
        }

        private static string GetFilePath(string[] manifest, string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
            {
                return null;
            }

            foreach (var line in manifest.Where(x => x.Contains($"\"{packageName}\"")))
            {
                string fileStart = "\"file:";

                if (line.Contains(fileStart))
                {
                    int fileStartIndex = line.IndexOf(fileStart) + fileStart.Length;
                    int fileEndIndex = line.LastIndexOf("\"");
                    return line.Substring(fileStartIndex, fileEndIndex - fileStartIndex).Replace("\\", "/");
                }
            }

            return null;
        }

        private static string GetGitPath(string[] manifest, string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
            {
                return null;
            }

            foreach (var line in manifest.Where(x => x.Contains($"\"{packageName}\"")))
            {
                if (line.Contains(".git\",") || line.Contains(".git#"))
                {
                    int startIndex = line.IndexOf("\"", line.IndexOf(':')) + 1;
                    int endIndex = line.Contains("#") ? line.IndexOf("#") : line.LastIndexOf("\"");
                    return line.Substring(startIndex, endIndex - startIndex);
                }
            }

            return null;
        }
    }
}
