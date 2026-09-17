//-----------------------------------------------------------------------
// <copyright file="BoardGameNetworkManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using OGT.BoardGame.Networking;
    using OGT.Networking;

    /// <summary>
    /// Puts <see cref="BoardGameManager"/> in a networked room. Lives next to it on the bootloader.
    /// <para>
    /// Host: build a <see cref="MessageRoom"/> for a fresh room code, <see cref="JoinRoom"/>, wait until <see cref="AreAllUsersReady"/>, then call
    /// <c>BoardGameManager.StartGame(config, LocalUser)</c> with the players' UserIds taken from <see cref="Users"/>.
    /// Client: <see cref="JoinRoom"/>, <see cref="SetReady"/>, and the game starts by itself when the host's
    /// start message arrives (listen to <c>BoardGameManager.OnGameCreated</c> as usual).
    /// </para>
    /// The app builds the <see cref="MessageRoom"/> itself, typically over a <see cref="WebSocketMessageConnection"/> whose url comes from the GetWebsocketRoomConnectionString cloud
    /// code function.
    /// </summary>
    public class BoardGameNetworkManager : Manager
    {
        private static readonly OGTLogger Logger = OGTLogger.Networking;

        private BoardGameManager boardGameManager;
        private BoardGameNetworkSync sync;
        private MessageRoom room;

        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action OnUsersChanged;
        public event Action<string> OnDesyncDetected;
        public event Action OnSyncFailed;
        public IReadOnlyList<UserInfo> Users => this.room != null ? this.room.Users : Array.Empty<UserInfo>();
        public bool AreAllUsersReady => this.room != null && this.room.AreAllUsersReady;

        /// <summary>
        /// Joins a room over the given connection. Any previous room is left first.
        /// </summary>
        public void JoinRoom(MessageRoom messageRoom)
        {
            if (messageRoom == null)
            {
                throw new ArgumentNullException(nameof(messageRoom));
            }

            if (this.boardGameManager == null)
            {
                throw new InvalidOperationException("BoardGameNetworkManager: cannot join a room without a BoardGameManager on the bootloader");
            }

            this.LeaveRoom();

            this.room = messageRoom;
            this.room.OnConnected += this.ForwardConnected;
            this.room.OnDisconnected += this.ForwardDisconnected;
            this.room.OnUsersChanged += this.ForwardUsersChanged;

            this.sync = new BoardGameNetworkSync(this.room, this.boardGameManager.StartGame);
            this.sync.OnDesyncDetected += this.ForwardDesyncDetected;
            this.sync.OnSyncFailed += this.ForwardSyncFailed;

            this.room.Connect();
        }

        public void LeaveRoom()
        {
            if (this.room == null)
            {
                return;
            }

            this.sync.OnDesyncDetected -= this.ForwardDesyncDetected;
            this.sync.OnSyncFailed -= this.ForwardSyncFailed;
            this.sync.Dispose();
            this.sync = null;

            this.room.OnConnected -= this.ForwardConnected;
            this.room.OnDisconnected -= this.ForwardDisconnected;
            this.room.OnUsersChanged -= this.ForwardUsersChanged;
            this.room.Leave();
            this.room.Dispose();
            this.room = null;

            this.OnUsersChanged?.Invoke();
        }

        /// <summary>
        /// Marks the local user as ready (or not) and tells the room. The host waits for <see cref="AreAllUsersReady"/>.
        /// </summary>
        public void SetReady(bool isReady)
        {
            if (this.room == null)
            {
                Logger.LogError("BoardGameNetworkManager: join a room before setting ready");
                return;
            }

            this.room.LocalUser.SetReady(isReady);
        }

        public RoomStats GetRoomStats() => this.room?.GetRoomStats();

        public override void OnManagerDestroyed()
        {
            this.LeaveRoom();

            if (this.boardGameManager != null)
            {
                this.boardGameManager.OnGameCreated -= this.ForwardGameCreated;
                this.boardGameManager.OnGameStarted -= this.ForwardGameStarted;
            }

            base.OnManagerDestroyed();
        }

        protected override Task InitializeManager(Bootloader bootloader)
        {
            this.boardGameManager = bootloader.FindManager<BoardGameManager>();

            if (this.boardGameManager == null)
            {
                Logger.LogError("BoardGameNetworkManager: no BoardGameManager found on the bootloader");
            }
            else
            {
                this.boardGameManager.OnGameCreated += this.ForwardGameCreated;
                this.boardGameManager.OnGameStarted += this.ForwardGameStarted;
            }

            return Task.CompletedTask;
        }

        private void Update()
        {
            this.room?.Update();
            this.sync?.Update();
        }

        private void ForwardGameCreated(BoardGame.BoardGame game) => this.sync?.NotifyGameCreated(game);

        private void ForwardGameStarted(BoardGame.BoardGame game) => this.sync?.NotifyGameStarted(game);

        private void ForwardConnected() => this.OnConnected?.Invoke();

        private void ForwardDisconnected() => this.OnDisconnected?.Invoke();

        private void ForwardUsersChanged() => this.OnUsersChanged?.Invoke();

        private void ForwardDesyncDetected(string reason) => this.OnDesyncDetected?.Invoke(reason);

        private void ForwardSyncFailed() => this.OnSyncFailed?.Invoke();
    }

    public static class UserInfoExtensions
    {
        public const string ReadyKey = "ready";

        public static bool IsReady(this UserInfo user)
        {
            return user?.CustomData != null && user.CustomData.TryGetValue(ReadyKey, out string value) && value == "1";
        }

        public static void SetReady(this UserInfo user, bool isReady)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user.CustomData == null)
            {
                user.CustomData = new Dictionary<string, string>();
            }

            if (isReady)
            {
                user.CustomData[ReadyKey] = "1";
            }
            else
            {
                user.CustomData.Remove(ReadyKey);
            }

            user.NotifyInfoChanged();
        }
    }
}
