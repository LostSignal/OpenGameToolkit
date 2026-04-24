//-----------------------------------------------------------------------
// <copyright file="ValidatePlayFabSessionTicketSubsystem.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY || USING_PLAYFAB

namespace OGT
{
    using System.Threading.Tasks;
    using OGT.Networking;

    public class ValidatePlayFabSessionTicketSubsystem : IGameServerSubsystem
    {
        private static readonly OGTLogger Logger = new("Networking");

#if !UNITY || UNITY_EDITOR
        private global::PlayFab.PlayFabAuthenticationContext titleAuthenticationContext;
#endif

        public string Name => nameof(ValidatePlayFabSessionTicketSubsystem);

        public void Initialize(GameServer gameServer)
        {
        }

        public Task<bool> Run()
        {
            return Task<bool>.FromResult(true);
        }

        public Task Shutdown()
        {
            return Task.Delay(0);
        }

        public async Task<bool> AllowPlayerToJoin(UserInfo userInfo)
        {
#if !UNITY || UNITY_EDITOR
            if (userInfo == null)
            {
                return false;
            }

            var sessionTicket = userInfo.GetSessionTicket();

            if (string.IsNullOrEmpty(sessionTicket))
            {
                Logger.LogError("ValidatePlayFabSessionTicketSubsystem requires all clients send their PlayFab session ticket in their CustomData in the \"SessionTicket\" key.");
                return false;
            }

            if (this.titleAuthenticationContext == null)
            {
                var getTitleAuthentication = await PlayFab.PlayFabUtil.GetTitleEntityTokenAsync();

                this.titleAuthenticationContext = new global::PlayFab.PlayFabAuthenticationContext
                {
                    EntityId = getTitleAuthentication.Result.Entity.Id,
                    EntityType = getTitleAuthentication.Result.Entity.Type,
                    EntityToken = getTitleAuthentication.Result.EntityToken,
                };
            }

            var authenticate = await global::PlayFab.PlayFabServerAPI.AuthenticateSessionTicketAsync(new global::PlayFab.ServerModels.AuthenticateSessionTicketRequest
            {
                SessionTicket = sessionTicket,
                AuthenticationContext = this.titleAuthenticationContext,
            });

            if (authenticate.Error == null && authenticate.Result != null)
            {
                userInfo.UserHexId = authenticate.Result.UserInfo.PlayFabId;
                userInfo.DisplayName = authenticate.Result.UserInfo.TitleInfo.DisplayName;

                return true;
            }
            else
            {
                return false;
            }

#else
            return await Task.FromResult(true);
#endif
        }

        public Task UpdatePlayerInfo(UserInfo userInfo)
        {
            userInfo.SetSessionTicket(null);
            return Task.Delay(0);
        }
    }
}

//// NOTE [bgish]: I used to have a PlayFabGameServer that handled rejecting users when the server was full, notifying the playfab backend when players
////               had joined / left, and validating session tickets.  The above class only validates session tickets, so we may want to rename  this
////               subsystem and move some of the below logic back into this class.

