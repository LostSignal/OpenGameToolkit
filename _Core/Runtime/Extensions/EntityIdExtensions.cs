//-----------------------------------------------------------------------
// <copyright file="EntityIdExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public static class EntityIdExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ToULong(this EntityId lhs) => EntityId.ToULong(lhs);
    }
}
