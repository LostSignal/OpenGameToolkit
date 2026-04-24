#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="Button.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.XR
{
    using System.Runtime.CompilerServices;

    public struct Button
    {
        private bool isPressed;
        private bool wasPressedThisFrame;
        private bool wasReleasedThisFrame;
        private float pressedTime;

        public bool IsPressed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.isPressed;
        }

        public bool WasPressedThisFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.wasPressedThisFrame;
        }

        public bool WasReleasedThisFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.wasReleasedThisFrame;
        }

        public float PressedTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.pressedTime;
        }

        public static Button Update(bool currentIsPressed, float currentPressedTime, bool newIsPressed, float deltaTime)
        {
            if (newIsPressed)
            {
                return currentIsPressed ?
                    new Button { isPressed = true, pressedTime = currentPressedTime + deltaTime } :
                    new Button { isPressed = true, wasPressedThisFrame = true };
            }
            else
            {
                return currentIsPressed == false ?
                    new Button { isPressed = false } :
                    new Button { isPressed = false, wasReleasedThisFrame = true };
            }
        }
    }
}
