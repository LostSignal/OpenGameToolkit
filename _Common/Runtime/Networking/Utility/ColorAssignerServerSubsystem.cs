//-----------------------------------------------------------------------
// <copyright file="ColorAssignerServerSubsystem.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class ColorAssignerServerSubsystem : IGameServerSubsystem
    {
        private const string ColorKey = "Color";
        private const string InvalidColor = "000000";

        private static readonly OGTLogger Logger = new("Networking");

        private static readonly List<string> AllColors = new List<string>
        {
            "FFFF00",
            "00FF00",
            "0000FF",
            "00FFFF",
            "FF00FF",
            "FF8500",
            "9EC500",
            "FF0000",
            "B20955",
            "AAAAFF",
            "009A1D",
            "9A008F",
        };

        private readonly System.Random random = new System.Random();
        private readonly Dictionary<long, string> userColorMap = new Dictionary<long, string>();
        private readonly List<string> colorPool = new List<string>();
        private readonly object colorLock = new object();

        private GameServer gameServer = null;

        public string Name => nameof(ColorAssignerServerSubsystem);

        public void Initialize(GameServer gameServer)
        {
            this.gameServer = gameServer;
        }

        public Task<bool> Run()
        {
            this.userColorMap.Clear();
            this.colorPool.Clear();
            this.colorPool.AddRange(AllColors);

            this.gameServer.ServerUserConnected += this.OnUserConnected;
            this.gameServer.ServerUserDisconnected += this.OnUserDisconnected;

            return Task.FromResult<bool>(true);
        }

        public Task Shutdown()
        {
            this.gameServer.ServerUserConnected -= this.OnUserConnected;
            this.gameServer.ServerUserDisconnected -= this.OnUserDisconnected;

            return Task.Delay(0);
        }

        public Task<bool> AllowPlayerToJoin(UserInfo userInfo)
        {
            return Task.FromResult<bool>(true);
        }

        public Task UpdatePlayerInfo(UserInfo userInfo)
        {
            userInfo.CustomData.AddOrOverwrite(ColorKey, this.GetColorForUser(userInfo));
            return Task.Delay(0);
        }

        private void OnUserConnected(UserInfo userInfo, bool wasReconnect)
        {
            string userColor = this.GetColorForUser(userInfo);

            if (userInfo.CustomData.TryGetValue(ColorKey, out string currentColor) == false || currentColor != userColor)
            {
                Logger.Log($"User {userInfo.UserId}'s Color = {userColor}");
                this.gameServer?.AddCustomDataToUser(userInfo, ColorKey, userColor);
            }
        }

        private string GetColorForUser(UserInfo userInfo)
        {
            lock (this.colorLock)
            {
                string color = null;

                if (this.userColorMap.TryGetValue(userInfo.UserId, out string userColor))
                {
                    color = userColor;
                }
                else if (this.colorPool.Count > 0)
                {
                    color = this.colorPool[this.random.Next(this.colorPool.Count)];
                    this.colorPool.Remove(color);
                    this.userColorMap.Add(userInfo.UserId, color);
                }
                else
                {
                    color = InvalidColor;
                }

                return color;
            }
        }

        private void OnUserDisconnected(UserInfo userInfo, bool wasConnectionLost)
        {
            lock (this.colorLock)
            {
                if (this.userColorMap.TryGetValue(userInfo.UserId, out string userColor))
                {
                    if (userColor != InvalidColor)
                    {
                        this.userColorMap.Remove(userInfo.UserId);
                        this.colorPool.Add(userColor);
                    }
                }
            }
        }
    }
}
