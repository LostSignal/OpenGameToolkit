//-----------------------------------------------------------------------
// <copyright file="UserInfoMessage.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    public class UserInfoMessage : Message
    {
        public const short Id = 5;

        public UserInfoMessage()
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
