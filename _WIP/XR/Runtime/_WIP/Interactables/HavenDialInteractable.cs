#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenDialInteractable.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace OGT.Haven
{
    using OGT;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.XR.Interaction.Toolkit;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    [AddComponentMenu("Haven XR/Interactables/HXR Dial Interactable")]
    public class HavenDialInteractable : UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable
    {
#pragma warning disable 0649
        [SerializeField] private InteractionType dialType = InteractionType.ControllerRotation;
        [SerializeField] private Rigidbody rotatingRigidbody;
        [SerializeField] private Vector3 localRotationAxis;
        [SerializeField] private Vector3 localAxisStart;
        [SerializeField] private float rotationAngleMaximum;

        [Tooltip("If 0, this is a float dial going from 0 to 1, if not 0, that dial is int with that many steps")]
        [SerializeField] private int steps = 0;
        [SerializeField] private bool snapOnRelease = true;
        [SerializeField] private AudioBlock snapAudioBlock;
#pragma warning restore 0649

        private UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor grabbingInteractor;
        private Quaternion grabbedRotation;
        private Vector3 startingWorldAxis;
        private float currentAngle = 0;
        private int currentStep = 0;
        private float stepSize;
        private Transform syncTransform;
        private Transform originalTransform;

        public enum InteractionType
        {
            ControllerRotation,
            ControllerPull,
        }

        public DialTurnedAngleEvent OnDialAngleChanged { get; set; }

        public DialTurnedStepEvent OnDialStepChanged { get; set; }

        public DialChangedEvent OnDialChanged { get; set; }

        public float CurrentAngle => this.currentAngle;

        public int CurrentStep => this.currentStep;

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            if (this.isSelected)
            {
                if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed)
                {
                    this.startingWorldAxis = this.originalTransform.TransformDirection(this.localAxisStart);

                    Vector3 worldAxisStart = this.syncTransform.TransformDirection(this.localAxisStart);
                    Vector3 worldRotationAxis = this.syncTransform.TransformDirection(this.localRotationAxis);
                    Vector3 newRight;
                    float angle;

                    if (this.dialType == InteractionType.ControllerRotation)
                    {
                        Quaternion difference = this.grabbingInteractor.transform.rotation * Quaternion.Inverse(this.grabbedRotation);

                        newRight = difference * worldAxisStart;

                        // Get the new angle between the original right and this new right along the up axis
                        angle = Vector3.SignedAngle(this.startingWorldAxis, newRight, worldRotationAxis);

                        if (angle < 0)
                        {
                            angle = 360 + angle;
                        }
                    }
                    else
                    {
                        Vector3 centerToController = this.grabbingInteractor.transform.position - this.transform.position;
                        centerToController.Normalize();

                        newRight = centerToController;

                        angle = Vector3.SignedAngle(this.startingWorldAxis, newRight, worldRotationAxis);

                        if (angle < 0)
                        {
                            angle = 360 + angle;
                        }
                    }

                    // If the angle is < 0 or > to the max rotation, we clamp but TO THE CLOSEST (a simple clamp would clamp
                    // of an angle of 350 for a 0-180 angle range would clamp to 180, when we want to clamp to 0)
                    if (angle > this.rotationAngleMaximum)
                    {
                        float upDiff = 360 - angle;
                        float lowerDiff = angle - this.rotationAngleMaximum;

                        if (upDiff < lowerDiff)
                        {
                            angle = 0;
                        }
                        else
                        {
                            angle = this.rotationAngleMaximum;
                        }
                    }

                    float finalAngle = angle;
                    if (!this.snapOnRelease && this.steps > 0)
                    {
                        int step = Mathf.RoundToInt(angle / this.stepSize);
                        finalAngle = step * this.stepSize;

                        if (!Mathf.Approximately(finalAngle, this.currentAngle))
                        {
                            if (this.snapAudioBlock != null)
                            {
                                this.snapAudioBlock.PlayOneShot(this.transform.position);
                            }

                            this.OnDialStepChanged.Invoke(step);
                            this.OnDialChanged.Invoke(this);
                            this.currentStep = step;
                        }
                    }

                    // First, we use the raw angle to move the sync transform, that allow to keep the proper current rotation
                    // even if we snap during rotation
                    newRight = Quaternion.AngleAxis(angle, worldRotationAxis) * this.startingWorldAxis;
                    angle = Vector3.SignedAngle(worldAxisStart, newRight, worldRotationAxis);
                    Quaternion newRot = Quaternion.AngleAxis(angle, worldRotationAxis) * this.syncTransform.rotation;

                    // Then we redo it but this time using finalAngle, that will snap if needed.
                    newRight = Quaternion.AngleAxis(finalAngle, worldRotationAxis) * this.startingWorldAxis;
                    this.currentAngle = finalAngle;
                    this.OnDialAngleChanged.Invoke(finalAngle);
                    this.OnDialChanged.Invoke(this);
                    finalAngle = Vector3.SignedAngle(worldAxisStart, newRight, worldRotationAxis);
                    Quaternion newRBRotation = Quaternion.AngleAxis(finalAngle, worldRotationAxis) * this.syncTransform.rotation;

                    if (this.rotatingRigidbody != null)
                    {
                        this.rotatingRigidbody.MoveRotation(newRBRotation);
                    }
                    else
                    {
                        this.transform.rotation = newRBRotation;
                    }

                    this.syncTransform.transform.rotation = newRot;
                    this.grabbedRotation = this.grabbingInteractor.transform.rotation;
                }
            }
        }

        public override bool IsSelectableBy(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor interactor)
        {
            return base.IsSelectableBy(interactor) && (this.interactionLayers.value & interactor.interactionLayers.value) != 0;
        }

        protected override void OnSelectEntered(SelectEnterEventArgs selectEnterEventArgs)
        {
            this.grabbedRotation = selectEnterEventArgs.interactorObject.transform.rotation;
            this.grabbingInteractor = selectEnterEventArgs.interactorObject;

            // Create an object that will track the rotation
            var syncObj = new GameObject("TEMP_DialSyncTransform");
            this.syncTransform = syncObj.transform;

            if (this.rotatingRigidbody != null)
            {
                this.syncTransform.SetPositionAndRotation(this.rotatingRigidbody.position, this.rotatingRigidbody.rotation);
            }
            else
            {
                this.syncTransform.SetPositionAndRotation(this.transform.position, this.transform.rotation);
            }

            base.OnSelectEntered(selectEnterEventArgs);
        }

        protected override void OnSelectExited(SelectExitEventArgs selectExitEventArgs)
        {
            base.OnSelectExited(selectExitEventArgs);

            if (this.snapOnRelease && this.steps > 0)
            {
                Vector3 right = this.transform.TransformDirection(this.localAxisStart);
                Vector3 up = this.transform.TransformDirection(this.localRotationAxis);

                float angle = Vector3.SignedAngle(this.startingWorldAxis, right, up);
                if (angle < 0)
                {
                    angle = 360 + angle;
                }

                int step = Mathf.RoundToInt(angle / this.stepSize);
                angle = step * this.stepSize;

                if (angle != this.currentAngle)
                {
                    if (this.snapAudioBlock != null)
                    {
                        this.snapAudioBlock.PlayOneShot(this.transform.position);
                    }

                    this.OnDialStepChanged.Invoke(step);
                    this.OnDialChanged.Invoke(this);
                    this.currentStep = step;
                }

                Vector3 newRight = Quaternion.AngleAxis(angle, up) * this.startingWorldAxis;
                angle = Vector3.SignedAngle(right, newRight, up);

                this.currentAngle = angle;

                if (this.rotatingRigidbody != null)
                {
                    Quaternion newRot = Quaternion.AngleAxis(angle, up) * this.rotatingRigidbody.rotation;
                    this.rotatingRigidbody.MoveRotation(newRot);
                }
                else
                {
                    Quaternion newRot = Quaternion.AngleAxis(angle, up) * this.transform.rotation;
                    this.transform.rotation = newRot;
                }
            }

            Destroy(this.syncTransform.gameObject);
        }

        private void Start()
        {
            this.localAxisStart.Normalize();
            this.localRotationAxis.Normalize();

            if (this.rotatingRigidbody == null)
            {
                this.rotatingRigidbody = this.GetComponentInChildren<Rigidbody>();
            }

            this.currentAngle = 0;

            GameObject obj = new GameObject("Dial_Start_Copy");
            this.originalTransform = obj.transform;
            this.originalTransform.SetParent(this.transform.parent);
            this.originalTransform.localRotation = this.transform.localRotation;
            this.originalTransform.localPosition = this.transform.localPosition;

            if (this.steps > 0)
            {
                this.stepSize = this.rotationAngleMaximum / this.steps;
            }
            else
            {
                this.stepSize = 0.0f;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Handles.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
            Handles.DrawSolidArc(this.transform.position, this.transform.TransformDirection(this.localRotationAxis), this.transform.TransformDirection(this.localAxisStart), this.rotationAngleMaximum, 0.2f);
        }

#endif

        [System.Serializable]
        public class DialTurnedAngleEvent : UnityEvent<float>
        {
        }

        [System.Serializable]
        public class DialTurnedStepEvent : UnityEvent<int>
        {
        }

        [System.Serializable]
        public class DialChangedEvent : UnityEvent<HavenDialInteractable>
        {
        }
    }
}

#endif
