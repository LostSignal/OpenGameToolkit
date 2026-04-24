//-----------------------------------------------------------------------
// <copyright file="PlayFabUserInfoManager.cs" company="Full Circle Games">
//     Copyright (c) Full Circle Games. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY && USING_PLAYFAB

namespace OGT.PlayFab
{
    using OGT.Networking;
    using System.Threading.Tasks;

    public class PlayFabUserInfoManager : Manager, IUserInfoManager
    {
        public bool IsUserInfoReady { get; private set; }

        public long UserId { get; private set; }

        public string UserHexId { get; private set; }

        public string DisplayName { get; private set; }

        private UserInfo userInfo = new UserInfo();

        private PlayFabManager playfabManager;

        protected override async Task InitializeManager(Bootloader bootloader)
        {
            this.playfabManager = bootloader.FindManager<PlayFabManager>();

            while (this.playfabManager.IsInitialized == false)
            {
                await Task.Delay(100);
            }

            this.UserId = this.playfabManager.User.PlayFabNumericId;
            this.UserHexId = this.playfabManager.User.PlayFabId;
            this.DisplayName = this.playfabManager.User.DisplayName;
            this.IsUserInfoReady = true;
        }

        public UserInfo GetMyUserInfo()
        {
            this.userInfo.UserId = this.UserId;
            this.userInfo.UserHexId = this.UserHexId;
            this.userInfo.DisplayName = this.DisplayName;
            this.userInfo.SetSessionTicket(this.playfabManager.Login.SessionTicket);

            return userInfo;
        }
    }
}

#endif
