//-----------------------------------------------------------------------
// <copyright file="UnityDispatcher.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using UnityEngine;

    public class UnityDispatcher : MonoBehaviour
    {
        public event EventHandler OnBackButtonPressed;
        public event EventHandler OnUpdate;
        public event EventHandler OnLateUpdate;
        public event EventHandler OnFixedUpdate;
        public event EventHandler OnApplicationQuitting;
        public event EventHandler<bool> OnApplicationFocusChanged;

        private void Update()
        {
            try
            {
                this.OnUpdate?.Invoke(null, null);
            }
            catch (Exception e)
            {
                Debug.LogError($"UnityDispatcher Update Exception: {e}");
            }

#if UNITY_ANDROID || UNITY_EDITOR || UNITY_STANDALONE
            try
            {
                // NOTE [bgish]: this catches the Android Back Button
                if (UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame == true)
                {
                    this.OnBackButtonPressed?.Invoke(null, null);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"UnityDispatcher Back Button Exception: {e}");
            }
#endif
        }

        private void LateUpdate()
        {
            this.OnLateUpdate?.Invoke(null, null);
        }

        private void FixedUpdate()
        {
            this.OnFixedUpdate?.Invoke(null, null);
        }

        private void OnApplicationQuit()
        {
            this.OnApplicationQuitting?.Invoke(null, null);
        }

        private void OnApplicationFocus(bool focus)
        {
            this.OnApplicationFocusChanged?.Invoke(null, focus);
        }
    }
}
