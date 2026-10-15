//-----------------------------------------------------------------------
// <copyright file="BoardGameDistributedAuthoritySync.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_NETCODE_GAMEOBJECTS

namespace OGT.BoardGame
{
    using System;
    using OGT.Networking;
    using Unity.Collections;
    using Unity.Netcode;

#if USING_UGS_AUTH
    using Unity.Services.Authentication;
#endif

    /// <summary>
    /// Keeps the <see cref="BoardGameManager"/>'s current game in sync across a Netcode for GameObjects session (distributed
    /// authority or client/server). The owner of this object is the host: it publishes the config of every game it starts
    /// and owns the replicated action list. Anyone can submit actions through an RPC; the host validates them and appends
    /// them to the list, and every peer applies new list entries to its own board. Late joiners rebuild the game from the
    /// config and replay the list. When the game ends (or the host destroys it) the config and actions are cleared.
    /// When there is no spawned network session this component does nothing, so local play is unaffected.
    /// Place it on a NetworkObject whose ownership follows the session owner.
    /// </summary>
    public class BoardGameDistributedAuthoritySync : NetworkBehaviour, IAwake, ILateUpdate
    {
        private static readonly OGTLogger Logger = OGTLogger.Networking;

        private readonly NetworkWriter actionWriter = new();
        private readonly NetworkReader actionReader = new();

        private NetworkVariable<NetworkedConfig> networkConfig;
        private NetworkList<NetworkedAction> networkActions;

        private BoardGameManager boardGameManager;
        private BoardGame syncedGame;
        private int syncedGameNumber;
        private bool isStartingNetworkedGame;
        private bool isApplyingActions;
        private bool isResyncing;
        private bool isCleanupPending;
        private BoardGame finishedGame;

        private bool IsNetworked => this.IsSpawned && this.NetworkManager != null && this.NetworkManager.IsListening;

        public void OnAwake(Bootloader bootloader)
        {
            this.boardGameManager = bootloader.FindManager<BoardGameManager>();
            this.boardGameManager.OnGameCreated += this.HandleGameCreated;
            this.boardGameManager.OnGameStarted += this.HandleGameStarted;
            this.boardGameManager.OnGameDestroyed += this.HandleGameDestroyed;

            // We may have been spawned (late join) before the bootloader activated us
            this.SyncWithNetworkGame();
        }

