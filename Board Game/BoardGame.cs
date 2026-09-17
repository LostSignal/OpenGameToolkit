//-----------------------------------------------------------------------
// <copyright file="BoardGame.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Newtonsoft.Json;
using OGT.Networking;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Stats = System.Collections.Generic.Dictionary<string, object>;

namespace OGT.BoardGame
{
    public enum Team
    {
        Blue,
        Orange,
        Green,
        Pink,
        Yellow,
        White,
        Red,
        Black,
    }

    public struct BoardPosition
    {
        public int Row { get; set; }
        public int Column { get; set; }
    }

    /// <summary>
    /// Where an action being performed on a board game originated from.
    /// </summary>
    public enum ActionSource
    {
        /// <summary>Performed by the local user (should be broadcast to the room).</summary>
        Local,

        /// <summary>Received live from another user in the room.</summary>
        Remote,

        /// <summary>Replayed from history while catching up (visualizers may skip animation).</summary>
        Replay,
    }

    public readonly struct PerformedAction
    {
        public PerformedAction(int actionId, IBoardGameAction action, ActionSource source)
        {
            this.ActionId = actionId;
            this.Action = action;
            this.Source = source;
        }

        public int ActionId { get; }

        public IBoardGameAction Action { get; }

        public ActionSource Source { get; }
    }

    public class BoardGameCharacter
    {
        public int CharacterId { get; set; }
        public string CharacterStatsId { get; set; }
        public string CharacterName { get; set; }
        public string OwningUserId { get; set; }
        public Team Team { get; set; }
        public BoardPosition Position { get; set; }
        public Stats Stats { get; set; }
        public int TileId { get; set; } = -1;

        public BoardGameCharacter Copy()
        {
            return new BoardGameCharacter
            {
                CharacterId = this.CharacterId,
                CharacterStatsId = this.CharacterStatsId,
                CharacterName = this.CharacterName,
                OwningUserId = this.OwningUserId,
                Team = this.Team,
                Position = this.Position,
                Stats = this.Stats != null ? new Stats(this.Stats) : new Stats(),
            };
        }
    }

    public abstract class BoardGameConfig
    {
        public Guid GameId { get; set; }
        public List<UserInfo> Players { get; set; }
        public List<BoardGameCharacter> Characters { get; set; }
        public Dictionary<string, Stats> CharacterStats { get; set; }
        public int Seed { get; set; }
        public abstract Type GetGameBoardType();
    }

    // Die Rolled
    //    Players
    //       Player 1, Dice Type/Result
    //       Player 2, Dice Type/Result

    // Grenade Thrown
    //   Die Rolled (Player 1, Grenade Accuracy, Dice Type = blah, Dice Value = blah)
    //   Action Played (Throw Grenade) - Plays Animation of character 1 throwing grenade at character 2, then updates health of character 2 based on accuracy of grenade throw and character 2's dodge stat.
    //

    // Shoot
    //   Die Rolled (Player 1 - Damanage, Player 2 - Defense)
    //   Action Played (Shoot) - Plays Animation of character 1 shooting at character 2, then updates health of character 2 based on damage of the shot and character 2's defense stat.

    // Update Tile Positions
    // Update Character Positions
    // Should Character Position be a Stat?

    // Sync Board State
    // After Action Played, make a Generate Reactions function

    // Halo Flashpoint
    //   Character Moved (Character Id, List of Tiles)
    //   Character Meleed
    //   Character Crouched
    //   Character Stands Up
    //   Character Throws Grenade (Character Id, Target Character Id)
    //   Character Health Changed
    //   Character Shoots
    //   Turn/Round Changed

    // Maze Protocol
    //   Tile Moved
    //   Tiles Shifted
    //
    //   Character Moved (Character Id, List of Tiles)
    //
    //   Gadget Picked Up
    //   Gadget Spwaned
    //
    //   Intel Picked Up
    //   Intel Spawned
    //
    //   Extraction Point Spawned
    //   Extraction Point Reached
    //
    //   Gadget Used (Player Id, Character Id, Gadget Id, Target Character Id)
    //


