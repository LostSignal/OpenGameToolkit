#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenSimple.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Haven
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using UnityEngine;


    [AddComponentMenu("Haven XR/Interactables/HXR Simple")]
    public class HavenSimple : UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable, IAwake, IValidate
    {
#pragma warning disable 0649
        [SerializeField] private HavenSimpleSettingsObject havenSimpleSettings;
        [SerializeField] private bool disableRayGrab;
#pragma warning restore 0649

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            EditorUtil.SetIfNull(this, ref this.havenSimpleSettings, "c533b0e320be29a468a40f3bad7648b2");
            HavenInteractableUtil.SetupInteractable(this);

            report.AssertNotNull(this, this.havenSimpleSettings, nameof(this.havenSimpleSettings));
        }

        public void OnAwake(Bootloader bootloader)
        {
            this.havenSimpleSettings.Apply(this);
        }

#if USING_UNITY_XR_INTERACTION_TOOLKIT
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

            return base.IsSelectableBy(interactor);
        }

        protected override void Awake()
        {
            base.Awake();
            ActivationManager.Register(this);
        }
#endif
    }
}
