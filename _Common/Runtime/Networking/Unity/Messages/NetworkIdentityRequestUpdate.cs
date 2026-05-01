//-----------------------------------------------------------------------
// <copyright file="NetworkIdentityRequestUpdate.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    public class NetworkIdentityRequestUpdate : Message
    {
        public const short Id = 204;

        public long NetworkId { get; set; }

        public bool IsEnabled { get; set; }

        public string ResourceName { get; set; }

        public Position3D Position { get; set; }

        public Rotation Rotation { get; set; }

        public int BehaviourCount { get; set; }

        public bool DestoryOnDisconnect { get; set; }

        public bool CanChangeOwner { get; set; }

        public override string GetTypeName() => nameof(NetworkIdentityRequestUpdate);

        public override short GetId()
        {
            return Id;
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);

            this.NetworkId = (long)reader.ReadPackedUInt64();
            this.IsEnabled = reader.ReadBoolean();
            this.ResourceName = reader.ReadString();
            this.Position = reader.ReadPosition3D();
            this.Rotation = reader.ReadRotation();
            this.BehaviourCount = (int)reader.ReadPackedUInt32();
            this.DestoryOnDisconnect = reader.ReadBoolean();
            this.CanChangeOwner = reader.ReadBoolean();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.WritePackedUInt64((ulong)this.NetworkId);
            writer.Write(this.IsEnabled);
            writer.Write(this.ResourceName);
            writer.Write(this.Position);
            writer.Write(this.Rotation);
            writer.WritePackedUInt32((uint)this.BehaviourCount);
            writer.Write(this.DestoryOnDisconnect);
            writer.Write(this.CanChangeOwner);
        }
    }
}
