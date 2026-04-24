//-----------------------------------------------------------------------
// <copyright file="HavenSnapTurnProvider.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace OGT.Haven
{
    using OGT.XR;
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;

    public class HavenSnapTurnProvider : SnapTurnProviderBase
    {
        private static readonly Vector2 Zero = new Vector2(0.0f, 0.0f);

        private HavenHand turnHand;
        private bool isLeftHand;

        public void UpdateSettings(HavenRigSettings settings, HavenHand leftHand, HavenHand rightHand)
        {
            this.enabled = settings.TurnMode == TurnMode.Snap;
            this.turnAmount = settings.SnapTurnAmmount;
            this.isLeftHand = settings.MovementHand != Hand.Left;
            this.turnHand = this.isLeftHand ? leftHand : rightHand;
            this.enableTurnAround = settings.EnableTurnAround;
        }

        protected override Vector2 ReadInput()
        {
            if (this.turnHand == null || this.turnHand.IsTeleporting || this.turnHand.DidJustTeleport)
            {
                return Zero;
            }

            return this.isLeftHand ? XRControllersState.Instance.LeftStick : XRControllersState.Instance.RightStick;
        }
    }
}

#endif
