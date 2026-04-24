//-----------------------------------------------------------------------
// <copyright file="IgnoreHitDetection.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if true //UNITY

namespace Lost
{
    using System.Collections.Generic;
    using OGT;
    using UnityEngine;

    public class IgnoreHitDetection : MonoBehaviour, IValidate
    {
        private static readonly Dictionary<long, IgnoreHitDetection> IgnoreHitDetectionColliders = new();

#pragma warning disable 0649
        [Tooltip("The Colliders that ignore hit detection")]
        [SerializeField] private List<Collider> colliders;
#pragma warning restore 0649

        public static bool TryGetIgnoreHitDetection(Collider collider, out IgnoreHitDetection ignoreHitDetection)
        {
            return IgnoreHitDetectionColliders.TryGetValue(collider.GetEntityId(), out ignoreHitDetection);
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNullOrEmpty(this, this.colliders, nameof(this.colliders));
        }

        private void OnEnable()
        {
            for (int i = 0; i < this.colliders.Count; i++)
            {
                IgnoreHitDetectionColliders.Add(this.colliders[i].GetEntityId(), this);
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < this.colliders.Count; i++)
            {
                IgnoreHitDetectionColliders.Remove(this.colliders[i].GetEntityId());
            }
        }
    }
}

#endif
