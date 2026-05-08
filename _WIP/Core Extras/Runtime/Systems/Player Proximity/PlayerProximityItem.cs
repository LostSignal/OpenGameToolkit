//-----------------------------------------------------------------------
// <copyright file="PlayerProximityItem.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using UnityEngine;

    public struct PlayerProximityItem
    {
        public Matrix4x4 WorldToLocal;
        public Area Area;
        public bool IsInProximity;
        public bool IsDynamic;
        public bool IsInitialized;
        public PlayerProximity PlayerProximity;
        public Transform Transform;
    }
}

#endif
