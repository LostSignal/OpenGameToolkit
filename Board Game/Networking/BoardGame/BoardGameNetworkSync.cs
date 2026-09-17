//-----------------------------------------------------------------------
// <copyright file="BoardGameNetworkSync.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BoardGame.Networking
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using OGT.Networking;

    public enum SyncState
    {
        /// <summary>No networked game is known.</summary>
        NoGame,

        /// <summary>A game is running and every known action has been applied.</summary>
        InGame,

        /// <summary>A game is running but a gap in the action sequence is being filled.</summary>
        AwaitingMissingActions,

        /// <summary>The game is being rebuilt from the config and full action history (late join or desync recovery).</summary>
        Resyncing,
    }

    public sealed class SyncOptions
    {
        /// <summary>Gets or sets how long to wait for a targeted user to answer a <see cref="MissingActions"/> before asking the next one.</summary>
        public double MissingActionsTimeoutSeconds { get; set; } = 3;

        /// <summary>Gets or sets how many times every candidate is asked before giving up.</summary>
        public int MaxResponderRounds { get; set; } = 3;

        /// <summary>Gets or sets the approximate payload size at which an action history reply is split into multiple messages.</summary>
        public int ReplyChunkBytes { get; set; } = 24 * 1024;

        /// <summary>Gets or sets how long after connecting a client waits before asking the room for a game in progress.</summary>
        public double LateJoinDelaySeconds { get; set; } = 1;
    }

    /// <summary>
    /// Keeps every client in a <see cref="MessageRoom"/> running the same deterministic board game. Any game started through
    /// <see cref="OGT.BoardGameManager"/> by the local user is broadcast to the room (the local user becomes the host), games
    /// started by other users are started locally on their behalf, each client broadcasts the actions it performs, and everyone
    /// applies remote actions in id order. Gaps are filled by asking a single deterministic responder for the missing range.
    /// Pure C#; call <see cref="Update"/> every frame.
    /// </summary>
    public sealed class BoardGameNetworkSync : IDisposable
    {
        private static readonly OGTLogger Logger = OGTLogger.Networking;
        private static readonly Random InstanceRandom = new Random();

        private readonly MessageRoom room;
        private readonly Action<BoardGameConfig, UserInfo> startGame;
        private readonly Func<double> clock;
        private readonly SyncOptions options;
        private readonly NetworkWriter actionWriter = new();
        private readonly NetworkReader actionReader = new();
        private readonly SortedDictionary<int, byte[]> pendingActionsById = new();
        private readonly List<string> responderCandidates = new();
        private readonly List<int> scratchIds = new();

        private BoardGame game;
        private int gameInstanceId;
        private string hostUserId;
        private string cachedConfigJson;
        private int highestSeenActionId = -1;
        private int resyncTargetActionId = -1;
        private bool isStartingRemoteGame;
        private bool isDisposed;

        // Outstanding MissingActions request
        private bool isAwaitingMissingActions;
        private int missingLastKnownActionId;
        private bool missingIncludesStart;
        private int candidateIndex;
        private int responderRounds;
        private double missingRequestTime;
        private int requestSerial;

        // Request that could not be sent yet (no other users, or waiting for the late join delay)
        private bool hasDeferredRequest;
        private int deferredLastKnownActionId;
        private bool deferredIncludesStart;
        private double deferredRequestDueTime;

        // Host started a game while offline; announce it once we are connected
        private bool hasPendingStartBroadcast;

        public BoardGameNetworkSync(MessageRoom room, Action<BoardGameConfig, UserInfo> startGame, Func<double> clockSeconds = null, SyncOptions options = null)
        {
            this.room = room ?? throw new ArgumentNullException(nameof(room));
            this.startGame = startGame ?? throw new ArgumentNullException(nameof(startGame));
            this.clock = clockSeconds ?? CreateStopwatchClock();
            this.options = options ?? new SyncOptions();

            this.room.RegisterMessageType<BoardGameStart>();
            this.room.RegisterMessageType<ActionsPlayed>();
            this.room.RegisterMessageType<MissingActions>();

            this.room.OnConnected += this.HandleRoomConnected;
            this.room.OnDisconnected += this.HandleRoomDisconnected;
            this.room.OnUsersChanged += this.HandleUsersChanged;
            this.room.OnMessageReceived += this.HandleMessage;
        }

        /// <summary>Raised when a remote action could not be applied. A full resync is requested automatically.</summary>
        public event Action<string> OnDesyncDetected;

        /// <summary>Raised when nobody in the room could supply the missing actions.</summary>
        public event Action OnSyncFailed;

        public event Action<SyncState> OnStateChanged;

        public SyncState State { get; private set; } = SyncState.NoGame;

        public int GameInstanceId => this.gameInstanceId;

        public string HostUserId => this.hostUserId;

        public bool IsHost => this.gameInstanceId != 0 && this.hostUserId == this.room.LocalUser.UserId;

        /// <summary>Gets the networked game currently being synchronized, or null.</summary>
        public BoardGame Game => this.game;

        public string CachedConfigJson => this.cachedConfigJson;

        public bool IsAwaitingMissingActions => this.isAwaitingMissingActions;

        /// <summary>
        /// Throws away the local game state and rebuilds it from the room (config plus full action history).
        /// </summary>
        public void RequestFullResync()
        {
            this.ThrowIfDisposed();

            if (this.gameInstanceId == 0)
            {
                return;
            }

            this.pendingActionsById.Clear();
            this.resyncTargetActionId = int.MaxValue;
            this.SetState(SyncState.Resyncing);
            this.BeginMissingRequest(-1, includeStart: true);
        }

        public void Update()
        {
            if (this.isDisposed)
            {
                return;
            }

            double now = this.clock();

            if (this.hasDeferredRequest && now >= this.deferredRequestDueTime && this.room.IsConnected && this.room.Users.Count > 1)
            {
                this.hasDeferredRequest = false;
                this.BeginMissingRequest(this.deferredLastKnownActionId, this.deferredIncludesStart);
            }

            if (this.isAwaitingMissingActions && now - this.missingRequestTime >= this.options.MissingActionsTimeoutSeconds)
            {
                this.AdvanceResponder();
            }
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;
            this.DetachGame();

            this.room.OnConnected -= this.HandleRoomConnected;
            this.room.OnDisconnected -= this.HandleRoomDisconnected;
            this.room.OnUsersChanged -= this.HandleUsersChanged;
            this.room.OnMessageReceived -= this.HandleMessage;
        }

        private static Func<double> CreateStopwatchClock()
        {
            var stopwatch = Stopwatch.StartNew();
            return () => stopwatch.Elapsed.TotalSeconds;
        }

        // ---- Room events -------------------------------------------------------------------------------------------

        private void HandleRoomConnected()
        {
            if (this.game == null || this.gameInstanceId == 0)
            {
                // Late join: give the roster a moment to fill in, then ask for a game in progress
                this.DeferRequest(-1, includeStart: true, this.clock() + this.options.LateJoinDelaySeconds);
            }
            else if (this.hasPendingStartBroadcast)
            {
                // We hosted a game while offline; the room has never heard of it
                this.hasPendingStartBroadcast = false;
                this.BroadcastStart();
            }
            else
            {
                // Reconnected mid game: catch up on anything we missed
                this.DeferRequest(this.game.LastActionId, includeStart: false, this.clock());
            }
        }

        private void BroadcastStart()
        {
            this.room.SendMessage(new BoardGameStart
            {
                GameInstanceId = this.gameInstanceId,
                HostUserId = this.hostUserId,
                LastActionId = this.game != null ? this.game.LastActionId : -1,
                ConfigJson = this.cachedConfigJson,
            });
        }

        private void HandleRoomDisconnected()
        {
            this.CancelRequests();
        }

        private void HandleUsersChanged()
        {
            if (this.isAwaitingMissingActions && this.candidateIndex < this.responderCandidates.Count)
            {
                string target = this.responderCandidates[this.candidateIndex];

                if (this.room.TryGetUser(target, out _) == false)
                {
                    this.AdvanceResponder();
                }
            }
        }

        // ---- Game events -------------------------------------------------------------------------------------------

        /// <summary>Call from BoardGameManager.OnGameCreated (before StartBoardGame runs) for every game, local or remote.</summary>
        public void NotifyGameCreated(BoardGame created)
        {
            this.DetachGame();
            this.game = created;
            this.game.OnActionPerformed += this.HandleActionPerformed;
        }

        /// <summary>Call from BoardGameManager.OnGameStarted (after StartBoardGame ran). A game the local user started becomes the hosted game.</summary>
        public void NotifyGameStarted(BoardGame started)
        {
            if (started != this.game)
            {
                return;
            }

            if (this.isStartingRemoteGame)
            {
                return; // Started on behalf of a remote host; the message handler owns the state
            }

            // The local user started a game while in the room: they are now the host
            this.CancelRequests();
            this.pendingActionsById.Clear();
            this.highestSeenActionId = -1;
            this.resyncTargetActionId = -1;
            this.gameInstanceId = InstanceRandom.Next(1, int.MaxValue);
            this.hostUserId = this.room.LocalUser.UserId;
            this.cachedConfigJson = JsonUtil.Serialize(started.Config, includeTypeInformation: true);
            this.SetState(SyncState.InGame);

            if (this.room.IsConnected)
            {
                this.BroadcastStart();
            }
            else
            {
                this.hasPendingStartBroadcast = true;
            }
        }

        private void DetachGame()
        {
            if (this.game != null)
            {
                this.game.OnActionPerformed -= this.HandleActionPerformed;
                this.game = null;
            }
        }

        private void HandleActionPerformed(PerformedAction performed)
        {
            if (performed.Source != ActionSource.Local || this.gameInstanceId == 0)
            {
                return;
            }

            if (performed.ActionId > this.highestSeenActionId)
            {
                this.highestSeenActionId = performed.ActionId;
            }

            if (this.room.IsConnected == false)
            {
                return;
            }

            var message = new ActionsPlayed { GameInstanceId = this.gameInstanceId, FirstActionId = performed.ActionId };
            message.Actions.Add(this.SerializeAction(performed.Action));
            this.room.SendMessage(message);
        }

        // ---- Messages ----------------------------------------------------------------------------------------------

        private void HandleMessage(RoomMessage message)
        {
            switch (message)
            {
                case BoardGameStart start:
                    this.HandleBoardGameStart(start);
                    break;

                case ActionsPlayed actions:
                    this.HandleActionsPlayed(actions);
                    break;

                case MissingActions missing:
                    this.HandleMissingActions(missing);
                    break;
            }
        }

        private void HandleBoardGameStart(BoardGameStart start)
        {
            bool isNewInstance = start.GameInstanceId != this.gameInstanceId;
            bool isResyncOfCurrent = isNewInstance == false && this.State == SyncState.Resyncing;

            if (isNewInstance == false && isResyncOfCurrent == false)
            {
                return; // Duplicate start for the game we are already running
            }

            BoardGameConfig config;

            try
            {
                config = JsonUtil.Deserialize<BoardGameConfig>(start.ConfigJson, includeTypeInformation: true);
            }
            catch (Exception ex)
            {
                Logger.LogError($"BoardGameNetworkSync: failed to deserialize config for game {start.GameInstanceId}: {ex}");
                return;
            }

            bool wasAwaitingStart = this.isAwaitingMissingActions && this.missingIncludesStart;

            if (isNewInstance)
            {
                this.CancelRequests();
            }

            this.gameInstanceId = start.GameInstanceId;
            this.hostUserId = start.HostUserId;
            this.cachedConfigJson = start.ConfigJson;
            this.resyncTargetActionId = start.LastActionId;
            this.hasPendingStartBroadcast = false;

            this.StartRemoteGame(config);

            if (this.game == null)
            {
                return;
            }

            if (start.LastActionId <= -1)
            {
                // Fresh game, nothing to replay
                this.CompleteRequest();
                this.SetState(SyncState.InGame);
                return;
            }

            // The game already has history; replay it before going live
            this.SetState(SyncState.Resyncing);
            this.game.IsReplaying = true;

            if (wasAwaitingStart)
            {
                // The responder sends the ActionsPlayed chunks right behind this start
                this.missingRequestTime = this.clock();
            }
            else
            {
                // Unsolicited start with history (host announced a game it started while offline); ask for the actions
                this.BeginMissingRequest(-1, includeStart: false);
            }
        }

        private void HandleActionsPlayed(ActionsPlayed actions)
        {
            if (actions.GameInstanceId != this.gameInstanceId)
            {
                if (this.gameInstanceId == 0 && this.game == null && this.hasDeferredRequest == false && this.isAwaitingMissingActions == false)
                {
                    // A game we know nothing about is in progress
                    this.DeferRequest(-1, includeStart: true, this.clock());
                }

                return;
            }

            if (this.game == null)
            {
                return;
            }

            int lastActionIdBefore = this.game.LastActionId;
            SyncState stateBefore = this.State;
            bool wasAwaitingBefore = this.isAwaitingMissingActions;
            int requestSerialBefore = this.requestSerial;

            for (int i = 0; i < actions.Actions.Count; i++)
            {
                this.ProcessIncomingAction(actions.FirstActionId + i, actions.Actions[i]);

                if (this.State == SyncState.Resyncing && stateBefore != SyncState.Resyncing)
                {
                    break; // A desync was detected mid message; the rest is meaningless until the resync completes
                }
            }

            bool progressed = this.game.LastActionId > lastActionIdBefore;

            if (this.State == SyncState.Resyncing && this.game.LastActionId >= this.resyncTargetActionId)
            {
                this.game.IsReplaying = false;
                this.CompleteRequest();
                this.SetState(SyncState.InGame);
                return;
            }

            if (this.isAwaitingMissingActions == false)
            {
                return;
            }

            if (wasAwaitingBefore == false || requestSerialBefore != this.requestSerial)
            {
                return; // This message started the request; give the responder a chance to answer
            }

            bool fromTarget = this.candidateIndex < this.responderCandidates.Count && actions.SenderUserId == this.responderCandidates[this.candidateIndex];

            if (this.pendingActionsById.Count == 0 && this.State != SyncState.Resyncing)
            {
                this.CompleteRequest();
            }
            else if (progressed)
            {
                this.missingRequestTime = this.clock();
            }
            else if (fromTarget && this.State != SyncState.Resyncing)
            {
                // The responder answered but could not fill the gap, so it is behind too
                this.AdvanceResponder();
            }
        }

        private void HandleMissingActions(MissingActions request)
        {
            if (request.TargetUserId != this.room.LocalUser.UserId || this.game == null || this.gameInstanceId == 0)
            {
                return;
            }

            if (this.State == SyncState.Resyncing)
            {
                return; // We do not have trustworthy state to hand out
            }

            if (request.IncludeStart)
            {
                this.BroadcastStart();

                if (this.game.LastActionId >= 0)
                {
                    this.SendActionRange(0);
                }

                return;
            }

            if (request.GameInstanceId != this.gameInstanceId)
            {
                return; // They are behind by a whole game; they need to ask with IncludeStart
            }

            this.SendActionRange(request.LastKnownActionId + 1);
        }

        private void SendActionRange(int fromActionId)
        {
            var message = new ActionsPlayed { GameInstanceId = this.gameInstanceId, FirstActionId = fromActionId };
            int bytesInChunk = 0;

            for (int id = fromActionId; id <= this.game.LastActionId; id++)
            {
                byte[] bytes = this.SerializeAction(this.game.ActionHistory[id]);

                if (message.Actions.Count > 0 && bytesInChunk + bytes.Length > this.options.ReplyChunkBytes)
                {
                    this.room.SendMessage(message);
                    message = new ActionsPlayed { GameInstanceId = this.gameInstanceId, FirstActionId = id };
                    bytesInChunk = 0;
                }

                message.Actions.Add(bytes);
                bytesInChunk += bytes.Length;
            }

            // Always send the last chunk, even when empty, so the requester knows we are up to date
            this.room.SendMessage(message);
        }

        // ---- Action application ------------------------------------------------------------------------------------

        private void ProcessIncomingAction(int actionId, byte[] bytes)
        {
            if (actionId > this.highestSeenActionId)
            {
                this.highestSeenActionId = actionId;
            }

            int expected = this.game.LastActionId + 1;

            if (actionId < expected)
            {
                return; // Duplicate
            }

            if (actionId > expected)
            {
                this.pendingActionsById[actionId] = bytes;

                if (this.isAwaitingMissingActions == false)
                {
                    this.BeginMissingRequest(expected - 1, includeStart: false);
                }

                return;
            }

            if (this.ApplyAction(actionId, bytes) == false)
            {
                return;
            }

            // Drain anything that was parked behind the gap
            while (this.pendingActionsById.Count > 0)
            {
                int next = this.game.LastActionId + 1;

                if (this.pendingActionsById.TryGetValue(next, out byte[] nextBytes) == false)
                {
                    break;
                }

                this.pendingActionsById.Remove(next);

                if (this.ApplyAction(next, nextBytes) == false)
                {
                    return;
                }
            }

            // Anything left below the current id is stale
            this.scratchIds.Clear();

            foreach (int id in this.pendingActionsById.Keys)
            {
                if (id <= this.game.LastActionId)
                {
                    this.scratchIds.Add(id);
                }
            }

            for (int i = 0; i < this.scratchIds.Count; i++)
            {
                this.pendingActionsById.Remove(this.scratchIds[i]);
            }
        }

        private bool ApplyAction(int actionId, byte[] bytes)
        {
            IBoardGameAction action = this.game.CreateAction();

            if (action == null)
            {
                this.Desync($"{this.game.GetType().Name} does not support networked actions (CreateAction returned null)");
                return false;
            }

            try
            {
                this.actionReader.Replace(bytes);
                action.Deserialize(this.actionReader);
            }
            catch (Exception ex)
            {
                this.Desync($"Failed to deserialize action {actionId}: {ex.Message}");
                return false;
            }

            if (this.game.CanPerformAction(action) == false)
            {
                this.Desync($"Action {actionId} was rejected by the game rules");
                return false;
            }

            bool replaying = this.State == SyncState.Resyncing;
            this.game.CurrentActionSource = replaying ? ActionSource.Replay : ActionSource.Remote;
            this.game.IsReplaying = replaying;

            try
            {
                this.game.PerformAction(action);
            }
            catch (Exception ex)
            {
                this.Desync($"Action {actionId} threw while being applied: {ex.Message}");
                return false;
            }
            finally
            {
                this.game.CurrentActionSource = ActionSource.Local;

                if (replaying == false)
                {
                    this.game.IsReplaying = false;
                }
            }

            return true;
        }

        private void Desync(string reason)
        {
            Logger.LogWarning($"BoardGameNetworkSync: desync detected: {reason}");
            this.OnDesyncDetected?.Invoke(reason);

            if (this.State == SyncState.Resyncing)
            {
                // The replayed history itself is inconsistent; nothing more we can do
                this.FailRequest();
            }
            else
            {
                this.RequestFullResync();
            }
        }

        private byte[] SerializeAction(IBoardGameAction action)
        {
            this.actionWriter.SeekZero();
            action.Serialize(this.actionWriter);
            return this.actionWriter.ToArray();
        }

        private void StartRemoteGame(BoardGameConfig config)
        {
            this.pendingActionsById.Clear();
            this.highestSeenActionId = -1;
            this.isStartingRemoteGame = true;

            try
            {
                this.startGame(config, this.room.LocalUser);
            }
            finally
            {
                this.isStartingRemoteGame = false;
            }

            if (this.game == null)
            {
                Logger.LogError("BoardGameNetworkSync: NotifyGameCreated was not called while starting the game, actions will not be synchronized");
            }
        }

        // ---- MissingActions request state machine --------------------------------------------------------------------

        private void DeferRequest(int lastKnownActionId, bool includeStart, double dueTime)
        {
            this.hasDeferredRequest = true;
            this.deferredLastKnownActionId = lastKnownActionId;
            this.deferredIncludesStart = includeStart;
            this.deferredRequestDueTime = dueTime;
        }

        private void BeginMissingRequest(int lastKnownActionId, bool includeStart)
        {
            this.hasDeferredRequest = false;
            this.missingLastKnownActionId = lastKnownActionId;
            this.missingIncludesStart = includeStart;
            this.candidateIndex = 0;
            this.responderRounds = 0;
            this.requestSerial++;
            this.isAwaitingMissingActions = true;

            if (this.State == SyncState.InGame)
            {
                this.SetState(SyncState.AwaitingMissingActions);
            }

            this.BuildResponderCandidates();

            if (this.responderCandidates.Count == 0)
            {
                // Nobody to ask yet; try again when users show up
                this.isAwaitingMissingActions = false;
                this.DeferRequest(lastKnownActionId, includeStart, this.clock());
                return;
            }

            this.SendMissingRequest();
        }

        private void SendMissingRequest()
        {
            this.room.SendMessage(new MissingActions
            {
                GameInstanceId = this.gameInstanceId,
                LastKnownActionId = this.missingLastKnownActionId,
                IncludeStart = this.missingIncludesStart,
                TargetUserId = this.responderCandidates[this.candidateIndex],
            });

            this.missingRequestTime = this.clock();
        }

        private void AdvanceResponder()
        {
            this.candidateIndex++;

            if (this.candidateIndex >= this.responderCandidates.Count)
            {
                this.candidateIndex = 0;
                this.responderRounds++;
                this.BuildResponderCandidates();

                if (this.responderRounds >= this.options.MaxResponderRounds || this.responderCandidates.Count == 0)
                {
                    this.FailRequest();
                    return;
                }
            }

            this.SendMissingRequest();
        }

        private void BuildResponderCandidates()
        {
            this.responderCandidates.Clear();

            string localUserId = this.room.LocalUser.UserId;
            IReadOnlyList<UserInfo> users = this.room.Users;

            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].UserId != localUserId)
                {
                    this.responderCandidates.Add(users[i].UserId);
                }
            }

            // Room users are already sorted by UserId; the host (if present) goes first
            string host = this.hostUserId;

            if (host != null && host != localUserId && this.responderCandidates.Remove(host))
            {
                this.responderCandidates.Insert(0, host);
            }
        }

        private void CompleteRequest()
        {
            this.isAwaitingMissingActions = false;
            this.hasDeferredRequest = false;

            if (this.State == SyncState.AwaitingMissingActions)
            {
                this.SetState(SyncState.InGame);
            }
        }

        private void FailRequest()
        {
            bool hadGame = this.gameInstanceId != 0;

            this.isAwaitingMissingActions = false;
            this.hasDeferredRequest = false;
            this.pendingActionsById.Clear();

            if (this.game != null)
            {
                this.game.IsReplaying = false;
            }

            if (hadGame)
            {
                Logger.LogWarning("BoardGameNetworkSync: nobody in the room could supply the missing actions");
                this.SetState(this.game != null ? SyncState.InGame : SyncState.NoGame);
                this.OnSyncFailed?.Invoke();
            }
            else
            {
                // We asked whether a game was running and nobody answered; that just means there is none
                this.SetState(SyncState.NoGame);
            }
        }

        private void CancelRequests()
        {
            this.isAwaitingMissingActions = false;
            this.hasDeferredRequest = false;
            this.hasPendingStartBroadcast = false;
        }

        private void SetState(SyncState state)
        {
            if (this.State == state)
            {
                return;
            }

            this.State = state;
            this.OnStateChanged?.Invoke(state);
        }

        private void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(BoardGameNetworkSync));
            }
        }
    }
}
