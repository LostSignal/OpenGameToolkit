#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="SuperSocketServerTransport.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY && USING_SUPER_SOCKET

namespace OGT.Networking
{
    using System.Collections.Concurrent;
    using SuperSocket.SocketBase;
    using SuperSocket.SocketBase.Config;
    using SuperWebSocket;
    using UnityEngine;

    public class SuperSocketServerTransport : IServerTransportLayer
    {
        // websocket state
        private WebSocketServer webSocketServer;

        // connection / session tracking
        private ConcurrentQueue<ServerEvent> serverEvents = new ConcurrentQueue<ServerEvent>();
        private ConcurrentDictionary<long, WebSocketSession> connectionIdToSessionMap = new ConcurrentDictionary<long, WebSocketSession>();
        private ConcurrentDictionary<string, long> sessionIdToConnectionIdMap = new ConcurrentDictionary<string, long>();

        private readonly object connectionIdLock = new object();
        private long connectionIdCounter = 0;

        bool IServerTransportLayer.IsStarting
        {
            get { return this.IsServerStarting(); }
        }

        bool IServerTransportLayer.IsRunning
        {
            get { return this.IsServerRunning(); }
        }

        void IServerTransportLayer.Update()
        {
        }

        void IServerTransportLayer.Start(int port)
        {
            Lost.Logger.Assert(this.IsServerRunning() == false, "SuperSocketServerTransport: You can't start a server if one is already running.");

            this.connectionIdToSessionMap.Clear();
            this.sessionIdToConnectionIdMap.Clear();
            this.DestroyWebSocketServer();

            this.webSocketServer = new WebSocketServer();
            this.webSocketServer.NewDataReceived += this.MessageRecieved;
            this.webSocketServer.NewSessionConnected += this.NewSessionConnected;
            this.webSocketServer.SessionClosed += this.SessionClosed;

            // setup
            var serverConfig = new ServerConfig
            {
                Name = "SuperWebSocket",
                Ip = "Any",
                Port = port,
                Mode = SocketMode.Tcp,
                Security = "tls",
                KeepAliveInterval = 1,
                KeepAliveTime = 2,

                Certificate = new CertificateConfig
                {
                    FilePath = "MyCert.pfx",
                    Password = "O3Yse#qe!n*T14ObOtlw",
                }
            };

            bool setup = this.webSocketServer.Setup(serverConfig);

            if (setup == false)
            {
                Lost.Logger.LogErrorFormat("SuperSocketServerTransport: Setup on port {0} failed.", port);
            }

            // start
            bool start = setup == false ? false : this.webSocketServer.Start();

            if (setup && start == false)
            {
                Lost.Logger.LogError("SuperSocketServerTransport: Start failed.");
            }

            // Testing if we need to shutdown
            if (setup == false || start == false)
            {
                this.DestroyWebSocketServer();
            }
        }

        void IServerTransportLayer.SendData(long connectionId, byte[] data, uint offset, uint length)
        {
            Lost.Logger.Assert(this.IsServerRunning(), "SuperSocketServerTransport: Can't send data if server isn't running.");
            Lost.Logger.Assert(offset >= 0, "SuperSocketServerTransport: Can't send data with a negative offset.");
            Lost.Logger.Assert(length >= 0, "SuperSocketServerTransport: Can't send data with a negative length.");

            WebSocketSession webSocketSession;
            if (this.connectionIdToSessionMap.TryGetValue(connectionId, out webSocketSession))
            {
                webSocketSession.Send(data, (int)offset, (int)length);
            }
        }

        void IServerTransportLayer.Shutdown()
        {
            Lost.Logger.Assert(this.IsServerRunning(), "SuperSocketServerTransport: Can't Shutdown if it's not running.");

            if (this.IsServerRunning())
            {
                this.webSocketServer.Stop();
            }
        }

        bool IServerTransportLayer.TryDequeueServerEvent(out ServerEvent serverEvent)
        {
            return this.serverEvents.TryDequeue(out serverEvent);
        }

        private void MessageRecieved(WebSocketSession session, byte[] data)
        {
            this.serverEvents.Enqueue(new ServerEvent
            {
                EventType = ServerEventType.ReceivedData,
                ConnectionId = this.sessionIdToConnectionIdMap[session.SessionID],
                Data = data,
                Reliable = true,
            });
        }

        private void NewSessionConnected(WebSocketSession session)
        {
            long connectionId = 0L;

            lock (this.connectionIdLock)
            {
                connectionId = this.connectionIdCounter++;
            }

            this.connectionIdToSessionMap.TryAdd(connectionId, session);
            this.sessionIdToConnectionIdMap.TryAdd(session.SessionID, connectionId);

            this.serverEvents.Enqueue(new ServerEvent
            {
                EventType = ServerEventType.ConnectionOpened,
                ConnectionId = connectionId,
                Data = null,
            });
        }

        private void SessionClosed(WebSocketSession session, CloseReason closeReason)
        {
            string sessionId = session.SessionID;
            long connectionId = this.sessionIdToConnectionIdMap[sessionId];

            this.sessionIdToConnectionIdMap.TryRemove(sessionId, out long value1);
            this.connectionIdToSessionMap.TryRemove(connectionId, out WebSocketSession value2);

            if (closeReason == CloseReason.ClientClosing || closeReason == CloseReason.ServerShutdown)
            {
                this.serverEvents.Enqueue(new ServerEvent
                {
                    EventType = ServerEventType.ConnectionClosed,
                    ConnectionId = connectionId,
                    Data = null,
                });
            }
            else
            {
                Lost.Logger.LogFormat("SuperSocketServerTransport: ConnectionId {0} Connection Lost: {1}", connectionId, closeReason);

                this.serverEvents.Enqueue(new ServerEvent
                {
                    EventType = ServerEventType.ConnectionLost,
                    ConnectionId = connectionId,
                    Data = null,
                });
            }
        }

        private bool IsServerStarting()
        {
            ServerState state = this.webSocketServer == null ? ServerState.NotInitialized : this.webSocketServer.State;
            return state == ServerState.Starting;
        }

        private bool IsServerRunning()
        {
            ServerState state = this.webSocketServer == null ? ServerState.NotInitialized : this.webSocketServer.State;
            return state == ServerState.Running;
        }

        private void DestroyWebSocketServer()
        {
            if (this.webSocketServer != null)
            {
                this.webSocketServer.NewDataReceived -= this.MessageRecieved;
                this.webSocketServer.NewSessionConnected -= this.NewSessionConnected;
                this.webSocketServer.SessionClosed -= this.SessionClosed;
                this.webSocketServer = null;
            }
        }
    }
}

#endif
