#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="Bow.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost.Haven
{
    using OGT;
    using OGT.Haven;
    using OGT.XR;
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;
    using UnityEngine.XR.Interaction.Toolkit.Interactables;

    public class Bow : MonoBehaviour
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;
        private static readonly Quaternion Identity = Quaternion.identity;

#pragma warning disable 0649
        [SerializeField] private Transform bowObjectTransform;
        [SerializeField] private HavenGrabbable bowGrab;
        [SerializeField] private HavenSocket bowNotckSocket;

        [SerializeField] private AxisTest axisTest;
        [SerializeField] private XRBaseInteractable notchInteractable;

        [Header("Bow Line")]
        [SerializeField] private LineRenderer line;
        [SerializeField] private ParticleSystem lineParticle;
        [ColorUsage(true, true)]
        [SerializeField] public Color stringNormalCol, stringPulledCol;
#pragma warning restore 0649

#if USING_UNITY_XR_INTERACTION_TOOLKIT
        private BowArrow arrow;
        private Transform arrowTransform;
        private Vector3 arrowAttachPosition;

        private bool isPickedUp;
        private float pickupTime;

        private void Awake()
        {
            this.bowGrab.selectEntered.AddListener(this.GrabSelectEntered);
            this.bowGrab.selectExited.AddListener(this.GrabSelectExited);

            this.notchInteractable.selectExited.AddListener(this.SelectExited);
            this.bowNotckSocket.hoverEntered.AddListener(this.HoverEntered);
        }

        private void OnDestroy()
        {
            this.bowGrab.selectEntered.RemoveListener(this.GrabSelectEntered);
            this.bowGrab.selectExited.RemoveListener(this.GrabSelectExited);

            this.notchInteractable.selectExited.RemoveListener(this.SelectExited);
            this.bowNotckSocket.hoverEntered.RemoveListener(this.HoverEntered);
        }

        private void Update()
        {
            // Enabling/Disabling Ray Grab of bow based on if it's picked up
            if (this.isPickedUp && this.bowGrab.DisableRayGrab == false && Time.realtimeSinceStartup - this.pickupTime > 0.3f)
            {
                this.bowGrab.DisableRayGrab = true;
            }
            else if (this.isPickedUp == false)
            {
                this.bowGrab.DisableRayGrab = false;
            }

            this.line.material.SetColor("_EmissionColor", Color.Lerp(this.stringNormalCol, this.stringPulledCol, this.axisTest.Percentage));
            this.line.SetPosition(1, new Vector3(0, 0, this.notchInteractable.transform.localPosition.z));

            // NOTE [bgish]: For some reason, if you knock an arrow too quickly it will move and we need to set it for a couple frames
            if (this.arrowTransform != null &&
                (this.arrowTransform.localPosition != this.arrowAttachPosition || this.arrowTransform.localRotation != Identity))
            {
                this.arrowTransform.localPosition = this.arrowAttachPosition;
                this.arrowTransform.localRotation = Identity;
            }
        }

        private void LateUpdate()
        {
            if (this.notchInteractable.isSelected)
            {
                var rig = HavenRig.Instance;

                if (rig != null)
                {
                    if (rig.LeftHand.IsHolding(this.bowGrab))
                    {
                        this.bowObjectTransform.rotation = Quaternion.LookRotation(
                            (rig.LeftHandTransform.position - rig.RightHandTransform.position).normalized,
                            rig.LeftHandTransform.up);
                    }
                    else if (rig.RightHand.IsHolding(this.bowGrab))
                    {
                        this.bowObjectTransform.rotation = Quaternion.LookRotation(
                            (rig.RightHandTransform.position - rig.LeftHandTransform.position).normalized,
                            rig.RightHandTransform.up);
                    }
                }
            }
        }

        private void HoverEntered(HoverEnterEventArgs args)
        {
            this.bowNotckSocket.enabled = false;

            var interactable = args.interactableObject as XRBaseInteractable;

            this.arrow = interactable.GetComponent<BowArrow>();

            if (this.arrow == null)
            {
                Logger.LogError("Bow had a BowArrow object socketed, but not BowArrow script.", interactable);
                return;
            }

            this.arrow.SetNotched();
            this.arrowTransform = this.arrow.transform;
            this.arrowTransform.SetParent(this.notchInteractable.transform);
            this.arrowTransform.Reset();

            // Make sure arrow respects the attach position
            this.arrowAttachPosition = -this.arrow.AttachTransform.localPosition;
            this.arrowTransform.localPosition = this.arrowAttachPosition;

            var rig = HavenRig.Instance;

            if (rig != null && interactable != null)
            {
                if (rig.LeftHand.IsHolding(this.bowGrab))
                {
                    rig.RightHand.Deselect(interactable);
                    rig.RightHand.Select(this.notchInteractable);
                }
                else if (rig.RightHand.IsHolding(this.bowGrab))
                {
                    rig.LeftHand.Deselect(interactable);
                    rig.LeftHand.Select(this.notchInteractable);
                }
            }
        }

        private void GrabSelectEntered(SelectEnterEventArgs args)
        {
            this.isPickedUp = true;
            this.pickupTime = Time.realtimeSinceStartup;
        }

        private void GrabSelectExited(SelectExitEventArgs args)
        {
            this.isPickedUp = false;
        }

        private void SelectExited(SelectExitEventArgs args)
        {
            if (this.arrow != null && this.axisTest.Percentage != 0.0f)
            {
                this.arrowTransform.SetParent(null);
                this.arrow.Release(this.axisTest.Percentage);

                this.arrow = null;
                this.arrowTransform = null;

                this.bowNotckSocket.enabled = true;
            }

            this.line.material.SetColor("_EmissionColor", stringNormalCol);
            this.lineParticle.Play();
            this.axisTest.SetPercentage(0.0f);

            this.Update();
        }
#endif
    }
}
