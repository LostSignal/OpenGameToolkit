//-----------------------------------------------------------------------
// <copyright file="FakeMessageHub.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BoardGame.Networking.Tests
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Deterministic manual clock for driving heartbeats and timeouts in tests.
    /// </summary>
    public sealed class FakeClock
    {
        public double Now { get; private set; }

        public void Advance(double seconds) => this.Now += seconds;

        public double Read() => this.Now;
    }

    /// <summary>
    /// In-memory stand in for a Web PubSub group. Messages sent through a <see cref="FakeMessageConnection"/> are queued here
    /// and fanned out to the other connections in the same room on <see cref="Deliver"/>. Supports dropping, holding and
    /// reordering messages, echo, and simulated connection loss.
    /// </summary>
    public sealed class FakeMessageHub
    {
        private readonly List<FakeMessageConnection> connections = new List<FakeMessageConnection>();
        private readonly List<Envelope> inFlight = new List<Envelope>();
        private readonly Dictionary<FakeMessageConnection, int> dropAllCounts = new Dictionary<FakeMessageConnection, int>();
        private readonly Dictionary<(FakeMessageConnection, FakeMessageConnection), int> dropToCounts = new Dictionary<(FakeMessageConnection, FakeMessageConnection), int>();
        private readonly Dictionary<FakeMessageConnection, List<Envelope>> held = new Dictionary<FakeMessageConnection, List<Envelope>>();

        /// <summary>Gets or sets a value indicating whether senders receive their own messages back (Web PubSub does this unless noEcho is set).</summary>
        public bool Echo { get; set; }

        public int InFlightCount => this.inFlight.Count;

        public IReadOnlyList<FakeMessageConnection> Connections => this.connections;

        public FakeMessageConnection CreateConnection(string label = null)
        {
            var connection = new FakeMessageConnection(this, label ?? $"conn{this.connections.Count + 1}");
            this.connections.Add(connection);
            return connection;
        }

        /// <summary>Drops the next <paramref name="count"/> messages sent by <paramref name="from"/> for every recipient.</summary>
        public void DropNext(FakeMessageConnection from, int count = 1)
        {
            this.dropAllCounts[from] = count;
        }

        /// <summary>Drops the next <paramref name="count"/> messages sent by <paramref name="from"/> only for <paramref name="to"/>.</summary>
        public void DropNextTo(FakeMessageConnection from, FakeMessageConnection to, int count = 1)
        {
            this.dropToCounts[(from, to)] = count;
        }

        /// <summary>Holds every message sent by <paramref name="from"/> until <see cref="Release"/> is called.</summary>
        public void Hold(FakeMessageConnection from)
        {
            if (this.held.ContainsKey(from) == false)
            {
                this.held[from] = new List<Envelope>();
            }
        }

        /// <summary>Releases held messages from <paramref name="from"/>, optionally in reverse order, and stops holding.</summary>
        public void Release(FakeMessageConnection from, bool reverse = false)
        {
            if (this.held.TryGetValue(from, out List<Envelope> envelopes) == false)
            {
                return;
            }

            this.held.Remove(from);

            if (reverse)
            {
                envelopes.Reverse();
            }

            this.inFlight.AddRange(envelopes);
        }

        /// <summary>Delivers everything currently in flight to the recipients captured at send time.</summary>
        public void Deliver()
        {
            var batch = new List<Envelope>(this.inFlight);
            this.inFlight.Clear();

            foreach (Envelope envelope in batch)
            {
                foreach (FakeMessageConnection recipient in envelope.Recipients)
                {
                    byte[] copy = new byte[envelope.Data.Length];
                    Buffer.BlockCopy(envelope.Data, 0, copy, 0, copy.Length);
                    recipient.Receive(copy);
                }
            }
        }

        /// <summary>Simulates the network dropping out from under a connection (no UserLeft is sent).</summary>
        public void Kill(FakeMessageConnection connection)
        {
            connection.SimulateDrop();
        }

        internal void Submit(FakeMessageConnection from, byte[] data)
        {
            if (this.dropAllCounts.TryGetValue(from, out int dropAll) && dropAll > 0)
            {
                this.dropAllCounts[from] = dropAll - 1;
                return;
            }

            var recipients = new List<FakeMessageConnection>();

            foreach (FakeMessageConnection connection in this.connections)
            {
                if (connection.IsOpen == false || connection.RoomCode != from.RoomCode)
                {
                    continue;
                }

                if (connection == from && this.Echo == false)
                {
                    continue;
                }

                if (this.dropToCounts.TryGetValue((from, connection), out int dropTo) && dropTo > 0)
                {
                    this.dropToCounts[(from, connection)] = dropTo - 1;
                    continue;
                }

                recipients.Add(connection);
            }

            var envelope = new Envelope { From = from, Data = data, Recipients = recipients };

            if (this.held.TryGetValue(from, out List<Envelope> heldEnvelopes))
            {
                heldEnvelopes.Add(envelope);
            }
            else
            {
                this.inFlight.Add(envelope);
            }
        }

        private sealed class Envelope
        {
            public FakeMessageConnection From;
            public byte[] Data;
            public List<FakeMessageConnection> Recipients;
        }
    }

    public sealed class FakeMessageConnection : IMessageConnection
    {
        private readonly FakeMessageHub hub;
        private readonly Queue<Action> pendingEvents = new Queue<Action>();

        internal FakeMessageConnection(FakeMessageHub hub, string label)
        {
            this.hub = hub;
            this.Label = label;
        }

        public event Action OnConnected;

        public event Action<string> OnDisconnected;

        public event Action<ArraySegment<byte>> OnMessageReceived;

        public string Label { get; }

        /// <summary>Gets or sets the room this connection joins. Set it before <see cref="Connect"/>; defaults to "ROOM".</summary>
        public string RoomCode { get; set; } = "ROOM";

        /// <summary>Gets a value indicating whether the hub currently routes messages to and from this connection.</summary>
        public bool IsOpen { get; private set; }

        public ConnectionState State { get; private set; }

        public string LocalConnectionId => this.Label;

        public int SentCount { get; private set; }

        public bool HasPendingEvents => this.pendingEvents.Count > 0;

        public void Connect()
        {
            this.State = ConnectionState.Connecting;

            this.pendingEvents.Enqueue(() =>
            {
                this.IsOpen = true;
                this.State = ConnectionState.Connected;
                this.OnConnected?.Invoke();
            });
        }

        public void Disconnect()
        {
            if (this.State == ConnectionState.Disconnected)
            {
                return;
            }

            this.pendingEvents.Enqueue(() =>
            {
                bool wasOpen = this.IsOpen;
                this.IsOpen = false;
                this.State = ConnectionState.Disconnected;

                if (wasOpen)
                {
                    this.OnDisconnected?.Invoke("Disconnect requested");
                }
            });
        }

        public void Send(byte[] data, int offset, int count)
        {
            byte[] copy = new byte[count];
            Buffer.BlockCopy(data, offset, copy, 0, count);
            this.SentCount++;
            this.hub.Submit(this, copy);
        }

        public void Update()
        {
            while (this.pendingEvents.Count > 0)
            {
                this.pendingEvents.Dequeue()();
            }
        }

        public void Dispose()
        {
            this.IsOpen = false;
            this.State = ConnectionState.Disconnected;
            this.pendingEvents.Clear();
        }

        /// <summary>Simulates the transport coming back after <see cref="FakeMessageHub.Kill"/>.</summary>
        public void SimulateReconnect()
        {
            this.State = ConnectionState.Reconnecting;

            this.pendingEvents.Enqueue(() =>
            {
                this.IsOpen = true;
                this.State = ConnectionState.Connected;
                this.OnConnected?.Invoke();
            });
        }

        internal void SimulateDrop()
        {
            this.IsOpen = false;

            this.pendingEvents.Enqueue(() =>
            {
                this.State = ConnectionState.Reconnecting;
                this.OnDisconnected?.Invoke("Simulated network drop");
            });
        }

        internal void Receive(byte[] data)
        {
            this.pendingEvents.Enqueue(() =>
            {
                if (this.IsOpen)
                {
                    this.OnMessageReceived?.Invoke(new ArraySegment<byte>(data));
                }
            });
        }
    }
}
