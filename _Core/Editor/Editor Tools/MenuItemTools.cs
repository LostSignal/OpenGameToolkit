//-----------------------------------------------------------------------
// <copyright file="MenuItemTools.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.IO;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEditor.VersionControl;
    using UnityEngine;

    public static class MenuItemTools
    {
        private static readonly OGTLogger Logger = OGTLogger.OGTEditor;

        [MenuItem("Tools/OGT/Utility/Remove Empty Directories (Selected Directory)", priority = MenuItemPriorities.Utility + 0)]
        public static void RemoveEmptyDirectoriesFromSelectedDirectory()
        {
            if (Selection.objects?.Length != 1)
            {
                Logger.LogError("No Folder Object selected to remove directories from.");
                return;
            }

            var assetPath = AssetDatabase.GetAssetPath(Selection.objects[0]);
            var fullPath = AssetDatabase.GetAssetPath(Selection.objects[0]);

            if (Directory.Exists(fullPath) == false)
            {
                Logger.LogError("Selected object is not a directory.");
                return;
            }

            FileUtil.RemoveEmptyDirectories(fullPath);
            EditorApplication.delayCall += AssetDatabase.Refresh;
        }

        [MenuItem("Tools/OGT/Utility/Cleanup C# Files (Selected Directory)", priority = MenuItemPriorities.Utility + 1)]
        public static void ConvertAllCSharpFiles()
        {
            ConvertAllCSharpFiles(false);
        }

        [MenuItem("Tools/OGT/Utility/Disable Warnings In C# Files (Selected Directory)", priority = MenuItemPriorities.Utility + 2)]
        public static void DisableWarnings()
        {
            ConvertAllCSharpFiles(true);
            ConvertAllCSharpFiles(false);
        }

        [MenuItem("Tools/OGT/Utility/Force Recompile All Code", priority = MenuItemPriorities.Utility + 3)]
        public static void ForceRecompile()
        {
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation(UnityEditor.Compilation.RequestScriptCompilationOptions.CleanBuildCache);
        }

        [MenuItem("Tools/OGT/Utility/Get All Components In Open Prefab", priority = MenuItemPriorities.Utility + 4)]
        public static void GetAllComponentsInOpenPrefab()
        {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            if (prefabStage == null || prefabStage.prefabContentsRoot == null)
            {
                Logger.LogError("No prefab is currently open in Prefab Mode.");
                return;
            }

            var components = prefabStage.prefabContentsRoot
                .GetComponentsInChildren<Component>(true)
                .Select(t => t.GetType().FullName)
                .Distinct()
                .OrderBy(t => t);

            Logger.Log("Found Components:\n" + string.Join("\n", components));
        }

        [MenuItem("Tools/OGT/Utility/Get All Images In Open Prefab", priority = MenuItemPriorities.Utility + 5)]
        public static void GetAllImagesInOpenPrefab()
        {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();

            if (prefabStage == null || prefabStage.prefabContentsRoot == null)
            {
                Logger.LogError("No prefab is currently open in Prefab Mode.");
                return;
            }

            var images = prefabStage.prefabContentsRoot
                .GetComponentsInChildren<UnityEngine.UI.Image>(true)
                .Where(image => image.sprite != null)
                .Select(x => AssetDatabase.GetAssetPath(x.sprite))
                .Distinct()
                .OrderBy(t => t);

            Logger.Log("Images Found:\n\n" + string.Join("\n", images) + "\n");
        }

        [MenuItem("Tools/OGT/Generate Menu Items", priority = MenuItemPriorities.GenerateMenuItems)]
        public static void GenerateMenuItemsFile() => GenerateMenuItems.Generate();

        public static void ConvertAllCSharpFiles(bool disableWarnings)
        {
            string path = ".";

            // If a directory is selected, then only convert the things under the directory
            if (Selection.activeObject != null)
            {
                string rootDirectoryAssetPath = AssetDatabase.GetAssetPath(Selection.activeObject.GetEntityId());

                if (Directory.Exists(rootDirectoryAssetPath))
                {
                    path = rootDirectoryAssetPath;
                }
            }

            foreach (string file in Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories))
            {
                StringBuilder fileBuilder = new();

                string fileContents = FileUtil.ConvertLineEndings(File.ReadAllText(file), LineEndingsMode.Unix);
                string[] lines = fileContents.Split('\n');
                int lastLineIndex = lines.Length - 1;

                // Calculating the line of the file that actually has content
                while (lastLineIndex > 0 && string.IsNullOrEmpty(lines[lastLineIndex].Trim()))
                {
                    lastLineIndex--;
                }

                if (disableWarnings)
                {
                    fileBuilder.Append("#pragma warning disable\n\n");
                }

                // Converting each line to our project standards
                for (int i = 0; i <= lastLineIndex; i++)
                {
                    string convertedLine = lines[i];

                    convertedLine = convertedLine.TrimEnd();              // trim_trailing_whitespace = true
                    convertedLine = convertedLine.Replace("\t", "    ");  // indent_style = space, indent_size = 4
                    convertedLine += '\n';                                // insert_final_newline = true, end_of_line = lf

                    fileBuilder.Append(convertedLine);
                }

                // Generating the final file bytes
                MemoryStream finalFileContents = new();
                var utf8Encoder = new UTF8Encoding(true);
                var preamble = utf8Encoder.GetPreamble();

                if (preamble != null && preamble.Length > 0)
                {
                    finalFileContents.Write(preamble, 0, preamble.Length);
                }

                byte[] contents = utf8Encoder.GetBytes(FileUtil.ConvertLineEndings(fileBuilder.ToString()));
                finalFileContents.Write(contents, 0, contents.Length);

                byte[] finalFileBytes = finalFileContents.ToArray();

                // Checking if the file needs to be saved out
                if (AreByteArraysEqual(File.ReadAllBytes(file), finalFileBytes) == false)
                {
                    Logger.Log("Updating File " + file);

                    var asset = file.Replace("\\", "/").Replace("./", string.Empty);

                    if (asset.StartsWith("Packages/") == false && Provider.isActive)
                    {
                        Provider.Checkout(asset, CheckoutMode.Asset).Wait();
                    }

                    File.WriteAllBytes(file, finalFileBytes);
                }
            }
        }

        public static void GenerateFileFromTextAsset(string displayName, string filePath, string textAssetGuid)
        {
            var textAsset = EditorUtil.GetAssetByGuid<TextAsset>(textAssetGuid);

            if (textAsset != null)
            {
                if (File.Exists(filePath) == false)
                {
                    FileUtil.CreateFile(textAsset.text, filePath, false);
                }
                else
                {
                    try
                    {
                        FileUtil.UpdateFile(textAsset.text, filePath, false);
                    }
                    catch
                    {
                        Logger.LogErrorFormat("Unable to update {0} file. Is it read only?", displayName);
                    }
                }
            }
            else
            {
                Logger.LogErrorFormat("Unable to find {0} asset file!", displayName);
            }
        }

        private static bool AreByteArraysEqual(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
