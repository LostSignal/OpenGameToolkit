//-----------------------------------------------------------------------
// <copyright file="IGameServerStats.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    using System;

    public interface IGameServerStats
    {
        DateTime StartTime { get; }

        DateTime ShutdownTime { get; }

        uint MessagesSent { get; }

        uint BytesSent { get; }

        uint MaxConnectedUsers { get; }

        uint NumberOfReconnects { get; }

        uint NumberOfConnectionDrops { get; }
    }
}
