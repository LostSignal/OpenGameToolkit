//-----------------------------------------------------------------------
// <copyright file="HeightEstimator.cs" company="DefaultCompany">
//     Copyright (c) DefaultCompany. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.XR
{
    using OGT;
    using UnityEngine;

    public class HeightEstimator : MonoBehaviour
    {
        private static readonly OGTLogger Logger = new("XR");

        public Transform leftHand;
        public Transform rightHand;

        private float maxDistance;

        private void Update()
        {
            float distance = Vector3.Distance(leftHand.position, rightHand.position);

            if (distance > this.maxDistance)
            {
                this.maxDistance = distance;
                Logger.Log(this.maxDistance.ToString());
            }
        }
    }
}
