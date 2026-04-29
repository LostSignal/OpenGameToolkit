#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenRayScaler.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace OGT.Haven
{
    using OGT;
    using UnityEngine;

    [RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor))]
    public class HavenRayScaler : MonoBehaviour, IValidate
    {
#pragma warning disable 0649
        [SerializeField] private AnimationCurve scaleCurve;
        [SerializeField] private AnimationCurve rigScaleMultiplierCurve;
        [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor xrRayInteractor;
#pragma warning restore 0649

        private HavenRig havenRig;

        private void Awake()
        {
            this.havenRig = this.GetComponentInParent<HavenRig>();
        }

        private void Update()
        {
            float dot = Mathf.Clamp01(Vector3.Dot(this.transform.forward, Vector3.up));
            this.xrRayInteractor.velocity = this.scaleCurve.Evaluate(dot) * this.rigScaleMultiplierCurve.Evaluate(this.havenRig.RigScale);
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNull(this, this.xrRayInteractor, nameof(this.xrRayInteractor));
        }
    }
}

#endif
