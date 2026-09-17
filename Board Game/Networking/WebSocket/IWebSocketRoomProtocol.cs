//-----------------------------------------------------------------------
// <copyright file="IWebSocketRoomProtocol.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BoardGame.Networking
{
    public enum InboundFrameType
    {
        Unknown,
        Connected,
        Disconnected,
        Ack,
        GroupMessage,
    }

    public struct InboundFrame
    {
        public InboundFrameType Type;
        public string ConnectionId;
        public string UserId;
        public int AckId;
        public bool Success;
        public string Error;
        public string Group;
        public byte[] Data;
        public string Reason;
    }

    /// <summary>
    /// Describes the text envelope a WebSocket service uses for joining a room (group) and broadcasting to it.
    /// Keeps all service specific JSON out of <see cref="WebSocketMessageConnection"/>.
    /// </summary>
    public interface IWebSocketRoomProtocol
    {
        /// <summary>Gets the WebSocket sub protocol to negotiate, or null for none.</summary>
        string SubProtocol { get; }

        string BuildJoin(string room, int ackId);

        string BuildLeave(string room, int ackId);

        string BuildSend(string room, byte[] data, int offset, int count, int ackId);

        bool TryParseInbound(string text, out InboundFrame frame);
    }
}
