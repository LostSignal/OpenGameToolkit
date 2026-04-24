//-----------------------------------------------------------------------
// <copyright file="HavenContinuousMoveProvider.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace OGT.Haven
{
    using OGT.XR;
    using System;
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;

    public class HavenContinuousMoveProvider : ContinuousMoveProviderBase
    {
        private static readonly Vector2 zero = new Vector2(0.0f, 0.0f);

        private HavenHand moveHand;
        private bool isLeftHand;
        private bool allowStrafe;

        public void UpdateSettings(HavenRigSettings settings, float movementSpeed, Transform head, HavenHand leftHand, HavenHand rightHand)
        {
            this.enabled = settings.MovementMode == MovementMode.ContinuousOnly || settings.MovementMode == MovementMode.ContinuousAndTeleport;
            this.isLeftHand = settings.MovementHand == Hand.Left;
            this.moveHand = this.isLeftHand ? leftHand : rightHand;
            this.allowStrafe = settings.AllowStrafe;
            this.forwardSource =
                settings.MovementSource == ContinuousMovementSource.Head ? head :
                settings.MovementSource == ContinuousMovementSource.LeftHand ? leftHand.transform :
                settings.MovementSource == ContinuousMovementSource.RightHand ? rightHand.transform :
                throw new Exception("HavenRig.OnSettingsChagned: Unknown ContinuousMovementSource found");


            this.moveSpeed = movementSpeed;
        }

        protected override Vector2 ReadInput()
        {
            if (this.moveHand == null || this.moveHand.IsTeleporting)
            {
                return zero;
            }

            var result = this.isLeftHand ? XRControllersState.Instance.LeftStick : XRControllersState.Instance.RightStick;

            if (this.allowStrafe == false)
            {
                result = result.SetX(0);
            }

            return result;
        }
    }
}

#endif
