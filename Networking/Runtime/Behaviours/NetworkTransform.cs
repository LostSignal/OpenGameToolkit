//-----------------------------------------------------------------------
// <copyright file="NetworkTransform.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    using OGT;
    using UnityEngine;

    public class NetworkTransform : NetworkBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private float positionLerpPercentage = 0.1f;
        [SerializeField] private bool sendPosition = true;
        [SerializeField] private bool estimateVelocity = true;
        [SerializeField] private bool sendRotation = true;
        [SerializeField] private bool sendScale = false;
        [SerializeField] private Rigidbody rigidBody;
#pragma warning restore 0649

        private Vector3 desiredPosition;
        private Quaternion desiredRotation;
        private Vector3 desiredScale;

        private float lastVelocityUpdateTime = -1.0f;
        private Vector3 estimatedVelocity;
        private uint version;

        public void SetPositionLerpPercentage(float lerpPercentage)
        {
            this.positionLerpPercentage = lerpPercentage;
        }

        public override void Serialize(NetworkWriter writer)
        {
            bool sendPhysics = this.rigidBody != null;
            bool useGravity = sendPhysics && this.rigidBody.useGravity;
            bool freezeRotation = sendPhysics && this.rigidBody.freezeRotation;
            bool isKinematic = sendPhysics && this.rigidBody.isKinematic;

            int sent = 0;
            BitUtil.SetBit(ref sent, 0, this.sendPosition);
            BitUtil.SetBit(ref sent, 1, this.sendRotation);
            BitUtil.SetBit(ref sent, 2, this.sendScale);
            BitUtil.SetBit(ref sent, 3, sendPhysics);
            BitUtil.SetBit(ref sent, 4, useGravity);
            BitUtil.SetBit(ref sent, 5, freezeRotation);
            BitUtil.SetBit(ref sent, 6, isKinematic);

            writer.WritePackedUInt32(++this.version);
            writer.WritePackedUInt32((uint)sent);

            if (this.sendPosition)
            {
                writer.Write(NetworkTransformAnchor.InverseTransformPosition(this.transform.position));
            }

            if (this.sendRotation)
            {
                writer.Write(NetworkTransformAnchor.InverseTransformRotation(this.transform.rotation));
            }

            if (this.sendScale)
            {
                writer.Write(this.transform.localScale);
            }

            if (sendPhysics)
            {
#if UNITY_2023_3_OR_NEWER
                writer.Write(this.rigidBody.linearVelocity);
#else
                writer.Write(this.rigidBody.velocity);
#endif
            }
        }

        public override void Deserialize(NetworkReader reader)
        {
            uint messageVersion = reader.ReadPackedUInt32();

            if (this.version > messageVersion)
            {
                return;
            }

            this.version = messageVersion;

            int sent = (int)reader.ReadPackedUInt32();
            bool sentPosition = BitUtil.GetBit(sent, 0);
            bool sentRotation = BitUtil.GetBit(sent, 1);
            bool sentScale = BitUtil.GetBit(sent, 2);
            bool sentPhysics = BitUtil.GetBit(sent, 3);
            bool useGravity = BitUtil.GetBit(sent, 4);
            bool freezeRotation = BitUtil.GetBit(sent, 5);
            bool isKinematic = BitUtil.GetBit(sent, 6);

            if (sentPosition)
            {
                Vector3 newDesiredPosition = NetworkTransformAnchor.TransformPosition(reader.ReadVector3());

                if (this.lastVelocityUpdateTime < 0.0f)
                {
                    this.estimatedVelocity = Vector3.zero;
                }
                else
                {
                    float timeSinceLastUpdate = Time.realtimeSinceStartup - this.lastVelocityUpdateTime;

                    if (timeSinceLastUpdate > 0.01)
                    {
                        this.estimatedVelocity = (newDesiredPosition - this.desiredPosition) / timeSinceLastUpdate;
                    }
                }

                this.desiredPosition = newDesiredPosition;
                this.lastVelocityUpdateTime = Time.realtimeSinceStartup;
            }

            if (sentRotation)
            {
                this.desiredRotation = NetworkTransformAnchor.TransformRotation(reader.ReadQuaternion());
            }

            if (sentScale)
            {
                this.desiredScale = reader.ReadVector3();
            }

            // Early out and don't update physics if we're trying to get ownership of this object
            if (this.Identity.IsRequestingOwnership)
            {
                return;
            }

            if (sentPhysics)
            {
#if UNITY_2023_3_OR_NEWER
                this.rigidBody.linearVelocity = reader.ReadVector3();
#else
                this.rigidBody.velocity = reader.ReadVector3();
#endif
                this.rigidBody.useGravity = useGravity;
                this.rigidBody.freezeRotation = freezeRotation;
                this.rigidBody.isKinematic = isKinematic;
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (this.rigidBody == null)
            {
                this.rigidBody = this.GetComponent<Rigidbody>();
            }
        }

        protected override void Awake()
        {
            base.Awake();

            this.desiredPosition = this.transform.position;
            this.desiredRotation = this.transform.rotation;
            this.desiredScale = this.transform.localScale;
        }

        protected override void NetworkUpdate()
        {
            base.NetworkUpdate();

            if (this.IsOwner == false)
            {
                // Early out and don't update positions if we're trying to get ownership of this object
                if (this.Identity.IsRequestingOwnership)
                {
                    return;
                }

                if (this.sendPosition)
                {
                    if (this.rigidBody == null && this.estimateVelocity)
                    {
                        this.transform.position += this.estimatedVelocity * Time.deltaTime;
                    }

                    // If the desired position is more than 2 meters away, then do a warp
                    if ((this.transform.position - this.desiredPosition).sqrMagnitude > 9)
                    {
                        this.transform.position = this.desiredPosition;
                    }
                    else
                    {
                        this.transform.position = Vector3.Lerp(this.transform.position, this.desiredPosition, this.positionLerpPercentage);
                    }
                }

                if (this.sendRotation)
                {
                    this.transform.rotation = Quaternion.Slerp(this.transform.rotation, this.desiredRotation, 0.1f);
                }

                if (this.sendScale)
                {
                    this.transform.localScale = Vector3.Lerp(this.transform.localScale, this.desiredScale, 0.1f);
                }
            }
            else
            {
                // TODO [bgish]: Detect if we're no long moving, Call SendNetworkBehaviourMessage(true), and change the update frequency to be
                //               several seconds if that's the case.  Make sure to set it back to the original time if we're moving again.
            }
        }

        protected override SendConfig GetInitialSendConfig()
        {
            return new SendConfig
            {
                NetworkUpdateType = NetworkUpdateType.Tick,
                SendReliable = false,
                UpdateFrequency = 0.1f,
            };
        }
    }
}
