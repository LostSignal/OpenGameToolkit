//-----------------------------------------------------------------------
// <copyright file="NetworkIdentityUpdate.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    public class NetworkIdentityUpdate : Message
    {
        public const short Id = 203;

        public long NetworkId { get; set; }

        public long OwnerId { get; set; }

        public bool IsEnabled { get; set; }

        public string ResourceName { get; set; }

        public Position3D Position { get; set; }

        public Rotation Rotation { get; set; }

        public bool CanChangeOwner { get; set; }

        public override short GetId()
        {
            return Id;
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);

            this.NetworkId = (long)reader.ReadPackedUInt64();
            this.OwnerId = (long)reader.ReadPackedUInt64();
            this.IsEnabled = reader.ReadBoolean();
            this.ResourceName = reader.ReadString();
            this.Position = reader.ReadPosition3D();
            this.Rotation = reader.ReadRotation();
            this.CanChangeOwner = reader.ReadBoolean();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.WritePackedUInt64((ulong)this.NetworkId);
            writer.WritePackedUInt64((ulong)this.OwnerId);
            writer.Write(this.IsEnabled);
            writer.Write(this.ResourceName);
            writer.Write(this.Position);
            writer.Write(this.Rotation);
            writer.Write(this.CanChangeOwner);
        }
    }
}
