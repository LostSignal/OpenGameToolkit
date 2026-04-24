#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenGrabbableDialog.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace OGT.Haven
{
    using UnityEngine;


    [AddComponentMenu("Haven XR/Interactables/HXR Grabbable Dialog")]
    public class HavenGrabbableDialog : HavenGrabbable
    {
        //// TODO [bgish]: Instead of tracking Rotation, maybe have it so the dialog always lerps to the Haven Rig's Head

        protected override void Awake()
        {
            base.Awake();

            this.IsOffsetGrabbable = true;
            this.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.Instantaneous;
            this.attachEaseInTime = -1.0f;
            this.throwOnDetach = false;
            this.forceGravityOnDetach = false;

            this.trackPosition = true;
            this.smoothPosition = true;
            this.smoothPositionAmount = 3.0f;
            this.tightenPosition = 0.0f;

            this.trackRotation = true;
            this.smoothRotation = true;
            this.smoothRotationAmount = 0.5f;
            this.tightenRotation = 0.0f;

            var rigidBody = this.GetComponent<Rigidbody>();
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
        }
    }
}

#endif
