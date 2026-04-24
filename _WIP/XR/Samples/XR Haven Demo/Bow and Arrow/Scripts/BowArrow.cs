#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="BowArrow.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost.Haven
{
    using System.Collections;
    using System.Runtime.CompilerServices;
    using OGT;
    using OGT.Haven;
    using UnityEngine;
    using UnityEngine.Serialization;
    using UnityEngine.XR.Interaction.Toolkit;
    using UnityEngine.XR.Interaction.Toolkit.Interactors;

    public class BowArrow : MonoBehaviour
    {
#if !USING_UNITY_XR_INTERACTION_TOOLKIT
#pragma warning disable IDE0051, IDE0052
#endif

#pragma warning disable 0649
        [FormerlySerializedAs("speed")]
        [SerializeField] private float maxSpeed = 5.0f;
        [SerializeField] private float minSpeed = 30.0f;
        [SerializeField] private float maxHitForce = 10.0f;

        [SerializeField] private Transform tip;
        [SerializeField] private Rigidbody arrowRigidbody;
        [SerializeField] private Collider physicsCollider;
        [SerializeField] private HavenGrabbable grabbable;
        [SerializeField] private LayerMask collidesWith = ~0;

        [Header("Particles")]
        [SerializeField] private ParticleSystem trailParticle;
        [SerializeField] private ParticleSystem hitParticle;
        [SerializeField] private TrailRenderer trailRenderer;
#pragma warning restore 0649

        [Header("Sound")]
        [SerializeField] private AudioBlock arrowLaunchBlock;
        [SerializeField] private AudioBlock arrowHitBlock;

#if USING_UNITY_XR_INTERACTION_TOOLKIT

        private int originalInteractionLayers;
        private Vector3 lastPosition;
        private bool inAir;
        private bool isNotched;

        public Transform AttachTransform
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.grabbable.attachTransform;
        }

        public HavenGrabbable Grabbable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.grabbable;
        }

        public void Release(float value)
        {
            this.isNotched = false;

            // Making sure it's no longer grabbable
            this.originalInteractionLayers = this.grabbable.interactionLayers;
            this.grabbable.interactionLayers = 0;

            // Setting firing state
            this.transform.SetParent(null);
            this.inAir = true;
            this.arrowRigidbody.useGravity = true;
            this.arrowRigidbody.isKinematic = false;
            this.physicsCollider.enabled = false;

            // Setting the velocity
            Vector3 force = this.transform.forward * Mathf.Lerp(this.minSpeed, this.maxSpeed, value);
            this.arrowRigidbody.AddForce(force, ForceMode.Impulse);

            // Making sure visuals look right floating through the air
            this.StartCoroutine(this.RotateWithVelocity());

            // Effects and Sound
            this.ArrowParticles(true);
            this.arrowLaunchBlock.PlayOneShotIfNotNull(this.transform, value, Mathf.Max(0.7f, value));

            this.lastPosition = this.tip.position;
        }

        public void SetNotched()
        {
            this.isNotched = true;
            this.grabbable.enabled = false;
            this.arrowRigidbody.useGravity = false;
            this.arrowRigidbody.isKinematic = true;
            this.physicsCollider.enabled = false;
        }

        public void ArrowHaptic(XRBaseInteractor interactor)
        {
            var rig = HavenRig.Instance;

            if (rig != null)
            {
                rig.SendHapticImpluse(interactor, 0.7f, 0.05f);
            }
        }

        private void FixedUpdate()
        {
            if (this.inAir)
            {
                this.CheckCollision();
                this.lastPosition = this.tip.position;
            }
        }

        private void Awake()
        {
            this.grabbable.selectExited.AddListener(this.OnSelectExited);
        }

        private void OnDestroy()
        {
            this.grabbable.selectExited.RemoveListener(this.OnSelectExited);
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            if (this.isNotched == false)
            {
                this.arrowRigidbody.isKinematic = false;
                this.arrowRigidbody.useGravity = true;
                this.physicsCollider.enabled = true;
            }
        }

        private void CheckCollision()
        {
            if (Physics.Linecast(this.lastPosition, this.tip.position, out RaycastHit hitInfo, this.collidesWith, QueryTriggerInteraction.Ignore))
            {
                if (hitInfo.transform.TryGetComponent(out Rigidbody body))
                {
                    if (body.TryGetComponent(out Lantern lantern))
                    {
                        lantern.TurnOn();
                    }

                    if (body.TryGetComponent(out Potion potion))
                    {
                        potion.BreakPotion();
                        return;
                    }

                    this.arrowRigidbody.interpolation = RigidbodyInterpolation.None;
                    this.transform.SetParent(hitInfo.transform, true);

                    Vector3 forceVelocity = arrowRigidbody.linearVelocity;

                    if (forceVelocity.magnitude > this.maxHitForce)
                    {
                        forceVelocity = forceVelocity.normalized * this.maxHitForce;
                    }

                    body.AddForce(forceVelocity, ForceMode.Impulse);
                }

                if (hitInfo.collider.isTrigger == false)
                {
                    this.Stop();
                }
            }
        }

        private void Stop()
        {
            this.inAir = false;
            this.arrowRigidbody.useGravity = false;
            this.arrowRigidbody.isKinematic = true;
            this.grabbable.enabled = true;

            this.physicsCollider.enabled = false;
            this.grabbable.interactionLayers = this.originalInteractionLayers;

            // Effects / Sound
            this.ArrowParticles(false);
            this.arrowHitBlock.PlayOneShotIfNotNull(this.transform.position);
        }

        private IEnumerator RotateWithVelocity()
        {
            yield return new WaitForFixedUpdate();

            while (this.inAir)
            {
                Quaternion newRotation = Quaternion.LookRotation(this.arrowRigidbody.linearVelocity, this.transform.up);
                this.transform.rotation = newRotation;
                yield return null;
            }
        }

        private void ArrowParticles(bool release)
        {
            if (release)
            {
                this.trailParticle.Play();
                this.trailRenderer.emitting = true;
            }
            else
            {
                this.trailParticle.Stop();
                this.hitParticle.Play();
                this.trailRenderer.emitting = false;
            }
        }
#endif
    }
}
