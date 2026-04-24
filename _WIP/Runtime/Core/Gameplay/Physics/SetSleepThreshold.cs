//-----------------------------------------------------------------------
// <copyright file="SetSleepThreshold.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody))]
    public class SetSleepThreshold : GameBehavior, IAwake, IValidate
    {
#pragma warning disable 0649
        [SerializeField][HideInInspector] private Rigidbody rigidBody;
        [SerializeField] private float sleepThreshold = 0.00001f;
#pragma warning restore 0649

        public void OnAwake(Bootloader bootloader)
        {
            this.rigidBody.sleepThreshold = this.sleepThreshold;
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            this.EditorGetComponent(ref this.rigidBody);

            report.AssertNotNull(this, this.rigidBody, nameof(this.rigidBody));
        }
    }
}

#endif
