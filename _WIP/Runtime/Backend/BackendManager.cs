//-----------------------------------------------------------------------
// <copyright file="BackendManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace SNP
{
    using OGT;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class BackendManager : GameBehavior
    {
        //public event Action OnLogin;

        public bool IsLoggedIn { get; private set; }
        public string PlayerId { get; private set; }
        public string DisplayName { get; private set; }
        public string SessionTicket { get; private set; }

        public void Logout()
        {
            var loginCache = this.GetLoginCache();
            loginCache.LastLoginEmail = null;
            this.SaveLoginCache(loginCache);
        }

        // Bootload should be an empty scene (except for a visual graph)

        // Open Boadloader Scene (VR)
        //    Show 2D or 3D Loading Screen
        //    Boot
        //    Hide Loading
        //    Show Login
        //    Fade Out
        //    Hide Bootloader Content
        //    Load/Show Main Menu
        //    Fade Up

        // Open Boadloader Scene (2D)
        //    Show 2D Loading Screen
        //    Boot
        //    Hide Loading
        //    Show Login
        //    Hide Login
        //    Fade Out
        //    Hide Bootloader Content
        //    Load/Show Main Menu
        //    Fade Up

        // Bootloader
        //    Initializing Providers and Managers...
        //    Checking for latest Version...
        //      Addressables URL and What's New Info (This requires a backed, so maybe don't put this in booloader?)
        //      Quit/Force Updated if Needed
        //      Initialize Addressables
        //    OnBootedEvent
        //    
        // Bootloader Visual Script
        //    OnBooted
        //       LoginWith2FACode, LoginWithUsernameAndPassword or LoginAnnoymous
        //         OnLogin -> Show Main Menu

        // Log Modes
        //    2FA and Auth Code
        //    Username/Password and DeviceId
        //    Anonymous

        // Need Request2FALogin Dialog
        // Need LoginWithCode Dialog

        // LoginWithEmailAndPassword Dialog
        // CreateAccountWithEmailAndPassword Dialog

        // string GetAnonymousDeviceId
        // bool AutoLoginWithDeviceId
        // LogIn-LastLoginEmail
        // LogIn-AutoLoginWithDeviceId
        // LogIn-HasEverLoggedIn

        // Anonymous Login
        // 

        // LinkEmailWithAnonymousAccount
        //   Does 2FA and passes in , then calls  Server.LinkServerCustomId


        // Logout
        //   LastLoginEmail = null

        // This needs to have a Visual Scripting Node
        public Task<bool> IsLoginRequired()
        {
            // var loginCache = GetLoginCache();
            // If LastLoginEmail or PlayerId or AuthToken empty, then return true
            // If LastLoginTime is greater than 12 hours, then login with Auth Token
            //     * IF success, update LoginCache LastLoginTime = now, SessionTicket = new one

            return Task.FromResult(true);
        }

        public Task<bool> LoginWithAuthToken(string email, string playerId, string authToken, string version, bool isEditor)
        {
            return Task.FromResult(true);
        }

        public Task<bool> Request2FALoginCode(string email)
        {
            return Task.FromResult(true);
        }

        public Task<bool> LoginWith2FACode(string email, string code, string displayName, string version, bool isEditor)
        {
            return Task.FromResult(true);
        }

        private LoginCache GetLoginCache()
        {
            return new LoginCache();
        }

        private void SaveLoginCache(LoginCache loginCache)
        {
        }

        private class LoginCache
        {
            public string LastLoginEmail { get; set; }

            public Dictionary<string, LoginInfo> LoginInfo { get; set; }
        }

        private class LoginInfo
        {
            public string DeviceId { get; set; }
            public bool AutoLoginWithDeviceId { get; set; }
            public DateTime LastLoginTime { get; set; }
            public string PlayerId { get; set; }
            public string SessionTicket { get; set; }
            public bool HasEverLoggedIn { get; set; }
        }
    }
}