        public void OnLateUpdate(float deltaTime)
        {
            // Deferred from OnGameOver so the final action is appended to the list before it gets cleared
            if (this.isCleanupPending)
            {
                this.isCleanupPending = false;
                this.ClearNetworkState();
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            this.networkConfig.OnValueChanged += this.HandleConfigChanged;
            this.networkActions.OnListChanged += this.HandleActionsChanged;

            this.SyncWithNetworkGame();
        }

        public override void OnNetworkDespawn()
        {
            this.networkConfig.OnValueChanged -= this.HandleConfigChanged;
            this.networkActions.OnListChanged -= this.HandleActionsChanged;

            // Leave the local board alone so the player can still see it, just stop syncing it
            this.DetachGame();
            this.isCleanupPending = false;

            base.OnNetworkDespawn();
        }

        public override void OnGainedOwnership()
        {
            base.OnGainedOwnership();

            // The session owner migrated to us (e.g. the host left), so take over publishing the game we're already playing
            this.SyncWithNetworkGame();
        }

        public override void OnDestroy()
        {
            if (this.boardGameManager != null)
            {
                this.boardGameManager.OnGameCreated -= this.HandleGameCreated;
                this.boardGameManager.OnGameStarted -= this.HandleGameStarted;
                this.boardGameManager.OnGameDestroyed -= this.HandleGameDestroyed;
            }

            base.OnDestroy();
        }

        private void Awake()
        {
            // Network variables must exist before the NetworkObject spawns
            this.networkConfig = new NetworkVariable<NetworkedConfig>(new NetworkedConfig(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
            this.networkActions = new NetworkList<NetworkedAction>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

            // Can't derive from GameBehavior, so register for IAwake/ILateUpdate the same way it does
            ActivationManager.Register(this);
        }

        // ---- BoardGameManager events ----------------------------------------------------------------------------------

        private void HandleGameCreated(BoardGame game)
        {
            // Hooked before StartBoardGame runs so nothing the game raises is missed. The handlers check whether we're
            // networked when they fire, so a game created before this object spawned is still picked up later.
            this.HookGame(game);
        }

        private void HandleGameStarted(BoardGame game)
        {
            if (this.IsNetworked == false || this.isStartingNetworkedGame)
            {
                return;
            }

            if (this.HasAuthority)
            {
                // Host started a game, publish it and start a fresh action list
                this.PublishGame(game);
            }
            else if (this.HasNetworkConfig() && this.CanAdoptGame(game, this.networkConfig.Value))
            {
                // Client built the same game the host published (e.g. from shared session data), keep it
                this.AttachGame(game, this.networkConfig.Value.GameNumber);
                this.ApplyPendingActions(replay: true);
            }
            else
            {
                Logger.LogWarning("BoardGameDistributedAuthoritySync: Only the host can start a networked game, this game will not be synced until the host publishes a matching one");
            }
        }

        private void HandleGameDestroyed(BoardGame game)
        {
            this.UnhookGame(game);

            if (game == this.finishedGame)
            {
                this.finishedGame = null;
            }

            if (game != this.syncedGame)
            {
                return;
            }

            this.DetachGame();

            if (this.IsNetworked && this.HasAuthority)
            {
                this.ClearNetworkState();
            }
        }

        // ---- BoardGame events -----------------------------------------------------------------------------------------

        private void HandleActionPerformed(PerformedAction performed)
        {
            if (performed.Source != ActionSource.Local || this.IsNetworked == false || this.syncedGame == null || this.boardGameManager.CurrentBoardGame != this.syncedGame)
            {
                return;
            }

            byte[] bytes = this.SerializeAction(performed.Action);

            if (bytes.Length > NetworkedAction.MaxBytes)
            {
                Logger.LogError($"BoardGameDistributedAuthoritySync: Action {performed.ActionId} is {bytes.Length} bytes, more than the {NetworkedAction.MaxBytes} byte limit, it can not be synced");
                return;
            }

            this.SubmitActionRpc(this.syncedGameNumber, performed.ActionId, bytes);
        }

        private void HandleGameOver(Team winningTeam)
        {
            // Remembered so a finished game is never (re)published if we become the authority later
            this.finishedGame = this.boardGameManager?.CurrentBoardGame;

            if (this.IsNetworked && this.HasAuthority && this.syncedGame != null)
            {
                this.isCleanupPending = true;
            }
        }

        // ---- Network events -------------------------------------------------------------------------------------------

        private void HandleConfigChanged(NetworkedConfig previous, NetworkedConfig current)
        {
            if (this.HasAuthority)
            {
                return; // We wrote it
            }

            this.SyncWithNetworkGame();
        }

        private void HandleActionsChanged(NetworkListEvent<NetworkedAction> changeEvent)
        {
            if (changeEvent.Type == NetworkListEvent<NetworkedAction>.EventType.Add)
            {
                this.ApplyPendingActions(replay: false);
            }
        }

        /// <summary>Anyone can call this; it runs on the host, who validates the action and appends it to the replicated list.</summary>
        [Rpc(SendTo.Authority)]
        private void SubmitActionRpc(int gameNumber, int actionId, byte[] bytes, RpcParams rpcParams = default)
        {
            ulong senderClientId = rpcParams.Receive.SenderClientId;
            bool isLocal = senderClientId == this.NetworkManager.LocalClientId;

            if (this.IsValidSubmission(gameNumber, actionId, bytes, isLocal) == false)
            {
                if (isLocal)
                {
                    Logger.LogError($"BoardGameDistributedAuthoritySync: Host's own action {actionId} could not be added, resyncing");
                    this.ResyncFromNetwork();
                }
                else
                {
                    this.ActionRejectedRpc(gameNumber, actionId, this.RpcTarget.Single(senderClientId, RpcTargetUse.Temp));
                }

                return;
            }

            this.networkActions.Add(new NetworkedAction(gameNumber, bytes));

            // The list change callback normally does this, but don't rely on it firing for the writer
            this.ApplyPendingActions(replay: false);
        }

        /// <summary>Sent by the host to a client whose action could not be applied, its board has diverged and is rebuilt.</summary>
        [Rpc(SendTo.SpecifiedInParams)]
        private void ActionRejectedRpc(int gameNumber, int actionId, RpcParams rpcParams)
        {
            if (gameNumber != this.syncedGameNumber)
            {
                return;
            }

            Logger.LogWarning($"BoardGameDistributedAuthoritySync: Host rejected action {actionId}, resyncing");
            this.ResyncFromNetwork();
        }

        // ---- Sync -----------------------------------------------------------------------------------------------------

        private void SyncWithNetworkGame()
        {
            if (this.boardGameManager == null || this.IsNetworked == false)
            {
                return;
            }

            if (this.HasAuthority)
            {
                this.PublishCurrentGameIfNeeded();
                return;
            }

            if (this.HasNetworkConfig() == false)
            {
                // Game ended or there is none. Keep the board (e.g. game over screen), just stop syncing it
                this.DetachGame();
                return;
            }

            if (this.syncedGame != null && this.syncedGameNumber == this.networkConfig.Value.GameNumber)
            {
                this.ApplyPendingActions(replay: false);
                return;
            }

            this.StartNetworkedGame(forceRestart: false);
        }

        private void StartNetworkedGame(bool forceRestart)
        {
            NetworkedConfig published = this.networkConfig.Value;
            BoardGame current = this.boardGameManager.CurrentBoardGame;

            if (forceRestart == false && current != null && this.CanAdoptGame(current, published))
            {
                this.AttachGame(current, published.GameNumber);
                this.ApplyPendingActions(replay: true);
                return;
            }

            BoardGameConfig config;

            try
            {
                config = JsonUtil.Deserialize<BoardGameConfig>(published.Json, includeTypeInformation: true);
            }
            catch (Exception ex)
            {
                Logger.LogError($"BoardGameDistributedAuthoritySync: Failed to deserialize game config: {ex.Message}");
                this.DetachGame();
                return;
            }

            this.DetachGame();
            this.isStartingNetworkedGame = true;

            try
            {
                this.boardGameManager.StartGame(config, this.GetLocalUser(config));
            }
            finally
            {
                this.isStartingNetworkedGame = false;
            }

            this.AttachGame(this.boardGameManager.CurrentBoardGame, published.GameNumber);
            this.ApplyPendingActions(replay: true);
        }

        private void ResyncFromNetwork()
        {
            if (this.isResyncing || this.HasNetworkConfig() == false)
            {
                return;
            }

            this.isResyncing = true;

            try
            {
                this.StartNetworkedGame(forceRestart: true);
            }
            finally
            {
                this.isResyncing = false;
            }
        }

        private void ApplyPendingActions(bool replay)
        {
            if (this.syncedGame == null || this.isApplyingActions)
            {
                return;
            }

            bool failed = false;
            this.isApplyingActions = true;

            try
            {
                for (int actionId = this.syncedGame.LastActionId + 1; actionId < this.networkActions.Count; actionId++)
                {
                    NetworkedAction entry = this.networkActions[actionId];

                    if (entry.GameNumber != this.syncedGameNumber)
                    {
                        break; // Left over from another game, the new config hasn't arrived yet
                    }

                    if (this.TryApplyAction(actionId, entry.ToArray(), replay) == false)
                    {
                        failed = true;
                        break;
                    }
                }
            }
            finally
            {
                this.isApplyingActions = false;
            }

            if (failed)
            {
                if (this.isResyncing)
                {
                    Logger.LogError("BoardGameDistributedAuthoritySync: Replaying the networked game failed, it is no longer synced");
                    this.DetachGame();
                }
                else
                {
                    this.ResyncFromNetwork();
                }
            }
        }

        private bool TryApplyAction(int actionId, byte[] bytes, bool replay)
        {
            BoardGame game = this.syncedGame;
            IBoardGameAction action = game.CreateAction();

            if (action == null)
            {
                Logger.LogError($"BoardGameDistributedAuthoritySync: {game.GetType().Name} does not support networked actions (CreateAction returned null)");
                return false;
            }

            try
            {
                this.actionReader.Replace(bytes);
                action.Deserialize(this.actionReader);

                if (game.CanPerformAction(action) == false)
                {
                    Logger.LogWarning($"BoardGameDistributedAuthoritySync: Action {actionId} was rejected by the game rules");
                    return false;
                }

                game.CurrentActionSource = replay ? ActionSource.Replay : ActionSource.Remote;
                game.IsReplaying = replay;
                game.PerformAction(action);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"BoardGameDistributedAuthoritySync: Action {actionId} failed to apply: {ex.Message}");
                return false;
            }
            finally
            {
                game.CurrentActionSource = ActionSource.Local;
                game.IsReplaying = false;
            }
        }

        private bool IsValidSubmission(int gameNumber, int actionId, byte[] bytes, bool isLocal)
        {
            if (this.HasNetworkConfig() == false || gameNumber != this.networkConfig.Value.GameNumber || actionId != this.networkActions.Count)
            {
                return false;
            }

            if (isLocal)
            {
                return true; // Already applied to our board
            }

            // Make sure the host's own board, which is always caught up with the list, accepts it before everyone gets it
            if (this.syncedGame == null || this.syncedGame.LastActionId != this.networkActions.Count - 1)
            {
                return false;
            }

            IBoardGameAction action = this.syncedGame.CreateAction();

            if (action == null)
            {
                return false;
            }

            try
            {
                this.actionReader.Replace(bytes);
                action.Deserialize(this.actionReader);
                return this.syncedGame.CanPerformAction(action);
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"BoardGameDistributedAuthoritySync: Submitted action {actionId} could not be read: {ex.Message}");
                return false;
            }
        }

        private void ClearNetworkState()
        {
            if (this.IsNetworked == false || this.HasAuthority == false)
            {
                return;
            }

            // Keep the game number so the next game's actions can't be mistaken for this one's
            this.networkActions.Clear();
            this.networkConfig.Value = new NetworkedConfig { GameNumber = this.networkConfig.Value?.GameNumber ?? 0 };
            this.DetachGame();
        }

        /// <summary>The authority started a game: publish its config and a fresh action list (plus any actions it already played).</summary>
        private void PublishGame(BoardGame game)
        {
            int gameNumber = (this.networkConfig.Value?.GameNumber ?? 0) + 1;
            this.networkActions.Clear();
            this.networkConfig.Value = new NetworkedConfig { GameNumber = gameNumber, Json = SerializeConfig(game.Config) };
            this.AttachGame(game, gameNumber);
            this.isCleanupPending = false;

            // Normally empty, but the game may have been running before this object spawned or before OnAwake hooked us up
            for (int actionId = 0; actionId < game.ActionHistory.Count; actionId++)
            {
                byte[] bytes = this.SerializeAction(game.ActionHistory[actionId]);

                if (bytes.Length > NetworkedAction.MaxBytes)
                {
                    Logger.LogError($"BoardGameDistributedAuthoritySync: Action {actionId} is {bytes.Length} bytes, more than the {NetworkedAction.MaxBytes} byte limit, it can not be synced");
                    break;
                }

                this.networkActions.Add(new NetworkedAction(gameNumber, bytes));
            }
        }

        // The authority may have started its game before this object spawned, or before the bootloader activated this
        // component (OnAwake runs a frame or so after spawning), so publish whatever is running that isn't published yet
        private void PublishCurrentGameIfNeeded()
        {
            BoardGame current = this.boardGameManager.CurrentBoardGame;

            if (current == null || current == this.syncedGame || current == this.finishedGame)
            {
                return;
            }

            this.HookGame(current);
            this.PublishGame(current);
        }

        private void AttachGame(BoardGame game, int gameNumber)
        {
            // Adopted games may have been created before this object spawned, so make sure they're hooked up
            this.HookGame(game);
            this.syncedGame = game;
            this.syncedGameNumber = gameNumber;
        }

        private void HookGame(BoardGame game)
        {
            // Unsubscribing first keeps this idempotent
            this.UnhookGame(game);
            game.OnActionPerformed += this.HandleActionPerformed;
            game.OnGameOver += this.HandleGameOver;
        }

        private void UnhookGame(BoardGame game)
        {
            game.OnActionPerformed -= this.HandleActionPerformed;
            game.OnGameOver -= this.HandleGameOver;
        }

        private void DetachGame()
        {
            this.syncedGame = null;
            this.syncedGameNumber = 0;
        }

        private bool HasNetworkConfig() => string.IsNullOrEmpty(this.networkConfig.Value?.Json) == false;

        /// <summary>A game can be kept instead of restarted when it was built from the same config and nothing has been played on it yet.</summary>
        private bool CanAdoptGame(BoardGame game, NetworkedConfig published)
        {
            return game?.Config != null && game.LastActionId < 0 && SerializeConfig(game.Config) == published.Json;
        }

        private UserInfo GetLocalUser(BoardGameConfig config)
        {
            string localUserId = this.GetLocalUserId();

            if (localUserId != null && config.Players != null)
            {
                foreach (var player in config.Players)
                {
                    if (player.UserId == localUserId)
                    {
                        return player.Copy();
                    }
                }
            }

            // Not one of the players, join as a spectator
            return new UserInfo
            {
                UserId = localUserId ?? $"Client{this.NetworkManager.LocalClientId}",
                DisplayName = this.boardGameManager.LocalPlayer?.DisplayName ?? $"Client{this.NetworkManager.LocalClientId}",
            };
        }

        private string GetLocalUserId()
        {
#if USING_UGS_AUTH
            try
            {
                if (AuthenticationService.Instance.IsSignedIn)
                {
                    return AuthenticationService.Instance.PlayerId;
                }
            }
            catch (Exception)
            {
                // Unity Services not initialized, fall through
            }
#endif

            return this.boardGameManager.LocalPlayer?.UserId;
        }

        private byte[] SerializeAction(IBoardGameAction action)
        {
            this.actionWriter.SeekZero();
            action.Serialize(this.actionWriter);
            return this.actionWriter.ToArray();
        }

        private static string SerializeConfig(BoardGameConfig config) => JsonUtil.Serialize(config, includeTypeInformation: true);

        // ---- Network types --------------------------------------------------------------------------------------------

        /// <summary>The published game config. An empty Json means no networked game is running.</summary>
        internal class NetworkedConfig : INetworkSerializable, IEquatable<NetworkedConfig>
        {
            public int GameNumber;
            public string Json = string.Empty;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer)
                where T : IReaderWriter
            {
                this.Json ??= string.Empty;
                serializer.SerializeValue(ref this.GameNumber);
                serializer.SerializeValue(ref this.Json);
            }

            public bool Equals(NetworkedConfig other) => other != null && this.GameNumber == other.GameNumber && this.Json == other.Json;

            public override bool Equals(object obj) => this.Equals(obj as NetworkedConfig);

            public override int GetHashCode() => HashCode.Combine(this.GameNumber, this.Json);
        }

