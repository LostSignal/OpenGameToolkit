//-----------------------------------------------------------------------
// <copyright file="UserDisconnectedMessage.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    public class UserDisconnectedMessage : Message
    {
        public const short Id = 4;

        public string UserId { get; set; }

        public bool WasConnectionLost { get; set; }

        public override string GetTypeName() => nameof(UserDisconnectedMessage);

        public override short GetId()
        {
            return Id;
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);

            this.UserId = reader.ReadString();
            this.WasConnectionLost = reader.ReadBoolean();
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            writer.Write(this.UserId);
            writer.Write(this.WasConnectionLost);
        }
    }
}
