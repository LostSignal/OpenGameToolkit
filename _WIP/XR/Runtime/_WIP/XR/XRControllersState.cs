#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="XRControllersState.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace OGT.XR
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using UnityEngine;
    using UnityEngine.XR;

    public class XRControllersState
    {
        private static readonly StringBuilder StringBuilderCache = new ();
        private static readonly List<InputDevice> InputDevicesList = new ();
        private static readonly InputDeviceCharacteristics ControllerCharacteristics = InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.HeldInHand;
        private static readonly XRControllersState ControllersState = new ();

        // Left Hand
        private bool isLeftControllerConnected;
        private float leftTrigger;
        private float leftGrip;
        private Vector2 leftStick;
        private Button leftStickClick;
        private Button leftPrimaryButton;
        private Button leftSecondaryButton;
        private Button menu;
        private Vector3 leftPosition;
        private Quaternion leftRotation;

        // Right Hand
        private bool isRightControllerConnected;
        private float rightTrigger;
        private float rightGrip;
        private Vector2 rightStick;
        private Button rightStickClick;
        private Button rightPrimaryButton;
        private Button rightSecondaryButton;
        private Vector3 rightPosition;
        private Quaternion rightRotation;

        private int lastUpdatedFrame = -1;

        public XRControllersState()
        {
            UnityEngine.XR.XRDevice.deviceLoaded += DeviceLoaded;
            UnityEngine.XR.InputDevices.deviceConnected += DeviceConntected;

            void DeviceLoaded(string name)
            {
                InputDevices.GetDevices(InputDevicesList);
            }

            void DeviceConntected(InputDevice inputDevice)
            {
                InputDevices.GetDevices(InputDevicesList);
            }
        }

        public static XRControllersState Instance
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ControllersState;
        }

        public bool IsLeftControllerConnected
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.isLeftControllerConnected;
        }

        public float LeftTrigger
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.leftTrigger;
        }

        public float LeftGrip
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.leftGrip;
        }

        public Vector2 LeftStick
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.leftStick;
        }

        public Button LeftStickClick
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.leftStickClick;
        }

        public Button LeftPrimaryButtion
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.leftPrimaryButton;
        }

        public Button LeftSecondaryButton
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.leftSecondaryButton;
        }

        public Vector3 LeftPosition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.leftPosition;
        }

        public Quaternion LeftRotation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.leftRotation;
        }

        public Button Menu
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.menu;
        }

        public bool IsRightControllerConnected
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.isRightControllerConnected;
        }

        public float RightTrigger
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.rightTrigger;
        }

        public float RightGrip
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.rightGrip;
        }

        public Vector2 RightStick
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.rightStick;
        }

        public Button RightStickClick
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.rightStickClick;
        }

        public Button RightPrimaryButton
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.rightPrimaryButton;
        }

        public Button RightSecondaryButton
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.rightSecondaryButton;
        }

        public Vector3 RightPosition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.rightPosition;
        }

        public Quaternion RightRotation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.rightRotation;
        }

        public void UpdateValues()
        {
            int currentFrame = Time.frameCount;

            if (this.lastUpdatedFrame == currentFrame)
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            this.lastUpdatedFrame = currentFrame;
            this.isLeftControllerConnected = false;
            this.isRightControllerConnected = false;

            foreach (var device in InputDevicesList)
            {
                if ((device.characteristics & ControllerCharacteristics) > 0)
                {
                    device.TryGetFeatureValue(CommonUsages.trigger, out float trigger);
                    device.TryGetFeatureValue(CommonUsages.grip, out float grip);
                    device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 primary2DAxis);
                    device.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool primary2DAxisClick);
                    device.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButton);
                    device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool secondaryButton);
                    device.TryGetFeatureValue(CommonUsages.menuButton, out bool menuButton);
                    device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position);
                    device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation);

                    if ((device.characteristics & InputDeviceCharacteristics.Left) > 0)
                    {
                        this.isLeftControllerConnected = true;
                        this.leftTrigger = trigger;
                        this.leftGrip = grip;
                        this.leftStick = primary2DAxis;
                        this.leftStickClick = Button.Update(this.leftStickClick.IsPressed, this.leftStickClick.PressedTime, primary2DAxisClick, deltaTime);
                        this.leftPrimaryButton = Button.Update(this.leftPrimaryButton.IsPressed, this.leftPrimaryButton.PressedTime, primaryButton, deltaTime);
                        this.leftSecondaryButton = Button.Update(this.leftSecondaryButton.IsPressed, this.leftSecondaryButton.PressedTime, secondaryButton, deltaTime);
                        this.leftPosition = position;
                        this.leftRotation = rotation;
                        this.menu = Button.Update(this.menu.IsPressed, this.menu.PressedTime, menuButton, deltaTime);
                    }
                    else if ((device.characteristics & InputDeviceCharacteristics.Right) > 0)
                    {
                        this.isRightControllerConnected = true;
                        this.rightTrigger = trigger;
                        this.rightGrip = grip;
                        this.rightStick = primary2DAxis;
                        this.rightStickClick = Button.Update(this.rightStickClick.IsPressed, this.rightStickClick.PressedTime, primary2DAxisClick, deltaTime);
                        this.rightPrimaryButton = Button.Update(this.rightPrimaryButton.IsPressed, this.rightPrimaryButton.PressedTime, primaryButton, deltaTime);
                        this.rightSecondaryButton = Button.Update(this.rightSecondaryButton.IsPressed, this.rightSecondaryButton.PressedTime, secondaryButton, deltaTime);
                        this.rightPosition = position;
                        this.rightRotation = rotation;
                    }
                }
            }
        }

        public StringBuilder GetOculusDebugOutput()
        {
            StringBuilderCache.Clear();

            StringBuilderCache.Append($"Left: Connected = {this.isLeftControllerConnected},");
            StringBuilderCache.Append($" Trigger = {this.leftTrigger:0.00},");
            StringBuilderCache.Append($" Grip = {this.leftGrip:0.00},");
            StringBuilderCache.Append($" Stick = {this.leftStick},");
            StringBuilderCache.Append($" Stick Click = {this.leftStickClick.IsPressed},");
            StringBuilderCache.AppendLine();

            StringBuilderCache.Append($"Right: Connected = {this.isRightControllerConnected},");
            StringBuilderCache.Append($" Trigger = {this.rightTrigger:0.00},");
            StringBuilderCache.Append($" Grip = {this.rightGrip:0.00},");
            StringBuilderCache.Append($" Stick = {this.rightStick},");
            StringBuilderCache.Append($" Stick Click = {this.rightStickClick.IsPressed},");
            StringBuilderCache.AppendLine();

            StringBuilderCache.Append($" A = {this.rightPrimaryButton.IsPressed},");
            StringBuilderCache.Append($" B = {this.rightSecondaryButton.IsPressed},");
            StringBuilderCache.Append($" X = {this.leftPrimaryButton.IsPressed},");
            StringBuilderCache.Append($" Y = {this.leftSecondaryButton.IsPressed},");
            StringBuilderCache.Append($" Menu = {this.menu.IsPressed}");

            return StringBuilderCache;
        }

        public bool IsAnyButtonDown()
        {
            if (this.isLeftControllerConnected)
            {
                if (this.leftPrimaryButton.IsPressed || this.leftSecondaryButton.IsPressed)
                {
                    return true;
                }
            }

            if (this.IsRightControllerConnected)
            {
                if (this.rightPrimaryButton.IsPressed || this.rightSecondaryButton.IsPressed)
                {
                    return true;
                }
            }

            return false;
        }

        /* ------ HAPTICS ------
        List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();

        UnityEngine.XR.InputDevices.GetDevicesWithRole(UnityEngine.XR.InputDeviceRole.RightHanded, devices);

        foreach (var device in devices)
        {
            UnityEngine.XR.HapticCapabilities capabilities;
            if (device.TryGetHapticCapabilities(out capabilities))
            {
                    if (capabilities.supportsImpulse)
                    {
                        uint channel = 0;
                        float amplitude = 0.5f;
                        float duration = 1.0f;
                        device.SendHapticImpulse(channel, amplitude, duration);
                    }
            }
        }
        */
    }
}

#endif
