//-----------------------------------------------------------------------
// <copyright file="Validation.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    public static class Validation
    {
        private static readonly OGTLogger Logger = OGTLogger.OGTEditor;

        [MenuItem("Tools/OGT/Validation/Validate Open Scenes", priority = MenuItemPriorities.Validation + 0)]
        public static void ValidateOpenScenes()
        {
            var gameObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList<UnityEngine.Object>();

            ValidateObjects(gameObjects, true);
        }

        [MenuItem("Tools/OGT/Validation/Validate Prefabs and ScriptableObjects", priority = MenuItemPriorities.Validation + 1)]
        public static void ValidatePrefabsAndScriptableObjects()
        {
            var prefabsAndScriptableObjects = new List<UnityEngine.Object>();

            foreach (var gameObject in AssetDatabaseUtil.GetAllPrefabs("Finding Prefabs..."))
            {
                prefabsAndScriptableObjects.Add(gameObject);
            }

            foreach (var scriptableObject in AssetDatabaseUtil.GetAllScriptableObjects("Finding ScriptableObjects..."))
            {
                prefabsAndScriptableObjects.Add(scriptableObject);
            }

            ValidateObjects(prefabsAndScriptableObjects, false);
        }

        public static void ValidateObjects(List<UnityEngine.Object> objects, bool areSceneObjects)
        {
            var reportSummary = new StringBuilder();
            var report = new ValidationReport();
            var validators = GetActiveValidators();
            var totalObjectsCount = (float)objects.Count;
            int progressCount = 0;

            // Making sure OnValidate is called on all the objects
            foreach (var obj in objects)
            {
                EditorUtility.DisplayProgressBar("OnValidate...", obj.EditorGetAssetPath(), progressCount++ / totalObjectsCount);

                if (obj is GameObject gameObject)
                {
                    foreach (var monoBehaviour in gameObject.GetComponents<MonoBehaviour>())
                    {
                        CallOnValidate(monoBehaviour);
                    }
                }
                else if (obj is ScriptableObject scriptableObject)
                {
                    CallOnValidate(scriptableObject);
                }
            }

            // Going through every validator and reporting errors
            foreach (var validator in validators)
            {
                string title = validator.DisplayName + "...";
                int errorsFound = report.Errors.Count;
                int objectsScanned = 0;

                progressCount = 0;
                foreach (var obj in objects)
                {
                    EditorUtility.DisplayProgressBar(title, obj.EditorGetAssetPath(), progressCount++ / totalObjectsCount);

                    if (obj is GameObject gameObject)
                    {
                        validator.ValidateGameObject(report, gameObject, areSceneObjects, ref objectsScanned);
                    }
                    else if (obj is ScriptableObject scriptableObject)
                    {
                        validator.ValidateScriptableObject(report, scriptableObject);
                        objectsScanned++;
                    }
                }

                reportSummary.AppendLine($"    Validator {validator.DisplayName} scanned {objectsScanned} and found {report.Errors.Count - errorsFound} errors");
            }

            reportSummary.Insert(0, $"Validation Report Found {report.Errors.Count} Errors\n");

            if (report.Errors.Count > 0)
            {
                Logger.LogError(reportSummary.ToString());
            }
            else
            {
                Logger.Log(reportSummary.ToString());
            }

            // Printing out all the errors
            foreach (var error in report.Errors)
            {
                Logger.LogError($"{error.Error}\n{error.Description}\nPath = {error.AffectedObjectPath}\nType = {error.AffectedType}", error.AffectedObject);
            }

            EditorUtility.ClearProgressBar();
        }

        // TODO [bgish]: Eventually there will be project settings for enabling and disabling validators and this function will
        //               get the active validators from the projecs settings, but for now, we're just applaying all validators.
        private static List<Validator> GetActiveValidators()
        {
            var validatorClassTypes = TypeCache.GetTypesDerivedFrom<Validator>();
            var results = new List<Validator>();

            foreach (var validatorClassType in validatorClassTypes)
            {
                results.Add(Activator.CreateInstance(validatorClassType) as Validator);
            }

            return results;
        }

        private static void CallOnValidate(object obj)
        {
            if (obj == null)
            {
                return;
            }

            var type = obj.GetType();

            foreach (var method in type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
            {
                if (method.Name == "OnValidate" &&
                    method.ReturnType == typeof(void) &&
                    method.GetParameters().IsNullOrEmpty())
                {
                    method.Invoke(obj, null);
                }
            }
        }

        [EditorEvents.OnEnterPlayMode]
        private static void OnEnterPlayMode()
        {
            // TODO [bgish]: Add config options in Project Settings to turn this off
            ValidateOpenScenes();
        }
    }
}
