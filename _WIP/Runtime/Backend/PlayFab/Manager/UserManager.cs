//-----------------------------------------------------------------------
// <copyright file="UserManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY && USING_PLAYFAB

namespace OGT.PlayFab
{
    using System.Collections.Generic;
    using global::PlayFab.ClientModels;
    using global::PlayFab.Internal;

    public class UserManager
    {
        private readonly UserAccountInfo userAccountInfo;
        private readonly PlayFabManager playfabManager;

        public UserManager(PlayFabManager playfabManager, LoginResult loginResult)
        {
            this.playfabManager = playfabManager;
            this.playfabManager.GlobalPlayFabResultHandler += this.OnGlobalPlayFabResultHandler;

            this.userAccountInfo = loginResult?.InfoResultPayload?.AccountInfo;

            this.TitleInfo = this.userAccountInfo?.TitleInfo;
            this.PlayFabId = this.userAccountInfo?.PlayFabId;
            this.PlayFabNumericId = PlayFabManager.ConvertPlayFabIdToLong(this.PlayFabId);
            this.DisplayName = this.userAccountInfo?.TitleInfo?.DisplayName;
            this.FacebookId = this.userAccountInfo?.FacebookInfo?.FacebookId;
            this.AvatarUrl = this.userAccountInfo?.TitleInfo?.AvatarUrl;

            //// TODO [bgish]: Fire a AvatarUrlChanged event
            //// TODO [bgish]: Fire a DisplayNameChanged event
            //// TODO [bgish]: Fire a FacebookChanged event
        }

        public UserTitleInfo TitleInfo { get; private set; }

        public string PlayFabId { get; private set; }

        public long PlayFabNumericId { get; private set; }

        public string DisplayName { get; private set; }

        public string FacebookId { get; private set; }

        public string AvatarUrl { get; private set; }

        public bool IsFacebookLinked => string.IsNullOrEmpty(this.FacebookId) == false;

        public UnityTask<bool> ChangeDisplayName(string newDisplayName)
        {
            return UnityTask<bool>.Run(ChangeDisplayNameCoroutine());

            IEnumerator<bool> ChangeDisplayNameCoroutine()
            {
                var updateDisplayName = this.playfabManager.Do(new UpdateUserTitleDisplayNameRequest
                {
                    DisplayName = newDisplayName,
                });

                while (updateDisplayName.IsDone == false)
                {
                    yield return default;
                }

                // Early out if we got an error
                if (updateDisplayName.HasError)
                {
                    PlayFabMessages.HandleError(updateDisplayName.Exception);
                    yield return false;
                    yield break;
                }

                // TODO [bgish]: Fire off DisplayName changed event?
                // Updating the display name
                this.DisplayName = newDisplayName;

                yield return true;
            }
        }

        public UnityTask<bool> ChangeDisplayNameWithPopup()
        {
            return UnityTask<bool>.Run(ChangeDisplayNameWithPopupCoroutine());

            IEnumerator<bool> ChangeDisplayNameWithPopupCoroutine()
            {
                var stringInputBox = PlayFabMessages.ShowChangeDisplayNameInputBox(this.DisplayName);

                while (stringInputBox.IsCompleted == false)
                {
                    yield return default;
                }

                if (stringInputBox.Result.Result == StringInputResult.InputResult.Cancel)
                {
                    yield return false;
                    yield break;
                }

                var updateDisplayName = this.playfabManager.Do(new UpdateUserTitleDisplayNameRequest
                {
                    DisplayName = stringInputBox.Result.Text,
                });

                while (updateDisplayName.IsDone == false)
                {
                    yield return default;
                }

                // Early out if we got an error
                if (updateDisplayName.HasError)
                {
                    PlayFabMessages.HandleError(updateDisplayName.Exception);
                    yield return false;
                    yield break;
                }

                // TODO [bgish]: Fire off DisplayName changed event?
                // Updating the display name
                this.DisplayName = stringInputBox.Result.Text;

                yield return true;
            }
        }

        public bool HasFacebookPermission(string permission)
        {
#if USING_FACEBOOK_SDK
            var facebookPermissions = Facebook.Unity.AccessToken.CurrentAccessToken?.Permissions;

            if (facebookPermissions != null)
            {
                foreach (var p in facebookPermissions)
                {
                    if (p == permission)
                    {
                        return true;
                    }
                }
            }
#endif

            return false;
        }

        public bool HasFacebookPermissions(List<string> permissions)
        {
#if USING_FACEBOOK_SDK
            if (permissions.IsNullOrEmpty() == false)
            {
                foreach (var permission in permissions)
                {
                    if (this.HasFacebookPermission(permission) == false)
                    {
                        return false;
                    }
                }
            }

            return true;
#else

            return false;

#endif
        }

        private void OnGlobalPlayFabResultHandler(PlayFabRequestCommon request, PlayFabResultCommon result)
        {
            if (result is LinkFacebookAccountResult linkFacebookAccountResult)
            {
#if USING_FACEBOOK_SDK
                this.FacebookId = PF.Login.FacebookLoginResult?.AccessToken?.UserId;
#endif
            }
            else if (result is UnlinkFacebookAccountResult unlinkFacebookAccountResult)
            {
                this.FacebookId = null;
            }
            else if (result is EmptyResponse emptyResponse && request is UpdateAvatarUrlRequest)
            {
                // TODO [bgish]: Update AvatarUrl and fire a AvatarUrlChanged event
            }
            else if (result is UpdateUserTitleDisplayNameResult updateUserTitleDisplayNameResult)
            {
                // TODO [bgish]: Update DisplayName and fire a DisplayNameChanged event
            }
        }
    }
}

#endif
