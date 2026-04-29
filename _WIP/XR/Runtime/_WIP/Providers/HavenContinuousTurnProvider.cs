#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenContinuousTurnProvider.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace OGT.Haven
{
    using OGT.XR;
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;

    public class HavenContinuousTurnProvider : ContinuousTurnProviderBase
    {
        private static readonly Vector2 Zero = new Vector2(0.0f, 0.0f);

#pragma warning disable 0649
        [SerializeField] private float debounceTime;
        [SerializeField] private bool enableTurnAround;
#pragma warning restore 0649

        private float lastTurnTime;
        private HavenHand turnHand;
        private bool isLeftHand;

        public void UpdateSettings(HavenRigSettings settings, HavenHand leftHand, HavenHand rightHand)
        {
            this.enabled = settings.TurnMode == TurnMode.Continuous;
            this.turnSpeed = settings.ContinuousTurnSpeed;
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

        protected override float GetTurnAmount(Vector2 input)
        {
            // Special case for turn around
            if (this.enableTurnAround && input.y < -0.9f)
            {
                float currentTime = Time.realtimeSinceStartup;

                if (currentTime > this.lastTurnTime + this.debounceTime)
                {
                    this.lastTurnTime = currentTime;
                    return 180;
                }
            }

            return base.GetTurnAmount(input);
        }
    }
}

#endif
