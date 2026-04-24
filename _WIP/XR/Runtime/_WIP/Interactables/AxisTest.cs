#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="AxisTest.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.XR
{
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;

    ////
    //// NOTE [bgish]: Got some inspiration from here https://www.youtube.com/watch?v=xMPQa2MWmHk
    //// NOTE [bgish]: Need to detect if objectTransform has a Rigidbody and use that instead of the transform
    //// NOTE [bgish]: Need to transfer velocity to object after letting go of it
    ////
    public class AxisTest : MonoBehaviour, IValidate
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

#pragma warning disable 0649
        [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;
        [SerializeField] private Transform objectTransform;
        [SerializeField] private Rigidbody objectRigidbody;
        [SerializeField] private Vector3 startPosition;
        [SerializeField] private Vector3 endPosition;

        [HideInInspector]
        [SerializeField] private float percentage;
#pragma warning restore 0649

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNull(this, this.interactable, nameof(this.interactable));
            report.AssertNotNull(this, this.objectTransform, nameof(this.objectTransform));

            if (this.interactable != null && this.interactable.gameObject.layer != Layers.Interactable)
            {
                report.ReportError(this, "Wrong Layer", $"Interactable '{nameof(this.interactable)}' is not on the interactable layer, so won't be able to manipulate it.");
            }

            if (this.objectTransform != null && this.objectTransform.parent != this.transform)
            {
                report.ReportError(this, "Bad Parent", $"Object '{nameof(this.objectTransform)}' is not a direct child!");
            }
        }

#if USING_UNITY_XR_INTERACTION_TOOLKIT
        private Transform handTransform;
        private float startHandPercentage;
        private float startObjectPercentage;
        private float startToEndLength;
        private Vector3 bHat;

        public Transform ObjectTransform => this.objectTransform;

        public Vector3 StartPosition => this.startPosition;

        public Vector3 EndPosition => this.endPosition;

        public float Percentage
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.percentage;
        }

        public void SetPercentage(float percentage)
        {
            this.percentage = Mathf.Clamp01(percentage);
            Vector3 newPosition = Vector3.Lerp(this.startPosition, this.endPosition, this.percentage);

            if (this.objectRigidbody != null)
            {
                this.objectRigidbody.MovePosition(newPosition);
            }
            else
            {
                this.objectTransform.localPosition = newPosition;
            }
        }

        private void Awake()
        {
            this.interactable.firstSelectEntered.AddListener(this.OnSelectEnter);
            this.interactable.lastSelectExited.AddListener(this.OnSelectExit);
            this.enabled = false;

            if (this.objectRigidbody != null && this.objectRigidbody.isKinematic == false)
            {
                this.objectRigidbody.isKinematic = true;
            }

            this.PreCompute();
        }

        private void Update()
        {
            if (this.handTransform != null && this.objectRigidbody == null)
            {
                this.objectTransform.localPosition = this.GetNewPosition();
            }
        }

        private void FixedUpdate()
        {
            if (this.handTransform != null && this.objectRigidbody != null)
            {
                this.objectRigidbody.MovePosition(this.transform.localToWorldMatrix.MultiplyPoint(this.GetNewPosition()));
            }
        }

        private Vector3 GetNewPosition()
        {
            this.percentage = Mathf.Clamp01(this.startObjectPercentage + (this.GetCurrentUnclampedHandPercentage() - this.startHandPercentage));
            return Vector3.Lerp(this.startPosition, this.endPosition, this.percentage);
        }

        private void OnDestroy()
        {
            this.interactable.firstSelectEntered.RemoveListener(this.OnSelectEnter);
            this.interactable.lastSelectExited.RemoveListener(this.OnSelectExit);
        }

        private void OnValidate()
        {
            //// TODO [bgish]: Print error if handle is not on the interactable layer

            this.PreCompute();

            // Prepopulating the rigidbody if it exists
            if (this.objectTransform != null && this.objectRigidbody == null)
            {
                var rigidbody = this.objectTransform.GetComponent<Rigidbody>();

                if (rigidbody != null)
                {
                    this.objectRigidbody = rigidbody;
                    EditorUtil.SetDirty(this);
                }
            }

            LostLayers.SetInteractable(this.interactable.colliders);
        }

        private void PreCompute()
        {
            this.startToEndLength = (this.endPosition - this.startPosition).magnitude;
            this.bHat = (this.endPosition - this.startPosition).normalized;
        }

        private void OnSelectEnter(SelectEnterEventArgs args)
        {
            var interactor = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor;

            if (interactor == null)
            {
                return;
            }

            this.handTransform = interactor.transform;
            this.startObjectPercentage = this.percentage;
            this.startHandPercentage = this.GetCurrentUnclampedHandPercentage();
            this.enabled = true;
        }

        public void OnSelectExit(SelectExitEventArgs args)
        {
            this.enabled = false;
            this.handTransform = null;
        }

        private float GetCurrentUnclampedHandPercentage()
        {
            var handPosition = this.transform.worldToLocalMatrix.MultiplyPoint(this.handTransform.position);
            var a = handPosition - this.startPosition;
            return Vector3.Dot(a, this.bHat) / this.startToEndLength;
        }

        public void SetStartPosition()
        {
            this.startPosition = this.objectTransform.localPosition;
        }

        public void SetEndPosition()
        {
            this.endPosition = this.objectTransform.localPosition;
        }
#endif
    }
}
