//-----------------------------------------------------------------------
// <copyright file="HavenGrabbableSettings.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Haven
{
    using System;
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;

    [Serializable]
    public class HavenGrabbableSettings
    {
        [Space]
        [SerializeField] private InteractionLayerMask interactionLayers = -1;
        [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType movementType;
        [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.InteractableSelectMode selectMode;
        [SerializeField] private GameObject customReticle;

        [Header("Position")]
        [SerializeField] private bool trackPosition;

        [Indent(1)]
        [SerializeField]
        private bool smoothPosition;

        [Indent(2)]
        [Range(0.0f, 20.0f)]
        [SerializeField]
        private float smoothPositionAmount;

        [Indent(2)]
        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float tightenPosition;

        [Header("Rotation")]
        [SerializeField] private bool trackRotation;
        [SerializeField][Indent(1)] private bool smoothRotation;

        [Header("Throwing")]
        [SerializeField] private bool throwOnDetach;
        [SerializeField][Indent(1)] private float throwSmoothingDuration;
        [SerializeField][Indent(1)] private AnimationCurve throwSmoothingCurve;
        [SerializeField][Indent(1)] private float throwVelocityScale;
        [SerializeField][Indent(1)] private float throwAngularVelocityScale;

        [Header("Attaching")]
        [SerializeField] private bool retainTransformParent;
        [SerializeField] private bool forceGravityOnDetach;
        [SerializeField] private float attachEaseInTime;

        public void Apply(HavenGrabbable grabbable)
        {
#if USING_UNITY_XR_INTERACTION_TOOLKIT
            grabbable.interactionLayers = this.interactionLayers;
            grabbable.movementType = this.movementType;
            grabbable.selectMode = this.selectMode;
            grabbable.customReticle = this.customReticle;

            // Position
            grabbable.trackPosition = this.trackPosition;
            grabbable.smoothPosition = this.smoothPosition;
            grabbable.smoothPositionAmount = this.smoothPositionAmount;
            grabbable.tightenPosition = this.tightenPosition;

            // Rotation
            grabbable.trackRotation = this.trackRotation;
            grabbable.smoothRotation = this.smoothRotation;

            // Throwing
            grabbable.throwOnDetach = this.throwOnDetach;
            grabbable.throwSmoothingDuration = this.throwSmoothingDuration;
            grabbable.throwSmoothingCurve = this.throwSmoothingCurve;
            grabbable.throwVelocityScale = this.throwVelocityScale;
            grabbable.throwAngularVelocityScale = this.throwAngularVelocityScale;

            // Attaching
            grabbable.retainTransformParent = this.retainTransformParent;
            grabbable.forceGravityOnDetach = this.forceGravityOnDetach;
            grabbable.attachEaseInTime = this.attachEaseInTime;
#endif
        }
    }
}