        /// <summary>One entry of the replicated action list: an action serialized with <see cref="IBoardGameAction.Serialize"/>.</summary>
        internal struct NetworkedAction : INetworkSerializable, IEquatable<NetworkedAction>
        {
            public static readonly int MaxBytes = default(FixedList4096Bytes<byte>).Capacity;

            public int GameNumber;
            public FixedList4096Bytes<byte> Bytes;

            public NetworkedAction(int gameNumber, byte[] bytes)
            {
                this.GameNumber = gameNumber;
                this.Bytes = default;

                foreach (byte b in bytes)
                {
                    this.Bytes.Add(b);
                }
            }

            public byte[] ToArray()
            {
                var array = new byte[this.Bytes.Length];

                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = this.Bytes[i];
                }

                return array;
            }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer)
                where T : IReaderWriter
            {
                serializer.SerializeValue(ref this.GameNumber);

                if (serializer.IsWriter)
                {
                    byte[] array = this.ToArray();
                    serializer.SerializeValue(ref array);
                }
                else
                {
                    byte[] array = null;
                    serializer.SerializeValue(ref array);
                    this.Bytes.Clear();

                    foreach (byte b in array)
                    {
                        this.Bytes.Add(b);
                    }
                }
            }

            public bool Equals(NetworkedAction other) => this.GameNumber == other.GameNumber && this.Bytes.Equals(other.Bytes);

            public override bool Equals(object obj) => obj is NetworkedAction other && this.Equals(other);

            public override int GetHashCode() => HashCode.Combine(this.GameNumber, this.Bytes.GetHashCode());
        }
    }
}

#else

namespace OGT.BoardGame
{
    /// <summary>Netcode for GameObjects is not installed, so there is nothing to sync.</summary>
    public class BoardGameDistributedAuthoritySync : GameBehavior
    {
    }
}

#endif
