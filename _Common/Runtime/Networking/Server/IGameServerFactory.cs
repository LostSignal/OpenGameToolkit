//-----------------------------------------------------------------------
// <copyright file="IGameServerFactory.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    public interface IGameServerFactory
    {
        GameServer CreateGameServerAndStart(int port);
    }
}
