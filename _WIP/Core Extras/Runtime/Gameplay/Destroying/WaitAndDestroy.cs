//-----------------------------------------------------------------------
// <copyright file="WaitAndDestroy.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using UnityEngine;

    public class WaitAndDestroy : GameBehavior, IAwake, IUpdate
    {
#pragma warning disable 0649
        [SerializeField] private float waitTime = 10.0f;
        [SerializeField] private GameObject destroyEffect;
#pragma warning restore 0649

        private SpawnManager spawnManager;
        private float currentTime = 0.0f;

        public void OnAwake(Bootloader bootloader)
        {
            this.spawnManager = bootloader.FindManager<SpawnManager>();
            this.currentTime = this.waitTime;
        }

        public void OnUpdate(float deltaTime)
        {
            this.currentTime -= deltaTime;

            if (this.currentTime < 0.0f)
            {
                if (this.destroyEffect != null)
                {
                    GameObject destroyEffect = this.spawnManager.Spawn(this.destroyEffect);
                    destroyEffect.transform.position = this.transform.position;
                }

                this.spawnManager.Despawn(this.gameObject);
            }
        }
    }
}

#endif
