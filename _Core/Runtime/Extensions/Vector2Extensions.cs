//-----------------------------------------------------------------------
// <copyright file="Vector2Extensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public static class Vector2Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 SetX(this Vector2 vector, float x)
        {
            vector.x = x;
            return vector;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 SetY(this Vector2 vector, float y)
        {
            vector.y = y;
            return vector;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 AddZ(this Vector2 vector, float z)
        {
            return new Vector3(vector.x, vector.y, z);
        }
    }
}
