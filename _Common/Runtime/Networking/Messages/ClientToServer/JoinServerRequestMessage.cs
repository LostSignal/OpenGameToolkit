//-----------------------------------------------------------------------
// <copyright file="JoinServerRequestMessage.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    public class JoinServerRequestMessage : Message
    {
        public const short Id = 1;

        public JoinServerRequestMessage()
        {
            this.UserInfo = new UserInfo();
        }

        public UserInfo UserInfo { get; set; }

        public override short GetId()
        {
            return Id;
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);

            this.UserInfo.Deserialize(reader);
        }

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);

            this.UserInfo.Serialize(writer);
        }
    }
}
