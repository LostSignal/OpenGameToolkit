//-----------------------------------------------------------------------
// <copyright file="BoardGameMessages.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BoardGame.Networking
{
    using System.Collections.Generic;
    using OGT.Networking;

    /// <summary>
    /// Broadcast by the host when a new game starts, and sent in reply to a <see cref="MissingActions"/> with LastKnownActionId = -1.
    /// </summary>
    public sealed class BoardGameStart : RoomMessage
    {
        public const int Id = 300;

        public int GameInstanceId { get; set; }

        public string HostUserId { get; set; }

        /// <summary>Gets or sets the sender's last action id at the time of sending (-1 for a fresh game), so receivers know when they have caught up.</summary>
        public int LastActionId { get; set; } = -1;

        /// <summary>Gets or sets the config serialized with JsonUtil including type information.</summary>
        public string ConfigJson { get; set; }

        public override short GetId() => Id;

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(this.GameInstanceId);
            writer.Write(this.HostUserId);
            writer.Write(this.LastActionId);
            WriteText(writer, this.ConfigJson);
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            this.GameInstanceId = reader.ReadInt32();
            this.HostUserId = reader.ReadString();
            this.LastActionId = reader.ReadInt32();
            this.ConfigJson = ReadText(reader);
        }
    }

    /// <summary>
    /// One or more contiguous actions starting at <see cref="FirstActionId"/>, each serialized with the game's <see cref="IBoardGameAction"/>.
    /// </summary>
    public sealed class ActionsPlayed : RoomMessage
    {
        public const int Id = 301;

        public int GameInstanceId { get; set; }

        public int FirstActionId { get; set; }

        public List<byte[]> Actions { get; } = new List<byte[]>();

        public override short GetId() => Id;

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(this.GameInstanceId);
            writer.Write(this.FirstActionId);
            WriteByteArrayList(writer, this.Actions);
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            this.GameInstanceId = reader.ReadInt32();
            this.FirstActionId = reader.ReadInt32();
            ReadByteArrayList(reader, this.Actions);
        }
    }

    /// <summary>
    /// Asks a specific user for every action after <see cref="LastKnownActionId"/>. When <see cref="IncludeStart"/> is set the
    /// responder sends its <see cref="BoardGameStart"/> first (late join / full resync). Only <see cref="TargetUserId"/> answers,
    /// and it always answers with at least one (possibly empty) <see cref="ActionsPlayed"/> so the requester knows it is up to date.
    /// </summary>
    public sealed class MissingActions : RoomMessage
    {
        public const int Id = 302;

        public int GameInstanceId { get; set; }

        public int LastKnownActionId { get; set; } = -1;

        public bool IncludeStart { get; set; }

        public string TargetUserId { get; set; }

        public override short GetId() => Id;

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(this.GameInstanceId);
            writer.Write(this.LastKnownActionId);
            writer.Write(this.IncludeStart);
            writer.Write(this.TargetUserId);
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            this.GameInstanceId = reader.ReadInt32();
            this.LastKnownActionId = reader.ReadInt32();
            this.IncludeStart = reader.ReadBoolean();
            this.TargetUserId = reader.ReadString();
        }
    }
}
