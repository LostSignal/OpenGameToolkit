#pragma warning disable

////-----------------------------------------------------------------------
// <copyright file="BestHttpClientTransport.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_BEST_HTTP_PRO

namespace OGT.Networking
{
    using System;
    using System.Collections.Concurrent;
    using BestHTTP;
    using BestHTTP.WebSocket;
    using OGT.Networking;
    using UnityEngine;

    public class BestHttpClientTransport : IClientTransportLayer
    {
        private ConcurrentQueue<ClientEvent> clientEvents = new ConcurrentQueue<ClientEvent>();
        private bool isApplicationShuttingDown = false;
        private WebSocket webSocket = null;

        bool IClientTransportLayer.IsConnecting
        {
            get { return this.webSocket != null && this.webSocket.State == WebSocketStates.Connecting; }
        }

        bool IClientTransportLayer.IsConnected
        {
            get { return this.webSocket != null && this.webSocket.IsOpen; }
        }

        public BestHttpClientTransport()
        {
            // TODO [bgish]: Do I need to Setup the proxy?
            // HTTPManager.Proxy = new HTTPProxy(Uri address, Credentials credentials, false /*turn on tunneling*/, false /*sendWholeUri*/, bool nonTransparentForHttps);
            HTTPUpdateDelegator.OnBeforeApplicationQuit += this.OnApplicationQuitting;
        }

        void IClientTransportLayer.Connect(string connectionString)
        {
            this.DestoryConnection();

            if (this.webSocket == null)
            {
                HTTPManager.ConnectTimeout = TimeSpan.FromSeconds(1.0);

                // Create the WebSocket instance
                this.webSocket = new WebSocket(new Uri(connectionString));

#if !UNITY_WEBGL
                this.webSocket.StartPingThread = true;
#endif

                // Subscribe to the WS events
                this.webSocket.OnOpen += this.OnOpen;
                this.webSocket.OnBinary += this.OnBinaryReceived;
                this.webSocket.OnClosed += this.OnClosed;
                this.webSocket.OnError += this.OnError;
                this.webSocket.OnErrorDesc += this.OnErrorDescription;

                // Start connecting to the server
                this.webSocket.Open();
            }
        }

        void IClientTransportLayer.Update()
        {
        }

        void IClientTransportLayer.SendData(byte[] data, uint offset, uint length)
        {
            this.webSocket?.Send(data, offset, length);
        }

        void IClientTransportLayer.Shutdown()
        {
            this.DestoryConnection();
        }

        bool IClientTransportLayer.TryDequeueClientEvent(out ClientEvent clientEvent)
        {
            return this.clientEvents.TryDequeue(out clientEvent);
        }

        private void OnOpen(WebSocket ws)
        {
            OGT.Logger.Log("BestHttpClientTransport: Connection Opened");

            this.clientEvents.Enqueue(new ClientEvent
            {
                EventType = ClientEventType.ConnectionOpened,
                Data = null,
            });
        }

        private void OnBinaryReceived(WebSocket ws, byte[] data)
        {
            OGT.Logger.Log("BestHttpClientTransport: Binary Received");

            this.clientEvents.Enqueue(new ClientEvent
            {
                EventType = ClientEventType.ReceivedData,
                Data = data,
            });
        }

        private void OnClosed(WebSocket ws, UInt16 code, string message)
        {
            OGT.Logger.LogFormat("BestHttpClientTransport: WebSocket closed with Code: {0} Message: {1}", code, message);

            this.DestoryConnection();

            if (code == (ushort)WebSocketStausCodes.NormalClosure)
            {
                this.clientEvents.Enqueue(new ClientEvent
                {
                    EventType = ClientEventType.ConnectionClosed,
                    Data = null,
                });
            }
            else
            {
                this.clientEvents.Enqueue(new ClientEvent
                {
                    EventType = ClientEventType.ConnectionLost,
                    Data = null,
                });
            }
        }

        private void OnError(WebSocket ws, Exception ex)
        {
            // Checking for a know error when the application is shutting down
#if !UNITY_WEBGL || UNITY_EDITOR
            if (this.isApplicationShuttingDown && ws.InternalRequest.Response != null && ws.InternalRequest.Response.StatusCode == 101 && ws.InternalRequest.Response.Message == "Switching Protocols")
            {
                Lost.Logger.Log("BestHttpClientTransport: Skipping Known Error.");
                return;
            }
#endif

            string errorMsg = string.Empty;

#if !UNITY_WEBGL || UNITY_EDITOR
            if (ws.InternalRequest.Response != null)
            {
                errorMsg = string.Format("Status Code from Server: {0} and Message: {1}", ws.InternalRequest.Response.StatusCode, ws.InternalRequest.Response.Message);
            }
#endif

            Lost.Logger.LogErrorFormat("BestHttpClientTransport: An error occured {0}", (ex != null ? ex.Message : "Unknown Error " + errorMsg));

            this.DestoryConnection();
        }

        private void OnErrorDescription(WebSocket ws, string reason)
        {
            // Checking for a know error when the application is shutting down
            if (this.isApplicationShuttingDown && reason == "Request Aborted!")
            {
                Lost.Logger.Log("BestHttpClientTransport: Skipping Known Error Description.");
                return;
            }

            Lost.Logger.LogErrorFormat("BestHttpClientTransport: An error description occured: {0}", reason);
        }

        private bool OnApplicationQuitting()
        {
            this.isApplicationShuttingDown = true;
            this.DestoryConnection();

            HTTPManager.OnUpdate();

            return true;
        }

        private void DestoryConnection()
        {
            if (this.webSocket != null)
            {
                this.webSocket.Close();

                // NOTE [bgish]: This is a very special case, if the app is shutting down we
                //               still want to get any errors that might be happening, so
                //               we're going to keep these conections around.
                //if (this.isApplicationShuttingDown == false)
                {
                    this.webSocket.OnOpen -= this.OnOpen;
                    this.webSocket.OnBinary -= this.OnBinaryReceived;
                    this.webSocket.OnClosed -= this.OnClosed;
                    this.webSocket.OnError -= this.OnError;
                    this.webSocket.OnErrorDesc -= this.OnErrorDescription;
                }

                this.webSocket = null;
            }
        }
    }
}

#endif
