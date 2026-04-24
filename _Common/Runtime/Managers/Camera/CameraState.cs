//-----------------------------------------------------------------------
// <copyright file="CameraState.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEngine;

    public struct CameraState
    {
        public bool Exists;
        public Vector3 Position;
        public Vector3 Forward;
        public Vector3 EulerRotation;
        public float FieldOfView;
        public float CosOfFOV;

#if UNITY_6000_0_OR_NEWER
        public Camera Camera;
        public Transform Transform;
#endif

        public bool IsInView(Vector3 position)
        {
            Vector3 toPosition = (position - this.Position).normalized;
            return Vector3.Dot(this.Forward, toPosition) > this.CosOfFOV;
        }
    }
}
