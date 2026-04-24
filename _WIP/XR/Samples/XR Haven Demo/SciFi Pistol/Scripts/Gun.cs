#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="Gun.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost.XR
{
    using System;
    using System.Collections.Generic;
    using OGT;
    using Unity.FPS.Game;
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;
    using UnityEngine.XR.Interaction.Toolkit.Interactables;

    public class Gun : MonoBehaviour, IValidate, IAwake
    {
        [Serializable]
        private struct Recoil
        {
            public Vector3 MinRecoil;
            public Vector3 MaxRecoil;
            public float Snappiness;
            public float ReturnSpeed;
        }

#pragma warning disable 0649
        [SerializeField] private XRBaseInteractable interactable;
        [SerializeField] private Transform recoilTransform;
        [SerializeField] private Recoil hipFireRecoil;
        [SerializeField] private Recoil aimingRecoil;
        [SerializeField] private bool isAiming;

        [Header("Shooting")]
        [SerializeField] private ProjectileBase projectilePrefab;
        [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private Vector3 muzzleFlashScale = Vector3.one;
        [SerializeField] private Transform muzzle;

        [Tooltip("Angle for the cone in which the bullets will be shot randomly (0 means no spread at all)")]
        [SerializeField] private float bulletSpreadAngle = 0f;
#pragma warning restore 0649

        private Vector3 muzzleWorldVelocity;
        private Vector3 lastMuzzlePosition;
        private Vector3 currentRotation;
        private Vector3 targetRotation;

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNull(this, this.interactable, nameof(this.interactable));
        }

        public void OnAwake(Bootloader bootloader)
        {
#if USING_UNITY_XR_INTERACTION_TOOLKIT
            this.interactable.activated.AddListener(this.OnActivate);
#endif
        }

        private void Awake() => ActivationManager.Register(this);

        private void OnDestroy()
        {
#if USING_UNITY_XR_INTERACTION_TOOLKIT
            this.interactable.activated.RemoveListener(this.OnActivate);
#endif
        }

        private void OnValidate()
        {
            this.EditorGetComponent(ref this.interactable);
        }

        private void OnActivate(ActivateEventArgs _)
        {
            this.Shoot();
        }

        private void Shoot()
        {
            Recoil recoil = this.isAiming ? this.aimingRecoil : this.hipFireRecoil;

            var muzzlePosition = this.muzzle.position;
            var muzzleRotation = this.muzzle.rotation;
            var muzzleForward = this.muzzle.forward;
            var spreadAngleRatio = this.bulletSpreadAngle / 180.0f;

            //// // Spawn all bullets with random direction
            //// for (int i = 0; i < 1; i++)
            //// {
            ////     Vector3 shotDirection = Vector3.Slerp(muzzleForward, UnityEngine.Random.insideUnitSphere, spreadAngleRatio);
            ////     ProjectileBase newProjectile = GameObject.Instantiate(this.projectilePrefab, muzzlePosition, Quaternion.LookRotation(shotDirection));
            ////     newProjectile.Shoot(this.gameObject, this.muzzleWorldVelocity, 1.0f);
            //// }

            // Muzzle flash
            if (muzzleFlashPrefab != null)
            {
                var muzzleFlashInstance = GameObject.Instantiate(this.muzzleFlashPrefab, muzzlePosition, muzzleRotation, this.muzzle);
                muzzleFlashInstance.transform.localScale = this.muzzleFlashScale;
                GameObject.Destroy(muzzleFlashInstance, 2.0f);
            }

            // Updating recoil
            this.targetRotation += new Vector3(
                UnityEngine.Random.Range(recoil.MinRecoil.x, recoil.MaxRecoil.x),
                UnityEngine.Random.Range(-recoil.MinRecoil.y, recoil.MaxRecoil.y),
                UnityEngine.Random.Range(-recoil.MinRecoil.z, recoil.MaxRecoil.z));
        }

        private void Update()
        {
            Recoil recoil = this.isAiming ? this.aimingRecoil : this.hipFireRecoil;
            this.targetRotation = Vector3.Lerp(this.targetRotation, Vector3.zero, recoil.ReturnSpeed * Time.deltaTime);
            this.currentRotation = Vector3.Slerp(this.currentRotation, this.targetRotation, recoil.Snappiness * Time.deltaTime);
            this.recoilTransform.localRotation = Quaternion.Euler(this.currentRotation);

            // Updating Muzzle Velocity
            if (Time.deltaTime > 0)
            {
                var muzzlePosition = this.muzzle.position;
                this.muzzleWorldVelocity = (muzzlePosition - this.lastMuzzlePosition) / Time.deltaTime;
                this.lastMuzzlePosition = muzzlePosition;
            }
        }
    }
}
