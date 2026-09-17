//-----------------------------------------------------------------------
// <copyright file="MessageRoom.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BoardGame.Networking
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using OGT.Networking;

    public sealed class MessageRoomOptions
    {
        /// <summary>Gets or sets how often a <see cref="Heartbeat"/> is broadcast while connected.</summary>
        public double HeartbeatSeconds { get; set; } = 5;

        /// <summary>Gets or sets how long a user may stay silent before it is considered gone.</summary>
        public double UserTimeoutSeconds { get; set; } = 15;

        /// <summary>Gets or sets the maximum random delay before replying to a <see cref="UserJoined"/>, to avoid reply bursts. Zero replies immediately.</summary>
        public double JoinReplyJitterSeconds { get; set; } = 0.25;
    }

    /// <summary>
    /// Wraps an <see cref="IMessageConnection"/> with typed messages, a user roster, liveness tracking and statistics.
    /// Every message goes to everyone else in the room. All callbacks are raised from <see cref="Update"/>.
    /// </summary>
    public sealed class MessageRoom : IDisposable
    {
        private static readonly OGTLogger Logger = OGTLogger.Networking;

        private readonly IMessageConnection connection;
        private readonly UserInfo localUser;
        private readonly Func<double> clock;
        private readonly MessageRoomOptions options;
        private readonly MessageCollection messages = new MessageCollection();
        private readonly HashSet<short> registeredIds = new HashSet<short>();
        private readonly HashSet<short> unknownIdsLogged = new HashSet<short>();
        private readonly NetworkWriter writer = new NetworkWriter();
        private readonly List<UserInfo> users = new List<UserInfo>();
        private readonly Dictionary<string, double> lastSeenByUserId = new Dictionary<string, double>();
        private readonly Dictionary<short, MessageTypeStats> sentStats = new Dictionary<short, MessageTypeStats>();
        private readonly Dictionary<short, MessageTypeStats> receivedStats = new Dictionary<short, MessageTypeStats>();
        private readonly List<string> scratchUserIds = new List<string>();
        private readonly Random random = new Random();

        private double nextHeartbeatTime;
        private double rosterReplyDueTime = -1;
        private bool isJoined;
        private bool isDisposed;

        public UserInfo ConnectingUser => this.localUser;

        public MessageRoom(IMessageConnection connection, UserInfo localUser, Func<double> clockSeconds = null, MessageRoomOptions options = null, string roomCode = null)
        {
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
            this.localUser = localUser ?? throw new ArgumentNullException(nameof(localUser));
            this.clock = clockSeconds ?? CreateStopwatchClock();
            this.options = options ?? new MessageRoomOptions();
            this.RoomCode = roomCode;

            this.RegisterMessageType<UserJoined>();
            this.RegisterMessageType<UserLeft>();
            this.RegisterMessageType<UserInfoUpdated>();
            this.RegisterMessageType<Heartbeat>();

            this.users.Add(this.localUser);

            this.localUser.OnInfoChanged += this.HandleLocalUserInfoChanged;

            this.connection.OnConnected += this.HandleConnected;
            this.connection.OnDisconnected += this.HandleDisconnected;
            this.connection.OnMessageReceived += this.HandleMessageReceived;
        }

        public event Action OnConnected;

        public event Action OnDisconnected;

        public event Action OnUsersChanged;

        public event Action<UserInfo> OnUserJoined;

        public event Action<UserInfo> OnUserLeft;

        /// <summary>
        /// Raised for every non built-in message. The message instance is recycled after all handlers return, so copy anything you keep.
        /// </summary>
        public event Action<RoomMessage> OnMessageReceived;

        public UserInfo LocalUser => this.localUser;

        public IMessageConnection Connection => this.connection;

        public string RoomCode { get; private set; }

        /// <summary>Gets a value indicating whether <see cref="Join"/> has been called and <see cref="Leave"/> has not.</summary>
        public bool IsJoined => this.isJoined;

        /// <summary>Gets a value indicating whether the underlying connection is currently established.</summary>
        public bool IsConnected { get; private set; }

        /// <summary>Gets every known user in the room including the local user, sorted by UserId.</summary>
        public IReadOnlyList<UserInfo> Users => this.users;

        public void RegisterMessageType<T>()
            where T : RoomMessage, new()
        {
            short id = new T().GetId();

            if (this.registeredIds.Contains(id))
            {
                Logger.LogError($"MessageRoom: message type {typeof(T).Name} has duplicate id {id}");
                return;
            }

            this.registeredIds.Add(id);
            this.messages.RegisterMessage<T>();
        }

        public bool TryGetUser(string userId, out UserInfo user)
        {
            for (int i = 0; i < this.users.Count; i++)
            {
                if (this.users[i].UserId == userId)
                {
                    user = this.users[i];
                    return true;
                }
            }

            user = null;
            return false;
        }

        public void Connect()
        {
            this.ThrowIfDisposed();

            if (this.isJoined)
            {
                this.Leave();
            }

            this.isJoined = true;
            this.connection.Connect();
        }

        public void Leave()
        {
            if (this.isJoined == false)
            {
                return;
            }

            bool wasConnected = this.IsConnected;

            if (wasConnected)
            {
                this.SendMessage(new UserLeft { UserId = this.localUser.UserId });
            }

            this.isJoined = false;
            this.IsConnected = false;
            this.rosterReplyDueTime = -1;
            this.connection.Disconnect();
            this.ClearRemoteUsers();

            if (wasConnected)
            {
                this.OnDisconnected?.Invoke();
            }
        }

        public void SendMessage(RoomMessage message)
        {
            this.ThrowIfDisposed();

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (this.isJoined == false)
            {
                Logger.LogWarning($"MessageRoom: ignoring {message.GetTypeName()} because the room has not been joined");
                return;
            }

            message.SenderUserId = this.localUser.UserId;

            this.writer.SeekZero();
            message.Serialize(this.writer);
            byte[] data = this.writer.ToArray();

            this.RecordStats(this.sentStats, message.GetId(), message.GetTypeName(), data.Length);
            this.connection.Send(data, 0, data.Length);
        }

        /// <summary>Gets a value indicating whether the local user has called <see cref="SetReady"/> with true.</summary>
        public bool IsLocalUserReady => this.localUser.IsReady();

        /// <summary>Gets a value indicating whether every user in the room, including the local user, is ready.</summary>
        public bool AreAllUsersReady
        {
            get
            {
                for (int i = 0; i < this.users.Count; i++)
                {
                    if (this.users[i].IsReady() == false)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Edits the local user's info and broadcasts the change to the room.
        /// </summary>
        public void UpdateLocalUserInfo(Action<UserInfo> edit)
        {
            edit?.Invoke(this.localUser);

            if (this.IsConnected)
            {
                this.SendMessage(new UserInfoUpdated { User = this.localUser });
            }

            this.OnUsersChanged?.Invoke();
        }

        public void Update()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.connection.Update();

            if (this.IsConnected == false)
            {
                return;
            }

            double now = this.clock();

            if (this.rosterReplyDueTime >= 0 && now >= this.rosterReplyDueTime)
            {
                this.rosterReplyDueTime = -1;
                this.SendMessage(new UserInfoUpdated { User = this.localUser });
            }

            if (now >= this.nextHeartbeatTime)
            {
                this.nextHeartbeatTime = now + this.options.HeartbeatSeconds;
                this.SendMessage(new Heartbeat());
            }

            this.scratchUserIds.Clear();

            foreach (var pair in this.lastSeenByUserId)
            {
                if (now - pair.Value > this.options.UserTimeoutSeconds)
                {
                    this.scratchUserIds.Add(pair.Key);
                }
            }

            for (int i = 0; i < this.scratchUserIds.Count; i++)
            {
                this.RemoveUser(this.scratchUserIds[i], "timed out");
            }
        }

        public RoomStats GetRoomStats()
        {
            var stats = new RoomStats();

            foreach (var typeStats in this.sentStats.Values)
            {
                stats.Sent.Add(typeStats.Copy());
                stats.TotalMessagesSent += typeStats.Count;
                stats.TotalBytesSent += typeStats.TotalBytes;
            }

            foreach (var typeStats in this.receivedStats.Values)
            {
                stats.Received.Add(typeStats.Copy());
                stats.TotalMessagesReceived += typeStats.Count;
                stats.TotalBytesReceived += typeStats.TotalBytes;
            }

            stats.Sent.Sort((a, b) => a.Id.CompareTo(b.Id));
            stats.Received.Sort((a, b) => a.Id.CompareTo(b.Id));

            return stats;
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            if (this.isJoined)
            {
                this.Leave();
            }

            this.isDisposed = true;

            this.connection.OnConnected -= this.HandleConnected;
            this.connection.OnDisconnected -= this.HandleDisconnected;
            this.connection.OnMessageReceived -= this.HandleMessageReceived;
            this.localUser.OnInfoChanged -= this.HandleLocalUserInfoChanged;
            this.connection.Dispose();
        }

        private void HandleLocalUserInfoChanged()
        {
            if (this.IsConnected)
            {
                this.SendMessage(new UserInfoUpdated { User = this.localUser });
            }

            this.OnUsersChanged?.Invoke();
        }

        private static Func<double> CreateStopwatchClock()
        {
            var stopwatch = Stopwatch.StartNew();
            return () => stopwatch.Elapsed.TotalSeconds;
        }

        private static byte[] ToExactArray(ArraySegment<byte> segment)
        {
            if (segment.Array != null && segment.Offset == 0 && segment.Count == segment.Array.Length)
            {
                return segment.Array;
            }

            byte[] data = new byte[segment.Count];
            Buffer.BlockCopy(segment.Array, segment.Offset, data, 0, segment.Count);
            return data;
        }

        private void HandleConnected()
        {
            if (this.isJoined == false)
            {
                return;
            }

            this.IsConnected = true;

            double now = this.clock();
            this.nextHeartbeatTime = now + this.options.HeartbeatSeconds;

            // Nobody should time out right after a reconnect
            this.scratchUserIds.Clear();
            this.scratchUserIds.AddRange(this.lastSeenByUserId.Keys);

            for (int i = 0; i < this.scratchUserIds.Count; i++)
            {
                this.lastSeenByUserId[this.scratchUserIds[i]] = now;
            }

            this.SendMessage(new UserJoined { User = this.localUser });
            this.OnConnected?.Invoke();
        }

        private void HandleDisconnected(string reason)
        {
            if (this.IsConnected == false)
            {
                return;
            }

            this.IsConnected = false;
            this.rosterReplyDueTime = -1;
            Logger.Log($"MessageRoom: disconnected from room {this.RoomCode}: {reason}");
            this.OnDisconnected?.Invoke();
        }

        private void HandleMessageReceived(ArraySegment<byte> segment)
        {
            if (this.isJoined == false || segment.Array == null || segment.Count < sizeof(short))
            {
                return;
            }

            byte[] data = ToExactArray(segment);
            short id = MessageCollection.GetMessageId(data);

            if (this.registeredIds.Contains(id) == false)
            {
                if (this.unknownIdsLogged.Add(id))
                {
                    Logger.LogWarning($"MessageRoom: received unregistered message id {id}, ignoring (logged once per id)");
                }

                return;
            }

            RoomMessage message;

            try
            {
                message = this.messages.GetMessage(data) as RoomMessage;
            }
            catch (Exception ex)
            {
                Logger.LogError($"MessageRoom: failed to deserialize message id {id}: {ex}");
                return;
            }

            if (message == null)
            {
                return;
            }

            try
            {
                this.RecordStats(this.receivedStats, id, message.GetTypeName(), data.Length);

                if (message.SenderUserId == this.localUser.UserId)
                {
                    // Our own message echoed back by the transport
                    return;
                }

                this.TouchUser(message);

                if (this.HandleBuiltInMessage(message) == false)
                {
                    this.OnMessageReceived?.Invoke(message);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"MessageRoom: exception while handling {message.GetTypeName()}: {ex}");
            }
            finally
            {
                this.messages.RecycleMessage(message);
            }
        }

        private void TouchUser(RoomMessage message)
        {
            if (message is UserLeft)
            {
                return;
            }

            if (this.TryGetUser(message.SenderUserId, out _) == false)
            {
                // A user we never saw a UserJoined/UserInfoUpdated from (we probably joined mid stream). Add a placeholder so
                // the roster and timeouts stay consistent; its info gets filled in by its next UserInfoUpdated.
                this.UpsertUser(new UserInfo { UserId = message.SenderUserId, DisplayName = "Unknown" });
            }

            this.lastSeenByUserId[message.SenderUserId] = this.clock();
        }

        private bool HandleBuiltInMessage(RoomMessage message)
        {
            switch (message)
            {
                case UserJoined joined:
                    this.UpsertUser(joined.User);
                    this.ScheduleRosterReply();
                    return true;

                case UserInfoUpdated updated:
                    this.UpsertUser(updated.User);
                    return true;

                case UserLeft left:
                    this.RemoveUser(left.UserId, "left");
                    return true;

                case Heartbeat _:
                    return true;

                default:
                    return false;
            }
        }

        private void ScheduleRosterReply()
        {
            if (this.options.JoinReplyJitterSeconds <= 0)
            {
                this.SendMessage(new UserInfoUpdated { User = this.localUser });
                return;
            }

            double due = this.clock() + (this.random.NextDouble() * this.options.JoinReplyJitterSeconds);

            if (this.rosterReplyDueTime < 0 || due < this.rosterReplyDueTime)
            {
                this.rosterReplyDueTime = due;
            }
        }

        private void UpsertUser(UserInfo source)
        {
            if (source == null || source.UserId == this.localUser.UserId)
            {
                return;
            }

            if (this.TryGetUser(source.UserId, out UserInfo existing))
            {
                existing.CopyFrom(source);
                this.OnUsersChanged?.Invoke();
                return;
            }

            var user = new UserInfo(source);
            this.users.Add(user);
            this.users.Sort((a, b) => a.UserId.CompareTo(b.UserId));
            this.lastSeenByUserId[user.UserId] = this.clock();

            this.OnUserJoined?.Invoke(user);
            this.OnUsersChanged?.Invoke();
        }

        private void RemoveUser(string userId, string reason)
        {
            if (userId == this.localUser.UserId)
            {
                return;
            }

            this.lastSeenByUserId.Remove(userId);

            if (this.TryGetUser(userId, out UserInfo user) == false)
            {
                return;
            }

            this.users.Remove(user);
            Logger.Log($"MessageRoom: user {user.DisplayName} ({user.UserId}) {reason}");

            this.OnUserLeft?.Invoke(user);
            this.OnUsersChanged?.Invoke();
        }

        private void ClearRemoteUsers()
        {
            this.lastSeenByUserId.Clear();

            if (this.users.Count <= 1)
            {
                return;
            }

            this.users.Clear();
            this.users.Add(this.localUser);
            this.OnUsersChanged?.Invoke();
        }

        private void RecordStats(Dictionary<short, MessageTypeStats> table, short id, string name, int bytes)
        {
            if (table.TryGetValue(id, out MessageTypeStats stats) == false)
            {
                stats = new MessageTypeStats { Id = id, Name = name };
                table.Add(id, stats);
            }

            stats.Record(bytes);
        }

        private void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(MessageRoom));
            }
        }
    }
}
