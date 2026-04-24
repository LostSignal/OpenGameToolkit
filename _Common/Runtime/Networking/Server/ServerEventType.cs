//-----------------------------------------------------------------------
// <copyright file="ServerEventType.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    public enum ServerEventType
    {
        ConnectionOpened,
        ConnectionClosed,
        ConnectionLost,
        ReceivedData,
    }
}
