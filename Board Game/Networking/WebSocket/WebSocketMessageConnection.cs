//-----------------------------------------------------------------------
// <copyright file="WebSocketMessageConnection.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if true // !UNITY_WEBGL || UNITY_EDITOR

namespace OGT.BoardGame.Networking
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class WebSocketConnectionOptions
    {
        public int ReceiveChunkBytes { get; set; } = 16 * 1024;

        public int MaxMessageBytes { get; set; } = 1024 * 1024;

        public TimeSpan[] ReconnectBackoff { get; set; } =
        {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(4),
            TimeSpan.FromSeconds(8),
            TimeSpan.FromSeconds(16),
            TimeSpan.FromSeconds(30),
        };

        /// <summary>Gets or sets how many consecutive failed attempts are allowed before giving up. Negative means forever.</summary>
        public int MaxReconnectAttempts { get; set; } = -1;

        public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(15);

        /// <summary>Gets or sets how long a graceful <see cref="WebSocketMessageConnection.Disconnect"/> may take before the socket is aborted.</summary>
        public TimeSpan CloseTimeout { get; set; } = TimeSpan.FromSeconds(2);
    }

    /// <summary>
    /// Vanilla .NET <see cref="ClientWebSocket"/> implementation of <see cref="IMessageConnection"/> with automatic reconnect,
    /// serialized sends and whole-message receive. Contains no Unity dependencies. Every attempt asks the url provider for
    /// the same connection url, so a token that expires while disconnected needs a new connection. Not available on WebGL players.
    /// </summary>
    public sealed class WebSocketMessageConnection : IMessageConnection
    {
        private static readonly OGTLogger Logger = OGTLogger.Networking;

        private readonly string connectionString;
        private readonly IWebSocketRoomProtocol protocol;
        private readonly WebSocketConnectionOptions options;
        private readonly ConcurrentQueue<ConnectionEvent> events = new ConcurrentQueue<ConnectionEvent>();
        private readonly ConcurrentQueue<string> outbound = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> controlOutbound = new ConcurrentQueue<string>();
        private readonly SemaphoreSlim sendSignal = new SemaphoreSlim(0);
        private readonly object lifecycleLock = new object();

        private CancellationTokenSource cancellation;
        private Task runTask;
        private readonly string roomCode;
        private int nextAckId;
        private int joinAckId;
        private string pendingConnectionId;
        private volatile bool closeRequested;
        private volatile bool isJoined;
        private volatile bool isDisposed;

        public WebSocketMessageConnection(
            string connectionString,
            string roomCode,
            IWebSocketRoomProtocol protocol = null,
            WebSocketConnectionOptions options = null)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.roomCode = string.IsNullOrEmpty(roomCode) ? throw new ArgumentException("Room code is required", nameof(roomCode)) : roomCode;
            this.protocol = protocol ?? new WebPubSubRoomProtocol();
            this.options = options ?? new WebSocketConnectionOptions();
        }

        public event Action OnConnected;

        public event Action<string> OnDisconnected;

        public event Action<ArraySegment<byte>> OnMessageReceived;

        private enum EventKind
        {
            StateChanged,
            Connected,
            Disconnected,
            Message,
            Warning,
        }

        public ConnectionState State { get; private set; }

        public string LocalConnectionId { get; private set; }

        public void Connect()
        {
            this.ThrowIfDisposed();


            lock (this.lifecycleLock)
            {
                this.StopRunTask();

                this.closeRequested = false;
                this.isJoined = false;
                this.LocalConnectionId = null;
                ClearQueue(this.outbound);
                ClearQueue(this.controlOutbound);

                this.State = ConnectionState.Connecting;
                this.cancellation = new CancellationTokenSource();
                CancellationToken token = this.cancellation.Token;
                this.runTask = Task.Run(() => this.RunAsync(token));
            }
        }

        public void Disconnect()
        {
            lock (this.lifecycleLock)
            {
                if (this.runTask == null || this.closeRequested)
                {
                    return;
                }

                this.closeRequested = true;
                this.controlOutbound.Enqueue(this.protocol.BuildLeave(this.roomCode, this.NextAckId()));
                this.sendSignal.Release();

                // Watchdog: if the graceful close does not finish in time, abort everything.
                CancellationTokenSource watched = this.cancellation;
                Task.Delay(this.options.CloseTimeout).ContinueWith(_ => TryCancel(watched));
            }
        }

        public void Send(byte[] data, int offset, int count)
        {
            this.ThrowIfDisposed();

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (offset < 0 || count < 0 || offset + count > data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (this.runTask == null || this.closeRequested)
            {
                this.events.Enqueue(new ConnectionEvent { Kind = EventKind.Warning, Text = "Send ignored: connection is not open" });
                return;
            }

            this.outbound.Enqueue(this.protocol.BuildSend(this.roomCode, data, offset, count, this.NextAckId()));
            this.sendSignal.Release();
        }

        public void Update()
        {
            while (this.events.TryDequeue(out ConnectionEvent e))
            {
                switch (e.Kind)
                {
                    case EventKind.StateChanged:
                        this.State = e.State;
                        break;

                    case EventKind.Connected:
                        this.LocalConnectionId = e.Text;
                        this.State = ConnectionState.Connected;
                        this.Raise(() => this.OnConnected?.Invoke());
                        break;

                    case EventKind.Disconnected:
                        this.Raise(() => this.OnDisconnected?.Invoke(e.Text));
                        break;

                    case EventKind.Message:
                        this.Raise(() => this.OnMessageReceived?.Invoke(new ArraySegment<byte>(e.Data)));
                        break;

                    case EventKind.Warning:
                        Logger.LogWarning($"WebSocketMessageConnection: {e.Text}");
                        break;
                }
            }
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;

            lock (this.lifecycleLock)
            {
                // Try to leave gracefully; the background task finishes on its own or gets aborted by the watchdog.
                if (this.runTask != null && this.closeRequested == false)
                {
                    this.closeRequested = true;
                    this.controlOutbound.Enqueue(this.protocol.BuildLeave(this.roomCode, this.NextAckId()));
                    this.sendSignal.Release();

                    CancellationTokenSource watched = this.cancellation;
                    Task.Delay(this.options.CloseTimeout).ContinueWith(_ => TryCancel(watched));
                }

                this.runTask = null;
            }

            ClearQueue(this.events);
        }

        private static void ClearQueue<T>(ConcurrentQueue<T> queue)
        {
            while (queue.TryDequeue(out _))
            {
            }
        }

        private static void TryCancel(CancellationTokenSource source)
        {
            try
            {
                source?.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void StopRunTask()
        {
            this.closeRequested = true;
            TryCancel(this.cancellation);
            this.cancellation = null;
            this.runTask = null;
        }

        private int NextAckId()
        {
            return Interlocked.Increment(ref this.nextAckId);
        }

        private void Raise(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Logger.LogError($"WebSocketMessageConnection: exception in event handler: {ex}");
            }
        }

        private void Enqueue(EventKind kind, string text = null, byte[] data = null, ConnectionState state = ConnectionState.Disconnected)
        {
            this.events.Enqueue(new ConnectionEvent { Kind = kind, Text = text, Data = data, State = state });
        }

        private async Task RunAsync(CancellationToken token)
        {
            int failedAttempts = 0;
            bool wasEverJoined = false;

            try
            {
                while (token.IsCancellationRequested == false && this.closeRequested == false)
                {
                    this.Enqueue(EventKind.StateChanged, state: wasEverJoined? ConnectionState.Reconnecting: ConnectionState.Connecting);

                    ClientWebSocket socket = null;
                    string disconnectReason = null;
                    bool joinedThisAttempt = false;

                    try
                    {
                        if (string.IsNullOrEmpty(this.connectionString))
                        {
                            throw new InvalidOperationException("The connection string is empty");
                        }

                        socket = new ClientWebSocket();

                        if (string.IsNullOrEmpty(this.protocol.SubProtocol) == false)
                        {
                            socket.Options.AddSubProtocol(this.protocol.SubProtocol);
                        }

                        using (var connectCancellation = CancellationTokenSource.CreateLinkedTokenSource(token))
                        {
                            connectCancellation.CancelAfter(this.options.ConnectTimeout);
                            await socket.ConnectAsync(new Uri(this.connectionString, UriKind.Absolute), connectCancellation.Token).ConfigureAwait(false);
                        }

                        this.isJoined = false;
                        this.joinAckId = 0;
                        ClearQueue(this.controlOutbound);

                        using (var socketCancellation = CancellationTokenSource.CreateLinkedTokenSource(token))
                        {
                            Task receiveTask = this.ReceiveLoopAsync(socket, socketCancellation.Token);
                            Task sendTask = this.SendLoopAsync(socket, socketCancellation.Token);

                            Task finished = await Task.WhenAny(receiveTask, sendTask).ConfigureAwait(false);
                            joinedThisAttempt = this.isJoined;
                            socketCancellation.Cancel();

                            try
                            {
                                await finished.ConfigureAwait(false);
                            }
                            catch (OperationCanceledException)
                            {
                            }
                            catch (Exception ex)
                            {
                                disconnectReason = ex.Message;
                            }

                            try
                            {
                                await Task.WhenAll(receiveTask, sendTask).ConfigureAwait(false);
                            }
                            catch
                            {
                                // Already captured the interesting reason above
                            }
                        }

                        disconnectReason ??= this.closeRequested ? "Disconnect requested" : "Connection closed";
                    }
                    catch (OperationCanceledException)
                    {
                        disconnectReason = this.closeRequested ? "Disconnect requested" : "Cancelled";
                    }
                    catch (Exception ex)
                    {
                        disconnectReason = ex.Message;
                    }
                    finally
                    {
                        if (socket != null)
                        {
                            try
                            {
                                socket.Abort();
                            }
                            catch
                            {
                            }

                            socket.Dispose();
                        }

                        this.isJoined = false;
                    }

                    if (joinedThisAttempt)
                    {
                        wasEverJoined = true;
                        failedAttempts = 0;
                        this.Enqueue(EventKind.Disconnected, disconnectReason);
                    }
                    else
                    {
                        failedAttempts++;
                        this.Enqueue(EventKind.Warning, $"Connect attempt {failedAttempts} to room {this.roomCode} failed: {disconnectReason}");
                    }

                    if (this.closeRequested || token.IsCancellationRequested)
                    {
                        break;
                    }

                    if (this.options.MaxReconnectAttempts >= 0 && failedAttempts > this.options.MaxReconnectAttempts)
                    {
                        this.Enqueue(EventKind.Warning, $"Giving up on room {this.roomCode} after {failedAttempts} failed attempts");
                        break;
                    }

                    TimeSpan[] backoff = this.options.ReconnectBackoff;
                    TimeSpan delay = backoff == null || backoff.Length == 0
                        ? TimeSpan.FromSeconds(1)
                        : backoff[Math.Min(Math.Max(failedAttempts - 1, 0), backoff.Length - 1)];

                    try
                    {
                        await Task.Delay(delay, token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
            finally
            {
                this.Enqueue(EventKind.StateChanged, state: ConnectionState.Disconnected);
            }
        }

        private async Task ReceiveLoopAsync(ClientWebSocket socket, CancellationToken token)
        {
            byte[] buffer = new byte[this.options.ReceiveChunkBytes];
            var stream = new MemoryStream();

            while (token.IsCancellationRequested == false)
            {
                WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), token).ConfigureAwait(false);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    if (this.closeRequested)
                    {
                        return;
                    }

                    throw new WebSocketException($"Closed by server: {result.CloseStatus} {result.CloseStatusDescription}");
                }

                if (stream.Length + result.Count > this.options.MaxMessageBytes)
                {
                    throw new InvalidOperationException($"Received message exceeds {this.options.MaxMessageBytes} bytes");
                }

                stream.Write(buffer, 0, result.Count);

                if (result.EndOfMessage == false)
                {
                    continue;
                }

                string text = result.MessageType == WebSocketMessageType.Text
                    ? Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length)
                    : null;

                stream.SetLength(0);

                if (text != null && this.protocol.TryParseInbound(text, out InboundFrame frame))
                {
                    this.HandleFrame(frame);
                }
            }
        }

        private void HandleFrame(InboundFrame frame)
        {
            switch (frame.Type)
            {
                case InboundFrameType.Connected:
                    this.pendingConnectionId = frame.ConnectionId;
                    this.joinAckId = this.NextAckId();
                    this.controlOutbound.Enqueue(this.protocol.BuildJoin(this.roomCode, this.joinAckId));
                    this.sendSignal.Release();
                    break;

                case InboundFrameType.Ack:
                    if (this.joinAckId != 0 && frame.AckId == this.joinAckId)
                    {
                        if (frame.Success == false)
                        {
                            throw new InvalidOperationException($"Joining room {this.roomCode} was rejected: {frame.Error}");
                        }

                        this.isJoined = true;
                        this.Enqueue(EventKind.Connected, this.pendingConnectionId);
                        this.sendSignal.Release();
                    }
                    else if (frame.Success == false)
                    {
                        this.Enqueue(EventKind.Warning, $"Message ack {frame.AckId} failed: {frame.Error}");
                    }

                    break;

                case InboundFrameType.GroupMessage:
                    if (frame.Data != null)
                    {
                        this.Enqueue(EventKind.Message, data: frame.Data);
                    }

                    break;

                case InboundFrameType.Disconnected:
                    throw new WebSocketException($"Server disconnected: {frame.Reason}");
            }
        }

        private async Task SendLoopAsync(ClientWebSocket socket, CancellationToken token)
        {
            while (token.IsCancellationRequested == false)
            {
                await this.sendSignal.WaitAsync(token).ConfigureAwait(false);

                while (this.controlOutbound.TryDequeue(out string control))
                {
                    await SendTextAsync(socket, control, token).ConfigureAwait(false);
                }

                if (this.isJoined)
                {
                    while (this.outbound.TryDequeue(out string message))
                    {
                        await SendTextAsync(socket, message, token).ConfigureAwait(false);
                    }
                }

                if (this.closeRequested && this.controlOutbound.IsEmpty)
                {
                    await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Client disconnect", token).ConfigureAwait(false);
                    return;
                }
            }
        }

        private static Task SendTextAsync(ClientWebSocket socket, string text, CancellationToken token)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            return socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, token);
        }

        private void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(WebSocketMessageConnection));
            }
        }

        private struct ConnectionEvent
        {
            public EventKind Kind;
            public ConnectionState State;
            public string Text;
            public byte[] Data;
        }
    }
}

#endif
