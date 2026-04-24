#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenSocket.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Haven
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.XR.Interaction.Toolkit;

    [AddComponentMenu("Haven XR/Socket/HXR Socket")]
    public class HavenSocket : UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor, IAwake, IValidate
    {
#pragma warning disable 0649
        [SerializeField] private HavenSocketSettingsObject havenSocketSettings;
        [SerializeField] private bool disableInteractorAndInteractableOnSocketed;
        [SerializeField] private bool onlyAllowSpecificSocketTarget;
        [SerializeField] private UnityEvent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable> onSocketed;

        [ShowIf("onlyAllowSpecificSocketTarget", true)]
        [SerializeField] private string socketTargetName;
#pragma warning restore 0649

        public string SocketTargetName => this.socketTargetName;

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNull(this, this.havenSocketSettings, nameof(this.havenSocketSettings));
        }

        public void OnAwake(Bootloader bootloader)
        {
            this.havenSocketSettings.Apply(this);
        }

#if USING_UNITY_XR_INTERACTION_TOOLKIT

        public override bool CanHover(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRHoverInteractable interactable)
        {
            bool canHover = base.CanHover(interactable);

            if (canHover && this.onlyAllowSpecificSocketTarget)
            {
                var xrBaseInteractable = interactable as UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable;

                if (xrBaseInteractable != null)
                {
                    canHover &= HavenSocketTarget.GetSocketTargetName(xrBaseInteractable) == this.socketTargetName;
                }
            }

            return canHover;
        }

        public override bool CanSelect(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable interactable)
        {
            bool canSelect = base.CanSelect(interactable);

            if (canSelect && this.onlyAllowSpecificSocketTarget)
            {
                var xrBaseInteractable = interactable as UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable;

                if (xrBaseInteractable != null)
                {
                    canSelect &= HavenSocketTarget.GetSocketTargetName(xrBaseInteractable) == this.socketTargetName;
                }
            }

            return canSelect;
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            if (this.onSocketed != null)
            {
                this.onSocketed?.Invoke(args.interactableObject as UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable);
            }

            if (this.disableInteractorAndInteractableOnSocketed)
            {
                this.ExecuteDelayed(0.2f, () =>
                {
                    this.enabled = false;
                    this.socketActive = false;

                    var interactable = args.interactableObject as UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable;
                    var socketTarget = interactable != null ? interactable.GetComponent<HavenSocketTarget>() : null;

                    if (socketTarget != null)
                    {
                        var parent = this.attachTransform != null ? this.attachTransform : this.transform;
                        socketTarget.transform.SetParent(parent);
                        socketTarget.DisableInteractable();
                    }
                });
            }
        }

        protected override void Awake()
        {
            base.Awake();
            ActivationManager.Register(this);
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            EditorUtil.SetIfNull(this, ref this.havenSocketSettings, "c336bbd69f11b7d48aef5ba5aea19c37");
        }
#endif
    }
}
