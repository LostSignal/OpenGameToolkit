//-----------------------------------------------------------------------
// <copyright file="MeshColliderNegativeScaleValidator.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Validation
{
    using UnityEngine;

    public class MeshColliderNegativeScaleValidator : Validator
    {
        public override string DisplayName => "MeshCollider Negative Scale Validator";

        public override void ValidateGameObject(ValidationReport report, GameObject gameObject, bool isSceneObject, ref int objectsScanned)
        {
            foreach (var meshCollider in gameObject.GetComponents<MeshCollider>())
            {
                objectsScanned++;

                if (this.DoesTransformHaveNegativeScale(meshCollider.transform))
                {
                    string fullPath = meshCollider.transform.GetFullPathWithSceneName();

                    var description = $"MeshCollider {fullPath} has negative scaling which means it's MeshCollider will be recalcuated at runtime and not precalculated during build time.";

                    report.ReportError(meshCollider, "Negative MeshCollider Scaling", description);
                }
            }
        }

        public override void ValidateScriptableObject(ValidationReport report, ScriptableObject scriptableObject)
        {
        }

        private bool DoesTransformHaveNegativeScale(Transform transform)
        {
            if (transform == null)
            {
                return false;
            }
            else if (transform.localScale.x < 0 || transform.localScale.y < 0 || transform.localScale.z < 0)
            {
                return true;
            }
            else
            {
                return this.DoesTransformHaveNegativeScale(transform.parent);
            }
        }
    }
}
