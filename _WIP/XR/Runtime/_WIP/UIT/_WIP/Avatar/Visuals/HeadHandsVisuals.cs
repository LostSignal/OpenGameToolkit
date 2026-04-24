//-----------------------------------------------------------------------
// <copyright file="HeadHandsVisuals.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace OGT.Haven
{
    using OGT.Networking;
    using OGT.XR;
    using UnityEngine;

    public class HeadHandsVisuals : HavenAvatarVisuals
    {
#pragma warning disable 0649
        [Header("Head / Hand Transforms")]
        [SerializeField] private Transform head;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;
#pragma warning restore 0649

        private HavenRig rig;

        private bool desiredValuesSet;
        private Vector3 desiredHeadLocalPosition;
        private Quaternion desiredHeadLocalRotation;
        private Vector3 desiredLeftHandLocalPosition;
        private Quaternion desiredLeftHandLocalRotation;
        private Vector3 desiredRightHandLocalPosition;
        private Quaternion desiredRightHandLocalRotation;

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(this.rig.HeadTransform.localPosition);
            writer.Write(this.rig.HeadTransform.localRotation);
            writer.Write(this.rig.LeftHandTransform.localPosition);
            writer.Write(this.rig.LeftHandTransform.localRotation);
            writer.Write(this.rig.RightHandTransform.localPosition);
            writer.Write(this.rig.RightHandTransform.localRotation);
        }

        public override void Deserialize(NetworkReader reader)
        {
            this.desiredValuesSet = true;
            this.desiredHeadLocalPosition = reader.ReadVector3();
            this.desiredHeadLocalRotation = reader.ReadQuaternion();
            this.desiredLeftHandLocalPosition = reader.ReadVector3();
            this.desiredLeftHandLocalRotation = reader.ReadQuaternion();
            this.desiredRightHandLocalPosition = reader.ReadVector3();
            this.desiredRightHandLocalRotation = reader.ReadQuaternion();
        }

        private void Start()
        {
            if (this.IsOwner)
            {
                this.ShowVisuals(false);
                this.rig = HavenRig.Instance;
            }
        }

        private void LateUpdate()
        {
            if (this.desiredValuesSet)
            {
                this.head.localPosition = Vector3.Lerp(this.head.localPosition, this.desiredHeadLocalPosition, Time.deltaTime);
                this.head.localRotation = Quaternion.Slerp(this.head.localRotation, this.desiredHeadLocalRotation, 5.0f);

                this.leftHand.localPosition = Vector3.Lerp(this.leftHand.localPosition, this.desiredLeftHandLocalPosition, Time.deltaTime);
                this.leftHand.localRotation = Quaternion.Slerp(this.leftHand.localRotation, this.desiredLeftHandLocalRotation, 5.0f);

                this.rightHand.localPosition = Vector3.Lerp(this.rightHand.localPosition, this.desiredRightHandLocalPosition, Time.deltaTime);
                this.rightHand.localRotation = Quaternion.Slerp(this.rightHand.localRotation, this.desiredRightHandLocalRotation, 5.0f);
            }
        }
    }
}

#endif
