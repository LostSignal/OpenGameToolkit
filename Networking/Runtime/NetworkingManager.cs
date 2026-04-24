//-----------------------------------------------------------------------
// <copyright file="NetworkingManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using OGT;
    using UnityEngine;

    public interface IRoomProvider
    {
        Task<NetworkConnectionInfo> CreateOrJoinRoom(string roomName);
    }

    public class NetworkConnectionInfo
    {
        public string Ip { get; set; }

        public int Port { get; set; }
    }

    public sealed class NetworkingManager : Manager
    {
        private const string ValidMatchNameCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        private static readonly OGTLogger Logger = OGTLogger.Networking;

#pragma warning disable 0649
        [SerializeField] private Settings settings;
#pragma warning restore 0649

        public static NetworkingManager Instance
        {
            get
            {
                Debug.LogError("NetworkingManager.Instance no longer supported");
                return GameObject.FindAnyObjectByType<Bootloader>().FindManager<NetworkingManager>();
            }
        }

        private readonly ReadOnlyCollection<UserInfo> emptyConnectedUsersList = new ReadOnlyCollection<UserInfo>(new List<UserInfo>());
        private ConnectedUsersUpdatedDelegate onConnectedUsersUpdated;
        private bool originalRunInBackground;
        private bool isConnected;

        private IGameServerFactory gameServerFactory;
        private IGameClientFactory gameClientFactory;

        private Bootloader bootloader;
        private GameServer gameServer;
        private GameClient gameClient;

        public delegate void ConnectedUsersUpdatedDelegate();

        public event ConnectedUsersUpdatedDelegate OnConnectedUsersUpdated
        {
            add => this.OnConnectedUsersUpdated += value;
            remove => this.OnConnectedUsersUpdated -= value;
        }

        public static bool PrintDebugOutput
        {
            get
            {
                Logger.LogError("NetworkingManager.PrintDebugOutput is obsolete, use Logging Categories instead.");
                return GameObject.FindAnyObjectByType<Bootloader>().FindManager<NetworkingManager>().settings.PrintDebugOutput;
            }
        }

        public enum NetworkingMode
        {
            RunClientAndServer,
            RunClientAndLANServer,
            RunClientAndCloudServer,
        }

        protected override Task InitializeManager(Bootloader bootloader)
        {
            this.bootloader = bootloader;

            Application.quitting += this.OnApplicationQuit;
            Platform.OnUpdate += (e, o) => this.Update();

            this.originalRunInBackground = Application.runInBackground;

            return Task.CompletedTask;
        }

        public NetworkingMode Mode
        {
            get
            {
                return Application.isEditor ? this.settings.EditorNetworkingMode : this.settings.NetworkingMode;
            }

            set
            {
                if (Application.isEditor)
                {
                    this.settings.EditorNetworkingMode = value;
                }
                else
                {
                    this.settings.NetworkingMode = value;
                }
            }
        }

        public bool HasJoinedServer => this.gameClient?.HasJoinedServer == true;

        public ReadOnlyCollection<UserInfo> ConnectedUsers => this.gameClient?.ConnectedUsers ?? this.emptyConnectedUsersList;

        public static string GenerateRandomRoomName()
        {
            System.Random random = new System.Random();

            return BetterStringBuilder.New()
                .Append(ValidMatchNameCharacters[random.Next(0, ValidMatchNameCharacters.Length)])
                .Append(ValidMatchNameCharacters[random.Next(0, ValidMatchNameCharacters.Length)])
                .Append(ValidMatchNameCharacters[random.Next(0, ValidMatchNameCharacters.Length)])
                .Append(ValidMatchNameCharacters[random.Next(0, ValidMatchNameCharacters.Length)])
                .ToString();
        }

        //// public override void Initialize()
        //// {
        ////     this.StartCoroutine(Coroutine());
        //// 
        ////     IEnumerator Coroutine()
        ////     {
        ////         yield return UserInfoManager.WaitForInitialization();
        ////         this.SetInstance(this);
        ////     }
        //// }

        public void SetGameServerFactory(IGameServerFactory gameServerFactory)
        {
            this.gameServerFactory = gameServerFactory;
        }

        public void SetGameClientFactory(IGameClientFactory gameClientFactory)
        {
            this.gameClientFactory = gameClientFactory;
        }

        public void SendClientMessage(Message message)
        {
            this.gameClient?.SendMessage(message);
        }

        public NetworkIdentity InstantiateNetworkIdentity(string resourceName, Vector3 position)
        {
            return this.InstantiateNetworkIdentity(resourceName, position, Quaternion.identity);
        }

        public NetworkIdentity InstantiateNetworkIdentity(string resourceName, Vector3 position, Quaternion rotation)
        {
            var userInfoManager = this.bootloader.FindManager<IUserInfoManager>();
            var subsystem = this.gameClient.GetSubsystem<UnityGameClientSubsystem>();

            return subsystem.CreateDynamicNetworkIdentity(
                resourceName,
                NetworkIdentity.NewId(),
                userInfoManager.GetMyUserInfo().UserId,
                position,
                rotation);
        }

        public UserInfo GetUserInfo(long playerId)
        {
            if (this.gameClient?.UserInfo?.UserId == playerId)
            {
                return this.gameClient.UserInfo;
            }
            else if (this.gameClient?.ConnectedUsers?.Count > 0)
            {
                foreach (var user in this.gameClient.ConnectedUsers)
                {
                    if (user.UserId == playerId)
                    {
                        return user;
                    }
                }
            }

            return null;
        }

        //// OLD MATCHMAKING/LEGACY SERVER SYSTEM
        ////
        //// public UnityTask<bool> DoesRoomExist(GameServerInfo matchmakingInfo)
        //// {
        ////     return UnityTask<bool>.Run(Coroutine());
        ////
        ////     IEnumerator<bool> Coroutine()
        ////     {
        ////         if (this.Mode == NetworkingMode.RunClientAndServer)
        ////         {
        ////             yield return false;
        ////         }
        ////         else if (this.Mode == NetworkingMode.RunClientAndLANServer)
        ////         {
        ////             yield return false;
        ////         }
        ////         else if (this.Mode == NetworkingMode.RunClientAndCloudServer)
        ////         {
        ////             var matchmake = Lost.PlayFab.PlayFabManager.Instance.Do(this.GetMatchmakeRequest(matchmakingInfo, false));
        ////
        ////             // Wait for matchmake to finish
        ////             while (matchmake.IsDone == false)
        ////             {
        ////                 yield return false;
        ////             }
        ////
        ////             if (matchmake.HasError)
        ////             {
        ////                 yield break;
        ////             }
        ////             else if (matchmake.Value.Status == MatchmakeStatus.NoAvailableSlots)
        ////             {
        ////                 // Room doesn't exist
        ////                 yield return false;
        ////             }
        ////             else
        ////             {
        ////                 yield return true;
        ////             }
        ////         }
        ////         else
        ////         {
        ////             throw new NotImplementedException();
        ////         }
        ////     }
        //// }
        ////
        //// public UnityTask<bool> CreateOrJoinRoom(GameServerInfo matchmakingInfo)
        //// {
        ////     return UnityTask<bool>.Run(Coroutine());
        ////
        ////     IEnumerator<bool> Coroutine()
        ////     {
        ////         if (this.Mode == NetworkingMode.RunClientAndServer)
        ////         {
        ////             var startEditorServer = this.StartEditorLocalServer();
        ////
        ////             while (startEditorServer.IsDone == false)
        ////             {
        ////                 yield return false;
        ////             }
        ////
        ////             if (startEditorServer.HasError || this.gameServer.IsRunning == false)
        ////             {
        ////                 yield break;
        ////             }
        ////         }
        ////
        ////         string serverIp;
        ////         string ticket;
        ////         int port;
        ////
        ////         if (this.Mode == NetworkingMode.RunClientAndServer)
        ////         {
        ////             serverIp = this.editorServerIp;
        ////             port = this.editorServerPort;
        ////             ticket = null;
        ////         }
        ////         else if (this.Mode == NetworkingMode.RunClientAndLANServer)
        ////         {
        ////             serverIp = this.lanServerIp;
        ////             port = this.lanServerPort;
        ////             ticket = null;
        ////         }
        ////         else if (this.Mode == NetworkingMode.RunClientAndCloudServer)
        ////         {
        ////             var matchmake = Lost.PlayFab.PlayFabManager.Instance.Do(this.GetMatchmakeRequest(matchmakingInfo, true));
        ////
        ////             // Waiting for create to finish
        ////             while (matchmake.IsDone == false)
        ////             {
        ////                 yield return false;
        ////             }
        ////
        ////             if (matchmake.HasError)
        ////             {
        ////                 yield break;
        ////             }
        ////
        ////             serverIp = matchmake.Value.ServerPublicDNSName;
        ////             port = matchmake.Value.ServerPort.Value;
        ////             ticket = matchmake.Value.Ticket;
        ////         }
        ////         else
        ////         {
        ////             throw new NotImplementedException();
        ////         }
        ////
        ////         var connect = this.StartClient(serverIp, port);
        ////
        ////         while (connect.IsDone == false)
        ////         {
        ////             yield return false;
        ////         }
        ////
        ////         yield return connect.HasError == false && this.gameClient?.IsConnected == true;
        ////     }
        //// }

        public async Task<bool> EnterRoom(string roomId)
        {
            this.ShutdownClientAndServer();

            if (this.Mode == NetworkingMode.RunClientAndServer)
            {
                var startEditorServer = await this.StartEditorLocalServer();

                if (this.gameServer.IsRunning == false)
                {
                    return false;
                }
            }

            string serverIp;
            int port;

            if (this.Mode == NetworkingMode.RunClientAndServer)
            {
                serverIp = this.settings.EditorServerIp;
                port = this.settings.EditorServerPort;
            }
            else if (this.Mode == NetworkingMode.RunClientAndLANServer)
            {
                serverIp = this.settings.LanServerIp;
                port = this.settings.LanServerPort;
            }
            else if (this.Mode == NetworkingMode.RunClientAndCloudServer)
            {
                // Finding a room provider
                IRoomProvider roomProvider = this.bootloader.FindManager<IRoomProvider>();

                if (roomProvider == null)
                {
                    Logger.LogError($"Unable to find a Manager implementing {nameof(IRoomProvider)}.  Unable to join Room!");
                    return false;
                }

                var networkingRoomInfo = await roomProvider.CreateOrJoinRoom(roomId);

                // TODO [bgish]: Make sure there were no errors

                serverIp = networkingRoomInfo.Ip;
                port = networkingRoomInfo.Port;
            }
            else
            {
                throw new NotImplementedException();
            }

            var connect = await this.StartClient(serverIp, port);

            return this.gameClient?.IsConnected == true;
        }

        public void Shutdown()
        {
            this.ShutdownClientAndServer();
        }

        private void Update()
        {
            this.gameClient?.Update();
            this.gameServer?.Update();

            // Toggling run in background on/off
            if (this.isConnected == false && this.gameClient?.IsConnected == true)
            {
                Application.runInBackground = true;
                this.isConnected = true;
            }
            else if (this.isConnected && (this.gameClient == null || this.gameClient.IsConnected == false))
            {
                this.isConnected = false;
                Application.runInBackground = this.originalRunInBackground;
            }
        }

        private void OnApplicationQuit()
        {
            this.ShutdownClientAndServer();
        }

        private void OnDestroy()
        {
            this.ShutdownClientAndServer();
            this.onConnectedUsersUpdated = null;
        }

        private void ShutdownClientAndServer()
        {
            this.ShutdownGameClient();
            this.ShutdownGameServer();
        }

        private void ShutdownGameServer()
        {
            if (this.gameServer != null)
            {
                this.gameServer.Shutdown();
                this.gameServer = null;
            }
        }

        private void ShutdownGameClient()
        {
            if (this.gameClient != null)
            {
                this.gameClient.ClientUserConnected -= this.OnClientUserConnected;
                this.gameClient.ClientUserInfoUpdated -= this.OnClientUserInfoUpdated;
                this.gameClient.ClientUserDisconnected -= this.OnClientUserDisconnected;
                this.gameClient.ClientConnectedToServer -= this.OnClientConnectedToServer;
                this.gameClient.ClientDisconnectedFromServer -= this.OnClientDisconnectedFromServer;
                this.gameClient.Shutdown();
                this.gameClient = null;
            }
        }

        private async Task<bool> StartEditorLocalServer()
        {
            this.ShutdownGameServer();
            this.gameServer = this.gameServerFactory.CreateGameServerAndStart(this.settings.EditorServerPort);

            while (this.gameServer.IsStarting)
            {
                await Task.Delay(0);
            }

            if (this.gameServer.IsRunning == false)
            {
                this.ShutdownGameServer();
            }

            return this.gameServer.IsRunning;
        }

        private async Task<bool> StartClient(string ip, int port)
        {
            this.ShutdownGameClient();

            this.gameClient = this.gameClientFactory.CreateGameClientAndConnect(ip, port);
            this.gameClient.ClientUserConnected += this.OnClientUserConnected;
            this.gameClient.ClientUserInfoUpdated += this.OnClientUserInfoUpdated;
            this.gameClient.ClientUserDisconnected += this.OnClientUserDisconnected;
            this.gameClient.ClientConnectedToServer += this.OnClientConnectedToServer;
            this.gameClient.ClientDisconnectedFromServer += this.OnClientDisconnectedFromServer;

            while (this.gameClient.IsConnecting)
            {
                await Task.Delay(0);
            }

            if (this.gameClient.IsConnected == false)
            {
                this.ShutdownGameClient();
            }

            return this.gameClient.IsConnected;
        }

        private void OnClientUserConnected(UserInfo userInfo, bool wasReconnect)
        {
            this.onConnectedUsersUpdated?.Invoke();
        }

        private void OnClientUserInfoUpdated(UserInfo userInfo)
        {
            this.onConnectedUsersUpdated?.Invoke();
        }

        private void OnClientUserDisconnected(UserInfo userInfo, bool wasConnectionLost)
        {
            this.onConnectedUsersUpdated?.Invoke();
        }

        private void OnClientConnectedToServer()
        {
            this.onConnectedUsersUpdated?.Invoke();
        }

        private void OnClientDisconnectedFromServer()
        {
            this.onConnectedUsersUpdated?.Invoke();
        }

        [Serializable]
        public class Settings
        {
#pragma warning disable 0649
            [Header("Mode")]
            [SerializeField] private NetworkingMode networkingMode;
            [SerializeField] private NetworkingMode editorNetworkingMode;

            [Header("Editor Server Info")]
            [SerializeField] private string editorServerIp = "127.0.0.1";
            [SerializeField] private int editorServerPort = 9999;

            [Header("LAN Server Info")]
            [SerializeField] private string lanServerIp = "127.0.0.1";
            [SerializeField] private int lanServerPort = 7777;

            [Header("Debug")]
            [SerializeField] private bool printDebugOutput;
#pragma warning restore 0649

            public NetworkingMode NetworkingMode { get => networkingMode; set => networkingMode = value; }
            public NetworkingMode EditorNetworkingMode { get => editorNetworkingMode; set => editorNetworkingMode = value; }
            public string EditorServerIp { get => editorServerIp; set => editorServerIp = value; }
            public int EditorServerPort { get => editorServerPort; set => editorServerPort = value; }
            public string LanServerIp { get => lanServerIp; set => lanServerIp = value; }
            public int LanServerPort { get => lanServerPort; set => lanServerPort = value; }
            public bool PrintDebugOutput { get => printDebugOutput; set => printDebugOutput = value; }
        }
    }
}
