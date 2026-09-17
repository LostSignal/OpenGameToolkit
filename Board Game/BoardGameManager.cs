//-----------------------------------------------------------------------
// <copyright file="BoardGameManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Threading.Tasks;
    using OGT.BoardGame;
    using OGT.Networking;

    /// <summary>
    /// Owns the current board game. <see cref="StartGame"/> is the single entry point for both local and networked play:
    /// when a <see cref="BoardGameNetworkManager"/> is in a room, any game started here is broadcast to the room, and games
    /// started by other users in the room are started here on their behalf.
    /// </summary>
    public class BoardGameManager : Manager
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        private BoardGame.BoardGame currentBoardGame;

        /// <summary>Raised with the new game before StartBoardGame runs, so listeners can subscribe to its reactions in time.</summary>
        public Action<BoardGame.BoardGame> OnGameCreated;

        /// <summary>Raised after StartBoardGame has run and the game is ready to accept actions.</summary>
        public Action<BoardGame.BoardGame> OnGameStarted;

        public Action<BoardGame.BoardGame> OnGameDestroyed;

        public BoardGame.BoardGame CurrentBoardGame => this.currentBoardGame;

        /// <summary>Gets the user this device plays as in the current game.</summary>
        public UserInfo LocalPlayer { get; private set; }

        /// <summary>
        /// Starts a game for the given user. Players in the config whose UserId matches the user's are controlled locally.
        /// The config is used as is: set a non-zero Seed yourself, and keep it JSON serializable because it is sent to remote peers.
        /// </summary>
        public void StartGame(BoardGameConfig config, UserInfo localUser)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (localUser == null)
            {
                throw new ArgumentNullException(nameof(localUser));
            }

            this.DestroyCurrentBoard();

            this.LocalPlayer = localUser;

            this.currentBoardGame = Activator.CreateInstance(config.GetGameBoardType()) as BoardGame.BoardGame;
            this.OnGameCreated?.Invoke(this.currentBoardGame);
            this.currentBoardGame.StartBoardGame(config);
            this.OnGameStarted?.Invoke(this.currentBoardGame);
        }

        public void DestroyCurrentBoard()
        {
            if (this.currentBoardGame != null)
            {
                this.OnGameDestroyed?.Invoke(this.currentBoardGame);
                this.currentBoardGame = null;
                this.LocalPlayer = null;
            }
        }

        protected override Task InitializeManager(Bootloader bootloader)
        {
            return Task.CompletedTask;
        }
    }
}
