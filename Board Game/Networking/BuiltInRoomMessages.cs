//-----------------------------------------------------------------------
// <copyright file="BuiltInRoomMessages.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BoardGame.Networking
{
    using OGT.Networking;

    /// <summary>
    /// Broadcast by a user right after it connects (and after every reconnect). Everyone that receives it replies with <see cref="UserInfoUpdated"/>.
    /// </summary>
    public sealed class UserJoined : RoomMessage
    {
        public const int Id = 1;

        public UserInfo User { get; set; } = new UserInfo();

        public override short GetId() => Id;

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            this.User.Serialize(writer);
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            this.User ??= new UserInfo();
            this.User.Deserialize(reader);
        }
    }

    /// <summary>
    /// Broadcast by a user that is leaving gracefully. Ungraceful drops are detected through heartbeat timeouts.
    /// </summary>
    public sealed class UserLeft : RoomMessage
    {
        public string UserId { get; set; }

        public const int Id = 2;

        public override short GetId() => Id;

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(this.UserId);
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            this.UserId = reader.ReadString();
        }
    }

    /// <summary>
    /// Carries the full <see cref="UserInfo"/> of a user; sent in reply to <see cref="UserJoined"/> and whenever the local info changes.
    /// </summary>
    public sealed class UserInfoUpdated : RoomMessage
    {
        public UserInfo User { get; set; } = new UserInfo();

        public const int Id = 3;

        public override short GetId() => Id;

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            this.User.Serialize(writer);
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            this.User ??= new UserInfo();
            this.User.Deserialize(reader);
        }
    }

    /// <summary>
    /// Periodic liveness message so peers can detect ungraceful disconnects.
    /// </summary>
    public sealed class Heartbeat : RoomMessage
    {
        public const int Id = 4;

        public override short GetId() => Id;
    }
}
