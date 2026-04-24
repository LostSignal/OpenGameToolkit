//-----------------------------------------------------------------------
// <copyright file="ShutdownGameServerSubsystem.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    using System;
    using System.Threading.Tasks;

    public class ShutdownGameServerSubsystem : IGameServerSubsystem
    {
        private static readonly OGTLogger Logger = new ("Networking");

        private GameServer gameServer = null;
        private DateTime noUsersDateTime = DateTime.MinValue;
        private bool haveUsersEverJoined = false;
        private bool hasShutdownServer = false;
        private int currentUserCount = 0;

        public string Name => nameof(ShutdownGameServerSubsystem);

        public void Initialize(GameServer gameServer)
        {
            this.gameServer = gameServer;
        }

        public Task<bool> Run()
        {
            this.gameServer.ServerUserConnected += this.OnUserConnected;
            this.gameServer.ServerUserDisconnected += this.OnUserDisconnected;
            this.gameServer.ServerUpdated += this.OnServerUpdate;

            return Task.FromResult<bool>(true);
        }

        public Task Shutdown()
        {
            this.gameServer.ServerUserConnected -= this.OnUserConnected;
            this.gameServer.ServerUserDisconnected -= this.OnUserDisconnected;
            this.gameServer.ServerUpdated -= this.OnServerUpdate;

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

        private void OnUserConnected(UserInfo userInfo, bool wasReconnect)
        {
            this.haveUsersEverJoined = true;
            this.currentUserCount = this.gameServer.ConnectedUsers.Count;
        }

        private void OnUserDisconnected(UserInfo userInfo, bool wasConnectionLost)
        {
            this.currentUserCount = this.gameServer.ConnectedUsers.Count;

            if (this.currentUserCount == 0)
            {
                this.noUsersDateTime = DateTime.UtcNow;
            }
        }

        private void OnServerUpdate()
        {
            if (this.haveUsersEverJoined &&
                this.currentUserCount == 0 &&
                this.hasShutdownServer == false &&
                DateTime.UtcNow.Subtract(this.noUsersDateTime).TotalMinutes > 1)
            {
                this.hasShutdownServer = true;

                Logger.Log("ShutdownServerSubsystem has detected no users for one minute.  Shutting down the server!");
                this.gameServer.Shutdown();
            }
        }
    }
}
