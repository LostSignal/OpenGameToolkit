//-----------------------------------------------------------------------
// <copyright file="IMessageConnection.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BoardGame.Networking
{
    using System;

    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting,
    }

    /// <summary>
    /// A transport that delivers raw byte messages to everyone else in a room. Implementations may do their work on
    /// background threads, but they must only raise events from <see cref="Update"/> so consumers stay single threaded.
    /// </summary>
    public interface IMessageConnection : IDisposable
    {
        /// <summary>Fires every time the connection is established, including after an automatic reconnect.</summary>
        event Action OnConnected;

        /// <summary>Fires when an established connection is lost or closed. The argument is a human readable reason.</summary>
        event Action<string> OnDisconnected;

        /// <summary>Fires for every message received from the room. The segment is only valid for the duration of the callback.</summary>
        event Action<ArraySegment<byte>> OnMessageReceived;

        ConnectionState State { get; }

        /// <summary>Gets the transport level id of this connection (for example the Web PubSub connectionId), or null if unknown.</summary>
        string LocalConnectionId { get; }

        /// <summary>Starts connecting to the given url. Non blocking.</summary>
        void Connect();

        /// <summary>Gracefully closes the connection, flushing queued messages first (bounded). Non blocking.</summary>
        void Disconnect();

        /// <summary>Queues a message for everyone else in the room. The data is copied.</summary>
        void Send(byte[] data, int offset, int count);

        /// <summary>Pumps queued events on the calling thread. Call once per frame.</summary>
        void Update();
    }
}
