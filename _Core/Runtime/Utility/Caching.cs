//-----------------------------------------------------------------------
// <copyright file="Caching.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEngine;

    public static class Caching
    {
        // General
        public static readonly byte[] ByteBuffer = new byte[1024 * 1024]; // 1 MB
        public static readonly Vector3[] Vectors = new Vector3[500];

        // Physics
        public static readonly ContactPoint[] ContactPointsCache = new ContactPoint[50];
        public static readonly RaycastHit[] RaycastHitsCache = new RaycastHit[50];
        public static readonly Collider[] CollidersCache = new Collider[50];
    }
}
