//-----------------------------------------------------------------------
// <copyright file="AnimationCurveExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEngine;

    public static class AnimationCurveExtensions
    {
        public static float TimeLength(this AnimationCurve curve) => curve[curve.length - 1].time;
    }
}
