#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="XRManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Runtime.CompilerServices;
    using OGT.XR;
    using UnityEngine;
    using UnityEngine.InputSystem.UI;

    public sealed class XRManager : MonoBehaviour // Manager<XRManager>, IOnManagersReady
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;
        private static XRManager instance;

#pragma warning disable 0649
        [SerializeField] private XRUtilManager xrUtilManager;

        [Header("Event Systems")]
        [SerializeField] private InputSystemUIInputModule flatInputSystem;
        [SerializeField] private LostXRUIInputModule xrInputSystem;
#pragma warning restore 0649

        public static XRManager Instance => instance;

        public bool IsFlatMode
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.xrUtilManager.IsFlatMode;
        }

        public XRDevice CurrentDevice
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.xrUtilManager.CurrentDevice;
        }

        public void UpdateInputSystem()
        {
            this.UpdateInputSystem(this.IsFlatMode);
        }

        //// #if !USING_UNITY_XR_INTERACTION_TOOLKIT && UNITY_EDITOR
        ////
        //// [ShowEditorInfo]
        //// public string GetXRInteractionToolkitInfoMessage() => "Unity's XR Interaction Toolkit not present.  XR Manager will always return Pancake mode.";
        ////
        //// [ExposeInEditor("Add XR Interaction Toolkit Package")]
        //// public void AddXRInteractionToolkitPackage()
        //// {
        ////     PackageManagerUtil.Add("com.unity.xr.interaction.toolkit");
        //// }
        ////
        //// #endif
        ////
        //// #if !USING_UNITY_XR_MANAGEMENT && UNITY_EDITOR
        ////
        //// [ShowEditorInfo]
        //// public string GetXRManagementInfoMessage() => "Unity's XR Management is not present.  XR Manager will always return Pancake mode.";
        ////
        //// [ExposeInEditor("Add XR Manager Package")]
        //// public void AddXRManagementPackage()
        //// {
        ////     PackageManagerUtil.Add("com.unity.xr.management");
        //// }
        ////
        //// #endif
        ////
        //// public override void Initialize()
        //// {
        ////     #if !USING_UNITY_XR
        ////     this.UpdateInputSystem(true);
        ////     this.SetInstance(this);
        ////     #else
        ////     ManagersReady.Register(this);
        ////     #endif
        //// }

        [EditorEvents.OnExitPlayMode]
        private static void ResetInstance()
        {
            instance = null;
        }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            this.UpdateInputSystem();
            this.xrUtilManager.OnXRDeviceChange += this.UpdateInputSystem;
        }

        private void OnDestroy()
        {
            this.xrUtilManager.OnXRDeviceChange -= this.UpdateInputSystem;
        }

        private void UpdateInputSystem(bool isPancakeMode)
        {
            if (this.flatInputSystem)
            {
                this.flatInputSystem.enabled = isPancakeMode;
            }
            else
            {
                if (isPancakeMode == false)
                {
                    Logger.LogError("XR Manager doesn't have flatInputSystem specified.  Input will not work correctly.");
                }
            }

            if (this.xrInputSystem)
            {
                this.xrInputSystem.enabled = !isPancakeMode;
            }

            //// TODO [bgish]: Start/Stop listening for XR Keyboard
            //// this.ListenForXRKeyboard();
        }

        //// private void ListenForXRKeyboard()
        //// {
        ////     this.StartCoroutine(Coroutine());
        ////
        ////     static IEnumerator Coroutine()
        ////     {
        ////         XRKeyboard xrKeyboard = DialogManager.GetDialog<XRKeyboard>();
        ////
        ////         while (true)
        ////         {
        ////             if (InputFieldTracker.IsInputFieldSelected && xrKeyboard.Dialog.IsHidden)
        ////             {
        ////                 xrKeyboard.Dialog.Show();
        ////             }
        ////
        ////             // NOTE [bgish]: This is important and kinda hacky, we need to call InputFieldTracker.IsInputFieldSelected every
        ////             //               frame if we want to properly track the last known selection of the text input.  We only care
        ////             //               though if the keyboard dialog is showing, else we can just check every quarter second.
        ////             yield return xrKeyboard.Dialog.IsShowing ? null : WaitForUtil.Seconds(0.25f);
        ////         }
        ////     }
        //// }
    }
}
