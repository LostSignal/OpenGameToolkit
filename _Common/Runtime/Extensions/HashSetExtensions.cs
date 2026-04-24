//-----------------------------------------------------------------------
// <copyright file="HashSetExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;

    public static class HashSetExtensions
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        public static void AddRange<T>(this HashSet<T> hashSet, ICollection<T> collection)
        {
            foreach (var element in collection)
            {
                hashSet.Add(element);
            }
        }
    }
}
