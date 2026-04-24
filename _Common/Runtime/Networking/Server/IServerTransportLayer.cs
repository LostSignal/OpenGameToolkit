//-----------------------------------------------------------------------
// <copyright file="IServerTransportLayer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    public interface IServerTransportLayer
    {
        bool IsStarting { get; }

        bool IsRunning { get; }

        bool PrintDebugOutput { get; }

        void Start(int port, bool printDebugOutput);

        void Update();

        void SendData(long connectionId, byte[] data, uint offset, uint length, bool reliable);

        void Shutdown();

        bool TryDequeueServerEvent(out ServerEvent serverEvent);
    }
}
