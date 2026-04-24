//-----------------------------------------------------------------------
// <copyright file="ActorManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using UnityEngine;

    public sealed class ActorManager : Manager, IUpdate
    {
        private Transform mainCameraTransform;
        private Vector3 mainPlayerPosition;

        public Vector3 MainPlayerPosition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.mainPlayerPosition;
        }

        protected override Task InitializeManager(Bootloader bootloader)
        {
            return Task.CompletedTask;
        }

        public void OnUpdate(float deltaTime)
        {
            if (this.mainCameraTransform == null && Camera.main != null)
            {
                this.mainCameraTransform = Camera.main.transform;
            }

            if (this.mainCameraTransform != null)
            {
                this.mainPlayerPosition = this.mainCameraTransform.position;
            }
        }
    }
}

#endif
