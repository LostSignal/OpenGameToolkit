//-----------------------------------------------------------------------
// <copyright file="XRManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using OGT.XR;
    using UnityEngine;
    using UnityEngine.InputSystem.UI;

    public sealed class XRManager : Manager
    {
        private static readonly OGTLogger Logger = OGTLogger.XR;

        [SerializeField] private XRUtilManager xrUtilManager;
        [SerializeField] private bool initializeOnStartup = true;

        [Header("Event Systems")]
        [SerializeField] private InputSystemUIInputModule flatInputSystem;
        [SerializeField] private OGTXRUIInputModule xrInputSystem;

        public event System.Action OnXRDeviceChange
        {
            add => this.xrUtilManager.OnXRDeviceChange += value;
            remove => this.xrUtilManager.OnXRDeviceChange -= value;
        }

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

        protected override Task InitializeManager(Bootloader bootloader)
        {
            this.xrUtilManager.OnXRDeviceChange += this.UpdateInputSystem;
            this.xrUtilManager.Initialize();
            return Task.CompletedTask;
        }

        public override void OnManagerDestroyed()
        {
            this.xrUtilManager.OnXRDeviceChange -= this.UpdateInputSystem;
        }

        public void UpdateInputSystem()
        {
            //// TODO [bgish]: This feature is turned off for the moment
            //// var isPancakeMode = this.IsFlatMode;
            //// this.flatInputSystem.enabled = isPancakeMode;
            //// this.xrInputSystem.enabled = isPancakeMode == false;

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
    }
}
