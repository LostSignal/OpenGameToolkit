//-----------------------------------------------------------------------
// <copyright file="ClientEventType.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    public enum ClientEventType
    {
        ConnectionOpened,
        ConnectionClosed,
        ConnectionLost,
        ReceivedData,
    }
}
