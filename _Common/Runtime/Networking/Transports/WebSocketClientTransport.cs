#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="WebSocketClientTransport.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    using System;
    using System.Collections.Concurrent;
    using System.Net;
    using System.Net.Security;
    using System.Net.WebSockets;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;

    public class WebSocketClientTransport : IClientTransportLayer
    {
        private ConcurrentQueue<ClientEvent> clientEvents = new ConcurrentQueue<ClientEvent>();
        private ClientWebSocket clientWebSocket;
        private string connectionString;

        private CancellationTokenSource connectCancellationToken;
        private Timer checkConnectionTimer;
        private Task connectTask;

        bool IClientTransportLayer.IsConnecting => this.GetIsConnecting();

        bool IClientTransportLayer.IsConnected => this.GetIsConnected();

        void IClientTransportLayer.Connect(string connectionString)
        {
            this.Shutdown();

            this.connectionString = connectionString;
            this.clientWebSocket = new ClientWebSocket();
            this.clientWebSocket.Options.AddSubProtocol("Tls");

            // HACK [bgish]: Just accept any server certificate that comes our way
            ServicePointManager.ServerCertificateValidationCallback = delegate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors policyErrors)
            {
                return true;
            };

            this.connectCancellationToken = new CancellationTokenSource();
            this.connectTask = Task.Run(() => this.Connect(this.connectCancellationToken));
            this.checkConnectionTimer = new Timer(this.CheckConnection, null, 0, 1000);
        }

        void IClientTransportLayer.SendData(byte[] data, uint offset, uint length)
        {
            ArraySegment<byte> bytesToSend = new ArraySegment<byte>(data, (int)offset, (int)length);
            this.clientWebSocket?.SendAsync(bytesToSend, WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        void IClientTransportLayer.SendDataUnreliable(byte[] data, uint offset, uint length)
        {
            ArraySegment<byte> bytesToSend = new ArraySegment<byte>(data, (int)offset, (int)length);
            this.clientWebSocket?.SendAsync(bytesToSend, WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        void IClientTransportLayer.Shutdown()
        {
            this.Shutdown();
        }

        bool IClientTransportLayer.TryDequeueClientEvent(out ClientEvent clientEvent)
        {
            return this.clientEvents.TryDequeue(out clientEvent);
        }

        void IClientTransportLayer.Update()
        {
        }

        private bool GetIsConnected()
        {
            return this.clientWebSocket?.State == WebSocketState.Open;
        }

        private bool GetIsConnecting()
        {
            return this.clientWebSocket?.State == WebSocketState.Connecting;
        }

        private void Shutdown(bool connectionLost = false)
        {
            if (this.connectCancellationToken != null)
            {
                this.connectCancellationToken.Cancel();
                this.connectCancellationToken = null;
                this.connectTask = null;
            }

            if (this.clientWebSocket != null)
            {
                if (this.clientWebSocket.State == WebSocketState.Open || this.clientWebSocket.State == WebSocketState.Connecting)
                {
                    this.clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "WebSocketClientTransport.Shutdown()", CancellationToken.None);
                }

                this.clientWebSocket = null;

                this.clientEvents.Enqueue(new ClientEvent
                {
                    EventType = connectionLost ? ClientEventType.ConnectionLost : ClientEventType.ConnectionClosed,
                    Data = null,
                });
            }

            if (this.checkConnectionTimer != null)
            {
                this.checkConnectionTimer.Dispose();
                this.checkConnectionTimer = null;
            }

            this.connectionString = null;
        }

        private async Task Connect(CancellationTokenSource cancellationTokenSource)
        {
            CancellationToken cancelationToken = cancellationTokenSource.Token;

            try
            {
                await this.clientWebSocket.ConnectAsync(new Uri(this.connectionString, UriKind.Absolute), cancelationToken);

                while (this.GetIsConnecting() && cancelationToken.IsCancellationRequested == false)
                {
                    await Task.Delay(200);
                }

                if (this.GetIsConnected() == false)
                {
                    return;
                }

                this.clientEvents.Enqueue(new ClientEvent
                {
                    EventType = ClientEventType.ConnectionOpened,
                    Data = null,
                });

                ArraySegment<byte> bytesReceived = new ArraySegment<byte>(new byte[1024]);
                WebSocketReceiveResult result = null;

                while (this.GetIsConnected() && cancelationToken.IsCancellationRequested == false)
                {
                    result = await this.clientWebSocket.ReceiveAsync(bytesReceived, cancelationToken);

                    // Catching if the connection was closed
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        this.Shutdown(false);
                        return;
                    }

                    // Copying data into new byte array
                    byte[] data = new byte[result.Count];

                    for (int i = 0; i < result.Count; i++)
                    {
                        data[i] = bytesReceived.Array[bytesReceived.Offset + i];
                    }

                    this.clientEvents.Enqueue(new ClientEvent
                    {
                        EventType = ClientEventType.ReceivedData,
                        Data = data,
                    });
                }
            }
            catch (OperationCanceledException)
            {
                // Do Nothing
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                this.Shutdown(true);
            }
        }

        private void CheckConnection(object state)
        {
            if (this.clientWebSocket.State == WebSocketState.Closed || this.clientWebSocket.State == WebSocketState.Aborted || this.clientWebSocket.State == WebSocketState.CloseReceived)
            {
                this.Shutdown();
            }
        }
    }
}
