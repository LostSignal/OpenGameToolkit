//-----------------------------------------------------------------------
// <copyright file="IBoardGameAction.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BoardGame
{
    using OGT.Networking;

    /// <summary>
    /// A game specific action that can be sent over the network. Implementations must be deterministic:
    /// serializing then deserializing an action must produce an action that mutates the game identically.
    /// Implementations need a public parameterless constructor.
    /// </summary>
    public interface IBoardGameAction
    {
        void Serialize(NetworkWriter writer);

        void Deserialize(NetworkReader reader);
    }
}
