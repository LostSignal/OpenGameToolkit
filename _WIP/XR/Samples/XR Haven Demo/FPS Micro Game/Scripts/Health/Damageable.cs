//-----------------------------------------------------------------------
// <copyright file="Damageable.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if true //UNITY

namespace Lost
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using OGT;
    using UnityEngine;

    public class Damageable : MonoBehaviour, IValidate
    {
        private static readonly Dictionary<long, Damageable> DamangeableColliders = new();

        static Damageable()
        {
            //// TODO [bgish]: This doesn't exist anymore, need to fix
            //// UnityPlatform.OnReset += DamangeableColliders.Clear;
        }

#pragma warning disable 0649
        [Tooltip("The Health component this component applies damage to.")]
        [SerializeField] private Health health;

        [Tooltip("Multiplier to apply to the received damage")]
        [SerializeField] private float damageMultiplier = 1f;

        [Range(0, 1)]
        [Tooltip("Multiplier to apply to self damage")]
        [SerializeField] private float sensibilityToSelfdamage = 0.5f;

        [Tooltip("The Colliders that can take damage")]
        [SerializeField] private List<Collider> colliders;
#pragma warning restore 0649

        public Health Health
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.health;
        }

        public static bool TryGetDamageable(Collider collider, out Damageable damageable)
        {
            return DamangeableColliders.TryGetValue(collider.GetEntityId(), out damageable);
        }

        public void InflictDamage(float damage, bool isExplosionDamage, GameObject damageSource)
        {
            if (this.health)
            {
                float totalDamage = damage;

                // Skip the crit multiplier if it's from an explosion
                if (isExplosionDamage == false)
                {
                    totalDamage *= this.damageMultiplier;
                }

                // Potentially reduce damages if inflicted by self
                if (this.health.gameObject == damageSource)
                {
                    totalDamage *= this.sensibilityToSelfdamage;
                }

                // Apply the damages
                this.health.TakeDamage(totalDamage, damageSource);
            }
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNull(this, this.health, nameof(this.health));
            report.AssertNotNullOrEmpty(this, this.colliders, nameof(this.colliders));
        }

        private void OnValidate()
        {
            if (this.colliders == null)
            {
                this.colliders = new List<Collider>();
                EditorUtil.SetDirty(this);
            }
        }

        private void OnEnable()
        {
            for (int i = 0; i < this.colliders.Count; i++)
            {
                DamangeableColliders.Add(this.colliders[i].GetEntityId(), this);
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < this.colliders.Count; i++)
            {
                DamangeableColliders.Remove(this.colliders[i].GetEntityId());
            }
        }
    }
}

#endif
