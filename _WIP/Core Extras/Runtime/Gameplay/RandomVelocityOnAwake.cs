//-----------------------------------------------------------------------
// <copyright file="RandomVelocityOnAwake.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using UnityEngine;

    [RequireComponent(typeof(Rigidbody))]
    public class RandomVelocityOnAwake : GameBehavior, IAwake
    {
#pragma warning disable 0649
        [SerializeField] private float minVelocity = 0.5f;
        [SerializeField] private float maxVelocity = 1.0f;
#pragma warning restore 0649

        public void OnAwake(Bootloader bootloader)
        {
            var rigidBody = this.GetComponent<Rigidbody>();

#if UNITY_2023_3_OR_NEWER
            rigidBody.linearVelocity = Random.insideUnitSphere.normalized * Random.Range(this.minVelocity, this.maxVelocity);
#else
            rigidBody.velocity = Random.insideUnitSphere.normalized * Random.Range(this.minVelocity, this.maxVelocity);
#endif
        }
    }
}

#endif
