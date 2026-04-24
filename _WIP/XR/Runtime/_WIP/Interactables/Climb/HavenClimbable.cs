//-----------------------------------------------------------------------
// <copyright file="HavenClimbable.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Haven
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;

    [AddComponentMenu("Haven XR/Interactables/HXR Climbable")]
    public class HavenClimbable : UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable, IAwake, IValidate
    {
        private static readonly Dictionary<long, HavenHand> Hands = new();

#pragma warning disable 0649
        [SerializeField] private HavenClimbableSettingsObject havenClimbableSettings;
        [SerializeField] private Rigidbody climbRigidbody;
#pragma warning restore 0649

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNull(this, this.havenClimbableSettings, nameof(this.havenClimbableSettings));
            report.AssertNotNull(this, this.climbRigidbody, nameof(this.climbRigidbody));
            report.AssertTrue(this, this.climbRigidbody.isKinematic, nameof(this.climbRigidbody.isKinematic));
        }

        public void OnAwake(Bootloader bootloader)
        {
            this.havenClimbableSettings.Apply(this);
        }

#if USING_UNITY_XR_INTERACTION_TOOLKIT
        protected override void Awake()
        {
            base.Awake();
            ActivationManager.Register(this);
        }

        public override bool IsHoverableBy(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRHoverInteractor interactor)
        {
            return interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor && base.IsHoverableBy(interactor);
        }

        public override bool IsSelectableBy(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor interactor)
        {
            return interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor && base.IsSelectableBy(interactor);
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            var havenHand = this.GetHavenHand(args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor);

            if (havenHand != null)
            {
                havenHand.Rig.StartClimbing(havenHand.Hand);
            }

            base.OnSelectEntered(args);
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            var havenHand = this.GetHavenHand(args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor);

            if (havenHand != null)
            {
                havenHand.Rig.StopClimbing(havenHand.Hand);
            }

            base.OnSelectExited(args);
        }

        private HavenHand GetHavenHand(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
        {
            if (interactor == null)
            {
                return null;
            }

            if (Hands.TryGetValue(interactor.GetEntityId(), out HavenHand havenHand) == false)
            {
                havenHand = interactor.transform.parent.GetComponent<HavenHand>();

                if (havenHand != null)
                {
                    Hands.Add(interactor.GetEntityId(), havenHand);
                }
            }

            return havenHand;
        }

        [EditorEvents.OnExitPlayMode]
        private static void ResetHands()
        {
            Hands.Clear();
        }

        private void OnValidate()
        {
            EditorUtil.SetIfNull(this, ref this.havenClimbableSettings, "bf2e9105aa6b8fa4aaee8519fe305e62");
            EditorUtil.SetIfNull(this, ref this.climbRigidbody);

            if (this.climbRigidbody != null && this.climbRigidbody.isKinematic == false)
            {
                this.climbRigidbody.isKinematic = true;
                EditorUtil.SetDirty(this);
            }

            if (this.selectMode != UnityEngine.XR.Interaction.Toolkit.Interactables.InteractableSelectMode.Single)
            {
                this.selectMode = UnityEngine.XR.Interaction.Toolkit.Interactables.InteractableSelectMode.Single;
                EditorUtil.SetDirty(this);
            }

            HavenInteractableUtil.SetupInteractable(this);
        }
#endif
    }
}
