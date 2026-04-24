#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="UIXRController.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace OGT.Haven
{
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;

    public sealed class HavenUIController : XRController
    {
#pragma warning disable 0649
        [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor interactor;
#pragma warning restore 0649

        private bool isPressed = false;
        private bool previousIsPressed = false;

        private int lastCalculatedFrame;
        private bool isActive;
        private bool wasActivatedThisFrame;
        private bool wasDeactivatedThisFrame;

        protected override void Awake()
        {
            base.Awake();

            this.updateTrackingType = UpdateType.BeforeRender;
            this.enableInputActions = false;
            this.enableInputTracking = false;
        }

        public void EnableUISelect()
        {
            this.interactor.enabled = true;
            this.enabled = true;
        }

        public void DisableUISelect()
        {
            this.interactor.enabled = false;
            this.enabled = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartUIClick()
        {
            this.isPressed = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndUIClick()
        {
            this.isPressed = false;
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

                this.isActive = this.isPressed;
                this.wasActivatedThisFrame = this.previousIsPressed == false && this.isPressed;
                this.wasDeactivatedThisFrame = this.previousIsPressed && this.isPressed == false;
                this.previousIsPressed = this.isPressed;
            }

            controllerState.uiPressInteractionState.active = this.isActive;
            controllerState.uiPressInteractionState.activatedThisFrame = this.wasActivatedThisFrame;
            controllerState.uiPressInteractionState.deactivatedThisFrame = this.wasDeactivatedThisFrame;

            base.ApplyControllerState(updatePhase, controllerState);
        }
    }
}

#endif
