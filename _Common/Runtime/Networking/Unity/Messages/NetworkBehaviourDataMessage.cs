//-----------------------------------------------------------------------
// <copyright file="NetworkBehaviourDataMessage.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    public class NetworkBehaviourDataMessage : Message
    {
        public const short Id = 205;

        public long NetworkId { get; set; }

        public int BehaviourIndex { get; set; }

        public NetworkBehaviourDataSendType SendType { get; set; }

        public string DataKey { get; set; }

        public string DataValue { get; set; }

        public override short GetId()
        {
            return Id;
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);

            this.NetworkId = (long)reader.ReadPackedUInt64();
            this.BehaviourIndex = (int)reader.ReadPackedUInt32();
            this.SendType = (NetworkBehaviourDataSendType)reader.ReadPackedUInt32();
            this.DataKey = reader.ReadString();
            this.DataValue = reader.ReadString();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.WritePackedUInt64((ulong)this.NetworkId);
            writer.WritePackedUInt32((uint)this.BehaviourIndex);
            writer.WritePackedUInt32((uint)this.SendType);
            writer.Write(this.DataKey);
            writer.Write(this.DataValue);
        }
    }
}
