//-----------------------------------------------------------------------
// <copyright file="BoardGameNetworkSyncTests.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BoardGame.Networking.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;

    public class BoardGameNetworkSyncTests
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
        public void ConfigSerializerRoundTripsTypeAndReplacesDefaultLists()
        {
            var config = TestGameConfig.TwoPlayers("1", "2");
            config.Bonus = 7;
            config.Seed = 99;
            config.Tags.Add("extra");
            config.CharacterStats["Default"] = new Dictionary<string, object> { ["Health"] = 3 };

            string json = JsonUtil.Serialize(config, includeTypeInformation: true);
            BoardGameConfig result = JsonUtil.Deserialize<BoardGameConfig>(json, includeTypeInformation: true);

            Assert.IsInstanceOf<TestGameConfig>(result);
            var typed = (TestGameConfig)result;
            Assert.AreEqual(7, typed.Bonus);
            Assert.AreEqual(99, typed.Seed);
            CollectionAssert.AreEqual(new[] { "default", "extra" }, typed.Tags);
            Assert.AreEqual(2, typed.Players.Count);
            Assert.AreEqual("2", typed.Players[1].UserId);
            Assert.AreEqual(3L, (long)typed.CharacterStats["Default"]["Health"], "boxed ints come back as longs");
        }

        [Test]
        public void HostStartPropagatesConfigSeedAndPlayerIds()
        {
            TestClient a = this.AddClient("1", "A");
            TestClient b = this.AddClient("2", "B");
            this.JoinAll();

            var config = TestGameConfig.TwoPlayers("1", "2");
            config.Bonus = 5;
            config.Seed = 4242;
            a.Host(config);
            this.Pump();

            Assert.IsNotNull(a.Game);
            Assert.IsNotNull(b.Game);
            Assert.AreEqual(4242, a.Game.Config.Seed);
            Assert.AreEqual(a.Game.Config.Seed, b.Game.Config.Seed);
            Assert.AreEqual(5, b.Game.TestConfig.Bonus);
            Assert.AreEqual("1", a.Session.LocalUser.UserId);
            Assert.AreEqual("2", b.Session.LocalUser.UserId);
            Assert.AreEqual(SyncState.InGame, a.Sync.State);
            Assert.AreEqual(SyncState.InGame, b.Sync.State);
            Assert.AreEqual(a.Sync.GameInstanceId, b.Sync.GameInstanceId);
            Assert.IsTrue(a.Sync.IsHost);
            Assert.IsFalse(b.Sync.IsHost);
        }

        [Test]
        public void LocalActionsAreAppliedRemotelyInOrder()
        {
            TestClient a = this.AddClient("1", "A");
            TestClient b = this.AddClient("2", "B");
            this.JoinAll();
            a.Host(TestGameConfig.TwoPlayers("1", "2"));
            this.Pump();

            a.Act("1", 10);
            this.Pump();
            b.Act("2", 20);
            this.Pump();
            a.Act("1", 30);
            this.Pump();

            this.AssertInSync(a, b);
            Assert.AreEqual(3, a.Game.ActionHistory.Count);
            Assert.AreEqual(ActionSource.Local, a.PerformedActions[0].Source);
            Assert.AreEqual(ActionSource.Remote, b.PerformedActions[0].Source);
            Assert.AreEqual(ActionSource.Local, b.PerformedActions[1].Source);
        }

        [Test]
        public void DuplicateActionsAreIgnored()
        {
            TestClient a = this.AddClient("1", "A");
            TestClient b = this.AddClient("2", "B");
            this.JoinAll();
            a.Host(TestGameConfig.TwoPlayers("1", "2", alternateTurns: false));
            this.Pump();

            a.Act("1", 10);
            this.Pump();
            int scoreAfterFirst = b.Game.Score;

            var duplicate = new ActionsPlayed { GameInstanceId = a.Sync.GameInstanceId, FirstActionId = 0 };
            duplicate.Actions.Add(new TestAction { UserId = "1", Delta = 10 }.ToBytes());
            a.Room.SendMessage(duplicate);
            this.Pump();

            Assert.AreEqual(scoreAfterFirst, b.Game.Score);
            Assert.AreEqual(1, b.Game.ActionHistory.Count);
            Assert.AreEqual(0, b.Desyncs.Count);
        }

        [Test]
        public void DroppedActionIsRecoveredThroughMissingActions()
        {
            TestClient a = this.AddClient("1", "A");
            TestClient b = this.AddClient("2", "B");
            this.JoinAll();
            a.Host(TestGameConfig.TwoPlayers("1", "2", alternateTurns: false));
            this.Pump();

            a.Act("1", 1);
            this.Pump();

            this.hub.DropNext(a.Connection);
            a.Act("1", 2); // lost
            a.Act("1", 3);
            this.Pump();

            this.AssertInSync(a, b);
            Assert.AreEqual(3, b.Game.ActionHistory.Count);
            Assert.AreEqual(SyncState.InGame, b.Sync.State);
            Assert.IsFalse(b.Sync.IsAwaitingMissingActions);
            Assert.AreEqual(1, b.Room.GetRoomStats().Sent.First(s => s.Id == MissingActions.Id).Count);
        }

        [Test]
        public void ReorderedActionsAreBufferedAndApplied()
        {
            TestClient a = this.AddClient("1", "A");
            TestClient b = this.AddClient("2", "B");
            this.JoinAll();
            a.Host(TestGameConfig.TwoPlayers("1", "2", alternateTurns: false));
            this.Pump();

            this.hub.Hold(a.Connection);
            a.Act("1", 1);
            a.Act("1", 2);
            a.Act("1", 3);
            this.hub.Release(a.Connection, reverse: true);
            this.Pump();

            this.AssertInSync(a, b);
            Assert.AreEqual(3, b.Game.ActionHistory.Count);
            Assert.IsFalse(b.Sync.IsAwaitingMissingActions);
            Assert.AreEqual(SyncState.InGame, b.Sync.State);
        }

        [Test]
        public void LateJoinerReplaysHistoryAndThenGoesLive()
        {
            TestClient a = this.AddClient("1", "A");
            TestClient b = this.AddClient("2", "B");
            this.JoinAll();
            a.Host(TestGameConfig.TwoPlayers("1", "2"));
            this.Pump();
            a.Act("1", 1);
            this.Pump();
            b.Act("2", 2);
            this.Pump();

            TestClient c = this.AddClient("3", "C");
            c.Join();
            this.Pump();
            Assert.IsNull(c.Game, "late join waits for the delay");

            this.clock.Advance(1.5);
            this.Pump();

            Assert.IsNotNull(c.Game);
            this.AssertInSync(a, c);
            Assert.AreEqual(2, c.Game.ActionHistory.Count);
            Assert.AreEqual(SyncState.InGame, c.Sync.State);
            Assert.IsFalse(c.Game.IsReplaying);
            Assert.AreEqual("3", c.Session.LocalUser.UserId, "spectators still run the game");
            Assert.IsTrue(c.PerformedActions.All(p => p.Source == ActionSource.Replay));

            a.Act("1", 3);
            this.Pump();
            this.AssertInSync(a, c);
            Assert.AreEqual(ActionSource.Remote, c.PerformedActions.Last().Source);
        }

        [Test]
        public void LateJoinerWhenNoGameIsRunningStaysIdle()
        {
            TestClient a = this.AddClient("1", "A");
            TestClient b = this.AddClient("2", "B");
            this.JoinAll();

            this.clock.Advance(2);
            this.Pump();

            for (int i = 0; i < 12; i++)
            {
                this.clock.Advance(3.1);
                this.Pump();
            }

            Assert.AreEqual(SyncState.NoGame, a.Sync.State);
            Assert.AreEqual(SyncState.NoGame, b.Sync.State);
            Assert.AreEqual(0, a.SyncFailures);
            Assert.AreEqual(0, b.SyncFailures);
            Assert.IsFalse(a.Sync.IsAwaitingMissingActions);
        }

        [Test]
        public void ReconnectingClientCatchesUp()
        {
            TestClient a = this.AddClient("1", "A");
            TestClient b = this.AddClient("2", "B");
            this.JoinAll();
            a.Host(TestGameConfig.TwoPlayers("1", "2", alternateTurns: false));
            this.Pump();
            a.Act("1", 1);
            this.Pump();

            this.hub.Kill(b.Connection);
            this.Pump();
            a.Act("1", 2);
            a.Act("1", 3);
            this.Pump();
            Assert.AreEqual(1, b.Game.ActionHistory.Count);

            b.Connection.SimulateReconnect();
            this.Pump();

            this.AssertInSync(a, b);
            Assert.AreEqual(3, b.Game.ActionHistory.Count);
            Assert.AreEqual(SyncState.InGame, b.Sync.State);
            Assert.AreEqual(1, b.Session.StartCount, "catch up must not restart the game");
        }

        [Test]
        public void InvalidRemoteActionTriggersFullResync()
        {
            TestClient a = this.AddClient("1", "A");
            TestClient b = this.AddClient("2", "B");
            this.JoinAll();
            a.Host(TestGameConfig.TwoPlayers("1", "2"));
            this.Pump();
            a.Act("1", 5);
            this.Pump();

            // Forge an action that B's rules reject (player 1 acting on player 2's turn)
            var bogus = new ActionsPlayed { GameInstanceId = a.Sync.GameInstanceId, FirstActionId = 1 };
            bogus.Actions.Add(new TestAction { UserId = "1", Delta = 99 }.ToBytes());
            a.Room.SendMessage(bogus);
            this.Pump();

            Assert.AreEqual(1, b.Desyncs.Count);
            Assert.AreEqual(2, b.Session.StartCount, "resync rebuilds the game");
            Assert.AreEqual(SyncState.InGame, b.Sync.State);
            this.AssertInSync(a, b);
            Assert.AreEqual(1, b.Game.ActionHistory.Count);
            Assert.AreEqual(0, b.SyncFailures);
        }

        [Test]
        public void SilentResponderFallsBackToNextCandidate()
        {
            var syncOptions = new SyncOptions { MissingActionsTimeoutSeconds = 3, MaxResponderRounds = 3 };
            TestClient a = this.AddClient("1", "A", syncOptions);
            TestClient b = this.AddClient("2", "B", syncOptions);
            TestClient c = this.AddClient("3", "C", syncOptions);
            this.JoinAll();
            a.Host(TestGameConfig.TwoPlayers("1", "2", alternateTurns: false));
            this.Pump();
            a.Act("1", 1);
            this.Pump();

            this.hub.DropNextTo(a.Connection, b.Connection);
            a.Act("1", 2); // B misses this one, C gets it
            a.Act("1", 3);
            this.hub.Hold(a.Connection); // A goes quiet: its MissingActions reply never leaves
            this.Pump();

            Assert.IsTrue(b.Sync.IsAwaitingMissingActions);
            Assert.AreEqual(1, b.Game.ActionHistory.Count);

            this.clock.Advance(3.1);
            this.Pump();

            this.AssertInSync(c, b);
            Assert.AreEqual(3, b.Game.ActionHistory.Count);
            Assert.IsFalse(b.Sync.IsAwaitingMissingActions);
            Assert.AreEqual(0, b.SyncFailures);
        }

        [Test]
        public void NobodyAbleToAnswerRaisesSyncFailed()
        {
            var syncOptions = new SyncOptions { MissingActionsTimeoutSeconds = 3, MaxResponderRounds = 2 };
            TestClient a = this.AddClient("1", "A", syncOptions);
            TestClient b = this.AddClient("2", "B", syncOptions);
            this.JoinAll();
            a.Host(TestGameConfig.TwoPlayers("1", "2", alternateTurns: false));
            this.Pump();

            this.hub.Hold(a.Connection);
            a.Act("1", 1);
            a.Act("1", 2);
            this.hub.Release(a.Connection, reverse: true); // B sees id 1 first and asks A, who is now held
            this.hub.Hold(a.Connection);
            this.Pump();

            // The release contains action 0 too, so B actually recovers on its own from the buffered message
            Assert.AreEqual(2, b.Game.ActionHistory.Count);

            // Now create a real gap with A permanently silent
            this.hub.DropNextTo(a.Connection, b.Connection);
            this.hub.Release(a.Connection);
            a.Act("1", 3); // dropped for B
            a.Act("1", 4);
            this.hub.Hold(a.Connection);
            this.Pump();
            Assert.IsTrue(b.Sync.IsAwaitingMissingActions);

            for (int i = 0; i < 3; i++)
            {
                this.clock.Advance(3.1);
                this.Pump();
            }

            Assert.AreEqual(1, b.SyncFailures);
            Assert.IsFalse(b.Sync.IsAwaitingMissingActions);
            Assert.AreEqual(SyncState.InGame, b.Sync.State);
        }

        [Test]
        public void LargeHistoryReplyIsChunkedAndContiguous()
        {
            var syncOptions = new SyncOptions { ReplyChunkBytes = 100 };
            TestClient a = this.AddClient("1", "A", syncOptions);
            this.JoinAll();
            a.Host(TestGameConfig.TwoPlayers("1", "2", alternateTurns: false));
            this.Pump();

            for (int i = 0; i < 300; i++)
            {
                a.Act("1", i);
            }

            this.Pump();

            TestClient c = this.AddClient("3", "C", syncOptions);
            c.Join();
            this.Pump();
            this.clock.Advance(1.5);
            this.Pump();

            this.AssertInSync(a, c);
            Assert.AreEqual(300, c.Game.ActionHistory.Count);
            Assert.Greater(c.Room.GetRoomStats().Received.First(s => s.Id == ActionsPlayed.Id).Count, 10, "history arrived in chunks");
            Assert.AreEqual(SyncState.InGame, c.Sync.State);
        }

        [Test]
        public void HostingAgainStartsANewInstanceEverywhere()
        {
            TestClient a = this.AddClient("1", "A");
            TestClient b = this.AddClient("2", "B");
            this.JoinAll();
            a.Host(TestGameConfig.TwoPlayers("1", "2"));
            this.Pump();
            a.Act("1", 1);
            this.Pump();
            int firstInstance = b.Sync.GameInstanceId;

            a.Host(TestGameConfig.TwoPlayers("1", "2"));
            this.Pump();

            Assert.AreNotEqual(firstInstance, b.Sync.GameInstanceId);
            Assert.AreEqual(2, b.Session.StartCount);
            Assert.AreEqual(0, b.Game.ActionHistory.Count);
            this.AssertInSync(a, b);

            a.Act("1", 2);
            this.Pump();
            this.AssertInSync(a, b);
        }

        [Test]
        public void GamesStartedAfterLeavingTheRoomStayLocal()
        {
            TestClient a = this.AddClient("1", "A");
            TestClient b = this.AddClient("2", "B");
            this.JoinAll();
            a.Host(TestGameConfig.TwoPlayers("1", "2"));
            this.Pump();

            a.Sync.Dispose();
            a.Room.Leave();
            this.Pump();

            a.Session.StartGame(TestGameConfig.TwoPlayers("1", "2"), a.User);
            a.Act("1", 1);
            this.Pump();

            Assert.AreEqual(1, b.Session.StartCount, "the room must not see the local restart");
            Assert.AreEqual(0, b.Game.ActionHistory.Count, "hot seat actions must not leak to the room");
        }

        [Test]
        public void ReadyFlagsAreSharedAndGateTheHost()
        {
            TestClient a = this.AddClient("1", "A");
            TestClient b = this.AddClient("2", "B");
            this.JoinAll();

            Assert.IsFalse(a.Room.AreAllUsersReady);

            b.Room.LocalUser.SetReady(true);
            this.Pump();
            Assert.IsFalse(a.Room.AreAllUsersReady, "host is not ready yet");
            Assert.IsTrue(a.Room.Users.First(u => u.UserId == "2").IsReady());

            a.Room.LocalUser.SetReady(true);
            this.Pump();
            Assert.IsTrue(a.Room.AreAllUsersReady);
            Assert.IsTrue(b.Room.AreAllUsersReady);

            // Ready flags survive the newcomer handshake
            TestClient c = this.AddClient("3", "C");
            c.Join();
            this.Pump();
            Assert.IsFalse(a.Room.AreAllUsersReady);
            Assert.IsTrue(c.Room.Users.First(u => u.UserId == "1").IsReady());

            b.Room.LocalUser.SetReady(false);
            this.Pump();
            Assert.IsFalse(a.Room.Users.First(u => u.UserId == "2").IsReady());
        }

        [Test]
        public void GameStartedWhileOfflineIsAnnouncedOnConnect()
        {
            TestClient a = this.AddClient("1", "A");
            TestClient b = this.AddClient("2", "B");
            a.Join();
            b.Join();
            this.Pump();

            this.hub.Kill(a.Connection);
            this.Pump();

            a.Host(TestGameConfig.TwoPlayers("1", "2", alternateTurns: false));
            a.Act("1", 1);

            a.Connection.SimulateReconnect();
            this.Pump();

            Assert.IsNotNull(b.Game);
            this.AssertInSync(a, b);
            Assert.AreEqual(1, b.Game.ActionHistory.Count);
            Assert.AreEqual(SyncState.InGame, b.Sync.State);
        }

        private TestClient AddClient(string userId, string name, SyncOptions syncOptions = null)
        {
            var client = new TestClient(this.hub, this.clock, userId, name, null, syncOptions);
            this.clients.Add(client);
            return client;
        }

        private void JoinAll()
        {
            foreach (TestClient client in this.clients)
            {
                client.Join();
            }

            this.Pump();
        }

        private void Pump() => TestPump.Run(this.hub, this.clients);

        private void AssertInSync(TestClient expected, TestClient actual)
        {
            Assert.IsNotNull(expected.Game, expected.User.DisplayName + " has no game");
            Assert.IsNotNull(actual.Game, actual.User.DisplayName + " has no game");
            Assert.AreEqual(expected.Sync.GameInstanceId, actual.Sync.GameInstanceId, "game instance");
            Assert.AreEqual(expected.Game.Config.Seed, actual.Game.Config.Seed, "seed");
            Assert.AreEqual(expected.Game.ActionHistory.Count, actual.Game.ActionHistory.Count, "action count");
            Assert.AreEqual(expected.Game.Score, actual.Game.Score, "score");

            for (int i = 0; i < expected.Game.ActionHistory.Count; i++)
            {
                var e = (TestAction)expected.Game.ActionHistory[i];
                var r = (TestAction)actual.Game.ActionHistory[i];
                Assert.AreEqual(e.UserId, r.UserId, $"action {i} user");
                Assert.AreEqual(e.Delta, r.Delta, $"action {i} delta");
            }
        }
    }
}
