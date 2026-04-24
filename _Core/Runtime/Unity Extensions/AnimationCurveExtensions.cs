//-----------------------------------------------------------------------
// <copyright file="AnimationCurveExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections;
    using UnityEngine;

    public static class AnimationCurveExtensions
    {
        public static float TimeLength(this AnimationCurve curve)
        {
            var keys = curve.keys;
            return keys[keys.Length - 1].time;
        }
    }
}
