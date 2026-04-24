//-----------------------------------------------------------------------
// <copyright file="PackageManagerUtil.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY_EDITOR

namespace OGT
{
    using System.Collections.Generic;
    using System.IO;
    using OGT;
    using UnityEditor;
    using UnityEditor.PackageManager;
    using UnityEditor.PackageManager.Requests;

    public static class PackageManagerUtil
    {
        private static readonly OGTLogger Logger = OGTLogger.OGTEditor;

        private static readonly List<string> PackageIdsToAdd = new ();
        private static AddRequest addRequest = null;
        private static bool isProcessing = false;

        public static void Add(string packageId)
        {
            PackageIdsToAdd.Add(packageId);

            if (isProcessing == false)
            {
                isProcessing = true;
                EditorApplication.update += ProcessList;
            }
        }

        public static void AddGitPackage(string id, string gitUrl)
        {
            var manifestPath = "./Packages/manifest.json";
            var manifestText = File.ReadAllText(manifestPath);

            if (manifestText.Contains($"\"{id}\""))
            {
                return;
            }

            int dependenciesIndex = manifestText.IndexOf("dependencies");
            int leftBracketIndex = manifestText.IndexOf("{", dependenciesIndex);

            manifestText = manifestText.Insert(leftBracketIndex + 1, $"\n    \"{id}\": \"{gitUrl}\",\n");
            File.WriteAllText(manifestPath, manifestText);
        }

        private static void ProcessList()
        {
            if (PackageIdsToAdd.Count > 0 && addRequest == null)
            {
                var packageId = PackageIdsToAdd[0];
                PackageIdsToAdd.RemoveAt(0);
                addRequest = Client.Add(packageId);
            }
            else if (addRequest.IsCompleted)
            {
                if (addRequest.Status == StatusCode.Success)
                {
                    Logger.Log($"Package {addRequest.Result.packageId} Installed Successfully");
                }
                else if (addRequest.Status >= StatusCode.Failure)
                {
                    Logger.LogError($"Failed To Install Package {addRequest.Result.packageId}: {addRequest.Error.message}");
                }

                addRequest = null;
            }

            if (PackageIdsToAdd.Count == 0 && addRequest == null)
            {
                EditorApplication.update -= ProcessList;
                isProcessing = false;
            }
        }
    }
}

#endif
