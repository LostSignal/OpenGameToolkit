//-----------------------------------------------------------------------
// <copyright file="HavenTeleport.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !USING_UNITY_XR_INTERACTION_TOOLKIT
#pragma warning disable CS0414, IDE0051
#endif

namespace OGT.Haven
{
    using System;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.XR.Interaction.Toolkit;

    [AddComponentMenu("Haven XR/Interactables/HXR Teleport")]
    public class HavenTeleport : UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.BaseTeleportationInteractable, IStart
    {
        public enum TeleportType
        {
            Area,
            Anchor,
        }

#pragma warning disable 0649
        [SerializeField] private TeleportType type;

        [ShowIf("type", TeleportType.Anchor)]
        [Tooltip("The Transform that represents the teleportation destination.")]
        [SerializeField] private Transform anchorTransform;

        [ShowIf("type", TeleportType.Anchor)]
        [SerializeField] private bool matchAnchorOrientation = true;

        [Header("Hover")]
        [SerializeField] private UnityEvent onHoverStart;
        [SerializeField] private UnityEvent onHoverStop;
        [SerializeField] private UnityEvent onTeleport;
#pragma warning restore 0649

#if USING_UNITY_XR_INTERACTION_TOOLKIT

        private Action<UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor, UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportRequest> onTeleportAction;

        public event Action<UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor, UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportRequest> OnTeleport
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            add => this.onTeleportAction += value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            remove => this.onTeleportAction -= value;
        }

        private Transform AnchorOverrideTransform => this.anchorTransform != null ? this.anchorTransform : this.transform;

#endif

        public void OnStart()
        {
#if USING_UNITY_XR_INTERACTION_TOOLKIT
            this.teleportationProvider = HavenRig.Instance.TeleportationProvider;
            this.interactionManager = XRInteractionHelper.XRInteractionManagerInstance;

            this.firstHoverEntered.AddListener(this.OnFirstHoverEnter);
            this.lastHoverExited.AddListener(this.OnLastHoverExit);
#endif
        }

#if USING_UNITY_XR_INTERACTION_TOOLKIT
        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.firstHoverEntered.RemoveListener(this.OnFirstHoverEnter);
            this.lastHoverExited.RemoveListener(this.OnLastHoverExit);
        }

        protected void OnDrawGizmos()
        {
            if (this.type == TeleportType.Anchor)
            {
                Gizmos.color = Color.blue;
                GizmoHelpers.DrawWireCubeOriented(this.AnchorOverrideTransform.position, this.AnchorOverrideTransform.rotation, 1f);
                GizmoHelpers.DrawAxisArrows(this.AnchorOverrideTransform, 1f);
            }
        }

        protected override bool GenerateTeleportRequest(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactor, RaycastHit raycastHit, ref UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportRequest teleportRequest)
        {
            if (this.type == TeleportType.Area)
            {
                teleportRequest = new UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportRequest
                {
                    destinationPosition = raycastHit.point,
                    destinationRotation = this.transform.rotation,
                    matchOrientation = UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.MatchOrientation.WorldSpaceUp,
                };
            }
            else if (this.type == TeleportType.Anchor)
            {
                var anchorOverrideTransform = this.AnchorOverrideTransform;

                teleportRequest = new UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportRequest
                {
                    destinationPosition = anchorOverrideTransform.position,
                    destinationRotation = anchorOverrideTransform.rotation,
                    matchOrientation = this.matchAnchorOrientation ? UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.MatchOrientation.TargetUpAndForward : UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.MatchOrientation.WorldSpaceUp,
                };
            }
            else
            {
                throw new NotImplementedException();
            }

            try
            {
                this.onTeleport.SafeInvoke();
                this.onTeleportAction?.Invoke(interactor, teleportRequest);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return true;
        }

        private void OnValidate()
        {
            HavenInteractableUtil.SetupTeleport(this);

            if (this.teleportTrigger != TeleportTrigger.OnSelectEntered)
            {
                this.teleportTrigger = TeleportTrigger.OnSelectEntered;
                EditorUtil.SetDirty(this);
            }

            if (this.type == TeleportType.Anchor && this.anchorTransform == null)
            {
                this.anchorTransform = this.transform;
                EditorUtil.SetDirty(this);
            }

            if (this.interactionLayers != LostLayers.Teleport)
            {
                this.interactionLayers = LostLayers.Teleport;
                EditorUtil.SetDirty(this);
            }

            if (this.teleportationProvider != null)
            {
                this.teleportationProvider = null;
                EditorUtil.SetDirty(this);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            ActivationManager.Register(this);
        }

        private void OnFirstHoverEnter(HoverEnterEventArgs args)
        {
            this.onHoverStart.SafeInvoke();
        }

        private void OnLastHoverExit(HoverExitEventArgs args)
        {
            this.onHoverStop.SafeInvoke();
        }
#endif
    }
}
