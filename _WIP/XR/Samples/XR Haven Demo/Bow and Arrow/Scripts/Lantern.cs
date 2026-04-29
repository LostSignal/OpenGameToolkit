#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="Lantern.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost.Haven
{
    using UnityEngine;

    public class Lantern : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private bool activated;
        [SerializeField] private ParticleSystem hitParticle;
        [SerializeField] private Transform flameQuad;
#pragma warning restore 0649

        public void TurnOn()
        {
            if (this.activated)
            {
                return;
            }

            this.activated = true;

            this.hitParticle.Play();

            this.flameQuad.gameObject.SetActive(true);
        }
    }
}
