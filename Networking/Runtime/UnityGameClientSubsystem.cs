//-----------------------------------------------------------------------
// <copyright file="UnityGameClientSubsystem.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using UnityEngine.SceneManagement;

    public class UnityGameClientSubsystem : IGameClientSubsystem
    {
        private static readonly NetworkReader Reader = new NetworkReader(System.Array.Empty<byte>());
        private static readonly OGTLogger Logger = new("Unity Game Client");

        // private static readonly NetworkIdentityUpdate updateNetworkIdentityCache = new NetworkIdentityUpdate();
        private static readonly Dictionary<string, NetworkIdentity> ResourcePrefabCache = new Dictionary<string, NetworkIdentity>();

        // Dynamic Network Object Tracking
        private readonly Dictionary<long, NetworkIdentity> dynamicNetworkObjectsHash = new Dictionary<long, NetworkIdentity>();
        private readonly List<NetworkIdentity> dynamicNetworkObjectsList = new List<NetworkIdentity>();
        private readonly Dictionary<long, NetworkIdentity> sceneNetworkObjectsHash = new Dictionary<long, NetworkIdentity>();
        private readonly List<NetworkIdentity> sceneNetworkObjectsList = new List<NetworkIdentity>();
        private readonly List<NetworkIdentity> onSceneLoadIdentitiesCache = new List<NetworkIdentity>();

        private SpawnManager spawnManager;
        private GameClient gameClient;
        private bool isConnected;

        public UnityGameClientSubsystem(SpawnManager spawnManager)
        {
            this.spawnManager = spawnManager;
        }

        public void Initialize(GameClient gameClient)
        {
            this.gameClient = gameClient;
            this.gameClient.RegisterMessage<NetworkIdentityRequestUpdate>();
            this.gameClient.RegisterMessage<NetworkIdentityUpdate>();
            this.gameClient.RegisterMessage<NetworkIdentitiesDestroyed>();
            this.gameClient.RegisterMessage<NetworkBehaviourMessage>();
            this.gameClient.RegisterMessage<NetworkBehaviourDataMessage>();
            this.gameClient.RegisterMessage<NetworkIdentityOwnershipRequest>();
            this.gameClient.RegisterMessage<NetworkIdentityReleaseOwnershipRequest>();
        }

        public void Start()
        {
            this.gameClient.ClientReceivedMessage += this.ClientReceivedMessage;
            this.gameClient.ClientConnectedToServer += this.ClientConnected;
            this.gameClient.ClientDisconnectedFromServer += this.ClientDisconnected;

            this.sceneNetworkObjectsList.Clear();
            this.sceneNetworkObjectsHash.Clear();

            // Collecting all the scene network objects from every scene
            NetworkIdentity.GetAllSceneNetworkIdentities(this.sceneNetworkObjectsList);

            // Hashing all the scene network objects for quicker lookup
            foreach (var identity in this.sceneNetworkObjectsList)
            {
                this.AddSceneNetworkIdentityToHash(identity);
            }

            // Making sure we don't miss any if there's a scene load
            SceneManager.sceneLoaded += this.OnSceneLoaded;
        }

        public void Stop()
        {
            this.gameClient.ClientReceivedMessage -= this.ClientReceivedMessage;
            this.gameClient.ClientConnectedToServer -= this.ClientConnected;
            this.gameClient.ClientDisconnectedFromServer -= this.ClientDisconnected;

            this.sceneNetworkObjectsList.Clear();
            this.sceneNetworkObjectsHash.Clear();

            if (Platform.IsApplicationQuitting == false)
            {
                // Destroy all dynamic objects
                foreach (var dynamic in this.dynamicNetworkObjectsList)
                {
                    if (dynamic && dynamic.gameObject)
                    {
                        this.spawnManager.Despawn(dynamic.gameObject);
                    }
                }

                this.dynamicNetworkObjectsList.Clear();
                this.dynamicNetworkObjectsHash.Clear();
            }

            SceneManager.sceneLoaded -= this.OnSceneLoaded;
        }

        public void ClientReceivedMessage(Message message)
        {
            switch (message.GetId())
            {
                case NetworkIdentityUpdate.Id:
                {
                    var networkIdentityUpdatedMessage = (NetworkIdentityUpdate)message;

                    if (this.sceneNetworkObjectsHash.TryGetValue(networkIdentityUpdatedMessage.NetworkId, out NetworkIdentity sceneIdentity))
                    {
                        if (sceneIdentity)
                        {
                            networkIdentityUpdatedMessage.PopulateNetworkIdentity(sceneIdentity, false);
                        }
                        else
                        {
                            Logger.LogError($"Scene NetworkIdentity {networkIdentityUpdatedMessage.NetworkId} has been destoryed, but still getting updates.");
                        }
                    }
                    else if (this.dynamicNetworkObjectsHash.TryGetValue(networkIdentityUpdatedMessage.NetworkId, out NetworkIdentity dynamicIdentity))
                    {
                        networkIdentityUpdatedMessage.PopulateNetworkIdentity(dynamicIdentity, false);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(networkIdentityUpdatedMessage.ResourceName) == false)
                        {
                            var newIdentity = this.CreateDynamicNetworkIdentity(
                                networkIdentityUpdatedMessage.ResourceName,
                                networkIdentityUpdatedMessage.NetworkId,
                                networkIdentityUpdatedMessage.OwnerId,
                                networkIdentityUpdatedMessage.Position.ToVector3(),
                                networkIdentityUpdatedMessage.Rotation.ToQuaternion());

                            networkIdentityUpdatedMessage.PopulateNetworkIdentity(newIdentity, true);
                        }
                    }

                    break;
                }

                case NetworkBehaviourMessage.Id:
                {
                    var networkBehaviourMessage = (NetworkBehaviourMessage)message;

                    NetworkIdentity identity = this.GetNetworkIdentity(networkBehaviourMessage.NetworkId);

                    if (identity && identity.IsOwner == false)
                    {
                        Reader.Replace(networkBehaviourMessage.DataBytes);
                        identity.Behaviours[networkBehaviourMessage.BehaviourIndex].Deserialize(Reader);
                    }

                    break;
                }

                case NetworkBehaviourDataMessage.Id:
                {
                    var networkBehaviourDataMessage = (NetworkBehaviourDataMessage)message;

                    NetworkIdentity identity = this.GetNetworkIdentity(networkBehaviourDataMessage.NetworkId);

                    if (identity)
                    {
                        identity.Behaviours[networkBehaviourDataMessage.BehaviourIndex].OnReceiveNetworkData(networkBehaviourDataMessage.DataKey, networkBehaviourDataMessage.DataValue);
                    }

                    break;
                }

                case NetworkIdentitiesDestroyed.Id:
                {
                    var networkIdentitiesDestoryed = (NetworkIdentitiesDestroyed)message;

                    foreach (long networkId in networkIdentitiesDestoryed.DestroyedNetworkIds)
                    {
                        NetworkIdentity identity = this.GetNetworkIdentity(networkId);

                        if (identity)
                        {
                            this.spawnManager.Despawn(identity.gameObject);
                        }
                    }

                    break;
                }
            }
        }

        public void ClientConnected()
        {
            if (this.gameClient.PrintDebugOutput)
            {
                Logger.Log("UnityGameClientSubsystem: Client Connected");
            }

            this.isConnected = true;

            foreach (var sceneObject in this.sceneNetworkObjectsList)
            {
                sceneObject.RequestUpdate();
            }

            foreach (var dynamicObject in this.dynamicNetworkObjectsList)
            {
                dynamicObject.RequestUpdate();
            }
        }

        public void ClientDisconnected()
        {
            if (this.gameClient.PrintDebugOutput)
            {
                Logger.Log("UnityGameClientSubsystem: Client Disconnected");
            }

            this.isConnected = false;
        }

        public NetworkIdentity CreateDynamicNetworkIdentity(string resourceName, long networkId, long ownerId, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation)
        {
            if (this.gameClient.PrintDebugOutput)
            {
                Logger.Log($"UnityGameClientSubsystem: CreateDynamicNetworkIdentity({resourceName}, {networkId}, {ownerId})");
            }

            if (ResourcePrefabCache.TryGetValue(resourceName, out NetworkIdentity networkIdentityPrefab) == false)
            {
                networkIdentityPrefab = UnityEngine.Resources.Load<NetworkIdentity>(resourceName);
                ResourcePrefabCache.Add(resourceName, networkIdentityPrefab);
            }

            var newNetworkIdentity = UnityEngine.GameObject.Instantiate(networkIdentityPrefab);
            newNetworkIdentity.SetClient(this.gameClient);
            newNetworkIdentity.SetNetworkId(networkId);
            newNetworkIdentity.SetOwner(ownerId, newNetworkIdentity.CanChangeOwner);
            newNetworkIdentity.ResourceName = resourceName;
            newNetworkIdentity.transform.SetPositionAndRotation(position, rotation);

            if (this.isConnected)
            {
                newNetworkIdentity.RequestUpdate();
            }

            // Registering the new dynamic object
            this.dynamicNetworkObjectsHash.Add(newNetworkIdentity.NetworkId, newNetworkIdentity);
            this.dynamicNetworkObjectsList.Add(newNetworkIdentity);

            return newNetworkIdentity;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            this.onSceneLoadIdentitiesCache.Clear();
            NetworkIdentity.GetNetworkIdentitiesFromScene(this.onSceneLoadIdentitiesCache, scene);

            // Adding these newly loaded network identities to our list of identities
            this.sceneNetworkObjectsList.AddRange(this.onSceneLoadIdentitiesCache);

            // Adding these newly loaded network identities to our hash as well
            foreach (var identity in this.onSceneLoadIdentitiesCache)
            {
                this.AddSceneNetworkIdentityToHash(identity);
            }
        }

        private void AddSceneNetworkIdentityToHash(NetworkIdentity identity)
        {
            if (this.sceneNetworkObjectsHash.ContainsKey(identity.NetworkId))
            {
                Logger.LogError($"Multiple Network Identities with same Id: {identity.name} and {this.sceneNetworkObjectsHash[identity.NetworkId].name}");
                return;
            }

            this.sceneNetworkObjectsHash.Add(identity.NetworkId, identity);
            identity.SetClient(this.gameClient);

            if (this.isConnected)
            {
                identity.RequestUpdate();
            }
        }

        private NetworkIdentity GetNetworkIdentity(long networkId)
        {
            bool foundSceneIdentity = this.sceneNetworkObjectsHash.TryGetValue(networkId, out NetworkIdentity sceneIdentity);
            bool foundDynamicIdentity = this.dynamicNetworkObjectsHash.TryGetValue(networkId, out NetworkIdentity dynamicIdentity);

            return foundSceneIdentity ? sceneIdentity :
                foundDynamicIdentity? dynamicIdentity:
                null;
        }
    }

    public static class Extensions
    {
        public static void PopulateMessage(this NetworkIdentityRequestUpdate update, NetworkIdentity identity)
        {
            update.NetworkId = identity.NetworkId;
            update.IsEnabled = identity.gameObject.activeSelf;
            update.ResourceName = identity.ResourceName;
            update.Position = identity.transform.position.ToPosition();
            update.Rotation = identity.transform.rotation.ToRotation();
            update.BehaviourCount = identity.Behaviours.Length;
            update.DestoryOnDisconnect = identity.DestoryOnDisconnect;
            update.CanChangeOwner = identity.CanChangeOwner;
        }

        public static void PopulateMessage(this NetworkIdentityUpdate update, NetworkIdentity identity)
        {
            update.NetworkId = identity.NetworkId;
            update.OwnerId = identity.OwnerId;
            update.IsEnabled = identity.gameObject.activeSelf;
            update.ResourceName = identity.ResourceName;
            update.CanChangeOwner = identity.CanChangeOwner;
            update.Position = NetworkTransformAnchor.InverseTransformPosition(identity.transform.position).ToPosition();
            update.Rotation = NetworkTransformAnchor.InverseTransformRotation(identity.transform.rotation).ToRotation();
        }

        public static void PopulateNetworkIdentity(this NetworkIdentityUpdate update, NetworkIdentity identity, bool updatePositionAndRotation)
        {
            identity.SetOwner(update.OwnerId, update.CanChangeOwner);
            identity.gameObject.SafeSetActive(update.IsEnabled);
            identity.ResourceName = identity.ResourceName;

            if (updatePositionAndRotation)
            {
                identity.transform.SetPositionAndRotation(
                    NetworkTransformAnchor.TransformPosition(update.Position.ToVector3()),
                    NetworkTransformAnchor.TransformRotation(update.Rotation.ToQuaternion()));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Position3D ToPosition(this UnityEngine.Vector3 vector)
        {
            return new Position3D(vector.x, vector.y, vector.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rotation ToRotation(this UnityEngine.Quaternion rotation)
        {
            return new Rotation(rotation.x, rotation.y, rotation.z, rotation.w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Vector3 ToVector3(this Position3D position)
        {
            return new UnityEngine.Vector3(position.X, position.Y, position.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Quaternion ToQuaternion(this Rotation rotation)
        {
            return new UnityEngine.Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);
        }
    }
}
