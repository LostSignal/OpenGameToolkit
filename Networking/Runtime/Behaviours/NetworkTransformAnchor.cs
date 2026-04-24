//-----------------------------------------------------------------------
// <copyright file="NetworkTransformAnchor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    using OGT;
    using UnityEngine;

    public class NetworkTransformAnchor : MonoBehaviour
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        private static Transform anchorTransform;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "Must register for events.")]
        static NetworkTransformAnchor()
        {
            UnityPlatformProvider.OnReset += Reset;

            static void Reset()
            {
                anchorTransform = null;
            }
        }

        public static Vector3 InverseTransformPosition(Vector3 worldSpace)
        {
            return anchorTransform ? anchorTransform.InverseTransformPoint(worldSpace) : worldSpace;
        }

        public static Quaternion InverseTransformRotation(Quaternion worldRotation)
        {
            return anchorTransform ? Quaternion.Inverse(anchorTransform.rotation) * worldRotation : worldRotation;
        }

        public static Vector3 TransformPosition(Vector3 anchorSpacePosition)
        {
            return anchorTransform ? anchorTransform.TransformPoint(anchorSpacePosition) : anchorSpacePosition;
        }

        public static Quaternion TransformRotation(Quaternion anchorSpaceRotation)
        {
            return anchorTransform ? anchorTransform.rotation * anchorSpaceRotation : anchorSpaceRotation;
        }

        private void OnEnable()
        {
            if (anchorTransform)
            {
                Logger.LogError("NetworkTransformAnchor is already registered, this NetworkTransformAnchor will be ignored!", this);
                return;
            }

            anchorTransform = this.transform;
        }

        private void OnDisable()
        {
            anchorTransform = null;
        }
    }
}