    public class BoardGame
    {
        [JsonProperty] private List<UserInfo> players;
        [JsonProperty] private List<BoardGameCharacter> characters;
        [JsonProperty] private BoardGameConfig config;
        [JsonProperty] private int randomCallCount;
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)] private List<IBoardGameAction> actionHistory = new List<IBoardGameAction>();

        // Non Serialzied dictionaries and random instance
        private Dictionary<int, BoardGameCharacter> charactersById;
        private Dictionary<string, UserInfo> playersById;
        private Random random;

        [JsonIgnore] public Action<Reaction> OnReaction { get; set; }
        [JsonIgnore] public Action OnGameStart { get; set; }
        [JsonIgnore] public Action OnNewRound { get; set; }
        [JsonIgnore] public Action OnNewTurn { get; set; }
        [JsonIgnore] public Action<Team> OnGameOver { get; set; }

        /// <summary>
        /// Gets or sets the callback invoked after an action has been fully applied and recorded in <see cref="ActionHistory"/>.
        /// </summary>
        [JsonIgnore] public Action<PerformedAction> OnActionPerformed { get; set; }

        [JsonIgnore] public IReadOnlyList<UserInfo> Players => this.players;
        [JsonIgnore] public IReadOnlyList<BoardGameCharacter> Characters => this.characters;
        [JsonIgnore] public BoardGameConfig Config => this.config;

        /// <summary>
        /// Gets the flat, ordered list of every action performed on this game. The index of an action is its ActionId.
        /// </summary>
        [JsonIgnore] public IReadOnlyList<IBoardGameAction> ActionHistory => this.actionHistory;

        /// <summary>
        /// Gets the id of the most recently performed action, or -1 if none have been performed.
        /// </summary>
        [JsonIgnore] public int LastActionId => this.actionHistory.Count - 1;

        /// <summary>
        /// Gets or sets a value indicating whether actions are currently being replayed from history (visualizers may skip animations).
        /// </summary>
        [JsonIgnore] public bool IsReplaying { get; set; }

        /// <summary>
        /// Gets or sets where the action currently being performed came from. Set by the networking layer before calling PerformAction.
        /// </summary>
        [JsonIgnore] public ActionSource CurrentActionSource { get; set; } = ActionSource.Local;

        /// <summary>
        /// Creates an empty instance of this game's action type, ready to be deserialized into. Returns null if this game does not support networked actions.
        /// </summary>
        public virtual IBoardGameAction CreateAction() => null;

        public virtual bool CanPerformAction(IBoardGameAction action) => false;

        public virtual void PerformAction(IBoardGameAction action)
        {
            throw new NotSupportedException($"{this.GetType().Name} does not support generic actions. Derive from BoardGame<TAction>.");
        }

        public virtual void StartBoardGame(BoardGameConfig config)
        {
            this.config = config;
            this.random = new Random(config.Seed);
            this.actionHistory = new List<IBoardGameAction>();
            this.IsReplaying = false;
            this.CurrentActionSource = ActionSource.Local;

            // Initialize Players from Config
            this.players = new List<UserInfo>(config.Players.Count);

            foreach (var player in config.Players)
            {
                this.players.Add(player.Copy());
            }

            // Initialize Characters from Config
            this.characters = new List<BoardGameCharacter>(config.Characters.Count);

            foreach (var character in config.Characters)
            {
                this.characters.Add(character.Copy());
            }

            foreach (var character in this.characters)
            {
                character.Stats = new Stats(config.CharacterStats[character.CharacterStatsId]);
            }

            this.CreateDictionaries();
        }

        public UserInfo GetPlayerById(string userId) => this.playersById[userId];

        public BoardGameCharacter GetCharacterById(int characterId) => this.charactersById[characterId];

        public int GetRandomInteger(int min, int max)
        {
            this.randomCallCount++;
            return random.Next(min, max);
        }

        /// <summary>
        /// Appends a fully applied action to <see cref="ActionHistory"/> and raises <see cref="OnActionPerformed"/>.
        /// Call this only after the game state has been completely mutated by the action.
        /// </summary>
        protected void RecordAction(IBoardGameAction action)
        {
            this.actionHistory.Add(action);
            this.OnActionPerformed?.Invoke(new PerformedAction(this.actionHistory.Count - 1, action, this.CurrentActionSource));
        }

        [OnDeserialized]
        protected virtual void OnDeserializedMethod(StreamingContext context)
        {
            this.random = new Random(this.config.Seed);

            for (int i = 0; i < this.randomCallCount; i++)
            {
                this.random.Next();
            }

            this.actionHistory ??= new List<IBoardGameAction>();
            this.CreateDictionaries();
        }

        private void CreateDictionaries()
        {
            this.playersById = new Dictionary<string, UserInfo>();

            foreach (var player in this.players)
            {
                this.playersById.Add(player.UserId, player);
            }

            this.charactersById = new Dictionary<int, BoardGameCharacter>();
            foreach (var character in this.characters)
            {
                this.charactersById.Add(character.CharacterId, character);
            }
        }
    }

    /// <summary>
    /// Base class for board games driven by a single strongly typed action type. Provides the validate -> apply -> record
    /// template that the networking layer relies on so that every peer applies actions identically.
    /// </summary>
    /// <typeparam name="TAction">The game's action type.</typeparam>
    public abstract class BoardGame<TAction> : BoardGame
        where TAction : class, IBoardGameAction, new()
    {
        public sealed override IBoardGameAction CreateAction() => new TAction();

        public sealed override bool CanPerformAction(IBoardGameAction action) => this.CanPerformAction((TAction)action);

        public sealed override void PerformAction(IBoardGameAction action) => this.PerformAction((TAction)action);

        public abstract bool CanPerformAction(TAction action);

        public void PerformAction(TAction action)
        {
            if (this.CanPerformAction(action) == false)
            {
                throw new InvalidOperationException("Cannot perform action.");
            }

            this.OnPerformAction(action);
            this.RecordAction(action);
        }

        /// <summary>
        /// Applies an already validated action to the game state. Do not call directly; use <see cref="PerformAction(TAction)"/>.
        /// </summary>
        protected abstract void OnPerformAction(TAction action);
    }

    public abstract class Reaction
    {
    }

    public class NewGame : Reaction
    {
        public BoardGameConfig Config { get; set; }

        public override string ToString()
        {
            return this.Config == null
                ? "NewGame: Config = null"
                : $"NewGame: {this.Config.GetType().Name}, Players = {this.Config.Players?.Count ?? 0}, Characters = {this.Config.Characters?.Count ?? 0}, Seed = {this.Config.Seed}";
        }
    }

    public class GameOver : Reaction
    {
        public Team WinningTeam;

        public override string ToString()
        {
            return $"GameOver: WinningTeam = {this.WinningTeam}";
        }
    }

    public class NewRound : Reaction
    {
        public int Round { get; set; }

        public override string ToString()
        {
            return $"NewRound: Round = {this.Round}";
        }
    }

    public class NewTurn : Reaction
    {
        public int Round { get; set; }
        public int Turn { get; set; }
        public string CurrentUserId { get; set; }
        public Team CurrentTeam { get; set; }
        public int CurrentCharacterId { get; set; }

        public override string ToString()
        {
            return $"NewTurn: Round = {this.Round}, Turn = {this.Turn}, CurrentUserId = {this.CurrentUserId}, CurrentTeam = {this.CurrentTeam}, CurrentCharacterId = {this.CurrentCharacterId}";
        }
    }

    public class StatChangedReaction : Reaction
    {
        public int CharacterId { get; set; }
        public string StatName { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }

        public override string ToString()
        {
            return $"StatChangedReaction: CharacterId = {this.CharacterId}, StatName = {this.StatName}, OldValue = {this.OldValue ?? "null"}, NewValue = {this.NewValue ?? "null"}";
        }
    }
}
