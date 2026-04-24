//-----------------------------------------------------------------------
// <copyright file="GameClient.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    //// TODO [bgish]: Keep a bool called isConnectedToServer and turn it on/off where appropiate
    ////               If the user tries to send a message while not connected, then throw an error.

    public class GameClient
    {
        private static readonly OGTLogger Logger = new("Game Client");

        private readonly IClientTransportLayer transportLayer;
        private readonly MessageCollection messageCollection;
        private readonly UserInfo myUserInfo;
        private readonly bool printDebugOutput;

        // Used for serializing messages to a byte buffer
        private readonly NetworkWriter messageWriter = new NetworkWriter();

        // User/connection management
        private readonly Dictionary<long, UserInfo> userIdToUserInfoMap = new Dictionary<long, UserInfo>();
        private readonly HashSet<long> knownUserIds = new HashSet<long>();
        private readonly List<UserInfo> users = new List<UserInfo>();
        private readonly ReadOnlyCollection<UserInfo> readonlyUsersList;

        // Subsystem Tracking
        private readonly List<IGameClientSubsystem> subsystems = new List<IGameClientSubsystem>();

        private string lastKnownConnectionString = null;

        // Delegate Events
        private ClientUserConnectedDelegate clientUserConnected;
        private ClientUserDisconnectedDelegate clientUserDisconnected;
        private ClientUserInfoUpdatedDelegate clientUserInfoUpdated;
        private ClientReceivedMessageDelegate clientReceivedMessage;
        private ClientConnectedToServerDelegate clientConnectedToServer;
        private ClientFailedToConnectToServerDelegate clientFailedToConnectToServer;
        private ClientDisconnectedFromServerDelegate clientDisconnectedFromServer;
        private ClientLostConnectionToServerDelegate clientLostConnectionToServer;
        private ClientUpdatedDelegate clientUpdated;

        public GameClient(IClientTransportLayer transportLayer, UserInfo userInfo, bool printDebugOutput)
        {
            this.transportLayer = transportLayer;
            this.UserId = userInfo.UserId;
            this.printDebugOutput = printDebugOutput;
            this.readonlyUsersList = new ReadOnlyCollection<UserInfo>(this.users);

            this.myUserInfo = new UserInfo();
            this.myUserInfo.CopyFrom(userInfo);

            this.messageCollection = new MessageCollection();

            // client to server
            this.messageCollection.RegisterMessage<JoinServerRequestMessage>();
            this.messageCollection.RegisterMessage<UpdateUserInfoMessage>();

            // server to client
            this.messageCollection.RegisterMessage<JoinServerResponseMessage>();
            this.messageCollection.RegisterMessage<UserDisconnectedMessage>();
            this.messageCollection.RegisterMessage<UserInfoMessage>();
        }

        public delegate void ClientUserConnectedDelegate(UserInfo userInfo, bool wasReconnect);

        public delegate void ClientUserDisconnectedDelegate(UserInfo userInfo, bool wasConnectionLost);

        public delegate void ClientUserInfoUpdatedDelegate(UserInfo userInfo);

        public delegate void ClientReceivedMessageDelegate(Message message);

        public delegate void ClientConnectedToServerDelegate();

        public delegate void ClientFailedToConnectToServerDelegate();

        public delegate void ClientDisconnectedFromServerDelegate();

        public delegate void ClientLostConnectionToServerDelegate();

        public delegate void ClientUpdatedDelegate();

        public event ClientUserConnectedDelegate ClientUserConnected
        {
            add => this.clientUserConnected += value;
            remove => this.clientUserConnected -= value;
        }

        public event ClientUserDisconnectedDelegate ClientUserDisconnected
        {
            add => this.clientUserDisconnected += value;
            remove => this.clientUserDisconnected -= value;
        }

        public event ClientUserInfoUpdatedDelegate ClientUserInfoUpdated
        {
            add => this.clientUserInfoUpdated += value;
            remove => this.clientUserInfoUpdated -= value;
        }

        public event ClientReceivedMessageDelegate ClientReceivedMessage
        {
            add => this.clientReceivedMessage += value;
            remove => this.clientReceivedMessage -= value;
        }

        public event ClientConnectedToServerDelegate ClientConnectedToServer
        {
            add => this.clientConnectedToServer += value;
            remove => this.clientConnectedToServer -= value;
        }

        public event ClientFailedToConnectToServerDelegate ClientFailedToConnectToServer
        {
            add => this.clientFailedToConnectToServer += value;
            remove => this.clientFailedToConnectToServer -= value;
        }

        public event ClientDisconnectedFromServerDelegate ClientDisconnectedFromServer
        {
            add => this.clientDisconnectedFromServer += value;
            remove => this.clientDisconnectedFromServer -= value;
        }

        public event ClientLostConnectionToServerDelegate ClientLostConnectionToServer
        {
            add => this.clientLostConnectionToServer += value;
            remove => this.clientLostConnectionToServer -= value;
        }

        public event ClientUpdatedDelegate ClientUpdated
        {
            add => this.clientUpdated += value;
            remove => this.clientUpdated -= value;
        }

        public bool PrintDebugOutput => this.printDebugOutput;

        public ReadOnlyCollection<UserInfo> ConnectedUsers
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.readonlyUsersList;
        }

        public long UserId { get; private set; }

        public bool HasJoinedServer { get; private set; }

        public UserInfo UserInfo => this.myUserInfo;

        public bool IsConnecting
        {
            get { return this.transportLayer.IsConnecting; }
        }

        public bool IsConnected
        {
            get { return this.transportLayer.IsConnected; }
        }

        public void RegisterMessage<T>()
            where T : Message, new()
        {
            this.messageCollection.RegisterMessage<T>();
        }

        public GameClient RegisterSubsystem<T>()
            where T : IGameClientSubsystem, new()
        {
            var subsystem = new T();
            subsystem.Initialize(this);
            this.subsystems.Add(subsystem);

            return this;
        }

        public GameClient RegisterSubsystem(IGameClientSubsystem subsystem)
        {
            subsystem.Initialize(this);
            this.subsystems.Add(subsystem);
            return this;
        }

        public T GetSubsystem<T>()
            where T : IGameClientSubsystem
        {
            foreach (var subsystem in this.subsystems)
            {
                if (subsystem is T subsystemT)
                {
                    return subsystemT;
                }
            }

            return default;
        }

        public void Connect(string connectionString)
        {
            Logger.Assert(string.IsNullOrEmpty(connectionString) == false, "Can't connect to null or empty connection string!");

            if (this.lastKnownConnectionString != null && this.lastKnownConnectionString != connectionString && this.IsConnected)
            {
                this.Shutdown();
            }
            else if (this.IsConnected && this.lastKnownConnectionString == connectionString)
            {
                return;
            }

            // if we're trying to connect to a new server, then reset everything
            if (connectionString != this.lastKnownConnectionString)
            {
                this.Reset();
                this.lastKnownConnectionString = connectionString;
            }

            if (this.transportLayer.IsConnected == false)
            {
                this.transportLayer.Connect(connectionString);
                this.StartSubsystems();
            }
        }

        public void SendMessage(Message message, bool reliable = true)
        {
            if (this.HasJoinedServer == false && message.GetId() != JoinServerRequestMessage.Id)
            {
                // TODO [bgish]: throw some sort of error?
                return;
            }

            this.messageWriter.SeekZero();
            message.Serialize(this.messageWriter);

            if (reliable)
            {
                this.transportLayer.SendData(this.messageWriter.RawBuffer, 0, (uint)this.messageWriter.Position);
            }
            else
            {
                this.transportLayer.SendDataUnreliable(this.messageWriter.RawBuffer, 0, (uint)this.messageWriter.Position);
            }
        }

        public void Shutdown()
        {
            this.StopSubsystems();
            this.Reset();
        }

        public void Update()
        {
            this.transportLayer?.Update();

            while (this.transportLayer.TryDequeueClientEvent(out ClientEvent clientEvent))
            {
                switch (clientEvent.EventType)
                {
                    case ClientEventType.ConnectionOpened:
                        this.ConnectionOpened();
                        break;

                    case ClientEventType.ConnectionClosed:
                        this.ConnectionClosed();
                        break;

                    case ClientEventType.ConnectionLost:
                        this.ConnectionLost();
                        break;

                    case ClientEventType.ReceivedData:
                        this.ReceivedData(clientEvent.Data);
                        break;

                    default:
                        Logger.LogErrorFormat("Found unknown ClientEvent type {0}", clientEvent.EventType);
                        break;
                }
            }

            try
            {
                this.clientUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Encountered an exception updating the client: {ex.Message}");
                Logger.LogException(ex);
            }
        }

        public void UpdateUserInfo(UserInfo myUserInfo)
        {
            this.myUserInfo.CopyFrom(myUserInfo);

            var requestUpdateUserInfo = (UpdateUserInfoMessage)this.messageCollection.GetMessage(UpdateUserInfoMessage.Id);
            requestUpdateUserInfo.UserInfo = this.myUserInfo;

            this.SendMessage(requestUpdateUserInfo);

            this.messageCollection.RecycleMessage(requestUpdateUserInfo);
        }

        private void ConnectionOpened()
        {
            var requestJoinServer = (JoinServerRequestMessage)this.messageCollection.GetMessage(JoinServerRequestMessage.Id);
            requestJoinServer.UserInfo = this.myUserInfo;

            this.SendMessage(requestJoinServer);

            this.messageCollection.RecycleMessage(requestJoinServer);
        }

        private void ConnectionClosed()
        {
            this.Reset();
            this.clientDisconnectedFromServer?.Invoke();
        }

        private void ConnectionLost()
        {
            // NOTE [bgish]: If we lost connectoin, we may want to reconnect, so lets say
            //               everyone's connection was lost and not reset everything.
            for (int i = 0; i < this.users.Count; i++)
            {
                this.clientUserDisconnected?.Invoke(this.users[i], true);
            }

            this.users.Clear();
            this.userIdToUserInfoMap.Clear();
            this.HasJoinedServer = false;

            this.clientLostConnectionToServer?.Invoke();
        }

        private void ReceivedData(byte[] data)
        {
            Message message = this.messageCollection.GetMessage(data);

            switch (message.GetId())
            {
                case JoinServerResponseMessage.Id:
                    var joinServerResponse = (JoinServerResponseMessage)message;

                    if (this.printDebugOutput)
                    {
                        Logger.LogFormat("JoinServerResponseMessage.Accepted = {0}", joinServerResponse.Accepted);
                    }

                    if (joinServerResponse.Accepted)
                    {
                        this.HasJoinedServer = true;
                        this.clientConnectedToServer?.Invoke();
                    }
                    else
                    {
                        this.clientFailedToConnectToServer?.Invoke();
                    }

                    break;

                case UserInfoMessage.Id:
                    {
                        var userInfoMessage = (UserInfoMessage)message;

                        if (userInfoMessage.UserInfo.UserId != this.UserId)
                        {
                            if (this.printDebugOutput)
                            {
                                Logger.Log($"Received UserInfoMessage for {userInfoMessage.UserInfo}");
                            }

                            this.AddOrUpdateUserInfo(userInfoMessage.UserInfo);
                        }
                        else
                        {
                            if (this.printDebugOutput)
                            {
                                Logger.Log($"Received UserInfoMessage For Myself {userInfoMessage.UserInfo}");
                            }

                            this.myUserInfo.CopyFrom(userInfoMessage.UserInfo);
                            this.clientUserInfoUpdated?.Invoke(this.myUserInfo);
                        }

                        break;
                    }

                case UserDisconnectedMessage.Id:
                    {
                        var userDisconnectedMessage = (UserDisconnectedMessage)message;
                        long userId = userDisconnectedMessage.UserId;

                        Logger.LogFormat("UserDisconnectedMessage For UserId {0}", userDisconnectedMessage.UserId);

                        UserInfo removedUserInfo = this.RemoveUserInfo(userId);

                        if (removedUserInfo != null)
                        {
                            this.clientUserDisconnected?.Invoke(removedUserInfo, userDisconnectedMessage.WasConnectionLost);
                        }

                        break;
                    }

                default:
                    break;
            }

            this.clientReceivedMessage?.Invoke(message);

            this.messageCollection.RecycleMessage(message);
        }

        private UserInfo AddOrUpdateUserInfo(UserInfo messageUserInfo)
        {
            if (this.userIdToUserInfoMap.TryGetValue(messageUserInfo.UserId, out UserInfo userInfo))
            {
                userInfo.CopyFrom(messageUserInfo);

                this.clientUserInfoUpdated?.Invoke(userInfo);
            }
            else
            {
                userInfo = new UserInfo();
                userInfo.CopyFrom(messageUserInfo);

                this.userIdToUserInfoMap.Add(userInfo.UserId, userInfo);
                this.users.Add(userInfo);

                bool wasReconnect = this.knownUserIds.Contains(userInfo.UserId);
                this.knownUserIds.Add(userInfo.UserId);

                this.clientUserConnected?.Invoke(userInfo, wasReconnect);
            }

            return userInfo;
        }

        private UserInfo RemoveUserInfo(long userId)
        {
            if (this.userIdToUserInfoMap.TryGetValue(userId, out UserInfo userInfo))
            {
                this.userIdToUserInfoMap.Remove(userId);
                this.users.Remove(userInfo);

                return userInfo;
            }

            return null;
        }

        private void Reset()
        {
            this.lastKnownConnectionString = null;
            this.HasJoinedServer = false;
            this.users.Clear();
            this.userIdToUserInfoMap.Clear();
            this.knownUserIds.Clear();

            if (this.transportLayer.IsConnected)
            {
                this.StopSubsystems();
                this.transportLayer.Shutdown();
            }
        }

        private void StartSubsystems()
        {
            foreach (var subsystem in this.subsystems)
            {
                subsystem.Start();
            }
        }

        private void StopSubsystems()
        {
            foreach (var subsystem in this.subsystems)
            {
                subsystem.Stop();
            }
        }
    }
}
