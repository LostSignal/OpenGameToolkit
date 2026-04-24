#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="GrabXRController.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace OGT.Haven
{
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;

    public sealed class HavenGrabController : XRController
    {
#pragma warning disable 0649
        [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor interactor;
#pragma warning restore 0649

        private bool isGrabPressed = false;
        private bool previousIsGrabPressed = false;

        private bool isActivatePressed = false;
        private bool previousIsActivatePressed = false;

        private int lastCalculatedFrame;
        private bool isGrabActive;
        private bool isActivateActive;

        private bool wasGrabActivatedThisFrame;
        private bool wasGrabDeactivatedThisFrame;

        private bool wasActivateActivatedThisFrame;
        private bool wasActivateDeactivatedThisFrame;

        public bool HasSelection
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.interactor.hasSelection;
        }

        public bool HasHover
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.interactor.hasHover;
        }

        public UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor Interactor
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.interactor;
        }

        protected override void Awake()
        {
            base.Awake();

            this.updateTrackingType = UpdateType.BeforeRender;
            this.enableInputActions = false;
            this.enableInputTracking = false;
        }

        public void EnableGrab()
        {
            this.interactor.enabled = true;
            this.enabled = true;
        }

        public void DisableGrab()
        {
            this.interactor.enabled = false;
            this.enabled = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartGrab()
        {
            this.isGrabPressed = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndGrab()
        {
            this.isGrabPressed = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartActivate()
        {
            this.isActivatePressed = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndActivate()
        {
            this.isActivatePressed = false;
        }

        protected override void ApplyControllerState(XRInteractionUpdateOrder.UpdatePhase updatePhase, XRControllerState controllerState)
        {
            if (controllerState == null)
            {
                return;
            }

            int currentFrame = Time.frameCount;

            if (this.lastCalculatedFrame != currentFrame)
            {
                this.lastCalculatedFrame = currentFrame;

                // Grabbing
                this.isGrabActive = this.isGrabPressed;
                this.wasGrabActivatedThisFrame = this.previousIsGrabPressed == false && this.isGrabPressed;
                this.wasGrabDeactivatedThisFrame = this.previousIsGrabPressed && this.isGrabPressed == false;
                this.previousIsGrabPressed = this.isGrabPressed;

                // Activating
                this.isActivateActive = this.isActivatePressed;
                this.wasActivateActivatedThisFrame = this.previousIsActivatePressed == false && this.isActivatePressed;
                this.wasActivateDeactivatedThisFrame = this.previousIsActivatePressed && this.isActivatePressed == false;
                this.previousIsActivatePressed = this.isActivatePressed;
            }

            controllerState.selectInteractionState.active = this.isGrabActive;
            controllerState.selectInteractionState.activatedThisFrame = this.wasGrabActivatedThisFrame;
            controllerState.selectInteractionState.deactivatedThisFrame = this.wasGrabDeactivatedThisFrame;

            controllerState.activateInteractionState.active = this.isActivateActive;
            controllerState.activateInteractionState.activatedThisFrame = this.wasActivateActivatedThisFrame;
            controllerState.activateInteractionState.deactivatedThisFrame = this.wasActivateDeactivatedThisFrame;

            base.ApplyControllerState(updatePhase, controllerState);
        }
    }
}

#endif
