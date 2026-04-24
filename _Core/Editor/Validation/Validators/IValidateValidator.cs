//-----------------------------------------------------------------------
// <copyright file="IValidateValidator.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Validation
{
    using UnityEngine;

    public class IValidateValidator : Validator
    {
        public override string DisplayName => "IValidate Validator";

        public override void ValidateGameObject(ValidationReport report, GameObject gameObject, bool isSceneObject, ref int objectsScanned)
        {
            foreach (var validate in gameObject.GetComponents<IValidate>())
            {
                validate.Validate(report, isSceneObject);
                objectsScanned++;
            }
        }

        public override void ValidateScriptableObject(ValidationReport report, ScriptableObject scriptableObject)
        {
            if (scriptableObject is IValidate validate)
            {
                validate.Validate(report, false);
            }
        }
    }
}
