//-----------------------------------------------------------------------
// <copyright file="XRPanelSettings.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.XR
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "OGT/XR/XR Panel Settings")]
    public class XRPanelSettings : Resource
    {
        [Tooltip("The forwards distance from the camera that this object should be placed.")]
        [SerializeField] private float canvasDistanceForwards = 2.0f;

        [Tooltip("The upwards distance from the camera that this object should be placed.")]
        [SerializeField] private float canvasDistanceUpwards = 0.0f;

        [Tooltip("The speed at which this object changes its position.")]
        [SerializeField] private float positionLerpSpeed = 2.0f;

        [Tooltip("The speed at which this object changes its rotation.")]
        [SerializeField] private float rotationLerpSpeed = 10.0f;

        public float CanvasDistanceForwards
        {
            get => this.canvasDistanceForwards;
        }

        public float CanvasDistanceUpwards
        {
            get => this.canvasDistanceUpwards;
        }

        public float PositionLerpSpeed
        {
            get => this.positionLerpSpeed;
        }

        public float RotationLerpSpeed
        {
            get => this.rotationLerpSpeed;
        }
    }
}
