//-----------------------------------------------------------------------
// <copyright file="PlayFabMatchmakingManager.cs" company="Full Circle Games">
//     Copyright (c) Full Circle Games. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if false // UNITY && USING_PLAYFAB

namespace OGT.PlayFab
{
    using Networking;
    using global::PlayFab.ClientModels;
    using OGT.CloudFunctions;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;

    public class PlayFabMatchmakingManager : OldManager<PlayFabMatchmakingManager>, IRoomProvider
    {
        public UnityTask<NetworkConnectionInfo> CreateOrJoinRoom(string roomName)
        {
            return UnityTask<NetworkConnectionInfo>.Run(Coroutine());

            IEnumerator<NetworkConnectionInfo> Coroutine()
            {
                var enterRoom = CloudFunctionsManager.Instance.Rooms_EnterRoom(roomName);

                // Waiting for enterRoom to finish
                while (enterRoom.IsCompleted == false)
                {
                    yield return default;
                }

                if (enterRoom.Result.Success == false)
                {
                    Lost.Logger.LogError($"Failed to Enter Room {roomName}: " + enterRoom.Result.Exception);
                    yield break;
                }

                var roomServerInfo = enterRoom.Result.ResultObject;

                string ip = enterRoom.Result.ResultObject.FQDN;
                int port = enterRoom.Result.ResultObject.Ports.Where(x => x.Name == "game_port").FirstOrDefault().Num;

                if (NetworkingManager.PrintDebugOutput)
                {
                    Lost.Logger.Log($"Connecting to Sever {ip}, Port = {port}, Room Id = {roomServerInfo.RoomId}, Session Id = {roomServerInfo.SessionId}, Server Id = {roomServerInfo.ServerId}");
                }

                yield return new NetworkConnectionInfo { Ip = ip, Port = port };
            }
        }

        public override void Initialize()
        {
            this.SetInstance(this);
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

#endif
