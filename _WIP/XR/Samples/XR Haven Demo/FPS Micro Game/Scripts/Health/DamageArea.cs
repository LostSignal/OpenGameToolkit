//-----------------------------------------------------------------------
// <copyright file="DamageArea.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if true //UNITY

namespace Lost
{
    using System.Collections.Generic;
    using UnityEngine;

    public class DamageArea : MonoBehaviour
    {
        private static readonly Dictionary<Health, Damageable> UniqueDamagedHealths = new Dictionary<Health, Damageable>();

#pragma warning disable 0649
        [Tooltip("Area of damage when the projectile hits something")]
        [SerializeField] private float areaOfEffectDistance = 5.0f;

        [Tooltip("Damage multiplier over distance for area of effect")]
        [SerializeField] private AnimationCurve damageRatioOverDistance;

        [Header("Debug")]
        [Tooltip("Color of the area of effect radius")]
        [SerializeField] private Color areaOfEffectColor = Color.red * 0.5f;
#pragma warning restore 0649

        public void InflictDamageInArea(float damage, Vector3 center, LayerMask layers, QueryTriggerInteraction interaction, GameObject owner)
        {
            UniqueDamagedHealths.Clear();

            // Create a collection of unique health components that would be damaged in the area of effect (in order to avoid damaging a same entity multiple times)
            int count = Physics.OverlapSphereNonAlloc(center, areaOfEffectDistance, OGT.Caching.CollidersCache, layers, interaction);

            for (int i = 0; i < count; i++)
            {
                Collider collider = OGT.Caching.CollidersCache[i];

                if (Damageable.TryGetDamageable(collider, out Damageable damageable))
                {
                    Health health = damageable.Health;
                    if (health && !UniqueDamagedHealths.ContainsKey(health))
                    {
                        UniqueDamagedHealths.Add(health, damageable);
                    }
                }
            }

            // Apply damages with distance falloff
            Vector3 position = this.transform.position;

            foreach (Damageable uniqueDamageable in UniqueDamagedHealths.Values)
            {
                float distance = Vector3.Distance(uniqueDamageable.transform.position, position);
                uniqueDamageable.InflictDamage(damage * this.damageRatioOverDistance.Evaluate(distance / this.areaOfEffectDistance), true, owner);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = this.areaOfEffectColor;
            Gizmos.DrawSphere(this.transform.position, this.areaOfEffectDistance);
        }
    }
}

#endif
