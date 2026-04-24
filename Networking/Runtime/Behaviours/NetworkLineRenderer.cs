//-----------------------------------------------------------------------
// <copyright file="NetworkLineRenderer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    using OGT;
    using UnityEngine;

    [RequireComponent(typeof(LineRenderer))]
    public class NetworkLineRenderer : NetworkBehaviour
    {
        private static readonly OGTLogger Logger = OGTLogger.Networking;

#pragma warning disable 0649
        [SerializeField] private LineRenderer lineRenderer;
#pragma warning restore 0649

        private uint version;

        //// TODO [bgish]: Would love it if we could attach this to an event that fires everytime our line renderer data is updated
        public void LineRendererUpdated(bool sendReliably = true)
        {
            this.SendNetworkBehaviourMessage(sendReliably);
        }

        public override void Serialize(NetworkWriter writer)
        {
            int positionCount = this.lineRenderer.GetPositions(OGT.Caching.Vectors);
            bool useWorldSpace = this.lineRenderer.useWorldSpace;

            writer.WritePackedUInt32(++this.version);
            writer.Write(this.lineRenderer.enabled);
            writer.WritePackedUInt32((uint)positionCount);

            for (int i = 0; i < positionCount; i++)
            {
                if (useWorldSpace)
                {
                    writer.Write(NetworkTransformAnchor.InverseTransformPosition(OGT.Caching.Vectors[i]));
                }
                else
                {
                    writer.Write(OGT.Caching.Vectors[i]);
                }
            }

            if (NetworkingManager.PrintDebugOutput)
            {
                Logger.Log($"NetworkLineRenderer {this.GetEntityId()} serialized {positionCount} positions.", this);
            }
        }

        public override void Deserialize(NetworkReader reader)
        {
            var messageVersion = reader.ReadPackedUInt32();

            if (this.version > messageVersion)
            {
                return;
            }

            this.version = messageVersion;
            this.lineRenderer.enabled = reader.ReadBoolean();
            int positionCount = (int)reader.ReadPackedUInt32();

            bool useWorldSpace = this.lineRenderer.useWorldSpace;

            for (int i = 0; i < positionCount; i++)
            {
                OGT.Caching.Vectors[i] = useWorldSpace ? NetworkTransformAnchor.TransformPosition(reader.ReadVector3()) : reader.ReadVector3();
            }

            this.lineRenderer.positionCount = positionCount;
            this.lineRenderer.SetPositions(OGT.Caching.Vectors);

            if (NetworkingManager.PrintDebugOutput)
            {
                Logger.Log($"NetworkLineRenderer {this.GetEntityId()} deserialized {positionCount} positions.", this);
            }
        }

        //// private Vector3[] desiredPositions = new Vector3[100];
        //// private uint desiredPositionsCount;
        ////
        //// // BUG [bgish]: THIS IS CURRENTLY NEVER GETTING CALLED, we probably need to make it so non-owners actually call Update
        //// //              The real answer may be that Network Line renderers don't do any lerping and that is a completely different component
        //// protected override void NetworkUpdate()
        //// {
        ////     base.NetworkUpdate();
        ////
        ////     if (this.IsOwner == false)
        ////     {
        ////         if (this.doPositionsChangeFrequently == false)
        ////         {
        ////             // NOTE [bgish]: If they don't change frequently, then just set them to the desired
        ////             this.lineRenderer.positionCount = (int)this.desiredPositionsCount;
        ////             this.lineRenderer.SetPositions(this.desiredPositions);
        ////         }
        ////         else
        ////         {
        ////             throw new System.NotImplementedException();
        ////
        ////             //// // NOTE [bgish]: If they do chnage often, then lets lerp them to their desired position
        ////             //// this.currentPositionsCount = (uint)this.lineRenderer.GetPositions(this.currentPositions);
        ////             ////
        ////             //// // Lerp all the position that we already have
        ////             //// for (int i = 0; i < this.currentPositionsCount; i++)
        ////             //// {
        ////             ////     float distance = Vector3.Distance(this.currentPositions[i], this.desiredPositions[i]);
        ////             ////     this.currentPositions[i] = Vector3.MoveTowards(this.currentPositions[i], this.desiredPositions[i], distance * this.UpdateFrequency);
        ////             //// }
        ////             ////
        ////             //// // If more have been added then snap to the new desired positions
        ////             //// if (this.currentPositionsCount != this.desiredPositionsCount)
        ////             //// {
        ////             ////     this.lineRenderer.positionCount = (int)this.desiredPositionsCount;
        ////             ////
        ////             ////     for (uint i = this.currentPositionsCount; i < this.desiredPositionsCount; i++)
        ////             ////     {
        ////             ////         this.currentPositions[i] = this.desiredPositions[i];
        ////             ////     }
        ////             //// }
        ////             ////
        ////             //// this.lineRenderer.SetPositions(this.currentPositions);
        ////         }
        ////     }
        //// }

        protected override SendConfig GetInitialSendConfig()
        {
            return new SendConfig
            {
                NetworkUpdateType = NetworkUpdateType.Manual,
                SendReliable = false,
                UpdateFrequency = 0.1f,
            };
        }

        private void Start()
        {
            if (this.IsOwner)
            {
                this.LineRendererUpdated();
            }
        }
    }
}
