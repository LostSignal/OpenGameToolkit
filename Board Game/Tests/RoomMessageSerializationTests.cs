//-----------------------------------------------------------------------
// <copyright file="RoomMessageSerializationTests.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BoardGame.Networking.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using NUnit.Framework;
    using OGT.Networking;

    public class RoomMessageSerializationTests
    {
        private MessageCollection messages;

        [SetUp]
        public void SetUp()
        {
            this.messages = new MessageCollection();
            this.messages.RegisterMessage<UserJoined>();
            this.messages.RegisterMessage<UserLeft>();
            this.messages.RegisterMessage<UserInfoUpdated>();
            this.messages.RegisterMessage<Heartbeat>();
            this.messages.RegisterMessage<BoardGameStart>();
            this.messages.RegisterMessage<ActionsPlayed>();
            this.messages.RegisterMessage<MissingActions>();
        }

        [Test]
        public void UserJoinedRoundTripsUserInfoAndSender()
        {
            var message = new UserJoined
            {
                SenderUserId = "sender-1234",
                User = new UserInfo { UserId = "42", DisplayName = "Zoë 🎲", CustomData = { ["team"] = "blue" } },
            };

            var result = (UserJoined)this.RoundTrip(message);

            Assert.AreEqual(message.SenderUserId, result.SenderUserId);
            Assert.AreEqual("42", result.User.UserId);
            Assert.AreEqual("Zoë 🎲", result.User.DisplayName);
            Assert.AreEqual("blue", result.User.CustomData["team"]);
        }

        [Test]
        public void UserLeftAndHeartbeatRoundTrip()
        {
            var left = (UserLeft)this.RoundTrip(new UserLeft { SenderUserId = "7", UserId = "99" });
            Assert.AreEqual("7", left.SenderUserId);
            Assert.AreEqual("99", left.UserId);

            var heartbeat = (Heartbeat)this.RoundTrip(new Heartbeat { SenderUserId = "8" });
            Assert.AreEqual("8", heartbeat.SenderUserId);
        }

        [Test]
        public void BoardGameStartRoundTripsLargeConfigJson()
        {
            // Well past the 32 KB NetworkWriter string limit
            string json = new string('x', 100_000) + "ünïcödé";

            var message = new BoardGameStart
            {
                SenderUserId = "1",
                GameInstanceId = 123456,
                HostUserId = "host-987654321",
                LastActionId = 17,
                ConfigJson = json,
            };

            var result = (BoardGameStart)this.RoundTrip(message);

            Assert.AreEqual(123456, result.GameInstanceId);
            Assert.AreEqual("host-987654321", result.HostUserId);
            Assert.AreEqual(17, result.LastActionId);
            Assert.AreEqual(json, result.ConfigJson);
        }

        [Test]
        public void BoardGameStartPreservesNullConfigJson()
        {
            var result = (BoardGameStart)this.RoundTrip(new BoardGameStart { ConfigJson = null });
            Assert.IsNull(result.ConfigJson);
        }

        [Test]
        public void ActionsPlayedRoundTripsListIncludingOversizedEntries()
        {
            byte[] big = new byte[70_000]; // over the 64 KB WriteBytesAndSize limit
            new Random(1).NextBytes(big);

            var message = new ActionsPlayed { SenderUserId = "5", GameInstanceId = 9, FirstActionId = 3 };
            message.Actions.Add(new byte[] { 1, 2, 3 });
            message.Actions.Add(Array.Empty<byte>());
            message.Actions.Add(big);

            var result = (ActionsPlayed)this.RoundTrip(message);

            Assert.AreEqual(9, result.GameInstanceId);
            Assert.AreEqual(3, result.FirstActionId);
            Assert.AreEqual(3, result.Actions.Count);
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, result.Actions[0]);
            Assert.AreEqual(0, result.Actions[1].Length);
            CollectionAssert.AreEqual(big, result.Actions[2]);
        }

        [Test]
        public void MissingActionsRoundTrips()
        {
            var result = (MissingActions)this.RoundTrip(new MissingActions { SenderUserId = "2", GameInstanceId = 4, LastKnownActionId = -1, IncludeStart = true, TargetUserId = "77" });

            Assert.AreEqual(4, result.GameInstanceId);
            Assert.AreEqual(-1, result.LastKnownActionId);
            Assert.IsTrue(result.IncludeStart);
            Assert.AreEqual("77", result.TargetUserId);
        }

        [Test]
        public void MessageIdIsInTheFirstTwoBytes()
        {
            byte[] data = Serialize(new ActionsPlayed());
            Assert.AreEqual(ActionsPlayed.Id, MessageCollection.GetMessageId(data));
        }

        [Test]
        public void PooledMessagesAreReusedAcrossDeserializations()
        {
            byte[] first = Serialize(new UserLeft { UserId = "1" });
            byte[] second = Serialize(new UserLeft { UserId = "2" });

            Message a = this.messages.GetMessage(first);
            this.messages.RecycleMessage(a);
            Message b = this.messages.GetMessage(second);

            Assert.AreSame(a, b);
            Assert.AreEqual("2", ((UserLeft)b).UserId);
        }

        [Test]
        public void TextHelpersHandleEmptyAndUnicode()
        {
            var writer = new NetworkWriter();
            RoomMessage.WriteText(writer, string.Empty);
            RoomMessage.WriteText(writer, "日本語 text");
            RoomMessage.WriteText(writer, null);

            var reader = new NetworkReader(writer.ToArray());
            Assert.AreEqual(string.Empty, RoomMessage.ReadText(reader));
            Assert.AreEqual("日本語 text", RoomMessage.ReadText(reader));
            Assert.IsNull(RoomMessage.ReadText(reader));
        }

        [Test]
        public void NetBufferBase64EncodesOnlyWrittenBytes()
        {
            var buffer = new NetBuffer();
            buffer.WriteByte4(1, 2, 3, 4);
            buffer.WriteByte(5);

            string base64 = buffer.ToBase64();
            Assert.AreEqual(Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 }), base64);

            var decoded = new NetBuffer();
            int count = decoded.SetBytesFromBase64(base64);
            Assert.AreEqual(5, count);
            Assert.AreEqual(1, decoded.ReadByte());
            Assert.AreEqual(2, decoded.ReadByte());
            Assert.AreEqual(3, decoded.ReadByte());
            Assert.AreEqual(4, decoded.ReadByte());
            Assert.AreEqual(5, decoded.ReadByte());
        }

        [Test]
        public void GeneratedUserIdsAreNotEmpty()
        {
            for (int i = 0; i < 200; i++)
            {
                Assert.IsFalse(string.IsNullOrEmpty(UserInfo.GenerateRandomUserInfo().UserId));
            }
        }

        [Test]
        public void WebPubSubProtocolParsesFrames()
        {
            var protocol = new WebPubSubRoomProtocol();

            Assert.IsTrue(protocol.TryParseInbound("{\"type\":\"system\",\"event\":\"connected\",\"connectionId\":\"abc\",\"userId\":\"u1\"}", out InboundFrame connected));
            Assert.AreEqual(InboundFrameType.Connected, connected.Type);
            Assert.AreEqual("abc", connected.ConnectionId);

            Assert.IsTrue(protocol.TryParseInbound("{\"type\":\"ack\",\"ackId\":3,\"success\":false,\"error\":{\"name\":\"Forbidden\",\"message\":\"nope\"}}", out InboundFrame ack));
            Assert.AreEqual(InboundFrameType.Ack, ack.Type);
            Assert.AreEqual(3, ack.AckId);
            Assert.IsFalse(ack.Success);
            StringAssert.Contains("Forbidden", ack.Error);

            byte[] payload = Encoding.UTF8.GetBytes("hello");
            string send = protocol.BuildSend("ROOM", payload, 0, payload.Length, 9);
            StringAssert.Contains("\"sendToGroup\"", send);
            StringAssert.Contains("\"noEcho\":true", send);

            string inbound = "{\"type\":\"message\",\"from\":\"group\",\"group\":\"ROOM\",\"dataType\":\"binary\",\"data\":\"" + Convert.ToBase64String(payload) + "\"}";
            Assert.IsTrue(protocol.TryParseInbound(inbound, out InboundFrame message));
            Assert.AreEqual(InboundFrameType.GroupMessage, message.Type);
            CollectionAssert.AreEqual(payload, message.Data);

            Assert.IsFalse(protocol.TryParseInbound("not json", out _));
            Assert.IsFalse(protocol.TryParseInbound("{\"type\":\"message\",\"from\":\"server\",\"data\":\"x\"}", out _));
        }

        private static byte[] Serialize(RoomMessage message)
        {
            var writer = new NetworkWriter();
            message.Serialize(writer);
            return writer.ToArray();
        }

        private RoomMessage RoundTrip(RoomMessage message)
        {
            byte[] data = Serialize(message);
            var result = (RoomMessage)this.messages.GetMessage(data);
            Assert.AreNotSame(message, result);
            return result;
        }
    }
}
