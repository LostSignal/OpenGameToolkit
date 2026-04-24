//-----------------------------------------------------------------------
// <copyright file="TeleportXRController.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace OGT.Haven
{
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;

    public sealed class HavenTeleportController : XRController
    {
#pragma warning disable 0649
        [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor;
#pragma warning restore 0649

        private bool isEnabled;
        private bool previousEnabled;

        private int lastCalculatedFrame;
        private bool isActive;
        private bool wasActivatedThisFrame;
        private bool wasDeactivatedThisFrame;
        private int disabledCount = 0;

        protected override void Awake()
        {
            base.Awake();

            this.updateTrackingType = UpdateType.BeforeRender;
            this.enableInputActions = false;
            this.enableInputTracking = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnableTeleport()
        {
            if (this.isEnabled == false)
            {
                this.isEnabled = true;
                this.enabled = true;
                this.rayInteractor.enabled = true;
                this.disabledCount = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DisableTeleport()
        {
            if (this.isEnabled)
            {
                this.isEnabled = false;
                this.disabledCount = 0;
            }
        }

        protected override void ApplyControllerState(XRInteractionUpdateOrder.UpdatePhase updatePhase, XRControllerState controllerState)
        {
            int currentFrame = Time.frameCount;

            if (this.lastCalculatedFrame != currentFrame)
            {
                this.lastCalculatedFrame = currentFrame;

                this.isActive = this.previousEnabled && this.isEnabled == false;
                this.wasActivatedThisFrame = this.previousEnabled && this.isEnabled == false;
                this.wasDeactivatedThisFrame = false;

                this.previousEnabled = this.isEnabled;
            }

            controllerState.selectInteractionState.active = this.isActive;
            controllerState.selectInteractionState.activatedThisFrame = this.wasActivatedThisFrame;
            controllerState.selectInteractionState.deactivatedThisFrame = this.wasDeactivatedThisFrame;

            if (this.isEnabled == false)
            {
                this.disabledCount++;

                if (this.disabledCount > 2)
                {
                    this.enabled = false;
                    this.rayInteractor.enabled = false;
                }
            }

            base.ApplyControllerState(updatePhase, controllerState);
        }
    }
}

#endif
