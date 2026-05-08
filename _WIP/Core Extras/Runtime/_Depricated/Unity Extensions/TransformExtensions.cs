//-----------------------------------------------------------------------
// <copyright file="TransformExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using System.Collections;
    using UnityEngine;

    public static partial class TransformExtensions
    {
        public static Coroutine LookAt(this Transform transform, Transform lookAtTransform, float time)
        {
            return CoroutineRunner.Instance.StartCoroutine(LookAtCoroutine());

            IEnumerator LookAtCoroutine()
            {
                Quaternion startRotation = transform.rotation;

                float currentTime = 0.0f;

                while (currentTime / time < 1.0f)
                {
                    Quaternion lookAtRotation = Quaternion.LookRotation(lookAtTransform.position - transform.position);

                    transform.rotation = Quaternion.Lerp(startRotation, lookAtRotation, currentTime / time);

                    currentTime += Time.deltaTime;

                    yield return null;
                }

                transform.rotation = Quaternion.LookRotation(lookAtTransform.position - transform.position);
            }
        }
    }
}

#endif
