//-----------------------------------------------------------------------
// <copyright file="PlayFabMatchmakingManager.cs" company="Full Circle Games">
//     Copyright (c) Full Circle Games. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost.PlayFab
{
    using OGT.Networking;
    using global::PlayFab.ClientModels;
    using Lost.CloudFunctions;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;
    using OGT;
    using System.Threading.Tasks;

    public class PlayFabMatchmakingManager : Manager, IRoomProvider
    {
        private CloudFunctionsManager cloudFunctionsManager;

        protected override Task InitializeManager(Bootloader bootloader)
        {
            this.cloudFunctionsManager = bootloader.FindManager<CloudFunctionsManager>();

            return Task.CompletedTask;
        }

        public async Task<NetworkConnectionInfo> CreateOrJoinRoom(string roomName)
        {
            var enterRoom = await this.cloudFunctionsManager.Rooms_EnterRoom(roomName);

            if (enterRoom.Success == false)
            {
                Debug.LogError($"Failed to Enter Room {roomName}: " + enterRoom.Exception);
                return null;
            }

            var roomServerInfo = enterRoom.ResultObject;

            string ip = enterRoom.ResultObject.FQDN;
            int port = enterRoom.ResultObject.Ports.Where(x => x.Name == "game_port").FirstOrDefault().Num;

            if (NetworkingManager.PrintDebugOutput)
            {
                Debug.Log($"Connecting to Sever {ip}, Port = {port}, Room Id = {roomServerInfo.RoomId}, Session Id = {roomServerInfo.SessionId}, Server Id = {roomServerInfo.ServerId}");
            }

            return new NetworkConnectionInfo { Ip = ip, Port = port };
        }

        private MatchmakeRequest GetMatchmakeRequest(GameServerInfo info, bool startNewIfNoneFound)
        {
            var request = new MatchmakeRequest
            {
                GameMode = info.GameMode,
                BuildVersion = info.BuildVersion,
                Region = info.Region,
                StartNewIfNoneFound = startNewIfNoneFound,
            };

            if (string.IsNullOrEmpty(info.RoomName) == false)
            {
                request.TagFilter = new CollectionFilter
                {
                    Includes = new List<Container_Dictionary_String_String>
                    {
                        new Container_Dictionary_String_String
                        {
                            Data = new Dictionary<string, string>
                            {
                                { "Room", info.RoomName.ToUpper() },
                            },
                        },
                    },
                };
            }

            return request;
        }

        public class GameServerInfo
        {
            public string GameMode { get; set; }

            public string BuildVersion { get; set; }

            public Region Region { get; set; }

            public string RoomName { get; set; }
        }
    }
}
