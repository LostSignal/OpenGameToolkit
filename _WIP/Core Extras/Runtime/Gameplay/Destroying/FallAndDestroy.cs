//-----------------------------------------------------------------------
// <copyright file="FallAndDestroy.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    public class FallAndDestroy : GameBehavior, IUpdate, IAwake
    {
        private SpawnManager spawnManager;

        public void OnAwake(Bootloader bootloader)
        {
            this.spawnManager = bootloader.FindManager<SpawnManager>();
        }

        public void OnUpdate(float deltaTime)
        {
            if (this.transform.position.y < -500.0f)
            {
                this.spawnManager.Despawn(this.gameObject);
            }
        }
    }
}

#endif
