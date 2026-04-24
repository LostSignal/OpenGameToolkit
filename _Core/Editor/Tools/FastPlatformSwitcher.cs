//-----------------------------------------------------------------------
// <copyright file="FastPlatformSwitcher.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;
    using OGT;
    using UnityEditor;

    public static class FastPlatformSwitcher
    {
        private const string MenuItemFolder = "Tools/OGT/Fast Platform Switch (Beta)/";
        private const string MenuItemStandaloneWindows = MenuItemFolder + "Win";
        private const string MenuItemStandaloneWindows64 = MenuItemFolder + "Win64";
        private const string MenuItemStandaloneOSX = MenuItemFolder + "OSX";
        private const string MenuItemAndroid = MenuItemFolder + "Android";
        private const string MenuItemIOS = MenuItemFolder + "iOS";
        private const string MenuItemPS5 = MenuItemFolder + "PS5";
        private const string MenuItemXbox = MenuItemFolder + "Xbox";

        private static readonly OGTLogger Logger = OGTLogger.OGTEditor;

        private static readonly List<string> IgnoredDirectories = new List<string>
        {
            "TempArtifacts",
            "PackageCache",
            "BurstCache",
            "Search",
        };

        private static readonly HashSet<string> IgnoredFiles = new HashSet<string>
        {
            "ArtifactDB",
            "ArtifactDB-lock",
            "SourceAssetDB",
            "SourceAssetDB-lock",
            "ShaderCache.db",
            "transactions.db",
        };

        public static void SwitchTo(BuildTarget targetPlatform)
        {
            if (targetPlatform == 0)
            {
                Logger.LogWarning("You didn't select a valid Target Platform!");
                return;
            }

            var currentPlatform = EditorUserBuildSettings.activeBuildTarget;

            if (currentPlatform == targetPlatform)
            {
                Logger.LogWarning("You selected the current platform as the Target Platform!");
                return;
            }

            // Don't switch when compiling
            if (EditorApplication.isCompiling)
            {
                Logger.LogWarning("Could not switch platform because Unity is compiling!");
                return;
            }

            // Don't switch while playing
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Logger.LogWarning("Could not switch platform because Unity is in Play Mode!");
                return;
            }

            Logger.Log("Switching platform from " + currentPlatform + " to " + targetPlatform);

            // Save current Library folder state
            if (Directory.Exists("Library-" + currentPlatform))
            {
                DirectoryClear("Library-" + currentPlatform);
            }

            DirectoryCopy("Library", "Library-" + currentPlatform, true);

            // Restore new target Library folder state
            if (Directory.Exists("Library-" + targetPlatform))
            {
                DirectoryClear("Library");
                MoveDirectory("Library-" + targetPlatform, "Library");
            }

            var buildTargetGroup = BuildPipeline.GetBuildTargetGroup(targetPlatform);

            EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, targetPlatform);

            Logger.Log("Platform switched to " + targetPlatform);
        }

        [MenuItem(MenuItemStandaloneWindows, priority = MenuItemPriorities.FastPlatformSwitch + 0)]
        private static void SwitchPlatformToWindowsDesktop() => SwitchTo(BuildTarget.StandaloneWindows);

        [MenuItem(MenuItemStandaloneWindows, validate = true)]
        private static bool SwitchPlatformToWindowsDesktopValidate() => IsActive(BuildTarget.StandaloneWindows);

        [MenuItem(MenuItemStandaloneWindows64, priority = MenuItemPriorities.FastPlatformSwitch + 1)]
        private static void SwitchPlatformToWindowsDesktop64() => SwitchTo(BuildTarget.StandaloneWindows64);

        [MenuItem(MenuItemStandaloneWindows64, validate = true)]
        private static bool SwitchPlatformToWindowsDesktop64Validate() => IsActive(BuildTarget.StandaloneWindows64);

        [MenuItem(MenuItemStandaloneOSX, priority = MenuItemPriorities.FastPlatformSwitch + 2)]
        private static void SwitchPlatformToOSX() => SwitchTo(BuildTarget.StandaloneOSX);

        [MenuItem(MenuItemStandaloneOSX, validate = true)]
        private static bool SwitchPlatformToOSXValidate() => IsActive(BuildTarget.StandaloneOSX);

        [MenuItem(MenuItemIOS, priority = MenuItemPriorities.FastPlatformSwitch + 3)]
        private static void SwitchPlatformToIOS() => SwitchTo(BuildTarget.iOS);

        [MenuItem(MenuItemIOS, validate = true)]
        private static bool SwitchPlatformToIOSValidate() => IsActive(BuildTarget.iOS);

        [MenuItem(MenuItemAndroid, priority = MenuItemPriorities.FastPlatformSwitch + 4)]
        private static void SwitchPlatformToAndroid() => SwitchTo(BuildTarget.Android);

        [MenuItem(MenuItemAndroid, validate = true)]
        private static bool SwitchPlatformToAndroidValidate() => IsActive(BuildTarget.Android);

        [MenuItem(MenuItemPS5, priority = MenuItemPriorities.FastPlatformSwitch + 5)]
        private static void SwitchPlatformToPS5() => SwitchTo(BuildTarget.PS5);

        [MenuItem(MenuItemPS5, validate = true)]
        private static bool SwitchPlatformToPS5Validate() => IsActive(BuildTarget.PS5);

        [MenuItem(MenuItemXbox, priority = MenuItemPriorities.FastPlatformSwitch + 6)]
        private static void SwitchPlatformToXbox() => SwitchTo(BuildTarget.GameCoreXboxOne);

        [MenuItem(MenuItemXbox, validate = true)]
        private static bool SwitchPlatformToXboxValidate() => IsActive(BuildTarget.GameCoreXboxOne);

        private static bool IsActive(BuildTarget buildTarget) => EditorUserBuildSettings.activeBuildTarget != buildTarget;

        private static void DirectoryClear(string FolderName)
        {
            DirectoryInfo dir = new DirectoryInfo(FolderName);

            foreach (FileInfo fileInfo in dir.GetFiles())
            {
                if (IsFileBlacklisted(fileInfo.Name, fileInfo.FullName))
                {
                    continue;
                }

                fileInfo.Delete();
            }

            foreach (DirectoryInfo directoryInfo in dir.GetDirectories())
            {
                if (IsIgnoredDirectory(directoryInfo.FullName))
                {
                    continue;
                }

                DirectoryClear(directoryInfo.FullName);

                try
                {
                    directoryInfo.Delete(true);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error Deleting Directory - " + directoryInfo.FullName);
                    Logger.LogException(ex);
                }
            }
        }

        private static bool IsIgnoredDirectory(string fullPath)
        {
            fullPath = fullPath.Replace("\\", "/");

            bool isDirectory = Directory.Exists(fullPath);

            foreach (var directory in IgnoredDirectories)
            {
                if (fullPath.Contains($"/{directory}/"))
                {
                    return true;
                }
                else if (isDirectory && fullPath.EndsWith($"/{directory}"))
                {
                    return true;
                }
            }

            return false;
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (dir.Exists == false)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
            }

            bool destinationDirCreated = false;

            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                if (IsFileBlacklisted(file.Name, file.FullName))
                {
                    continue;
                }

                // If the destination directory does not exist, create it.
                if (destinationDirCreated == false && Directory.Exists(destDirName) == false)
                {
                    Directory.CreateDirectory(destDirName);
                    destinationDirCreated = true;
                }

                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, false);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private static void MoveDirectory(string source, string target)
        {
            var sourcePath = source.TrimEnd('\\', ' ');
            var targetPath = target.TrimEnd('\\', ' ');
            var files = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories)
                .GroupBy(s => Path.GetDirectoryName(s));

            foreach (var folder in files)
            {
                var targetFolder = folder.Key.Replace(sourcePath, targetPath);
                var directoryCreated = false;

                foreach (var file in folder)
                {
                    if (IsFileBlacklisted(Path.GetFileName(file), file))
                    {
                        continue;
                    }

                    if (directoryCreated == false)
                    {
                        Directory.CreateDirectory(targetFolder);
                        directoryCreated = true;
                    }

                    var targetFile = Path.Combine(targetFolder, Path.GetFileName(file));

                    if (File.Exists(targetFile))
                    {
                        File.Delete(targetFile);
                    }

                    File.Move(file, targetFile);
                }
            }

            Directory.Delete(source, true);
        }

        private static bool IsFileBlacklisted(string filename, string fullPath)
        {
            return IgnoredFiles.Contains(filename) ||
                   IsIgnoredDirectory(fullPath) ||
                   filename.StartsWith("shadercompiler-UnityShaderCompiler.exe");
        }
    }
}
