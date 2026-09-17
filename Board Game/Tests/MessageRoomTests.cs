//-----------------------------------------------------------------------
// <copyright file="MessageRoomTests.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BoardGame.Networking.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using OGT.Networking;

    public class MessageRoomTests
    {
        private FakeMessageHub hub;
        private FakeClock clock;
        private List<TestClient> clients;

        [SetUp]
        public void SetUp()
        {
            this.hub = new FakeMessageHub();
            this.clock = new FakeClock();
            this.clients = new List<TestClient>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (TestClient client in this.clients)
            {
                client.Dispose();
            }
        }

        [Test]
        public void ThreeClientsConvergeOnTheSameRoster()
        {
            TestClient a = this.AddClient("1", "A");
            TestClient b = this.AddClient("2", "B");
            TestClient c = this.AddClient("3", "C");

            a.Join();
            this.Pump();
            b.Join();
            this.Pump();
            c.Join();
            this.Pump();

            foreach (TestClient client in this.clients)
            {
                Assert.IsTrue(client.Room.IsConnected, client.User.DisplayName);
                CollectionAssert.AreEqual(new[] { "1", "2", "3" }, client.Room.Users.Select(u => u.UserId).ToArray(), client.User.DisplayName);
                Assert.AreEqual("B", client.Room.Users[1].DisplayName);
            }
        }

        [Test]
        public void RoomCodesAreIsolated()
        {
            TestClient a = this.AddClient("1", "A");
            TestClient b = this.AddClient("2", "B");

            a.Join("ONE");
            b.Join("TWO");
            this.Pump();

            Assert.AreEqual(1, a.Room.Users.Count);
            Assert.AreEqual(1, b.Room.Users.Count);
        }

        [Test]
        public void GracefulLeaveRemovesUserAndFiresEvents()
        {
            TestClient a = this.AddClient("1", "A");
            TestClient b = this.AddClient("2", "B");
            a.Join();
            b.Join();
            this.Pump();

            UserInfo left = null;
            a.Room.OnUserLeft += user => left = user;

            int disconnects = 0;
            b.Room.OnDisconnected += () => disconnects++;

            b.Room.Leave();
            this.Pump();

            Assert.IsNotNull(left);
            Assert.AreEqual("2", left.UserId);
            Assert.AreEqual(1, a.Room.Users.Count);
            Assert.IsFalse(b.Room.IsConnected);
            Assert.IsFalse(b.Room.IsJoined);
            Assert.AreEqual(1, disconnects);
            Assert.AreEqual(1, b.Room.Users.Count);
        }

        [Test]
        public void KilledConnectionIsEvictedAfterTimeout()
        {
            var options = new MessageRoomOptions { HeartbeatSeconds = 5, UserTimeoutSeconds = 15, JoinReplyJitterSeconds = 0 };
            TestClient a = this.AddClient("1", "A", options);
            TestClient b = this.AddClient("2", "B", options);
            a.Join();
            b.Join();
            this.Pump();

            this.hub.Kill(b.Connection);
            this.Pump();
            Assert.IsFalse(b.Room.IsConnected);
            Assert.AreEqual(2, a.Room.Users.Count, "no timeout yet");

            this.clock.Advance(10);
            this.Pump();
            Assert.AreEqual(2, a.Room.Users.Count, "still within the timeout");

            this.clock.Advance(6);
            this.Pump();
            Assert.AreEqual(1, a.Room.Users.Count, "evicted");
        }

        [Test]
        public void HeartbeatsKeepQuietClientsAlive()
        {
            var options = new MessageRoomOptions { HeartbeatSeconds = 5, UserTimeoutSeconds = 15, JoinReplyJitterSeconds = 0 };
            TestClient a = this.AddClient("1", "A", options);
            TestClient b = this.AddClient("2", "B", options);
            a.Join();
            b.Join();
            this.Pump();

            for (int i = 0; i < 10; i++)
            {
                this.clock.Advance(6);
                this.Pump();
            }

            Assert.AreEqual(2, a.Room.Users.Count);
            Assert.AreEqual(2, b.Room.Users.Count);
            Assert.Greater(a.Room.GetRoomStats().Received.First(s => s.Id == Heartbeat.Id).Count, 5);
        }

        [Test]
        public void ReconnectReannouncesAndRefreshesRoster()
        {
            TestClient a = this.AddClient("1", "A");
            TestClient b = this.AddClient("2", "B");
            a.Join();
            b.Join();
            this.Pump();

            this.hub.Kill(b.Connection);
            this.Pump();
            this.clock.Advance(20);
            this.Pump();
            Assert.AreEqual(1, a.Room.Users.Count);

            int connects = 0;
            b.Room.OnConnected += () => connects++;
            b.Connection.SimulateReconnect();
            this.Pump();

            Assert.AreEqual(1, connects);
            Assert.AreEqual(2, a.Room.Users.Count);
            Assert.AreEqual(2, b.Room.Users.Count);
        }

        [Test]
        public void EchoedMessagesAreFiltered()
        {
            this.hub.Echo = true;
            TestClient a = this.AddClient("1", "A");
            TestClient b = this.AddClient("2", "B");
            a.Room.RegisterMessageType<ChatMessage>();
            b.Room.RegisterMessageType<ChatMessage>();
            a.Join();
            b.Join();
            this.Pump();

            var receivedByA = new List<string>();
            var receivedByB = new List<string>();
            a.Room.OnMessageReceived += m => receivedByA.Add(((ChatMessage)m).Text);
            b.Room.OnMessageReceived += m => receivedByB.Add(((ChatMessage)m).Text);

            a.Room.SendMessage(new ChatMessage { Text = "hi" });
            this.Pump();

            Assert.AreEqual(0, receivedByA.Count);
            CollectionAssert.AreEqual(new[] { "hi" }, receivedByB);
            Assert.AreEqual(2, a.Room.Users.Count, "echo must not add the local user twice");
        }

        [Test]
        public void CustomMessagesCarrySenderAndPayload()
        {
            TestClient a = this.AddClient("1", "A");
            TestClient b = this.AddClient("2", "B");
            a.Room.RegisterMessageType<ChatMessage>();
            b.Room.RegisterMessageType<ChatMessage>();
            a.Join();
            b.Join();
            this.Pump();

            string sender = null;
            string text = null;
            b.Room.OnMessageReceived += m =>
            {
                sender = m.SenderUserId;
                text = ((ChatMessage)m).Text;
            };

            a.Room.SendMessage(new ChatMessage { Text = "hello" });
            this.Pump();

            Assert.AreEqual("1", sender);
            Assert.AreEqual("hello", text);
        }

        [Test]
        public void UnknownMessageIdsAreIgnored()
        {
            TestClient a = this.AddClient("1", "A");
            TestClient b = this.AddClient("2", "B");
            b.Room.RegisterMessageType<ChatMessage>();
            a.Join();
            b.Join();
            this.Pump();

            int received = 0;
            a.Room.OnMessageReceived += _ => received++;

            b.Room.SendMessage(new ChatMessage { Text = "you cannot read this" });
            b.Room.SendMessage(new ChatMessage { Text = "or this" });
            this.Pump();

            Assert.AreEqual(0, received);
            Assert.AreEqual(2, a.Room.Users.Count, "the room itself keeps working");
        }

        [Test]
        public void UpdateLocalUserInfoBroadcastsChanges()
        {
            TestClient a = this.AddClient("1", "A");
            TestClient b = this.AddClient("2", "B");
            a.Join();
            b.Join();
            this.Pump();

            int changes = 0;
            b.Room.OnUsersChanged += () => changes++;

            a.Room.UpdateLocalUserInfo(user => user.DisplayName = "Alice");
            this.Pump();

            Assert.AreEqual("Alice", b.Room.Users.First(u => u.UserId == "1").DisplayName);
            Assert.GreaterOrEqual(changes, 1);
        }

        [Test]
        public void StatsTrackCountsAndSizes()
        {
            TestClient a = this.AddClient("1", "A");
            TestClient b = this.AddClient("2", "B");
            a.Room.RegisterMessageType<ChatMessage>();
            b.Room.RegisterMessageType<ChatMessage>();
            a.Join();
            b.Join();
            this.Pump();

            a.Room.SendMessage(new ChatMessage { Text = "a" });
            a.Room.SendMessage(new ChatMessage { Text = "a much longer message" });
            this.Pump();

            RoomStats sent = a.Room.GetRoomStats();
            MessageTypeStats chat = sent.Sent.First(s => s.Id == ChatMessage.Id);
            Assert.AreEqual(2, chat.Count);
            Assert.Less(chat.MinBytes, chat.MaxBytes);
            Assert.AreEqual(chat.TotalBytes, chat.MinBytes + chat.MaxBytes);
            Assert.AreEqual((chat.MinBytes + chat.MaxBytes) / 2.0, chat.AverageBytes);
            Assert.AreEqual(sent.Sent.Sum(s => s.Count), sent.TotalMessagesSent);

            RoomStats received = b.Room.GetRoomStats();
            Assert.AreEqual(2, received.Received.First(s => s.Id == ChatMessage.Id).Count);
            Assert.AreEqual(chat.TotalBytes, received.Received.First(s => s.Id == ChatMessage.Id).TotalBytes);
        }

        private TestClient AddClient(string userId, string name, MessageRoomOptions options = null)
        {
            var client = new TestClient(this.hub, this.clock, userId, name, options);
            this.clients.Add(client);
            return client;
        }

        private void Pump() => TestPump.Run(this.hub, this.clients);

        private sealed class ChatMessage : RoomMessage
        {
            public const short Id = 1001;

            public string Text { get; set; }

            public override short GetId() => Id;

            public override void Serialize(NetworkWriter writer)
            {
                base.Serialize(writer);
                writer.Write(this.Text);
            }

            public override void Deserialize(NetworkReader reader)
            {
                base.Deserialize(reader);
                this.Text = reader.ReadString();
            }
        }
    }
}
