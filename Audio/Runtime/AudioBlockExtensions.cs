//-----------------------------------------------------------------------
// <copyright file="AudioBlockExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public static class AudioBlockExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PlayOneShotIfNotNull(this AudioBlock audioBlock, Vector3 position)
        {
            if (audioBlock)
            {
                audioBlock.PlayOneShot(position);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PlayOneShotIfNotNull(this AudioBlock audioBlock, Transform transform, float pitchPercentageOverride, float volumePercentageOverride)
        {
            if (audioBlock)
            {
                audioBlock.PlayOneShot(transform, pitchPercentageOverride, volumePercentageOverride);
            }
        }
    }
}
