//-----------------------------------------------------------------------
// <copyright file="HavenRigSettings.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace OGT.Haven
{
    using System;

    public enum Hand
    {
        Left,
        Right
    }

    public enum MovementMode
    {
        ContinuousAndTeleport,
        ContinuousOnly,
        TeleportOnly,
    }

    public enum TurnMode
    {
        Continuous,
        Snap,
    }

    public enum ContinuousMovementSource
    {
        Head,
        LeftHand,
        RightHand,
    }

    public enum ComfortMode
    {
        Comfortable,
        Moderate,
        Intense,
        Custom,
    }

    [Serializable]
    public struct HavenRigSettings
    {
        public static readonly HavenRigSettings Default = new HavenRigSettings
        {
            MovementMode = MovementMode.ContinuousAndTeleport,
            TurnMode = TurnMode.Snap,
            EnableTurnAround = false,

            MovementHand = Hand.Left,
            MovementSource = ContinuousMovementSource.Head,
            AllowStrafe = true,

            ContinuousTurnSpeed = 180f,
            SnapTurnAmmount = 30.0f,
        };

        public MovementMode MovementMode;
        public TurnMode TurnMode;
        public bool EnableTurnAround;

        // Continuous Movement
        public Hand MovementHand;
        public ContinuousMovementSource MovementSource;
        public bool AllowStrafe;

        // Turning Options
        public float ContinuousTurnSpeed;
        public float SnapTurnAmmount;
    }
}

#endif
