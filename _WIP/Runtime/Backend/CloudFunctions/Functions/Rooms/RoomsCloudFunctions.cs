//-----------------------------------------------------------------------
// <copyright file="RoomsCloudFunctions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY

namespace Lost.CloudFunctions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::PlayFab;
    using global::PlayFab.MultiplayerModels;

    public static class RoomsCloudFunctions
    {
        // TODO [bgish]: This hash is temporary and will eventually be replaced by Redis
        private static readonly Dictionary<string, RoomServerInfo> RoomIdToServerInfoMap = new Dictionary<string, RoomServerInfo>();
        private static readonly object ServerInfosLock = new object();

        private static readonly List<Room> Rooms = new List<Room>
        {
            new Room { Id = "Room1", Info = new RoomInfo { Name = "Room 1" }, Visibility = RoomVisibility.Public },
            new Room { Id = "Room2", Info = new RoomInfo { Name = "Room 2" }, Visibility = RoomVisibility.Public },
            new Room { Id = "Room3", Info = new RoomInfo { Name = "Room 3" }, Visibility = RoomVisibility.Public },
            new Room { Id = "Room4", Info = new RoomInfo { Name = "Room 4" }, Visibility = RoomVisibility.Public },
            new Room { Id = "Room5", Info = new RoomInfo { Name = "Room 5" }, Visibility = RoomVisibility.Public },
        };

        [CloudFunction("Rooms", "GetPublicRooms")]
        public static Task<List<Room>> GetPublicRooms(CloudFunctionContext context)
        {
            return Task.FromResult<List<Room>>(Rooms.Where(x => x.Visibility == RoomVisibility.Public).ToList());
        }

        [CloudFunction("Rooms", "EnterRoom")]
        public static async Task<RoomServerInfo> EnterRoom(CloudFunctionContext context, string roomId)
        {
            //// TODO [bgish]: Verify this is an actual room...
            //// TODO [bgish]: Verify this user has the right to enter this room...

            // Querying Title Data to figure out what server info we should be using
            var gameServerInfoKey = "GameServerInfo";

            var getTitleData = await global::PlayFab.PlayFabServerAPI.GetTitleDataAsync(new global::PlayFab.ServerModels.GetTitleDataRequest
            {
                Keys = new List<string> { gameServerInfoKey },
                AuthenticationContext = context.TitleAuthenticationContext,
            });

            GameServerInfo gameServerInfo = OGT.JsonUtil.Deserialize<GameServerInfo>(getTitleData.Result.Data[gameServerInfoKey]);

            // Querying if the server info already exists
            RoomServerInfo serverInfo = GetServerInfoForRoom(roomId);

            if (serverInfo == null)
            {
                serverInfo = new RoomServerInfo
                {
                    RoomId = roomId,
                    SessionId = Guid.NewGuid().ToString("D"),
                    BuildId = gameServerInfo.BuildId,
                };

                if (AddServerInfoIfDoesNotExist(serverInfo))
                {
                    DateTime start = DateTime.Now;

                    // We were successful registering our ServerInfo, they we need to spin up an actual server and set that data too
                    var request = await PlayFabMultiplayerAPI.RequestMultiplayerServerAsync(new RequestMultiplayerServerRequest
                    {
                        SessionId = serverInfo.SessionId,
                        BuildId = serverInfo.BuildId,
                        PreferredRegions = gameServerInfo.Regions,
                        AuthenticationContext = context.TitleAuthenticationContext,
                    });

                    DateTime end = DateTime.Now;

                    UnityEngine.Debug.Log($"RequestMultiplayerServerRequest Time = {end.Subtract(start).TotalMilliseconds}");

                    serverInfo.FQDN = request.Result.FQDN;
                    serverInfo.Ports = request.Result.Ports;
                    serverInfo.Region = request.Result.Region;
                    serverInfo.ServerId = request.Result.ServerId;

                    if (serverInfo.SessionId != request.Result.SessionId)
                    {
                        UnityEngine.Debug.LogError($"Multiplayer Session Id Changed from {serverInfo.SessionId} to {request.Result.SessionId}");
                        serverInfo.SessionId = request.Result.SessionId;
                    }

                    UpdateServerInfo(serverInfo);

                    return serverInfo;
                }
                else
                {
                    // NOTE [bgish]: This will fail if the winner has the RequestMultiplayerServerAsync fail due to lack of availability
                    // Someone beat us to the punch and added one before us, so lets poll till we get the server info
                    serverInfo = GetServerInfoForRoom(roomId);

                    int retryCount = 0;
                    while (string.IsNullOrEmpty(serverInfo?.FQDN))
                    {
                        await Task.Delay(100);
                        serverInfo = GetServerInfoForRoom(roomId);
                        retryCount++;

                        if (retryCount > 30)
                        {
                            throw new Exception("Retry Count Max Met!");
                        }
                    }

                    return serverInfo;
                }
            }
            else
            {
                var getServerInfo = await PlayFabMultiplayerAPI.GetMultiplayerServerDetailsAsync(new GetMultiplayerServerDetailsRequest
                {
                    BuildId = serverInfo.BuildId,
                    Region = serverInfo.Region,
                    SessionId = serverInfo.SessionId,
                    AuthenticationContext = context.TitleAuthenticationContext,
                });

                if (getServerInfo.Error == null)
                {
                    // If it didn't return an error, it should still be good
                    return serverInfo;
                }
                else
                {
                    // We know this session is no longer valid, so remove it from the cache and get a new one
                    DeleteServerInfoIfDataHasntChanged(serverInfo.RoomId, serverInfo.SessionId, serverInfo.Region);
                    return await EnterRoom(context, roomId);
                }
            }
        }

        //// NOTE [bgish]: The hope is that the below functions will be converted to an actual database backend like Redis

        private static RoomServerInfo GetServerInfoForRoom(string roomId)
        {
            // TODO [bgish]: Need to make this a call to Redis
            lock (ServerInfosLock)
            {
                return RoomIdToServerInfoMap.TryGetValue(roomId, out RoomServerInfo serverInfo) ? serverInfo : null;
            }
        }

        private static void DeleteServerInfoIfDataHasntChanged(string roomId, string sessonId, string region)
        {
            // TODO [bgish]: Need to make this a call to Redis
            lock (ServerInfosLock)
            {
                if (RoomIdToServerInfoMap.TryGetValue(roomId, out RoomServerInfo serverInfo))
                {
                    if (serverInfo.RoomId == roomId &&
                        serverInfo.SessionId == sessonId &&
                        serverInfo.Region == region)
                    {
                        RoomIdToServerInfoMap.Remove(roomId);
                    }
                }
            }
        }

        private static void UpdateServerInfo(RoomServerInfo serverInfo)
        {
            // TODO [bgish]: Need to make this a call to Redis
            lock (ServerInfosLock)
            {
                RoomIdToServerInfoMap[serverInfo.RoomId] = serverInfo;
            }
        }

        private static bool AddServerInfoIfDoesNotExist(RoomServerInfo serverInfo)
        {
            // TODO [bgish]: Need to make this a call to Redis
            lock (ServerInfosLock)
            {
                if (RoomIdToServerInfoMap.ContainsKey(serverInfo.RoomId) == false)
                {
                    RoomIdToServerInfoMap.Add(serverInfo.RoomId, serverInfo);
                    return true;
                }

                return false;
            }
        }

        private class GameServerInfo
        {
            public string BuildId { get; set; }

            public List<string> Regions { get; set; }
        }
    }
}

#endif
