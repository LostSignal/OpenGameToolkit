//-----------------------------------------------------------------------
// <copyright file="Rotate.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using UnityEngine;

    public class Rotate : GameBehavior, IAwake, IUpdate
    {
#pragma warning disable 0649
        [SerializeField] private Vector3 rotationSpeed;
#pragma warning restore 0649

        private Vector3 eulerRotation;

        public void OnAwake(Bootloader bootloader)
        {
            this.eulerRotation = this.transform.localEulerAngles;
        }

        public void OnUpdate(float deltaTime)
        {
            this.eulerRotation = new Vector3(
                this.eulerRotation.x + (this.rotationSpeed.x * deltaTime),
                this.eulerRotation.y + (this.rotationSpeed.y * deltaTime),
                this.eulerRotation.z + (this.rotationSpeed.z * deltaTime));

            this.transform.localEulerAngles = this.eulerRotation;
        }
    }
}

#endif
