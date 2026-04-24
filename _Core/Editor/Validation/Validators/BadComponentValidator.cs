//-----------------------------------------------------------------------
// <copyright file="BadComponentValidator.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Validation
{
    using UnityEditor;
    using UnityEngine;

    public class BadComponentValidator : Validator
    {
        public override string DisplayName => "Bad Component Validator";

        public override void ValidateGameObject(ValidationReport report, GameObject gameObject, bool isSceneObject, ref int objectsScanned)
        {
            string sourcePath = isSceneObject ? gameObject.scene.path : AssetDatabase.GetAssetPath(gameObject);

            foreach (var component in gameObject.GetComponents<Component>())
            {
                if (component == null)
                {
                    if (isSceneObject)
                    {
                        report.ReportError(gameObject, "Invalid Component", $"Found Invalid Componet in Scene '{sourcePath}' on '{gameObject.transform.GetFullPath()}' ");
                    }
                    else
                    {
                        report.ReportError(gameObject, "Invalid Component", $"Found Invalid Componet in Prefab '{sourcePath}' on '{gameObject.transform.GetFullPath()}'");
                    }
                }

                objectsScanned++;
            }
        }

        public override void ValidateScriptableObject(ValidationReport report, ScriptableObject scriptableObject)
        {
        }
    }
}