/*
#if !UNITY_2018_3_OR_NEWER && ENABLE_PLAYFABSERVER_API

//// NOTE [bgish]: Will I ever need to set the Server Instance and Tag Data?
////
//// var setGameServerInstanceData = PlayFabServerAPI.SetGameServerInstanceDataAsync(new SetGameServerInstanceDataRequest
//// {
////     LobbyId = this.serverSettings.LobbyId,
////     GameServerData = "",
//// });
////
//// PlayFabServerAPI.SetGameServerInstanceTagsAsync(new SetGameServerInstanceTagsRequest
//// {
////     LobbyId = this.serverSettings.LobbyId,
////     Tags = new System.Collections.Generic.Dictionary<string, string>() { { "Tag1", "Value1" }, { "Tag2", "Value2" } },
//// });

namespace Lost.Networking
{
    using System;
    using System.Collections.Generic;
    using System.Timers;
    using PlayFab;
    using PlayFab.ServerModels;
    using UnityEngine;

    public abstract class PlayFabGameServer : GameServer
    {
        /// <summary>
        /// When playfab launches a game instance it passing all needed data via the command
        /// line.  This wraps up all that data into a ServerSettingsData instance.
        /// </summary>
        public static PlayFabServerSettingsData GetServerDataFromCommandLine()
        {
            var data = new PlayFabServerSettingsData();
            data.IsExternalServer = false;
            data.Tags = null;

            foreach (string arg in System.Environment.GetCommandLineArgs())
            {
                string[] argArray = arg.Split('=');

                if (argArray.Length < 2)
                {
                    continue;
                }

                var key = argArray[0].Contains("-") ? argArray[0].Replace("-", string.Empty).Trim() : argArray[0].Trim();
                var value = argArray[1].Trim();

                switch (key.ToLower())
                {
                    // playfab data
                    case "title_secret_key":
                        {
                            data.TitleSecretKey = value;
                            break;
                        }

                    case "playfab_api_endpoint":
                        {
                            data.PlayFabApiEndpoint = value;

                            // Ex: https://87a6.playfabapi.com:443
                            string removedHttps = value.ToLower().Replace("https://", string.Empty);
                            int firstDot = removedHttps.IndexOf('.');
                            data.TitleId = removedHttps.Substring(0, firstDot);

                            break;
                        }

                    case "game_id":
                        {
                            data.LobbyId = value;
                            break;
                        }

                    // server data
                    case "server_host_domain":
                        {
                            data.ServerHostDomain = value;
                            break;
                        }

                    case "server_host_port":
                        {
                            int defaultHostPort = 7777;
                            int hostPort = 0;

                            if (int.TryParse(value, out hostPort) == false)
                            {
                                Debug.LogErrorFormat("PlayFabGameServer: Unable to parse server_host_port {0}, using {1} instead.  This may break your game.", value, defaultHostPort);
                            }

                            data.ServerHostPort = hostPort > 0 ? hostPort : defaultHostPort;
                            break;
                        }

                    case "server_host_region":
                        {
                            data.ServerHostRegion = (Region)Enum.Parse(typeof(Region), value);
                            break;
                        }

                    // game mode data
                    case "game_mode":
                        {
                            data.GameMode = value;
                            break;
                        }

                    case "game_build_version":
                        {
                            data.GameBuildVersion = value;
                            break;
                        }

                    case "custom_data":
                        {
                            data.CustomData = value;
                            break;
                        }

                    // logging data
                    case "log_file_path":
                        {
                            data.LogFilePath = value;
                            break;
                        }

                    case "output_files_directory_path":
                        {
                            data.OutputFilesDirectory = value;
                            break;
                        }
                }
            }

            return data;
        }

        private PlayFabServerSettingsData serverSettings;
        private Timer heartbeatTimer;

        // Reconnection members
        private Dictionary<long, ReconnectInfo> reconnectUsers = new Dictionary<long, ReconnectInfo>();
        private bool isClosedToNewUsers = false;

        public PlayFabServerSettingsData ServerSettingsData
        {
            get { return this.serverSettings; }
        }

        public PlayFabGameServer(IServerTransportLayer transportLayer, PlayFabServerSettingsData serverSettings) : base(transportLayer)
        {
            Debug.Assert(serverSettings != null, "Server Settings Must Not Be Null!");

            this.serverSettings = serverSettings;

            PlayFabSettings.staticSettings.DeveloperSecretKey = serverSettings.TitleSecretKey;
            PlayFabSettings.staticSettings.TitleId = serverSettings.TitleId;
        }

        public bool Start()
        {
            return this.Start(this.serverSettings.ServerHostPort);
        }

        public override bool CanUserJoinServer(JoinServerRequestMessage joinServerRequestMessage)
        {
            if (this.serverSettings.IsDebugLocalBuild)
            {
                return true;
            }

            // Testing if they're already connected
            foreach (var user in this.ConnectedUsers)
            {
                if (joinServerRequestMessage.UserInfo.UserId == user.UserId)
                {
                    return true;
                }
            }

            // Testing if this is a reconnect
            if (this.reconnectUsers.TryGetValue(joinServerRequestMessage.UserInfo.UserId, out ReconnectInfo reconnectInfo))
            {
                if (this.isClosedToNewUsers && reconnectInfo.MatchmakerTicket == joinServerRequestMessage.CustomData)
                {
                    joinServerRequestMessage.UserInfo.SetPlayFabId(reconnectInfo.PlayFabId);
                    joinServerRequestMessage.UserInfo.SetUsername(reconnectInfo.Username);
                    joinServerRequestMessage.UserInfo.SetDisplayName(reconnectInfo.DisplayName);
                    return true;
                }
            }

            var redeemMatchmakerTicket = PlayFabServerAPI.RedeemMatchmakerTicketAsync(new RedeemMatchmakerTicketRequest
            {
                LobbyId = this.serverSettings.LobbyId,
                Ticket = joinServerRequestMessage.CustomData,
            });

            redeemMatchmakerTicket.Wait();

            if (redeemMatchmakerTicket.Exception != null)
            {
                Debug.LogErrorFormat("PlayFabGameServer: Unable to Redeem Matchmaker Ticket: Exception {0}", redeemMatchmakerTicket.Exception.Message);
            }
            else if (redeemMatchmakerTicket.Result.Error != null)
            {
                Debug.LogErrorFormat("PlayFabGameServer: Unable to Redeem Matchmaker Ticket: Error {0}", redeemMatchmakerTicket.Result.Error.ErrorMessage);
            }
            else if (redeemMatchmakerTicket.Result.Result.TicketIsValid == false)
            {
                Debug.LogErrorFormat("PlayFabGameServer: Unable to Redeem Matchmaker Ticket: Error {0}", redeemMatchmakerTicket.Result.Error.ErrorMessage);
            }
            else
            {
                string playfabId = redeemMatchmakerTicket.Result.Result.UserInfo.PlayFabId;
                string displayName = redeemMatchmakerTicket.Result.Result.UserInfo.TitleInfo.DisplayName;
                string username = redeemMatchmakerTicket.Result.Result.UserInfo.Username;
                long userId = System.Convert.ToInt64(playfabId, 16);

                if (userId != joinServerRequestMessage.UserInfo.UserId)
                {
                    Debug.LogErrorFormat("PlayFabGameServer: User {0}, {1}, {2} Passed in invalid info, actual info is {3}, {4}, {5}",
                        joinServerRequestMessage.UserInfo.GetPlayFabId(),
                        joinServerRequestMessage.UserInfo.GetDisplayName(),
                        joinServerRequestMessage.UserInfo.UserId,
                        playfabId,
                        displayName,
                        userId);

                    this.NotifyMatchmakerPlayerLeft(playfabId);

                    // TODO [bgish]: Set BadCallCount?

                    return false;
                }
                else
                {
                    // We pased everything, so register the user and go!
                    joinServerRequestMessage.UserInfo.SetPlayFabId(playfabId);
                    joinServerRequestMessage.UserInfo.SetUsername(username);
                    joinServerRequestMessage.UserInfo.SetDisplayName(displayName);

                    // Remembering this conection so they can re-connect mid session and not have to
                    if (this.reconnectUsers.ContainsKey(joinServerRequestMessage.UserInfo.UserId) == false)
                    {
                        this.reconnectUsers.Add(joinServerRequestMessage.UserInfo.UserId, new ReconnectInfo
                        {
                            PlayFabId = playfabId,
                            Username = username,
                            DisplayName = displayName,
                            MatchmakerTicket = joinServerRequestMessage.CustomData,
                        });
                    }

                    return true;
                }
            }

            return false;
        }

        public override bool OnServerStart()
        {
            if (this.serverSettings.IsDebugLocalBuild)
            {
                return true;
            }

            if (serverSettings.IsExternalServer)
            {
                // Registering the Game
                var registerGameResult = PlayFabServerAPI.RegisterGameAsync(new RegisterGameRequest
                {
                    ServerPublicDNSName = this.serverSettings.ServerHostDomain,
                    ServerPort = this.serverSettings.ServerHostPort.ToString(),
                    Build = this.serverSettings.GameBuildVersion,
                    GameMode = this.serverSettings.GameMode,
                    Region = this.serverSettings.ServerHostRegion,
                });

                registerGameResult.Wait();

                if (registerGameResult.Exception != null)
                {
                    Debug.LogErrorFormat("PlayFabGameServer: Unable to RegisterGame: Exception {0}", registerGameResult.Exception.Message);
                    return false;
                }
                else if (registerGameResult.Result.Error != null)
                {
                    Debug.LogErrorFormat("PlayFabGameServer: Unable to RegisterGame: Error {0}", registerGameResult.Result.Error.ErrorMessage);
                    return false;
                }
                else
                {
                    Debug.Log("PlayFabGameServer: RegisterGame Complete!");
                }

                this.serverSettings.LobbyId = registerGameResult.Result.Result.LobbyId;

                // Setting up a heart beat timer
                this.heartbeatTimer = new Timer(45 * 1000);
                this.heartbeatTimer.Elapsed += this.OnTimedEvent;
                this.heartbeatTimer.AutoReset = true;
                this.heartbeatTimer.Enabled = true;
                this.heartbeatTimer.Start();
            }

            return true;
        }

        public override void OnUserClosedConnection(UserInfo userInfo)
        {
        }

        public override void UserDisconnected(UserInfo userInfo, bool wasConnectionLost)
        {
            this.NotifyMatchmakerPlayerLeft(userInfo.GetPlayFabId());
        }

        public override void OnServerShutdown()
        {
            if (this.serverSettings.IsDebugLocalBuild)
            {
                return;
            }

            // if this was an external server, do some extra shutdown steps
            if (this.serverSettings.IsExternalServer)
            {
                // Shutting down the heartbeat timer
                if (this.heartbeatTimer != null)
                {
                    this.heartbeatTimer.Dispose();
                    this.heartbeatTimer = null;
                }

                // Telling the matchmaker we're done
                if (string.IsNullOrEmpty(this.serverSettings.LobbyId) == false)
                {
                    var deregisterGame = PlayFabServerAPI.DeregisterGameAsync(new DeregisterGameRequest
                    {
                        LobbyId = this.serverSettings.LobbyId,
                    });

                    deregisterGame.Wait();

                    if (deregisterGame.Exception != null)
                    {
                        Debug.LogErrorFormat("PlayFabGameServer: Unable To Deregister Game: Exception {0}", deregisterGame.Exception.Message);
                    }
                    else if (deregisterGame.Result.Error != null)
                    {
                        Debug.LogErrorFormat("PlayFabGameServer: Unable To Deregister Game: Error {0}", deregisterGame.Result.Error.ErrorMessage);
                    }
                }
            }
        }

        public void CloseServerToNewUsers()
        {
            if (this.serverSettings.IsDebugLocalBuild)
            {
                return;
            }

            this.isClosedToNewUsers = true;

            var setGameServerInstanceState = PlayFabServerAPI.SetGameServerInstanceStateAsync(new SetGameServerInstanceStateRequest
            {
                LobbyId = this.serverSettings.LobbyId,
                State = GameInstanceState.Closed,
            });

            setGameServerInstanceState.Wait();

            if (setGameServerInstanceState.Exception != null)
            {
                Debug.LogErrorFormat("PlayFabGameServer: Unable to Set Game's Instance State to Closed: Exception {0}", setGameServerInstanceState.Exception.Message);
            }
            else if (setGameServerInstanceState.Result.Error != null)
            {
                Debug.LogErrorFormat("PlayFabGameServer: Unable to Set Game's Instance State to Closed: Error {0}", setGameServerInstanceState.Result.Error.ErrorMessage);
            }
            else
            {
                Debug.Log("PlayFabGameServer: Successfully Set Game's Instance State to Closed.");
            }
        }

        public void OpenServerToNewUsers()
        {
            if (this.serverSettings.IsDebugLocalBuild)
            {
                return;
            }

            var setGameServerInstanceState = PlayFabServerAPI.SetGameServerInstanceStateAsync(new SetGameServerInstanceStateRequest
            {
                LobbyId = this.serverSettings.LobbyId,
                State = GameInstanceState.Open,
            });

            setGameServerInstanceState.Wait();

            if (setGameServerInstanceState.Exception != null)
            {
                Debug.LogErrorFormat("PlayFabGameServer: Unable to Set Game's Instance State To Open: Exception {0}", setGameServerInstanceState.Exception.Message);
            }
            else if (setGameServerInstanceState.Result.Error != null)
            {
                Debug.LogErrorFormat("PlayFabGameServer: Unable to Set Game's Instance State To Open: Error {0}", setGameServerInstanceState.Result.Error.ErrorMessage);
            }
            else
            {
                Debug.Log("PlayFabGameServer: Successfully Set Game's Instance State to Open.");
                this.isClosedToNewUsers = false;
            }
        }

        private void NotifyMatchmakerPlayerLeft(string playfabId)
        {
            if (this.serverSettings.IsDebugLocalBuild)
            {
                return;
            }

            // If we got here, then the user tampered with id/username tell the matchmaker they've left
            var playerLeftRequest = PlayFabServerAPI.NotifyMatchmakerPlayerLeftAsync(new NotifyMatchmakerPlayerLeftRequest
            {
                LobbyId = this.serverSettings.LobbyId,
                PlayFabId = playfabId,
            });

            playerLeftRequest.Wait();

            if (playerLeftRequest.Exception != null)
            {
                Debug.LogErrorFormat("PlayFabGameServer: Unable To Notify Player Left: Exception {0}", playerLeftRequest.Exception.Message);
            }
            else if (playerLeftRequest.Result.Error != null)
            {
                Debug.LogErrorFormat("PlayFabGameServer: Unable To Notify Player Left: Error {0}", playerLeftRequest.Result.Error.ErrorMessage);
            }
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (this.serverSettings.IsDebugLocalBuild)
            {
                return;
            }

            var refreshGameServerInstance = PlayFabServerAPI.RefreshGameServerInstanceHeartbeatAsync(new RefreshGameServerInstanceHeartbeatRequest
            {
                LobbyId = this.serverSettings.LobbyId,
            });

            refreshGameServerInstance.Wait();

            if (refreshGameServerInstance.Exception != null)
            {
                Debug.LogErrorFormat("PlayFabGameServer: Unable to Refresh Game Server Instance: Exception {0}", refreshGameServerInstance.Exception.Message);
            }
            else if (refreshGameServerInstance.Result.Error != null)
            {
                Debug.LogErrorFormat("PlayFabGameServer: Unable to Refresh Game Server Instance: Error {0}", refreshGameServerInstance.Result.Error.ErrorMessage);
            }
        }

        private class ReconnectInfo
        {
            public string PlayFabId { get; set; }
            public string Username { get; set; }
            public string DisplayName { get; set; }
            public string MatchmakerTicket { get; set; }
        }
    }
}
*/

#endif
