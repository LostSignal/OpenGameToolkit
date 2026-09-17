//-----------------------------------------------------------------------
// <copyright file="TestBoardGame.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BoardGame.Networking.Tests
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using OGT.Networking;

    public sealed class TestAction : IBoardGameAction
    {
        public string UserId { get; set; }

        public int Delta { get; set; }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(this.UserId);
            writer.Write(this.Delta);
        }

        public void Deserialize(NetworkReader reader)
        {
            this.UserId = reader.ReadString();
            this.Delta = reader.ReadInt32();
        }

        public byte[] ToBytes()
        {
            var writer = new NetworkWriter();
            this.Serialize(writer);
            return writer.ToArray();
        }
    }

    public sealed class TestGameConfig : BoardGameConfig
    {
        public int Bonus { get; set; } = 1;

        /// <summary>Gets or sets a value indicating whether players must alternate turns. Off lets one player act repeatedly.</summary>
        public bool AlternateTurns { get; set; } = true;

        /// <summary>A list with initializer defaults, to prove the serializer replaces instead of appends.</summary>
        public List<string> Tags { get; set; } = new List<string> { "default" };

        public TestGameConfig()
        {
            this.Players = new List<UserInfo>();
            this.Characters = new List<BoardGameCharacter>();
            this.CharacterStats = new Dictionary<string, Dictionary<string, object>>();
        }

        public override Type GetGameBoardType() => typeof(TestBoardGame);

        public static TestGameConfig TwoPlayers(string userA, string userB, bool alternateTurns = true)
        {
            return new TestGameConfig
            {
                AlternateTurns = alternateTurns,
                Players =
                {
                    new UserInfo { UserId = userA, DisplayName = "A" },
                    new UserInfo { UserId = userB, DisplayName = "B" },
                },
            };
        }
    }

    /// <summary>
    /// Tiny deterministic game: each action adds Delta plus a seeded random number to the score, so seed and action
    /// order both show up in the final score.
    /// </summary>
    public sealed class TestBoardGame : BoardGame<TestAction>
    {
        [JsonProperty] private int score;
        [JsonProperty] private int turnIndex;

        [JsonIgnore] public int Score => this.score;

        [JsonIgnore] public TestGameConfig TestConfig => (TestGameConfig)this.Config;

        [JsonIgnore] public string ActiveUserId => this.Players[this.turnIndex % this.Players.Count].UserId;

        public override void StartBoardGame(BoardGameConfig config)
        {
            base.StartBoardGame(config);
            this.score = 0;
            this.turnIndex = 0;
            this.OnReaction?.Invoke(new NewGame { Config = config });
        }

        public override bool CanPerformAction(TestAction action)
        {
            if (this.TestConfig.AlternateTurns)
            {
                return action.UserId == this.ActiveUserId;
            }

            foreach (UserInfo player in this.Players)
            {
                if (player.UserId == action.UserId)
                {
                    return true;
                }
            }

            return false;
        }

        protected override void OnPerformAction(TestAction action)
        {
            this.score += action.Delta + this.TestConfig.Bonus + this.GetRandomInteger(0, 1000);
            this.turnIndex++;
        }
    }

    /// <summary>
    /// Mirrors BoardGameManager.StartGame without Unity.
    /// </summary>
    public sealed class FakeBoardGameSession
    {
        public event Action<BoardGame> OnGameCreated;

        public event Action<BoardGame> OnGameStarted;

        public BoardGame CurrentGame { get; private set; }

        public UserInfo LocalUser { get; private set; }

        public int StartCount { get; private set; }

        public void StartGame(BoardGameConfig config, UserInfo localUser)
        {
            this.StartCount++;
            this.LocalUser = localUser;
            this.CurrentGame = (BoardGame)Activator.CreateInstance(config.GetGameBoardType());
            this.OnGameCreated?.Invoke(this.CurrentGame);
            this.CurrentGame.StartBoardGame(config);
            this.OnGameStarted?.Invoke(this.CurrentGame);
        }
    }

    /// <summary>
    /// One simulated client: connection + room + session + sync.
    /// </summary>
    public sealed class TestClient : IDisposable
    {
        public TestClient(FakeMessageHub hub, FakeClock clock, string userId, string name, MessageRoomOptions roomOptions = null, SyncOptions syncOptions = null)
        {
            this.User = new UserInfo { UserId = userId, DisplayName = name };
            this.Connection = hub.CreateConnection(name);
            this.Room = new MessageRoom(this.Connection, this.User, clock.Read, roomOptions ?? new MessageRoomOptions { JoinReplyJitterSeconds = 0 });
            this.Session = new FakeBoardGameSession();
            this.Sync = new BoardGameNetworkSync(this.Room, this.Session.StartGame, clock.Read, syncOptions);
            this.Session.OnGameCreated += this.Sync.NotifyGameCreated;
            this.Session.OnGameStarted += this.Sync.NotifyGameStarted;

            this.Session.OnGameCreated += game => game.OnActionPerformed += performed => this.PerformedActions.Add(performed);
            this.Sync.OnDesyncDetected += reason => this.Desyncs.Add(reason);
            this.Sync.OnSyncFailed += () => this.SyncFailures++;
        }

        public UserInfo User { get; }

        public FakeMessageConnection Connection { get; }

        public MessageRoom Room { get; }

        public FakeBoardGameSession Session { get; }

        public BoardGameNetworkSync Sync { get; }

        public TestBoardGame Game => this.Session.CurrentGame as TestBoardGame;

        public List<PerformedAction> PerformedActions { get; } = new List<PerformedAction>();

        public List<string> Desyncs { get; } = new List<string>();

        public int SyncFailures { get; private set; }

        public void Join(string roomCode = "ROOM")
        {
            this.Connection.RoomCode = roomCode;
            this.Room.Connect();
        }

        /// <summary>Starts a game the way the host does it: straight through the session, which the sync picks up and broadcasts.</summary>
        public void Host(BoardGameConfig config) => this.Session.StartGame(config, this.User);

        public void Update()
        {
            this.Room.Update();
            this.Sync.Update();
        }

        public void Act(string userId, int delta)
        {
            this.Game.PerformAction(new TestAction { UserId = userId, Delta = delta });
        }

        public void Dispose()
        {
            this.Sync.Dispose();
            this.Room.Dispose();
        }
    }

    public static class TestPump
    {
        /// <summary>
        /// Updates every client and delivers hub traffic until nothing is pending. Returns the number of rounds used.
        /// </summary>
        public static int Run(FakeMessageHub hub, IReadOnlyList<TestClient> clients, int maxRounds = 200)
        {
            for (int round = 1; round <= maxRounds; round++)
            {
                bool anyWork = false;

                foreach (TestClient client in clients)
                {
                    anyWork |= client.Connection.HasPendingEvents;
                    client.Update();
                }

                if (hub.InFlightCount > 0)
                {
                    anyWork = true;
                    hub.Deliver();
                }

                foreach (TestClient client in clients)
                {
                    anyWork |= client.Connection.HasPendingEvents;
                }

                if (anyWork == false)
                {
                    return round;
                }
            }

            throw new InvalidOperationException($"Traffic did not settle within {maxRounds} rounds");
        }
    }
}
