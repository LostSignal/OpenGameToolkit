//-----------------------------------------------------------------------
// <copyright file="ICollectionExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public static class ICollectionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddIfNotNull<T>(this ICollection<T> collection, T value)
            where T : class
        {
            if (value != null)
            {
                collection.Add(value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AddIfNotNullAndUnique<T>(this ICollection<T> collection, T value)
            where T : class
        {
            if (value != null && collection.Contains(value) == false)
            {
                collection.Add(value);
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AddIfUnique<T>(this ICollection<T> collection, T value)
        {
            if (collection.Contains(value) == false)
            {
                collection.Add(value);
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ContainsAll<T>(this ICollection<T> collection, ICollection<T> values)
        {
            if (values.IsNullOrEmpty())
            {
                return true;
            }

            foreach (var value in values)
            {
                if (collection.Contains(value) == false)
                {
                    return false;
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }
    }
}
