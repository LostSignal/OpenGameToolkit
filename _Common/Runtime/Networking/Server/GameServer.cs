//-----------------------------------------------------------------------
// <copyright file="GameServer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public class GameServer
    {
        private static readonly OGTLogger Logger = new("Game Server");

        private readonly IServerTransportLayer transportLayer;
        private readonly MessageCollection messageCollection;

        // Used for serializing messages to a byte buffer
        private readonly NetworkWriter messageWriter = new NetworkWriter();

        // User management
        private readonly Dictionary<long, UserInfo> connectionIdToUserInfoMap = new Dictionary<long, UserInfo>();
        private readonly List<UserInfo> users = new List<UserInfo>();
        private readonly HashSet<long> knownUserIds = new HashSet<long>();
        private readonly ReadOnlyCollection<UserInfo> readonlyUsersList;

        // Subsystem Tracking
        private readonly List<IGameServerSubsystem> subsystems = new List<IGameServerSubsystem>();

        private readonly ConcurrentList<UserInfo> failedJoinConnectionIds = new ConcurrentList<UserInfo>(20);
        private readonly List<UserInfo> failed = new List<UserInfo>();

        private readonly ConcurrentList<UserInfo> successfulJoinUserInfos = new ConcurrentList<UserInfo>(20);
        private readonly List<UserInfo> successes = new List<UserInfo>();

        // Tracking data
        private readonly ServerStats stats = new ServerStats();

        // Events
        private ServerUserConnectedDelegate serverUserConnected;
        private ServerUserInfoUpdatedDelegate serverUserInfoUpdated;
        private ServerUserDisconnectedDelegate serverUserDisconnected;
        private ServerReceivedMessageDelegate serverReceivedMessage;
        private ServerStartedDelegate serverStarted;
        private ServerStartedDelegate serverUpdated;
        private ServerShutdownDelegate serverShutdown;

        public GameServer(IServerTransportLayer transportLayer)
        {
            this.transportLayer = transportLayer;
            this.messageCollection = new MessageCollection();
            this.readonlyUsersList = new ReadOnlyCollection<UserInfo>(this.users);

            // client to server
            this.messageCollection.RegisterMessage<JoinServerRequestMessage>();
            this.messageCollection.RegisterMessage<UpdateUserInfoMessage>();

            // server to client
            this.messageCollection.RegisterMessage<JoinServerResponseMessage>();
            this.messageCollection.RegisterMessage<UserDisconnectedMessage>();
            this.messageCollection.RegisterMessage<UserInfoMessage>();
        }

        public delegate void ServerUserConnectedDelegate(UserInfo userInfo, bool isReconnect);

        public delegate void ServerUserInfoUpdatedDelegate(UserInfo userInfo);

        public delegate void ServerUserDisconnectedDelegate(UserInfo userInfo, bool wasConnectionLost);

        public delegate void ServerReceivedMessageDelegate(UserInfo userInfo, Message message, bool reliable);

        public delegate void ServerStartedDelegate();

        public delegate void ServerUpdatedDelegate();

        public delegate void ServerShutdownDelegate();

        public event ServerUserConnectedDelegate ServerUserConnected
        {
            add => this.serverUserConnected += value;
            remove => this.serverUserConnected -= value;
        }

        public event ServerUserInfoUpdatedDelegate ServerUserInfoUpdated
        {
            add => this.serverUserInfoUpdated += value;
            remove => this.serverUserInfoUpdated -= value;
        }

        public event ServerUserDisconnectedDelegate ServerUserDisconnected
        {
            add => this.serverUserDisconnected += value;
            remove => this.serverUserDisconnected -= value;
        }

        public event ServerReceivedMessageDelegate ServerReceivedMessage
        {
            add => this.serverReceivedMessage += value;
            remove => this.serverReceivedMessage -= value;
        }

        public event ServerStartedDelegate ServerStarted
        {
            add => this.serverStarted += value;
            remove => this.serverStarted -= value;
        }

        public event ServerStartedDelegate ServerUpdated
        {
            add => this.serverUpdated += value;
            remove => this.serverUpdated -= value;
        }

        public event ServerShutdownDelegate ServerShutdown
        {
            add => this.serverShutdown += value;
            remove => this.serverShutdown -= value;
        }

        public IGameServerStats Stats
        {
            get { return this.stats; }
        }

        public bool IsStarting
        {
            get { return this.transportLayer.IsStarting; }
        }

        public bool IsRunning
        {
            get { return this.transportLayer.IsRunning; }
        }

        public bool PrintDebugOutput => this.transportLayer.PrintDebugOutput;

        public ReadOnlyCollection<UserInfo> ConnectedUsers
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.readonlyUsersList;
        }

        public GameServer RegisterSubsystem<T>()
            where T : IGameServerSubsystem, new()
        {
            var subsystem = new T();
            subsystem.Initialize(this);
            this.subsystems.Add(subsystem);

            return this;
        }

        public void RegisterMessage<T>()
            where T : Message, new()
        {
            this.messageCollection.RegisterMessage<T>();
        }

        public bool Start(int port, bool printDebugOutput)
        {
            if (this.transportLayer.IsRunning == false)
            {
                this.stats.StartTime = DateTime.UtcNow;
                this.transportLayer.Start(port, printDebugOutput);

                if (this.transportLayer.IsRunning)
                {
                    bool success = this.StartSubsystems().Result;

                    if (success)
                    {
                        this.serverStarted?.Invoke();
                        return true;
                    }
                    else
                    {
                        this.transportLayer.Shutdown();
                        return false;
                    }
                }
            }

            return true;
        }

        public virtual void Shutdown()
        {
            if (this.transportLayer.IsRunning)
            {
                this.stats.ShutdownTime = DateTime.UtcNow;
                this.StopSubsystems().Wait();

                this.transportLayer.Shutdown();

                this.serverShutdown?.Invoke();
            }
        }

        public void SendMessageToUser(UserInfo userInfo, Message message, bool reliable = true)
        {
            this.SendMessageToConnection(userInfo.ConnectionId, message, reliable);
        }

        public void SendMessageToConnection(long connectionId, Message message, bool reliable = true)
        {
            this.GetDataFromMessage(message, out byte[] data, out uint length);
            this.SendData(connectionId, data, length, reliable);
        }

        public void SendMessageToAll(Message message, bool reliable = true)
        {
            this.GetDataFromMessage(message, out byte[] data, out uint length);

            for (int i = 0; i < this.users.Count; i++)
            {
                this.SendData(this.users[i].ConnectionId, data, length, reliable);
            }
        }

        public void SendMessageToAllExcept(UserInfo userInfo, Message message, bool reliable = true)
        {
            this.GetDataFromMessage(message, out byte[] data, out uint length);

            for (int i = 0; i < this.users.Count; i++)
            {
                if (this.users[i].UserId == userInfo.UserId)
                {
                    continue;
                }

                this.SendData(this.users[i].ConnectionId, data, length, reliable);
            }
        }

        public void AddCustomDataToUser(UserInfo userInfo, string key, string value)
        {
            if (this.users == null)
            {
                return;
            }

            foreach (var user in this.users)
            {
                if (user.UserId == userInfo.UserId)
                {
                    if (user.CustomData.ContainsKey(key))
                    {
                        user.CustomData[key] = value;
                    }
                    else
                    {
                        user.CustomData.Add(key, value);
                    }

                    // Sending the update off to all the clients
                    if (this.PrintDebugOutput)
                    {
                        Logger.Log($"Updateing UserData {user.UserId.ToString()}");
                    }

                    var userInfoMessage = (UserInfoMessage)this.messageCollection.GetMessage(UserInfoMessage.Id);
                    userInfoMessage.UserInfo = user;
                    this.SendMessageToAll(userInfoMessage);
                    this.messageCollection.RecycleMessage(userInfoMessage);

                    break;
                }
            }
        }

        public virtual void Update()
        {
            this.transportLayer?.Update();

            while (this.transportLayer.TryDequeueServerEvent(out ServerEvent serverEvent))
            {
                switch (serverEvent.EventType)
                {
                    case ServerEventType.ConnectionOpened:
                        this.ConnectionOpened(serverEvent.ConnectionId);
                        break;

                    case ServerEventType.ConnectionClosed:
                        this.ConnectionClosed(serverEvent.ConnectionId);
                        break;

                    case ServerEventType.ConnectionLost:
                        this.ConnectionLost(serverEvent.ConnectionId);
                        break;

                    case ServerEventType.ReceivedData:
                        this.ReceivedData(serverEvent.ConnectionId, serverEvent.Data, serverEvent.Reliable);
                        break;

                    default:
                        Logger.LogErrorFormat("Found unknown ServerEvent type {0}", serverEvent.EventType.ToString());
                        break;
                }
            }

            // Sending out all our failed join messages
            this.failedJoinConnectionIds.GetItems(this.failed);

            if (this.failed.Count > 0)
            {
                foreach (var fail in this.failed)
                {
                    this.SendJoinServerResponse(fail.ConnectionId, fail.UserId, false);
                }

                this.failedJoinConnectionIds.RemoveItems(this.failed);
                this.failed.Clear();
            }

            // Sending out all our success join messages
            this.successfulJoinUserInfos.GetItems(this.successes);

            if (this.successes.Count > 0)
            {
                foreach (var success in this.successes)
                {
                    this.SendJoinServerResponse(success.ConnectionId, success.UserId, true);
                    this.AddOrUpdateUserInfo(success, true);
                }

                this.successfulJoinUserInfos.RemoveItems(this.successes);
                this.successes.Clear();
            }

            try
            {
                this.serverUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Encountered an exception updating the server: {ex.Message}");
                Logger.LogException(ex);
            }
        }

        private void ConnectionOpened(long connectionId)
        {
            // Do nothing about this connection till we get a RequestJoinServerMessage
        }

        private void ConnectionClosed(long connectionId)
        {
            this.CloseConnection(connectionId, false);
        }

        private void ConnectionLost(long connectionId)
        {
            this.stats.NumberOfConnectionDrops++;
            this.CloseConnection(connectionId, true);
        }

        private void ReceivedData(long connectionId, byte[] data, bool reliable)
        {
            Message message = this.messageCollection.GetMessage(data);

            this.connectionIdToUserInfoMap.TryGetValue(connectionId, out UserInfo userInfo);

            switch (message.GetId())
            {
                case JoinServerRequestMessage.Id:
                {
                    var requestJoinServer = (JoinServerRequestMessage)message;

                    // Checking if this connection has already registered a user info, if so, they've already joined
                    if (userInfo != null)
                    {
                        Logger.LogErrorFormat("Got a JoinServerRequestMessage from ConnectionId {0} who has already joined.", userInfo.ConnectionId.ToString());
                        this.SendJoinServerResponse(connectionId, userInfo.UserId, true);
                        break;
                    }

                    if (requestJoinServer.UserInfo != null)
                    {
                        if (this.PrintDebugOutput)
                        {
                            Logger.LogFormat("Received JoinServerRequestMessage from UserId {0}", requestJoinServer.UserInfo.UserId.ToString());
                        }

                        requestJoinServer.UserInfo.ConnectionId = connectionId;
                        _ = this.RequestJoinServer(new UserInfo(requestJoinServer.UserInfo));
                    }
                    else
                    {
                        Logger.LogError("JoinServerRequestMessage had a null UserInfo object.");
                        this.SendJoinServerResponse(connectionId, -1, false);
                    }

                    break;
                }

                case UpdateUserInfoMessage.Id:
                {
                    if (userInfo == null)
                    {
                        Logger.LogError("Unregistered user tried to send a messages.");
                        break;
                    }

                    var updateUserInfoMessage = (UpdateUserInfoMessage)message;

                    if (updateUserInfoMessage.UserInfo != null)
                    {
                        updateUserInfoMessage.UserInfo.ConnectionId = connectionId;
                    }
                    else
                    {
                        Logger.LogError("UpdateUserInfoMessage had a null UserInfo object.");
                        break;
                    }

                    if (this.PrintDebugOutput)
                    {
                        Logger.LogFormat("Received UpdateUserInfoMessage from UserId {0}", (object)updateUserInfoMessage.UserInfo.UserId.ToString());
                    }

                    // checking is someone is trying to hack by changing their userId to something else
                    if (userInfo.UserId != updateUserInfoMessage.UserInfo.UserId)
                    {
                        Logger.LogErrorFormat("User {0} is trying to change thier Id to {1}", (object)userInfo.UserId.ToString(), (object)updateUserInfoMessage.UserInfo.UserId.ToString());
                    }
                    else
                    {
                        this.AddOrUpdateUserInfo(updateUserInfoMessage.UserInfo);
                    }

                    break;
                }

                default:
                {
                    if (userInfo == null)
                    {
                        Logger.LogError("Unregistered user tried to send a messages.");
                    }
                    else
                    {
                        this.serverReceivedMessage?.Invoke(userInfo, message, reliable);
                    }

                    break;
                }
            }

            this.messageCollection.RecycleMessage(message);
        }

        private void AddOrUpdateUserInfo(UserInfo userInfoUpdate, bool sendToAll = false)
        {
            UserInfoMessage userInfoMessage = null;
            UserInfo existingUserInfo = null;
            UserInfo newUserInfo = null;

            for (int i = 0; i < this.users.Count; i++)
            {
                if (this.users[i].UserId == userInfoUpdate.UserId)
                {
                    existingUserInfo = this.users[i];
                }
            }

            // checking if this is just a user info updated
            if (existingUserInfo != null)
            {
                existingUserInfo.CopyFrom(userInfoUpdate);
                this.serverUserInfoUpdated?.Invoke(existingUserInfo);
            }
            else
            {
                // if we got here, then this is a newly connected user
                newUserInfo = new UserInfo();
                newUserInfo.CopyFrom(userInfoUpdate);

                // making sure this user is now in the list and map
                this.users.Add(newUserInfo);

                bool isReconnect = this.knownUserIds.Contains(newUserInfo.UserId);

                this.knownUserIds.Add(newUserInfo.UserId);
                this.stats.NumberOfReconnects += (uint)(isReconnect ? 1 : 0);

                // Telling base class and event that someone has connected
                this.serverUserConnected?.Invoke(newUserInfo, isReconnect);

                // Since this user is new, send them all the known user infos to them
                userInfoMessage = (UserInfoMessage)this.messageCollection.GetMessage(UserInfoMessage.Id);

                for (int i = 0; i < this.users.Count; i++)
                {
                    userInfoMessage.UserInfo = this.users[i];
                    this.SendMessageToUser(newUserInfo, userInfoMessage);
                }

                this.messageCollection.RecycleMessage(userInfoMessage);
            }

            UserInfo user = newUserInfo ?? existingUserInfo;

            // telling all other users that someone's info has been updated
            userInfoMessage = (UserInfoMessage)this.messageCollection.GetMessage(UserInfoMessage.Id);
            userInfoMessage.UserInfo = user;

            if (sendToAll)
            {
                this.SendMessageToAll(userInfoMessage);
            }
            else
            {
                this.SendMessageToAllExcept(user, userInfoMessage);
            }

            this.messageCollection.RecycleMessage(userInfoMessage);

            // Making sure the user's connectionId is in the map
            if (this.connectionIdToUserInfoMap.ContainsKey(user.ConnectionId) == false)
            {
                this.connectionIdToUserInfoMap.Add(user.ConnectionId, user);
            }

            // Updating max connected stat
            if (this.users.Count > this.stats.MaxConnectedUsers)
            {
                this.stats.MaxConnectedUsers = (uint)this.users.Count;
            }
        }

        private void CloseConnection(long connectionId, bool connectionLost)
        {
            if (this.connectionIdToUserInfoMap.TryGetValue(connectionId, out UserInfo userInfo) == false)
            {
                return;
            }

            if (userInfo.ConnectionId != connectionId)
            {
                Logger.LogErrorFormat("Trying to close old connection {0}.  User {1} already has connectionId {2}", (object)connectionId.ToString(), (object)userInfo.UserId.ToString(), (object)userInfo.ConnectionId.ToString());
                return;
            }

            // Removing this connection from all our maps and lists
            this.connectionIdToUserInfoMap.Remove(userInfo.ConnectionId);

            int usersRemovedCount = 0;
            for (int i = this.users.Count - 1; i >= 0; i--)
            {
                if (this.users[i].UserId == userInfo.UserId)
                {
                    this.users.RemoveAt(i);
                    usersRemovedCount++;
                }
            }

            Logger.AssertFormat(usersRemovedCount == 1, "Couldn't properly remove users {0}.  Found {1} when should have found 1", (object)userInfo.UserId.ToString(), (object)usersRemovedCount.ToString());

            // Send the disconnected message to all remaining users
            var userDisconnected = (UserDisconnectedMessage)this.messageCollection.GetMessage(UserDisconnectedMessage.Id);
            userDisconnected.UserId = userInfo.UserId;
            userDisconnected.WasConnectionLost = connectionLost;
            this.SendMessageToAll(userDisconnected);
            this.messageCollection.RecycleMessage(userDisconnected);

            // Telling child classes of the disconnect
            this.serverUserDisconnected?.Invoke(userInfo, connectionLost);
        }

        private void SendData(long connectionId, byte[] data, uint length, bool reliable = true)
        {
            if (this.transportLayer.IsRunning)
            {
                this.stats.MessagesSent += 1;
                this.stats.BytesSent += length;
                this.transportLayer.SendData(connectionId, data, 0, length, reliable);
            }
        }

        private void GetDataFromMessage(Message message, out byte[] data, out uint length)
        {
            this.messageWriter.SeekZero();
            message.Serialize(this.messageWriter);

            data = this.messageWriter.RawBuffer;
            length = (uint)this.messageWriter.Position;
        }

        private async Task<bool> StartSubsystems()
        {
            var runs = new List<Task<bool>>(this.subsystems.Count);

            foreach (var subsystem in this.subsystems)
            {
                runs.Add(subsystem.Run());
            }

            await Task.WhenAll(runs);

            return runs.Any(x => x.Result == false) == false;
        }

        private Task StopSubsystems()
        {
            var runs = new List<Task>(this.subsystems.Count);

            foreach (var subsystem in this.subsystems)
            {
                runs.Add(subsystem.Shutdown());
            }

            return Task.WhenAll(runs);
        }

        private async Task RequestJoinServer(UserInfo userInfo)
        {
            // Making sure all the subsystems say yes
            foreach (var subsystem in this.subsystems)
            {
                if (await subsystem.AllowPlayerToJoin(userInfo) == false)
                {
                    Logger.LogError($"Subsystem {subsystem.Name.ToString()} failed user {userInfo.UserId.ToString()}");
                    this.failedJoinConnectionIds.Add(userInfo);
                    return;
                }
            }

            // Making sure all subsystems have a chance to alter the players custom data
            foreach (var subsystem in this.subsystems)
            {
                await subsystem.UpdatePlayerInfo(userInfo);
            }

            this.successfulJoinUserInfos.Add(userInfo);
        }

        private void SendJoinServerResponse(long connectionId, long userId, bool success)
        {
            if (this.PrintDebugOutput)
            {
                Logger.Log($"Sending JoinServerResponseMessage to UserId {userId.ToString()} With Accepted = {success.ToString()}");
            }

            var joinServerResponse = (JoinServerResponseMessage)this.messageCollection.GetMessage(JoinServerResponseMessage.Id);
            joinServerResponse.Accepted = success;
            this.SendMessageToConnection(connectionId, joinServerResponse);
            this.messageCollection.RecycleMessage(joinServerResponse);
        }

        private class ServerStats : IGameServerStats
        {
            public DateTime StartTime { get; set; }

            public DateTime ShutdownTime { get; set; }

            public uint MessagesSent { get; set; }

            public uint BytesSent { get; set; }

            public uint MaxConnectedUsers { get; set; }

            public uint NumberOfReconnects { get; set; }

            public uint NumberOfConnectionDrops { get; set; }
        }
    }
}
