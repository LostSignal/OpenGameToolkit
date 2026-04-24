//-----------------------------------------------------------------------
// <copyright file="ApplyImpulse.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody))]
    public class ApplyImpulse : GameBehavior, IValidate
    {
#pragma warning disable 0649
        [SerializeField][HideInInspector] private Rigidbody rigidBody;
        [SerializeField] private float impulseForce = 1.0f;
#pragma warning restore 0649

        public void Apply(RaycastHit hit)
        {
            Vector3 force = hit.normal.normalized * -this.impulseForce;
            this.rigidBody.AddForceAtPosition(force, hit.point);
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNull(this, this.rigidBody, nameof(this.rigidBody));
        }
    }
}

#endif
