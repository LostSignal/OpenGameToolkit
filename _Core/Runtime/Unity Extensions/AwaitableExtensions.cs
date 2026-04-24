//-----------------------------------------------------------------------
// <copyright file="AwaitableExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEngine;

    public static class AwaitableExtensions
    {
        public static bool IsRunning(this Awaitable awaitable)
        {
            return awaitable.GetAwaiter().IsCompleted == false;
        }

        public static bool IsRunning<T>(this Awaitable<T> awaitable)
        {
            return awaitable.GetAwaiter().IsCompleted == false;
        }
    }
}
