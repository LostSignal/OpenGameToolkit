//-----------------------------------------------------------------------
// <copyright file="UserInfoManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using OGT.Networking;
    using System.Threading.Tasks;

    public interface IUserInfoManager
    {
        bool IsUserInfoReady { get; }

        string UserId { get; }

        string DisplayName { get; }

        UserInfo GetMyUserInfo();
    }

    public class UserInfoManager : Manager, IUserInfoManager
    {
        protected override Task InitializeManager(Bootloader bootloader)
        {
            return Task.CompletedTask;
        }

        private UserInfo userInfo = UserInfo.GenerateRandomUserInfo();

        public bool IsUserInfoReady => true;

        public string UserId => this.userInfo.UserId;

        public string DisplayName => this.userInfo.DisplayName;

        public UserInfo GetMyUserInfo() => this.userInfo;
    }
}
