//-----------------------------------------------------------------------
// <copyright file="Validator.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Validation
{
    using UnityEngine;

    public abstract class Validator
    {
        public abstract string DisplayName { get; }

        public abstract void ValidateGameObject(ValidationReport report, GameObject gameObject, bool isSceneObject, ref int objectsScanned);

        public abstract void ValidateScriptableObject(ValidationReport report, ScriptableObject scriptableObject);
    }
}
