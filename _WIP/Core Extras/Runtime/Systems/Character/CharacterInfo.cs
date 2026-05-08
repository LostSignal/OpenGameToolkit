//-----------------------------------------------------------------------
// <copyright file="CharacterInfo.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEngine;

    public struct CharacterInfo
    {
        public Vector3 Position;
        public Vector3 Forward;
        public Quaternion Rotation;
        public int TeamId;
    }
}
