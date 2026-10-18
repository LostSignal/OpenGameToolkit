//-----------------------------------------------------------------------
// <copyright file="BadGuidFixerEditorWindow.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public class BadGuidFixerEditorWindow : EditorWindow
    {
        private const string SettingsFileName = "OGT Bad Guids.json";

        private GuidPairData data;
        private Vector2 scroll;

        private static string SettingsFilePath => Path.Combine("ProjectSettings", SettingsFileName);

        public static void ShowWindow()
        {
            var window = GetWindow<BadGuidFixerEditorWindow>("Bad GUID Fixer");
            window.minSize = new Vector2(650, 300);
            window.LoadSettings();
        }

        [OGT.EditorEvents.InitializeOnLoad]
        private static void CheckForBadGuids()
        {
            var data = LoadGuidPairsData();

            if (data == null || data.pairs == null || data.pairs.Count == 0)
            {
                return;
            }

            // Check if any bad GUIDs exist in the project
            foreach (var pair in data.pairs)
            {
                if (string.IsNullOrWhiteSpace(pair.badGuid))
                    continue;

                if (CheckGuidExistsInProject(pair.badGuid))
                {
                    // Found a bad GUID in the project, show the window
                    ShowWindow();
                    break;
                }
            }
        }

        private void OnEnable() => LoadSettings();

        private void OnGUI()
        {
            DrawGuidTable();

            GUILayout.Space(12);

            using (new EditorGUI.DisabledScope(data.pairs.Count == 0))
            {
                if (GUILayout.Button("Scan Project"))
                {
                    var filesNeedingUpdate = ScanProject(this.data);
                    ReimportAssets(filesNeedingUpdate, this.data);
                    PrintResults(filesNeedingUpdate);
                }
            }

            void DrawGuidTable()
            {
                EditorGUI.BeginChangeCheck();

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(24);
                    EditorGUILayout.LabelField("Bad GUID", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("New GUID", EditorStyles.boldLabel, GUILayout.Width(280));
                    GUILayout.Space(28);
                }

                scroll = EditorGUILayout.BeginScrollView(scroll);

                for (int i = 0; i < data.pairs.Count; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        // Draw status icon
                        bool guidExists = CheckGuidExistsInProject(data.pairs[i].badGuid);
                        DrawStatusIcon(guidExists);

                        string badGuid = data.pairs[i].badGuid;
                        string newBadGuid = EditorGUILayout.TextField(badGuid);

                        if (newBadGuid != badGuid)
                        {
                            data.pairs[i].badGuid = newBadGuid;
                            SaveSettings();
                        }

                        using (new EditorGUI.DisabledScope(true))
                        {
                            EditorGUILayout.TextField(data.pairs[i].newGuid, GUILayout.Width(280));
                        }

                        if (GUILayout.Button("X", GUILayout.Width(24)))
                        {
                            data.pairs.RemoveAt(i);
                            SaveSettings();
                            GUIUtility.ExitGUI();
                        }
                    }
                }

                EditorGUILayout.EndScrollView();

                if (GUILayout.Button("Add Bad GUID"))
                {
                    data.pairs.Add(new GuidPair
                    {
                        badGuid = "",
                        newGuid = GenerateUnityStyleGuid()
                    });

                    SaveSettings();
                }

                if (EditorGUI.EndChangeCheck())
                {
                    SaveSettings();
                }
            }

            static void DrawStatusIcon(bool guidExists)
            {
                GUIStyle iconStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    alignment = TextAnchor.MiddleCenter
                };

                Color originalColor = GUI.color;
                GUI.color = guidExists ? Color.red : Color.green;

                string icon = guidExists ? "✗" : "✓";
                GUILayout.Label(icon, iconStyle, GUILayout.Width(20));

                GUI.color = originalColor;
            }

            static string GenerateUnityStyleGuid()
            {
                // Unity asset GUIDs are 32 lowercase hex chars, no dashes.
                return Guid.NewGuid().ToString("N");
            }
        }

        private static bool CheckGuidExistsInProject(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                return false;
            }

            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            return string.IsNullOrEmpty(assetPath) == false;
        }

        private static List<string> ScanProject(GuidPairData data)
        {
            var validPairs = data.pairs
                .Where(p => !string.IsNullOrWhiteSpace(p.badGuid))
                .ToList();

            if (validPairs.Count == 0)
            {
                Debug.LogWarning("[Bad GUID Fixer] No bad GUIDs entered.");
                return new List<string>();
            }

            var allAssetGuids = AssetDatabase.FindAssets("");
            var allAssetPaths = allAssetGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(IsScannableAssetPath)
                .Distinct()
                .ToList();

            // Add all .meta files in the project
            var allMetaFiles = Directory.GetFiles("Assets", "*.meta", SearchOption.AllDirectories)
                .Select(x => x.Replace("\\", "/"))
                .ToList();

            allAssetPaths.AddRange(allMetaFiles);

            var filesNeedingUpdate = new List<string>();

            try
            {
                for (int i = 0; i < allAssetPaths.Count; i++)
                {
                    string assetPath = allAssetPaths[i];

                    EditorUtility.DisplayProgressBar(
                        "Bad GUID Fixer",
                        assetPath,
                        i / (float)allAssetPaths.Count);

                    string fullPath = ToFullPath(assetPath);

                    string text;
                    try
                    {
                        text = File.ReadAllText(fullPath);
                    }
                    catch
                    {
                        continue;
                    }

                    foreach (var pair in validPairs)
                    {
                        if (text.Contains(pair.badGuid, StringComparison.OrdinalIgnoreCase))
                        {
                            filesNeedingUpdate.Add(assetPath);
                            break;
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return filesNeedingUpdate;
        }

        private static void ReimportAssets(List<string> assetPaths, GuidPairData data)
        {
            AssetDatabase.StartAssetEditing();

            try
            {
                foreach (string assetPath in assetPaths)
                {
                    string fileContents = File.ReadAllText(assetPath);

                    foreach (var guidPair in data.pairs)
                    {
                        fileContents = fileContents.Replace(guidPair.badGuid, guidPair.newGuid, StringComparison.OrdinalIgnoreCase);
                    }

                    File.WriteAllText(assetPath, fileContents);

                    if (assetPath.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                    {
                        AssetDatabase.ImportAsset(assetPath.Substring(0, assetPath.Length - 5), ImportAssetOptions.ForceUpdate);
                    }
                    else
                    {
                        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
        }

        private static void PrintResults(List<string> filesNeedingUpdate)
        {
            if (filesNeedingUpdate.Count == 0)
            {
                Debug.Log("[Bad GUID Fixer] No files found containing the bad GUIDs.");
                return;
            }

            Debug.Log($"[Bad GUID Fixer] Found {filesNeedingUpdate.Count} file(s) that need updates:");

            foreach (var assetPath in filesNeedingUpdate)
            {
                Debug.Log($"[Bad GUID Fixer] File needing update: {assetPath}");
            }
        }

        private static bool IsScannableAssetPath(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return false;
            }

            string extension = Path.GetExtension(assetPath).ToLowerInvariant();

            return extension is
                ".unity" or
                ".prefab" or
                ".asset" or
                ".mat" or
                ".controller" or
                ".anim" or
                ".overridecontroller" or
                ".playable" or
                ".meta";
        }

        private static string ToFullPath(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
            return Path.Combine(projectRoot, assetPath);
        }

        private void SaveSettings()
        {
            try
            {
                string projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
                string fullPath = Path.Combine(projectRoot, SettingsFilePath);

                // Ensure ProjectSettings directory exists
                string directory = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                }

                string json = JsonUtility.ToJson(this.data, true);
                File.WriteAllText(fullPath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Bad GUID Fixer] Failed to save settings: {ex.Message}");
            }
        }

        private void LoadSettings()
        {
            try
            {
                this.data = this.data ?? LoadGuidPairsData() ?? new GuidPairData();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Bad GUID Fixer] Failed to load settings: {ex.Message}");
            }
        }

        private static GuidPairData LoadGuidPairsData()
        {
            try
            {
                string projectRoot = Directory.GetParent(Application.dataPath)!.FullName;
                string fullPath = Path.Combine(projectRoot, SettingsFilePath);

                if (!File.Exists(fullPath))
                {
                    return null;
                }

                string json = File.ReadAllText(fullPath);
                return JsonUtility.FromJson<GuidPairData>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Bad GUID Fixer] Failed to load settings: {ex.Message}");
                return null;
            }
        }

        [Serializable]
        private class GuidPair
        {
            public string badGuid;
            public string newGuid;
        }

        [Serializable]
        private class GuidPairData
        {
            public List<GuidPair> pairs = new();
        }
    }
}
