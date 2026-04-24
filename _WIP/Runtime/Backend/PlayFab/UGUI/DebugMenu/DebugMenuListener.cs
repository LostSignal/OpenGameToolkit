//--------------------------------------------------------------------s---
// <copyright file="DebugMenuListener.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using UnityEngine;

    [RequireComponent(typeof(DebugMenu))]
    public class DebugMenuListener : GameBehavior, IValidate
    {
#pragma warning disable 0649
        [SerializeField][HideInInspector] private DebugMenu debugMenu;
#pragma warning restore 0649

        private UnityEngine.InputSystem.Controls.KeyControl keyboardKey = null;
        private float fingerHoldTime = 0.0f;
        private float keyHoldTime = 0.0f;

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            this.EditorGetComponent(ref this.debugMenu);

            report.AssertNotNull(this, this.debugMenu, nameof(this.debugMenu));
        }

        private void Update()
        {
            this.CheckTouch();
            this.CheckKeyboard();
        }

        private void CheckTouch()
        {
            if (UnityEngine.Input.touchCount == this.debugMenu.Settings.FingerDownCount)
            {
                this.fingerHoldTime += Time.unscaledDeltaTime;

                if (this.fingerHoldTime > this.debugMenu.Settings.FingerDownTime)
                {
                    this.fingerHoldTime = 0.0f;
                    this.debugMenu.ShowMenu();
                }
            }
            else
            {
                this.fingerHoldTime = 0.0f;
            }
        }

        private void CheckKeyboard()
        {
            if (this.keyboardKey == null)
            {
                var keyboard = UnityEngine.InputSystem.Keyboard.current;

                if (keyboard != null)
                {
                    this.keyboardKey = keyboard.FindKeyOnCurrentKeyboardLayout(this.debugMenu.Settings.Key.ToString());
                }
            }

            if (keyboardKey.wasPressedThisFrame)
            {
                this.keyHoldTime += Time.unscaledDeltaTime;

                if (this.keyHoldTime > this.debugMenu.Settings.KeyHoldTime)
                {
                    this.keyHoldTime = 0.0f;
                    this.debugMenu.ShowMenu();
                }
            }
            else
            {
                this.keyHoldTime = 0.0f;
            }
        }
    }
}

#endif
