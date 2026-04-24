//-----------------------------------------------------------------------
// <copyright file="ReferenceFinder.cs" company="Lost Signal LLC">
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
    using UnityEngine;

    public static class ReferenceFinder
    {
        private static readonly OGTLogger Logger = OGTLogger.OGTEditor;

        [MenuItem("Tools/OGT/References/Find All Outside References (Selected Directory)", priority = MenuItemPriorities.References + 0)]
        private static void FindAllOutsideReferences()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (Directory.Exists(path) == false)
            {
                Logger.LogError("You must select a directory.");
                return;
            }

            HashSet<string> dependencies = new HashSet<string>();

            foreach (var assetGuid in AssetDatabase.FindAssets(string.Empty, new string[] { path }))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);

                foreach (var asset in AssetDatabase.GetDependencies(assetPath, true))
                {
                    if (asset.StartsWith(path) ||
                        asset.StartsWith("Packages/com.unity.") ||
                        asset.Contains("/TextMesh Pro/"))
                    {
                        continue;
                    }

                    dependencies.AddIfUnique(asset);
                }
            }

            foreach (var dependency in dependencies.OrderBy(x => x))
            {
                Logger.Log(dependency, AssetDatabase.LoadAssetAtPath<Object>(dependency));
            }
        }

        [MenuItem("Tools/OGT/References/Find All References Outside Selected Directories", priority = MenuItemPriorities.References + 1)]
        private static void FindAllReferencesInAssets()
        {
            var selectedDirecoryGuids = GetSelectedDirectoryGuids();

            if (selectedDirecoryGuids == null)
            {
                return;
            }

            var dependentAssets = new Dictionary<string, HashSet<string>>();

            foreach (var assetGuid in AssetDatabase.FindAssets(string.Empty))
            {
                if (selectedDirecoryGuids.Contains(assetGuid))
                {
                    continue;
                }

                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);

                if (assetPath.StartsWith("Assets/") == false)
                {
                    continue;
                }

                foreach (var dependentAsset in AssetDatabase.GetDependencies(assetPath, true))
                {
                    var dependentAssetGuid = AssetDatabase.AssetPathToGUID(dependentAsset);

                    if (selectedDirecoryGuids.Contains(dependentAssetGuid))
                    {
                        if (dependentAssets.ContainsKey(dependentAsset))
                        {
                            dependentAssets[dependentAsset].Add(assetPath);
                        }
                        else
                        {
                            dependentAssets.Add(dependentAsset, new HashSet<string> { assetPath });
                        }
                    }
                }
            }

            var builder = new StringBuilder();
            foreach (var dependentAsset in dependentAssets)
            {
                builder.Clear();
                builder.AppendLine(dependentAsset.Key);

                foreach (var asset in dependentAsset.Value)
                {
                    builder.Append("    ");
                    builder.AppendLine(asset);
                }

                Debug.Log(builder.ToString());
            }

            HashSet<string> GetSelectedDirectoryGuids()
            {
                HashSet<string> guids = new HashSet<string>();

                foreach (var selectedObject in Selection.objects)
                {
                    string path = AssetDatabase.GetAssetPath(selectedObject);

                    if (Directory.Exists(path) == false)
                    {
                        Logger.LogError("You must select a directory.");
                        return null;
                    }

                    Debug.Log("Scanning Folder: " + path);
                    foreach (var metaFile in Directory.GetFiles(path, "*.meta", SearchOption.AllDirectories))
                    {
                        var metaFileContents = File.ReadAllLines(metaFile);

                        foreach (var line in metaFileContents)
                        {
                            if (line.StartsWith("guid: "))
                            {
                                guids.Add(line.Replace("guid:", string.Empty).Trim());
                                break;
                            }
                        }
                    }
                }

                return guids;
            }
        }
    }
}
