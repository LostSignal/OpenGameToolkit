//-----------------------------------------------------------------------
// <copyright file="UnityGameServerSubsystem.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    //// All Objects have a NetworkIdentity component(rename to just Identity so I can resuse?)
    //// All Static objects start with OwnerId = -1
    //// At startup Clients ask for state of NetworkIdentity.Id
    //// Server keeps track of every latest NetworkBehaviour message?

    //// NetworkIdetity.RequestUpdate(string resourceName)
    ////   sets the ResourceName and sends the NetworkIdentityRequestUpdate message

    //// NetworkManager.Startup()
    ////    Grab ALL NetworkIdentities in the scene
    ////    Call networkIdentity.RequestUpdate()

    //// NetworkManager.Instantiate(string resourceName)
    ////   Use addressables to create the object
    ////   Give a random NetworkId
    ////   Call networkIdentity.RequestUpdate(resourceName)

    //// Server

    //// NetworkManager.Instantiate
    ////    Creates the prefab and calls UpdateWithServer(resourceName);

    //// Client
    ////     * On Startup collect all Static NetworkIdentities in the scenes
    ////     * NetworkIdentities sends NetworkIdentityRequestUpdate on first Update tick
    ////     * When receives NetworkIdentityUpdate, it either updates existing on NetworkIdentity or Creates one
    ////     * When receives NetworkBehaviourMessage, forwards it onto the NetworkIdentity (if it exists)
    ////     * When NetworkIdentity destoryed (and you own it), send NetworkIdentitiesDestroyed message

    public class UnityGameServerSubsystem : IGameServerSubsystem
    {
        private const long InvalidId = long.MinValue;

        private readonly NetworkReader reader = new NetworkReader(Array.Empty<byte>());
        private readonly ServerState serverState = new ServerState();
        private static readonly OGTLogger Logger = new("Unity Game Server");


        // Message Cache
        private readonly NetworkIdentityUpdate networkIdentityUpdate = new NetworkIdentityUpdate();
        private readonly NetworkBehaviourMessage networkBehaviourMessage = new NetworkBehaviourMessage();
        private readonly NetworkIdentitiesDestroyed networkBehaviourDestoryed = new NetworkIdentitiesDestroyed();

        private GameServer gameServer;
        private long serverId = InvalidId;

        public enum NotifyType
        {
            All,
            AllOthers,
        }

        public string Name => nameof(UnityGameServerSubsystem);

        public void Initialize(GameServer gameServer)
        {
            this.gameServer = gameServer;
            this.gameServer.RegisterMessage<NetworkIdentityRequestUpdate>();
            this.gameServer.RegisterMessage<NetworkIdentityUpdate>();
            this.gameServer.RegisterMessage<NetworkBehaviourMessage>();
            this.gameServer.RegisterMessage<NetworkBehaviourDataMessage>();
            this.gameServer.RegisterMessage<NetworkIdentitiesDestroyed>();
            this.gameServer.RegisterMessage<NetworkIdentityOwnershipRequest>();
            this.gameServer.RegisterMessage<NetworkIdentityReleaseOwnershipRequest>();
        }

        public Task<bool> Run()
        {
            this.serverState.Start();

            this.gameServer.ServerReceivedMessage += this.ServerReceivedMessage;
            this.gameServer.ServerUserConnected += this.ServerUserConnected;
            this.gameServer.ServerUserDisconnected += this.ServerUserDisconnected;

            return Task.FromResult<bool>(true);
        }

        public Task Shutdown()
        {
            this.serverState.Stop();

            this.gameServer.ServerReceivedMessage -= this.ServerReceivedMessage;
            this.gameServer.ServerUserConnected -= this.ServerUserConnected;
            this.gameServer.ServerUserDisconnected -= this.ServerUserDisconnected;

            return Task.Delay(0);
        }

        public Task<bool> AllowPlayerToJoin(UserInfo userInfo)
        {
            return Task.FromResult<bool>(true);
        }

        public Task UpdatePlayerInfo(UserInfo userInfo)
        {
            return Task.Delay(0);
        }

        public void ServerReceivedMessage(UserInfo userInfo, Message message, bool reliable)
        {
            switch (message.GetId())
            {
                case NetworkIdentityRequestUpdate.Id:
                    {
                        var requestUpdateMessage = (NetworkIdentityRequestUpdate)message;
                        var unityNetworkObject = this.serverState.GetUnityNetworkObject(requestUpdateMessage.NetworkId);

                        // Making sure a "Server" player has been selected
                        if (this.serverId == InvalidId)
                        {
                            if (this.gameServer.PrintDebugOutput)
                            {
                                Logger.Log("NetworkIdentityRequestUpdate is Migrating Server...");
                            }

                            this.MigrateServerToNewUser(this.GetNextServerUser(), NotifyType.AllOthers);
                        }

                        if (this.gameServer.PrintDebugOutput)
                        {
                            Logger.Log("NetworkIdentityRequestUpdate " + requestUpdateMessage.NetworkId);
                        }

                        if (this.serverState.DestroyedSceneUnityNetworkObjects.Contains(requestUpdateMessage.NetworkId))
                        {
                            // The object they're requesting an update on was destroyed, so sending a destroyed message
                            this.networkBehaviourDestoryed.DestroyedNetworkIds.Clear();
                            this.networkBehaviourDestoryed.DestroyedNetworkIds.Add(requestUpdateMessage.NetworkId);
                            this.gameServer.SendMessageToUser(userInfo, this.networkBehaviourDestoryed);
                        }
                        else if (unityNetworkObject == null)
                        {
                            bool isSceneObject = requestUpdateMessage.ResourceName.IsNullOrWhitespace();

                            unityNetworkObject = new UnityNetworkObject
                            {
                                OwnerId = isSceneObject ? this.serverId : userInfo.UserId,
                                NetworkId = requestUpdateMessage.NetworkId,
                                IsEnabled = requestUpdateMessage.IsEnabled,
                                Position = requestUpdateMessage.Position,
                                Rotation = requestUpdateMessage.Rotation,
                                ResourceName = requestUpdateMessage.ResourceName,
                                BehaviourDatas = new List<BehaviourData>(requestUpdateMessage.BehaviourCount),
                                DestoryOnDisconnect = requestUpdateMessage.DestoryOnDisconnect,
                                CanChangeOwner = requestUpdateMessage.CanChangeOwner,
                                InitialCanChangeOwner = requestUpdateMessage.CanChangeOwner,
                            };

                            for (int i = 0; i < requestUpdateMessage.BehaviourCount; i++)
                            {
                                unityNetworkObject.BehaviourDatas.Add(new BehaviourData());
                            }

                            if (isSceneObject)
                            {
                                this.serverState.SceneUnityNetworkObjects.Add(unityNetworkObject.NetworkId, unityNetworkObject);
                            }
                            else
                            {
                                this.serverState.DynamicUnityGameObjects.Add(unityNetworkObject.NetworkId, unityNetworkObject);
                            }

                            this.SendIdentityUpdateMessage(unityNetworkObject);
                        }
                        else
                        {
                            this.SendIdentityUpdateMessage(unityNetworkObject, userInfo);
                            this.SendAllNetworkBehaviourDataToUser(unityNetworkObject, userInfo);
                        }

                        break;
                    }

                case NetworkIdentitiesDestroyed.Id:
                    {
                        var destroyedMessage = (NetworkIdentitiesDestroyed)message;

                        if (destroyedMessage.DestroyedNetworkIds?.Count > 0)
                        {
                            for (int i = 0; i < destroyedMessage.DestroyedNetworkIds.Count; i++)
                            {
                                long networkId = destroyedMessage.DestroyedNetworkIds[i];
                                var networkObject = this.serverState.GetUnityNetworkObject(networkId);

                                if (networkObject != null)
                                {
                                    if (networkObject.OwnerId == userInfo.UserId)
                                    {
                                        // If static, remove from static dictionary and add to destoryed static list
                                        // If dynamic, just remove from dynamic dictionary
                                        // Delete the object and forward to everyone
                                    }
                                }
                            }
                        }

                        // If this came from the owner of the object, then add

                        //

                        // Ignore for now, can't get this till ownership changing exists
                        break;
                    }

                case NetworkIdentityUpdate.Id:
                    {
                        // Ignore for now, can't get this till ownership changing exists
                        break;
                    }

                case NetworkBehaviourMessage.Id:
                    {
                        var networkBehaviourMessage = (NetworkBehaviourMessage)message;

                        var networkObject = this.serverState.GetUnityNetworkObject(networkBehaviourMessage.NetworkId);

                        if (networkObject != null && networkObject.OwnerId == userInfo.UserId)
                        {
                            if (networkObject.BehaviourDatas[networkBehaviourMessage.BehaviourIndex] == null)
                            {
                                networkObject.BehaviourDatas[networkBehaviourMessage.BehaviourIndex] = new BehaviourData();
                            }

                            var behaviourData = networkObject.BehaviourDatas[networkBehaviourMessage.BehaviourIndex];
                            behaviourData.Data.SetData(networkBehaviourMessage.DataBytes, networkBehaviourMessage.DataLength);

                            this.gameServer.SendMessageToAllExcept(userInfo, message, reliable);
                        }

                        break;
                    }

                case NetworkBehaviourDataMessage.Id:
                    {
                        var networkBehaviourDataMessage = (NetworkBehaviourDataMessage)message;

                        var networkObject = this.serverState.GetUnityNetworkObject(networkBehaviourDataMessage.NetworkId);

                        if (networkObject != null)
                        {
                            if (networkBehaviourDataMessage.SendType == NetworkBehaviourDataSendType.All)
                            {
                                this.gameServer.SendMessageToAll(networkBehaviourDataMessage);
                            }
                            else if (networkBehaviourDataMessage.SendType == NetworkBehaviourDataSendType.Others)
                            {
                                this.gameServer.SendMessageToAllExcept(userInfo, networkBehaviourDataMessage);
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                        }

                        break;
                    }

                case NetworkIdentityOwnershipRequest.Id:
                    {
                        var networkIdentityOwnershipRequest = (NetworkIdentityOwnershipRequest)message;

                        UnityNetworkObject unityNetworkObject = this.serverState.GetUnityNetworkObject(networkIdentityOwnershipRequest.NetworkId);

                        if (unityNetworkObject != null)
                        {
                            if (unityNetworkObject.CanChangeOwner)
                            {
                                if (this.gameServer.PrintDebugOutput)
                                {
                                    Logger.Log($"User {userInfo.UserId} requesting Ownership of {unityNetworkObject.NetworkId} Granted.");
                                }

                                unityNetworkObject.OwnerId = userInfo.UserId;
                                unityNetworkObject.CanChangeOwner = false;
                            }
                            else
                            {
                                if (this.gameServer.PrintDebugOutput)
                                {
                                    Logger.Log($"User {userInfo.UserId} requesting Ownership of {unityNetworkObject.NetworkId} Failed: CanChangeOwner = false.");
                                }
                            }

                            this.SendIdentityUpdateMessage(unityNetworkObject);     // Telling everyone about the ownership change
                            this.SendAllNetworkBehaviourDataToUser(unityNetworkObject, userInfo);     // Making sure the new owner has all the latest server info
                        }

                        break;
                    }

                case NetworkIdentityReleaseOwnershipRequest.Id:
                    {
                        var networkIdentityReleaseOwnershipRequest = (NetworkIdentityReleaseOwnershipRequest)message;

                        UnityNetworkObject unityNetworkObject = this.serverState.GetUnityNetworkObject(networkIdentityReleaseOwnershipRequest.NetworkId);

                        if (unityNetworkObject != null && unityNetworkObject.OwnerId == userInfo.UserId)
                        {
                            unityNetworkObject.CanChangeOwner = true;

                            //// TODO [bgish]: Possibly set the OwnerId to be whomever is the "Server"

                            this.SendIdentityUpdateMessage(unityNetworkObject);
                        }

                        break;
                    }
            }
        }

        public void ServerUserConnected(UserInfo userInfo, bool isReconnect)
        {
            if (this.gameServer.PrintDebugOutput)
            {
                Logger.Log($"SERVER: User Connected - {userInfo.UserHexId} - {userInfo.DisplayName}");
            }

            // Detecting if we're Migrating the server state to this new user
            if (this.serverId == InvalidId)
            {
                if (this.gameServer.PrintDebugOutput)
                {
                    Logger.Log("ServerUserConnected is Migrating Server...");
                }

                this.MigrateServerToNewUser(userInfo, NotifyType.AllOthers);
            }

            // Telling the newly connected use about all the dynamic objects
            foreach (var dynamic in this.serverState.DynamicUnityGameObjects.Values)
            {
                this.SendIdentityUpdateMessage(dynamic, userInfo);
                this.SendAllNetworkBehaviourDataToUser(dynamic, userInfo);
            }
        }

        public void ServerUserDisconnected(UserInfo userInfo, bool wasConnectionLost)
        {
            if (this.gameServer.PrintDebugOutput)
            {
                Logger.Log($"SERVER: User Connected - {userInfo.UserHexId} - {userInfo.DisplayName}");
            }

            // Making sure to destroy all object this user owns, but DestoryOnDisconnect is true
            this.networkBehaviourDestoryed.DestroyedNetworkIds.Clear();

            foreach (var dynamic in this.serverState.DynamicUnityGameObjects.Values)
            {
                if (dynamic.DestoryOnDisconnect && dynamic.OwnerId == userInfo.UserId)
                {
                    this.networkBehaviourDestoryed.DestroyedNetworkIds.Add(dynamic.NetworkId);
                }
            }

            if (this.networkBehaviourDestoryed.DestroyedNetworkIds.Count > 0)
            {
                this.gameServer.SendMessageToAllExcept(userInfo, this.networkBehaviourDestoryed);

                for (int i = 0; i < this.networkBehaviourDestoryed.DestroyedNetworkIds.Count; i++)
                {
                    this.serverState.DynamicUnityGameObjects.Remove(this.networkBehaviourDestoryed.DestroyedNetworkIds[i]);
                }
            }

            // Detecting if we need to migrate to a new user
            if (this.serverId == userInfo.UserId)
            {
                if (this.gameServer.PrintDebugOutput)
                {
                    Logger.Log("ServerUserDisconnected is Migrating Server...");
                }

                this.MigrateServerToNewUser(this.GetNextServerUser(), NotifyType.All);
            }
        }

        //// var users = this.gameServer?.ConnectedUsers;
        //// int userCount = users?.Count ?? 0;
        //// long oldServerId = this.serverId;
        //// long newServerId = userCount > 0 ? users[0].UserId : InvalidId;
        ////         this.serverId = newServerId;
        ////
        ////         Lost.Logger.Log($"Host Migration Detected: OldServerId = {oldServerId}, NewServerId = {newServerId}, User Count = {userCount}");
        ////
        ////         this.UpdateOwnerForAllUnityNetworkObjects(oldServerId, newServerId);
        ////
        ////         // Notify all users of the owner id change
        ////         if (userCount > 0)
        ////         {
        ////             Lost.Logger.Log($"Notifying All Users of Owner Update...");
        ////
        ////             foreach (var unityNetworkObject in this.serverState.GetAllUnityNetworkObjects())
        ////             {
        ////                 this.SendIdentityUpdateMessage(unityNetworkObject);
        ////            }
        ////        }

        private UserInfo GetNextServerUser()
        {
            int userCount = this.gameServer?.ConnectedUsers?.Count ?? 0;
            return userCount > 0 ? this.gameServer.ConnectedUsers[0] : null;
        }

        private void MigrateServerToNewUser(UserInfo userInfo, NotifyType notifyType)
        {
            long oldServerId = this.serverId;
            long newServerId = userInfo != null ? userInfo.UserId : InvalidId;
            this.serverId = newServerId;

            if (this.gameServer.PrintDebugOutput)
            {
                Logger.Log($"Host Migration Initiated: OldServerId = {oldServerId}, NewServerId = {newServerId}, User Count = {this.gameServer?.ConnectedUsers?.Count}");
            }

            this.UpdateOwnerForAllUnityNetworkObjects(oldServerId, newServerId);

            // Notify all other users of the owner id change
            if (this.gameServer?.ConnectedUsers == null)
            {
                return;
            }

            foreach (var user in this.gameServer.ConnectedUsers)
            {
                if (notifyType == NotifyType.All || user.UserId != userInfo.UserId)
                {
                    if (this.gameServer.PrintDebugOutput)
                    {
                        Logger.Log($"Host Migration is Notifying {user.UserId} about the OwnerId Update...");
                    }

                    foreach (var unityNetworkObject in this.serverState.GetAllUnityNetworkObjects())
                    {
                        this.SendIdentityUpdateMessage(unityNetworkObject);
                    }
                }
            }
        }

        private void UpdateOwnerForAllUnityNetworkObjects(long oldOwnerId, long newOwerId)
        {
            foreach (var unityNetworkObject in this.serverState.GetAllUnityNetworkObjects())
            {
                if (unityNetworkObject.OwnerId == oldOwnerId)
                {
                    unityNetworkObject.OwnerId = newOwerId;
                    unityNetworkObject.CanChangeOwner = unityNetworkObject.InitialCanChangeOwner;
                }
            }
        }

        private void SendIdentityUpdateMessage(UnityNetworkObject unityNetworkObject, UserInfo user = null)
        {
            this.networkIdentityUpdate.NetworkId = unityNetworkObject.NetworkId;
            this.networkIdentityUpdate.OwnerId = unityNetworkObject.OwnerId;
            this.networkIdentityUpdate.IsEnabled = unityNetworkObject.IsEnabled;
            this.networkIdentityUpdate.ResourceName = unityNetworkObject.ResourceName;
            this.networkIdentityUpdate.Position = unityNetworkObject.Position;
            this.networkIdentityUpdate.Rotation = unityNetworkObject.Rotation;
            this.networkIdentityUpdate.CanChangeOwner = unityNetworkObject.CanChangeOwner;

            if (user != null)
            {
                this.gameServer.SendMessageToUser(user, this.networkIdentityUpdate);
            }
            else
            {
                this.gameServer.SendMessageToAll(this.networkIdentityUpdate);
            }
        }

        private void SendAllNetworkBehaviourDataToUser(UnityNetworkObject unityNetworkObject, UserInfo userInfo)
        {
            // Send all the network behaviour data we know about
            this.networkBehaviourMessage.NetworkId = unityNetworkObject.NetworkId;

            for (int i = 0; i < unityNetworkObject.BehaviourDatas.Count; i++)
            {
                var behaviourData = unityNetworkObject.BehaviourDatas[i];

                if (behaviourData?.Data?.Length > 0)
                {
                    this.networkBehaviourMessage.BehaviourIndex = i;
                    this.networkBehaviourMessage.DataBytes = behaviourData.Data.Bytes;
                    this.networkBehaviourMessage.DataLength = behaviourData.Data.Length;

                    this.gameServer.SendMessageToUser(userInfo, this.networkBehaviourMessage);
                }
            }
        }

        private class UnityNetworkObject
        {
            public long NetworkId { get; set; }

            public long OwnerId { get; set; }

            public string ResourceName { get; set; }

            public List<BehaviourData> BehaviourDatas { get; set; }

            public Position3D Position { get; set; }

            public Rotation Rotation { get; set; }

            public bool IsEnabled { get; set; }

            public bool DestoryOnDisconnect { get; set; }

            public bool InitialCanChangeOwner { get; set; }

            public bool CanChangeOwner { get; set; }
        }

        private class BehaviourData
        {
            public RawByteArray Data { get; set; } = new RawByteArray();

            public int Version { get; set; }
        }

        private class RawByteArray
        {
            public int Length { get; private set; }

            public byte[] Bytes { get; private set; }

            public void SetData(byte[] newData, int newDataLength)
            {
                if (this.Bytes == null || newDataLength > this.Bytes.Length)
                {
                    this.Bytes = new byte[(int)(newDataLength * 1.5f)];
                }

                this.Length = newDataLength;

                for (int i = 0; i < newDataLength; i++)
                {
                    this.Bytes[i] = newData[i];
                }
            }

            public void SetData(byte[] data)
            {
                this.SetData(data, data.Length);
            }
        }

        private class ServerState
        {
            public Dictionary<long, UnityNetworkObject> SceneUnityNetworkObjects { get; } = new Dictionary<long, UnityNetworkObject>();

            public HashSet<long> DestroyedSceneUnityNetworkObjects { get; } = new HashSet<long>();

            public Dictionary<long, UnityNetworkObject> DynamicUnityGameObjects { get; } = new Dictionary<long, UnityNetworkObject>();

            public void Start()
            {
                // TODO [bgish]: Make this a Task and Load ServerState from PlayFab
                this.SceneUnityNetworkObjects.Clear();
                this.DestroyedSceneUnityNetworkObjects.Clear();
                this.DynamicUnityGameObjects.Clear();
            }

            public void Stop()
            {
                // TODO [bgish]: Make this a Task and Save ServerState to PlayFab
                this.SceneUnityNetworkObjects.Clear();
                this.DestroyedSceneUnityNetworkObjects.Clear();
                this.DynamicUnityGameObjects.Clear();
            }

            public IEnumerable<UnityNetworkObject> GetAllUnityNetworkObjects()
            {
                foreach (var scene in this.SceneUnityNetworkObjects)
                {
                    yield return scene.Value;
                }

                foreach (var dynamic in this.DynamicUnityGameObjects)
                {
                    yield return dynamic.Value;
                }
            }

            public UnityNetworkObject GetUnityNetworkObject(long networkId)
            {
                if (this.SceneUnityNetworkObjects.TryGetValue(networkId, out UnityNetworkObject sceneObject))
                {
                    return sceneObject;
                }
                else if (this.DynamicUnityGameObjects.TryGetValue(networkId, out UnityNetworkObject dynamicObject))
                {
                    return dynamicObject;
                }

                return null;
            }
        }
    }
}
