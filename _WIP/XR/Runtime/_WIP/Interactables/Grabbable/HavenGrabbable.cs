//-----------------------------------------------------------------------
// <copyright file="HavenOffsetGrabbable.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !USING_UNITY_XR_INTERACTION_TOOLKIT
#pragma warning disable CS0414, IDE0051
#endif

namespace OGT.Haven
{
    using OGT.Networking;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.XR.Interaction.Toolkit;

    [AddComponentMenu("Haven XR/Interactables/HXR Grabbable")]
    public class HavenGrabbable : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
    {
#pragma warning disable 0649
        [SerializeField] private HavenGrabbableSettingsObject havenGrabbableSettings;
        [SerializeField] private bool isOffsetGrabbable = true;
        [SerializeField] private bool disableRayGrab;

        [Header("Hover")]
        [SerializeField] private UnityEvent onHoverStart;
        [SerializeField] private UnityEvent onHoverStop;

        [Header("Grab")]
        [SerializeField] private UnityEvent onGrabStart;
        [SerializeField] private UnityEvent onGrabStop;

        [Header("Use")]
        [SerializeField] private UnityEvent onUseStart;
        [SerializeField] private UnityEvent onUseStop;
#pragma warning restore 0649

#if USING_UNITY_XR_INTERACTION_TOOLKIT
        private NetworkIdentity networkIdentity;
        private bool usingNetworking;
        private float originalTightenPosition;
        private float awakeTime;

        private bool isBeingHovered;
        private bool isBeingGrabbed;
        private bool isBeingUsed;

        private bool hasInitializedAttachTransformInfo;
        private Vector3 originalAttachTransformPosition;
        private Quaternion originalAttachTransformRotation;

        public bool IsBeingHovered => this.isBeingHovered;

        public bool IsBeingGrabbed => this.isBeingGrabbed;

        public bool IsBeingUsed => this.isBeingUsed;

        public bool IsOffsetGrabbable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.isOffsetGrabbable;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.isOffsetGrabbable = value;
        }

        public bool DisableRayGrab
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.disableRayGrab;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.disableRayGrab = value;
        }

        public override bool IsHoverableBy(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRHoverInteractor interactor)
        {
            if (this.disableRayGrab && interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor)
            {
                return false;
            }

            return base.IsHoverableBy(interactor);
        }

        public override bool IsSelectableBy(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor interactor)
        {
            if (this.disableRayGrab && interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor)
            {
                return false;
            }

            return base.IsSelectableBy(interactor) &&
                (this.interactionLayers.value & interactor.interactionLayers.value) != 0;
        }

        protected override void Awake()
        {
            base.Awake();

            this.networkIdentity = this.GetComponent<NetworkIdentity>();
            this.usingNetworking = this.networkIdentity != null;
            this.originalTightenPosition = this.tightenPosition;
            this.awakeTime = Time.realtimeSinceStartup;

            this.firstHoverEntered.AddListener(this.HoverStart);
            this.lastHoverExited.AddListener(this.HoverStop);
            this.selectEntered.AddListener(this.GrabStart);
            this.selectExited.AddListener(this.GrabStop);
            this.activated.AddListener(this.UseStart);
            this.deactivated.AddListener(this.UseStop);

            if (this.havenGrabbableSettings != null)
            {
                this.havenGrabbableSettings.Apply(this);
            }
        }

        protected override void OnSelectEntered(SelectEnterEventArgs selectEnterEventArgs)
        {
            if (this.isOffsetGrabbable)
            {
                if (this.attachTransform == null)
                {
                    this.attachTransform = new GameObject("Attach Transform").transform;
                    this.attachTransform.SetParent(this.transform);
                    this.attachTransform.Reset();
                }

                if (this.hasInitializedAttachTransformInfo == false)
                {
                    this.hasInitializedAttachTransformInfo = true;
                    this.originalAttachTransformPosition = this.attachTransform.localPosition;
                    this.originalAttachTransformRotation = this.attachTransform.localRotation;
                }

                var attachTransform = selectEnterEventArgs.interactorObject.GetAttachTransform(this);
                this.attachTransform.position = attachTransform.position;
                this.attachTransform.rotation = attachTransform.rotation;

                // Fixing bug with tighten position and direct interactors
                bool isDirect = selectEnterEventArgs.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor;
                this.tightenPosition = isDirect ? 1 : this.originalTightenPosition;
            }

            if (this.usingNetworking && Time.realtimeSinceStartup - this.awakeTime > 1.0f)
            {
                this.networkIdentity.RequestOwnership();
            }

            base.OnSelectEntered(selectEnterEventArgs);
        }

        protected override void OnSelectExited(SelectExitEventArgs selectExitedEventArgs)
        {
            if (this.isOffsetGrabbable)
            {
                this.attachTransform.localPosition = this.originalAttachTransformPosition;
                this.attachTransform.localRotation = this.originalAttachTransformRotation;
            }

            if (this.usingNetworking && Time.realtimeSinceStartup - this.awakeTime > 1.0f)
            {
                this.networkIdentity.ReleaseOwnership();
            }

            base.OnSelectExited(selectExitedEventArgs);
        }

        private void OnValidate()
        {
            EditorUtil.SetIfNull(this, ref this.havenGrabbableSettings, "7e6b6732524710d4dadd8d667f3fb00b");
            HavenInteractableUtil.SetupInteractable(this);
        }

        private void HoverStart(HoverEnterEventArgs hoverEnterEventArgs)
        {
            if (this.isBeingGrabbed == false)
            {
                this.onHoverStart?.Invoke();
                this.isBeingHovered = false;
            }
        }

        private void HoverStop(HoverExitEventArgs hoverExitEventArgs)
        {
            if (this.isBeingGrabbed == false)
            {
                this.isBeingHovered = true;
                this.onHoverStop?.Invoke();
            }
        }

        private void GrabStart(SelectEnterEventArgs selectEnterEventArgs)
        {
            this.isBeingGrabbed = true;
            this.onGrabStart?.Invoke();
        }

        private void GrabStop(SelectExitEventArgs selectExitEventArgs)
        {
            // NOTE [bgish]: If we're using it, but let go of it, make sure onUseStop event is fired
            if (this.isBeingUsed)
            {
                this.UseStop(null);
            }

            this.onGrabStop?.Invoke();
            this.isBeingGrabbed = false;
        }

        private void UseStart(ActivateEventArgs activateEventArgs)
        {
            this.isBeingUsed = true;
            this.onUseStart?.Invoke();
        }

        private void UseStop(DeactivateEventArgs deactivateEventArgs)
        {
            this.onUseStop?.Invoke();
            this.isBeingUsed = false;
        }
#endif
    }
}
