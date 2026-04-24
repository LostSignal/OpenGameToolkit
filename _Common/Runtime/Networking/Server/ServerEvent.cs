//-----------------------------------------------------------------------
// <copyright file="ServerEvent.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    public struct ServerEvent
    {
        public ServerEventType EventType;
        public long ConnectionId;
        public byte[] Data;
        public bool Reliable;
    }
}
