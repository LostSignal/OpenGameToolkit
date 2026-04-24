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
        public static Coroutine Animate(this AnimationCurve curve, System.Action<float, float> action)
        {
            return CoroutineRunner.Instance.StartCoroutine(AnimateCoroutine());

            IEnumerator AnimateCoroutine()
            {
                float maxTime = curve.TimeLength();
                float currentTime = 0;

                while (currentTime < maxTime)
                {
                    action.Invoke(currentTime, curve.Evaluate(currentTime));
                    currentTime += Time.deltaTime;
                    yield return null;
                }

                action.Invoke(maxTime, curve.Evaluate(maxTime));
            }
        }
    }
}
