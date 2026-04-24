//-----------------------------------------------------------------------
// <copyright file="ValidationReportExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    public static class ValidationReportExtensions
    {
        private static readonly OGTLogger Logger = new("Validation");

        public static void ReportError(this ValidationReport report, object obj, string error, string description)
        {
            report.CreateValidationError(obj, error, description);
        }

        public static void AssertAddressable(this ValidationReport report, MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour.EditorIsAddressable() == false)
            {
                string description = $"MonoBehaviour {monoBehaviour.GetType().Name} \"{GetFullName(monoBehaviour)}\" is not marked as Addressable";
                report.CreateValidationError(monoBehaviour, "Assert Addressable", description);
            }
        }

        public static void AssertIsValid(this ValidationReport report, MonoBehaviour monoBehaviour, AssetReference assetReference, string nameOfObject)
        {
#if UNITY_EDITOR
            var editorAsset = assetReference.editorAsset;

            if (editorAsset == null || editorAsset.EditorIsAddressable() == false)
            {
                string description = $"MonoBehaviour {monoBehaviour.GetType().Name} \"{GetFullName(monoBehaviour)}\" has Invalid AssetReference {nameOfObject}";
                report.CreateValidationError(monoBehaviour, "Assert IsValid", description);
            }
#endif
        }

        public static bool AssertNotNullOrEmpty(this ValidationReport report, MonoBehaviour monoBehaviour, string str, string nameOfObject)
        {
            if (string.IsNullOrEmpty(str) == false)
            {
                return false;
            }

            string description = $"MonoBehaviour {monoBehaviour.GetType().Name} \"{GetFullName(monoBehaviour)}\" has null or empty string {nameOfObject}";

            report.CreateValidationError(monoBehaviour, "Assert Not Null Or Empty", description);

            return true;
        }


        public static bool AssertNotNullOrEmpty(this ValidationReport report, MonoBehaviour monoBehaviour, ICollection collection, string nameOfObject)
        {
            if (collection != null && collection.Count > 0)
            {
                return false;
            }

            string description = $"MonoBehaviour {monoBehaviour.GetType().Name} \"{GetFullName(monoBehaviour)}\" has null or empty collection {nameOfObject}";

            report.CreateValidationError(monoBehaviour, "Assert Not Null Or Empty", description);

            return true;
        }

        public static void AssertNotNull(this ValidationReport report, MonoBehaviour monoBehaviour, object obj, string nameOfObject)
        {
            if (IsNull(obj) == false)
            {
                return;
            }

            string description = $"MonoBehaviour {monoBehaviour.GetType().Name} \"{GetFullName(monoBehaviour)}\" has null object {nameOfObject}";

            report.CreateValidationError(monoBehaviour, "Assert Not Null", description);
        }

        public static void AssertNull(this ValidationReport report, MonoBehaviour monoBehaviour, object obj, string nameOfObject)
        {
            if (IsNull(obj))
            {
                return;
            }

            string description = $"MonoBehaviour {monoBehaviour.GetType().Name} \"{GetFullName(monoBehaviour)}\" has non null object {nameOfObject}";

            report.CreateValidationError(monoBehaviour, "Assert Null", description);
        }

        public static void AssertEqual(this ValidationReport report, MonoBehaviour monoBehaviour, double currentValue, double desiredValue, string nameOfObject)
        {
            if (currentValue == desiredValue)
            {
                return;
            }

            string description = $"MonoBehaviour {monoBehaviour.GetType().Name} \"{GetFullName(monoBehaviour)}\" has unequal value {nameOfObject} {currentValue} != {desiredValue}";
            report.CreateValidationError(monoBehaviour, "Assert Equal", description);
        }

        public static void AssertEqual(this ValidationReport report, MonoBehaviour monoBehaviour, string currentValue, string desiredValue, string nameOfObject)
        {
            if (currentValue == desiredValue)
            {
                return;
            }

            string description = $"MonoBehaviour {monoBehaviour.GetType().Name} \"{GetFullName(monoBehaviour)}\" has unequal value {nameOfObject} '{currentValue}' != '{desiredValue}'";
            report.CreateValidationError(monoBehaviour, "Assert Equal", description);
        }

        public static void AssertGreaterThan(this ValidationReport report, MonoBehaviour monoBehaviour, Decimal currentValue, Decimal greaterThanValue, string nameOfObject)
        {
            if (currentValue > greaterThanValue)
            {
                return;
            }

            string description = $"MonoBehaviour {monoBehaviour.GetType().Name} \"{GetFullName(monoBehaviour)}\" has value less than {nameOfObject} {currentValue} < {greaterThanValue}";
            report.CreateValidationError(monoBehaviour, "Assert Greater Than", description);
        }

        public static void AssertFalse(this ValidationReport report, MonoBehaviour monoBehaviour, bool obj, string nameOfObject)
        {
            if (obj == false)
            {
                return;
            }

            string description = $"MonoBehaviour {monoBehaviour.GetType().Name} \"{GetFullName(monoBehaviour)}\" has true value {nameOfObject}";
            report.CreateValidationError(monoBehaviour, "Assert False", description);
        }

        public static void AssertTrue(this ValidationReport report, MonoBehaviour monoBehaviour, bool obj, string nameOfObject)
        {
            if (obj)
            {
                return;
            }

            string description = $"MonoBehaviour {monoBehaviour.GetType().Name} \"{GetFullName(monoBehaviour)}\" has false value {nameOfObject}";
            report.CreateValidationError(monoBehaviour, "Assert True", description);
        }

        private static string GetFullName(ScriptableObject scriptableObject)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetAssetPath(scriptableObject);
#else
            return string.Empty;
#endif
        }

        private static string GetFullName(Component component)
        {
            return GetFullName(component.gameObject);
        }

        private static string GetFullName(GameObject gameObject)
        {
            return gameObject.transform.GetFullPathWithSceneName();
        }

        private static bool IsNull(object obj)
        {
            return obj is UnityEngine.Object unityObject ? unityObject == null : obj == null;
        }

        private static void CreateValidationError(this ValidationReport report, object obj, string error, string description)
        {
            if (obj is GameObject gameObject)
            {
                report.Errors.Add(new ValidationError
                {
                    AffectedObject = gameObject,
                    AffectedObjectPath = GetFullName(gameObject),
                    AffectedType = typeof(GameObject).Name,
                    Error = error,
                    Description = description,
                });
            }
            else if (obj is Component monoBehaviour)
            {
                report.Errors.Add(new ValidationError
                {
                    AffectedObject = monoBehaviour,
                    AffectedObjectPath = GetFullName(monoBehaviour),
                    AffectedType = monoBehaviour.GetType().Name,
                    Error = error,
                    Description = description,
                });
            }
            else if (obj is ScriptableObject scriptableObject)
            {
                report.Errors.Add(new ValidationError
                {
                    AffectedObject = scriptableObject,
                    AffectedObjectPath = GetFullName(scriptableObject),
                    AffectedType = scriptableObject.GetType().Name,
                    Error = error,
                    Description = description,
                });
            }
            else
            {
                report.Errors.Add(new ValidationError
                {
                    AffectedObject = obj,
                    AffectedObjectPath = string.Empty,
                    AffectedType = obj.GetType().Name,
                    Error = error,
                    Description = description,
                });
            }
        }
    }
}
