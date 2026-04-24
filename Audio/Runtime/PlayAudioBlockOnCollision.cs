//-----------------------------------------------------------------------
// <copyright file="PlayAudioBlockOnCollision.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEngine;

    public class PlayAudioBlockOnCollision : GameBehavior, IValidate, IAwake
    {
#pragma warning disable 0649
        [SerializeField] private AudioBlock audioBlock;
        [SerializeField] private float delayToPlayOnAwake = 0.5f;
        [SerializeField] private LayerMask layerFilter = ~0;
#pragma warning restore 0649

        private float awakeTime;
        private bool isReady;

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNull(this, this.audioBlock, nameof(this.audioBlock));
        }

        public void OnAwake(Bootloader bootloader)
        {
            this.isReady = true;
            this.awakeTime = Time.time;
        }

        private void OnCollisionEnter(Collision other)
        {
            // Early out if we're not ready or not enough time has passed since awake
            if (this.isReady == false || Time.time - this.awakeTime < this.delayToPlayOnAwake)
            {
                return;
            }

            // Making sure it matches the layer filter
            if (this.layerFilter != 0)
            {
                if ((other.gameObject.layer & this.layerFilter) == 0)
                {
                    return;
                }
            }

            int contactsCount = other.GetContacts(Caching.ContactPointsCache);

            if (contactsCount > 0)
            {
                this.audioBlock.PlayOneShot(Caching.ContactPointsCache[0].point);
            }
        }
    }
}
