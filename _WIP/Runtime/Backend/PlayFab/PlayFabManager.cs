//-----------------------------------------------------------------------
// <copyright file="PlayFabManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT.PlayFab
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::PlayFab;
    using global::PlayFab.ClientModels;
    using global::PlayFab.Internal;
    using OGT.IAP;
    using Newtonsoft.Json;
    using UnityEngine;

    public sealed class PlayFabManager : Manager
    {
        private static ISerializerPlugin serializerPlugin;

#pragma warning disable 0649
        [SerializeField] private LoginMethod loginMethod;
        [SerializeField] private Settings settings;
#pragma warning restore 0649

        private bool regainFocusCoroutineRunning;
        private DateTime lostFocusTime = DateTime.UtcNow;

        public delegate void Action<T1, T2, T3, T4, T5>(T1 arg, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

        public static PlayFabManager Instance
        {
            get
            {
                Debug.LogError("PlayFabManager.Instance no longer supported!");
                return GameObject.FindFirstObjectByType<Bootloader>().FindManager<PlayFabManager>();
            }
        }

        public string CatalogVersion { get; private set; }

        public int CloudScriptRevision { get; private set; }

        protected override async Task InitializeManager(Bootloader bootloader)
        {
            PlayFabSettings.GlobalErrorHandler += this.OnGlobalErrorEvent;

            // yield return UnityPurchasingManager.WaitForInitialization();

            PlayFabSettings.staticSettings.TitleId = PlayFabRuntimeSettings.TitleId;
            this.CatalogVersion = PlayFabRuntimeSettings.CatalogVersion;
            this.CloudScriptRevision = PlayFabRuntimeSettings.CloudScriptRevision;

            this.Login = new LoginManager(this);

            // Starting the Logging In loop
            var combinedParams = new GetPlayerCombinedInfoRequestParams
            {
                GetTitleData = this.settings.LoadTitleDataKeys.Count > 0,
                TitleDataKeys = this.settings.LoadTitleDataKeys,
                GetUserInventory = this.settings.LoadInventory,
                GetUserVirtualCurrency = this.settings.LoadVirtualCurrency,
                GetPlayerProfile = this.settings.LoadPlayerProfile,
                GetCharacterList = this.settings.LoadCharacters,
                GetUserAccountInfo = true,
            };

            LostMessages.BootloaderLoggingIn();
            var login = this.Login.Login(this.loginMethod, combinedParams, this.settings.FacebookPermissions);

            while (login.IsDone == false)
            {
                await Task.Delay(100);
            }

            this.Catalog = new CatalogManager(this);
            this.Character = new CharacterManager(this, login.Value?.InfoResultPayload?.CharacterList);
            this.CloudScript = new CloudScriptManager();
            this.Inventory = new InventoryManager(this, login.Value?.InfoResultPayload?.UserInventory);
            this.Purchasing = new PurchasingManager(this);

            this.PushNotifications = new PushNotificationManager(this);
            this.Store = new StoreManager(this);
            this.TitleData = new TitleDataManager(this, login.Value?.InfoResultPayload?.TitleData);
            this.TitleNews = new TitleNewsManager(this);
            this.User = new UserManager(this, login.Value);
            this.VirtualCurrency = new VirtualCurrencyManager(this, login.Value?.InfoResultPayload?.UserVirtualCurrency);

            throw new System.NotImplementedException();

            //// // Catalog
            //// if (this.settings.LoadCatalog)
            //// {
            ////     LostMessages.BootloaderDownloadingCatalog();
            ////     yield return this.Catalog.GetCatalog();
            //// }
            //// 
            //// // Stores
            //// if (this.settings.LoadStoresAtStartup?.Count > 0)
            //// {
            ////     LostMessages.BootloaderLoadingStores();
            //// 
            ////     foreach (var store in this.settings.LoadStoresAtStartup)
            ////     {
            ////         // TODO [bgish]: Tell the loading dialog that we're getting store "X"
            ////         yield return this.Store.GetStore(store);
            ////     }
            //// }
            //// 
            //// // Initializing purchasing, but no need to wait on it later
            //// if (this.settings.LoadPurchasing)
            //// {
            ////     LostMessages.BootloaderInitializingPurchasing();
            ////     yield return this.Purchasing.Initialize();
            //// }
            //// 
            //// // Push Notifications
            //// if (Application.platform == RuntimePlatform.IPhonePlayer && this.settings.RegisterIosPushNotificationsAtStartup)
            //// {
            ////     this.PushNotifications.RegisterPushNotificationsWithPlayFab();
            //// }
            //// else if (Application.platform == RuntimePlatform.Android)
            //// {
            ////     this.PushNotifications.RegisterPushNotificationsWithPlayFab();
            //// }
        }

        public void OverrideCatalogVersion(string newCatalogVersion)
        {
            this.CatalogVersion = newCatalogVersion;
        }

        public void OverrideCloudScriptRevision(int newCloudScriptRevision)
        {
            this.CloudScriptRevision = newCloudScriptRevision;
        }

        public enum TitleDataSerializationMethod
        {
            PlayFab,
            JsonDotNet,
            Unity,
        }

        public static ISerializerPlugin SerializerPlugin
        {
            get
            {
                if (serializerPlugin == null)
                {
                    serializerPlugin = global::PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
                }

                return serializerPlugin;
            }
        }

        public Action<PlayFabRequestCommon, PlayFabResultCommon> GlobalPlayFabResultHandler { get; set; }

        public CatalogManager Catalog { get; private set; }

        public CharacterManager Character { get; private set; }

        public CloudScriptManager CloudScript { get; private set; }

        public InventoryManager Inventory { get; private set; }

        public LoginManager Login { get; private set; }

        public PurchasingManager Purchasing { get; private set; }

        public PushNotificationManager PushNotifications { get; private set; }

        public StoreManager Store { get; private set; }

        public TitleDataManager TitleData { get; private set; }

        public TitleNewsManager TitleNews { get; private set; }

        public UserManager User { get; private set; }

        public VirtualCurrencyManager VirtualCurrency { get; private set; }

        public static long ConvertPlayFabIdToLong(string playfabId)
        {
            return System.Convert.ToInt64(playfabId, 16);
        }

        public UnityTask<string> GetRealtimeMessagesUrl()
        {
            throw new NotImplementedException();
        }

        public UnityTask<ExecuteCloudScriptResult> Do(ExecuteCloudScriptRequest request)
        {
            return this.Do<ExecuteCloudScriptRequest, ExecuteCloudScriptResult>(request, PlayFabClientAPI.ExecuteCloudScriptAsync);
        }

        public UnityTask<EmptyResponse> Do(UpdateAvatarUrlRequest request)
        {
            return this.Do<UpdateAvatarUrlRequest, EmptyResponse>(request, PlayFabClientAPI.UpdateAvatarUrlAsync);
        }

        public UnityTask<UpdateUserTitleDisplayNameResult> Do(UpdateUserTitleDisplayNameRequest request)
        {
            return this.Do<UpdateUserTitleDisplayNameRequest, UpdateUserTitleDisplayNameResult>(request, PlayFabClientAPI.UpdateUserTitleDisplayNameAsync);
        }

        public UnityTask<GetAccountInfoResult> Do(GetAccountInfoRequest request)
        {
            return this.Do<GetAccountInfoRequest, GetAccountInfoResult>(request, PlayFabClientAPI.GetAccountInfoAsync);
        }

        public UnityTask<GetContentDownloadUrlResult> Do(GetContentDownloadUrlRequest request)
        {
            return this.Do<GetContentDownloadUrlRequest, GetContentDownloadUrlResult>(request, PlayFabClientAPI.GetContentDownloadUrlAsync);
        }

        public UnityTask<GetUserInventoryResult> Do(GetUserInventoryRequest request)
        {
            return this.Do<GetUserInventoryRequest, GetUserInventoryResult>(request, PlayFabClientAPI.GetUserInventoryAsync);
        }

        public UnityTask<GetPlayerCombinedInfoResult> Do(GetPlayerCombinedInfoRequest request)
        {
            return this.Do<GetPlayerCombinedInfoRequest, GetPlayerCombinedInfoResult>(request, PlayFabClientAPI.GetPlayerCombinedInfoAsync);
        }

        public UnityTask<GetPlayerSegmentsResult> Do(GetPlayerSegmentsRequest request)
        {
            return this.Do<GetPlayerSegmentsRequest, GetPlayerSegmentsResult>(request, PlayFabClientAPI.GetPlayerSegmentsAsync);
        }

        public UnityTask<WriteEventResponse> Do(WriteClientPlayerEventRequest request)
        {
            return this.Do<WriteClientPlayerEventRequest, WriteEventResponse>(request, PlayFabClientAPI.WritePlayerEventAsync);
        }

        public UnityTask<GetTimeResult> RefreshServerTime()
        {
            return this.Do<GetTimeRequest, GetTimeResult>(new GetTimeRequest(), PlayFabClientAPI.GetTimeAsync);
        }

        //// User Data Related Functions ------------------

        public UnityTask<GetUserDataResult> Do(GetUserDataRequest request)
        {
            return this.Do<GetUserDataRequest, GetUserDataResult>(request, PlayFabClientAPI.GetUserDataAsync);
        }

        public UnityTask<UpdateUserDataResult> Do(UpdateUserDataRequest request)
        {
            return this.Do<UpdateUserDataRequest, UpdateUserDataResult>(request, PlayFabClientAPI.UpdateUserDataAsync);
        }

        //// Player Statistics Related Functions ------------------

        public UnityTask<GetPlayerStatisticsResult> Do(GetPlayerStatisticsRequest request)
        {
            return this.Do<GetPlayerStatisticsRequest, GetPlayerStatisticsResult>(request, PlayFabClientAPI.GetPlayerStatisticsAsync);
        }

        //// Title Data Related Functions ------------------

        public UnityTask<GetTitleDataResult> Do(GetTitleDataRequest request)
        {
            return this.Do<GetTitleDataRequest, GetTitleDataResult>(request, PlayFabClientAPI.GetTitleDataAsync);
        }

        public UnityTask<GetTitleNewsResult> Do(GetTitleNewsRequest request)
        {
            return this.Do<GetTitleNewsRequest, GetTitleNewsResult>(request, PlayFabClientAPI.GetTitleNewsAsync);
        }

        //// Push Notification Related Functions ------------------

        public UnityTask<AndroidDevicePushNotificationRegistrationResult> Do(AndroidDevicePushNotificationRegistrationRequest request)
        {
            return this.Do<AndroidDevicePushNotificationRegistrationRequest, AndroidDevicePushNotificationRegistrationResult>(request, PlayFabClientAPI.AndroidDevicePushNotificationRegistrationAsync);
        }

        public UnityTask<RegisterForIOSPushNotificationResult> Do(RegisterForIOSPushNotificationRequest request)
        {
            return this.Do<RegisterForIOSPushNotificationRequest, RegisterForIOSPushNotificationResult>(request, PlayFabClientAPI.RegisterForIOSPushNotificationAsync);
        }

        //// Leaderboard Related Functions ------------------

        public UnityTask<GetLeaderboardResult> Do(GetLeaderboardRequest request)
        {
            return this.Do<GetLeaderboardRequest, GetLeaderboardResult>(request, PlayFabClientAPI.GetLeaderboardAsync);
        }

        public UnityTask<GetLeaderboardResult> Do(GetFriendLeaderboardRequest request)
        {
            return this.Do<GetFriendLeaderboardRequest, GetLeaderboardResult>(request, PlayFabClientAPI.GetFriendLeaderboardAsync);
        }

        public UnityTask<GetLeaderboardAroundPlayerResult> Do(GetLeaderboardAroundPlayerRequest request)
        {
            return this.Do<GetLeaderboardAroundPlayerRequest, GetLeaderboardAroundPlayerResult>(request, PlayFabClientAPI.GetLeaderboardAroundPlayerAsync);
        }

        public UnityTask<GetFriendLeaderboardAroundPlayerResult> Do(GetFriendLeaderboardAroundPlayerRequest request)
        {
            return this.Do<GetFriendLeaderboardAroundPlayerRequest, GetFriendLeaderboardAroundPlayerResult>(request, PlayFabClientAPI.GetFriendLeaderboardAroundPlayerAsync);
        }

        //// Purchasing Related Functions ------------------

        public UnityTask<ConfirmPurchaseResult> Do(ConfirmPurchaseRequest request)
        {
            return this.Do<ConfirmPurchaseRequest, ConfirmPurchaseResult>(request, PlayFabClientAPI.ConfirmPurchaseAsync);
        }

        public UnityTask<ValidateIOSReceiptResult> Do(ValidateIOSReceiptRequest request)
        {
            return this.Do<ValidateIOSReceiptRequest, ValidateIOSReceiptResult>(request, PlayFabClientAPI.ValidateIOSReceiptAsync);
        }

        public UnityTask<ValidateGooglePlayPurchaseResult> Do(ValidateGooglePlayPurchaseRequest request)
        {
            return this.Do<ValidateGooglePlayPurchaseRequest, ValidateGooglePlayPurchaseResult>(request, PlayFabClientAPI.ValidateGooglePlayPurchaseAsync);
        }

        public UnityTask<ValidateAmazonReceiptResult> Do(ValidateAmazonReceiptRequest request)
        {
            return this.Do<ValidateAmazonReceiptRequest, ValidateAmazonReceiptResult>(request, PlayFabClientAPI.ValidateAmazonIAPReceiptAsync);
        }

        //// Friends Related Functions ------------------

        public UnityTask<AddFriendResult> Do(AddFriendRequest request)
        {
            // should update the cached list after it's done
            return this.Do<AddFriendRequest, AddFriendResult>(request, PlayFabClientAPI.AddFriendAsync);
        }

        public UnityTask<RemoveFriendResult> Do(RemoveFriendRequest request)
        {
            // should update the cached list after it's done
            return this.Do<RemoveFriendRequest, RemoveFriendResult>(request, PlayFabClientAPI.RemoveFriendAsync);
        }

        public UnityTask<GetFriendsListResult> Do(GetFriendsListRequest request)
        {
            // TODO [bgish] - should really cache this, and update friends list whenever add/remove.  If cached, then return the list instead of call this
            return this.Do<GetFriendsListRequest, GetFriendsListResult>(request, PlayFabClientAPI.GetFriendsListAsync);
        }

        //// Matchmaking Related Functions ------------------

        public UnityTask<CurrentGamesResult> Do(CurrentGamesRequest request)
        {
            return this.Do<CurrentGamesRequest, CurrentGamesResult>(request, PlayFabClientAPI.GetCurrentGamesAsync);
        }

        public UnityTask<MatchmakeResult> Do(MatchmakeRequest request)
        {
            return this.Do<MatchmakeRequest, MatchmakeResult>(request, PlayFabClientAPI.MatchmakeAsync);
        }

        public UnityTask<TResult> Do<TRequest, TResult>(TRequest request, Func<TRequest, object, Dictionary<string, string>, Task<PlayFabResult<TResult>>> playfabFunction)
            where TRequest : PlayFabRequestCommon
            where TResult : PlayFabResultCommon
        {
            return UnityTask<TResult>.Run(this.DoIterator(request, playfabFunction));
        }

        public IEnumerator<TResult> DoIterator<TRequest, TResult>(TRequest request, Func<TRequest, object, Dictionary<string, string>, Task<PlayFabResult<TResult>>> playfabFunction)
            where TRequest : PlayFabRequestCommon
            where TResult : PlayFabResultCommon
        {
            var task = playfabFunction.Invoke(request, null, null);

            while (task.IsCompleted == false)
            {
                yield return null;
            }

            if (task.Result.Error != null)
            {
                throw new PlayFabException(task.Result.Error);
            }

            // Checking cloud script logging for errors
            if (task.Result.Result is ExecuteCloudScriptResult executeCloudScriptResult)
            {
                for (int i = 0; i < executeCloudScriptResult.Logs.Count; i++)
                {
                    if (executeCloudScriptResult.Logs[i].Level == "Error")
                    {
                        throw new PlayFabCloudScriptException(executeCloudScriptResult.Logs[i].Message);
                    }
                }
            }

            this.GlobalPlayFabResultHandler?.Invoke(request, task.Result.Result);

            yield return task.Result.Result;
        }

        private void OnDestroy()
        {
            PlayFabSettings.GlobalErrorHandler -= this.OnGlobalErrorEvent;
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                double minutesSinceLostFocus = this.lostFocusTime.Subtract(DateTime.UtcNow).TotalMinutes;

                if (minutesSinceLostFocus > 1)
                {
                    CoroutineRunner.Instance.StartCoroutine(RefreshServerTimeCoroutine());
                }
            }
            else
            {
                this.lostFocusTime = DateTime.UtcNow;
            }

            IEnumerator RefreshServerTimeCoroutine()
            {
                if (this.regainFocusCoroutineRunning)
                {
                    yield break;
                }

                this.regainFocusCoroutineRunning = true;

                // NOTE [bgish]: We're getting the server time on application focus because if we've been gone for too long
                //               then it will fail with PlayFabErrorCode.InvalidSessionTicket and we'll reboot.
                yield return this.RefreshServerTime();

                this.regainFocusCoroutineRunning = false;
            }
        }

        private void OnApplicationQuit()
        {
            PlayFabClientAPI.ForgetAllCredentials();
        }

        private void OnGlobalErrorEvent(PlayFabError error)
        {
            if (error.Error == PlayFabErrorCode.InvalidSessionTicket)
            {
                //// CoroutineRunner.Instance.ExecuteAtEndOfFrame(Bootloader.Reboot);
            }
        }

        [Serializable]
        public class Settings
        {
#pragma warning disable 0649, CA2235
            [SerializeField] private string catalogVersion = "1.0";
            [SerializeField] private List<string> facebookPermissions = new List<string> { "public_profile", "email", "user_friends" };
            [SerializeField] private List<string> loadTitleDataKeys;
            [SerializeField] private List<string> loadStoresAtStartup;
            [SerializeField] private bool loadInventory;
            [SerializeField] private bool loadVirtualCurrency;
            [SerializeField] private bool loadPlayerProfile;
            [SerializeField] private bool loadCatalog;
            [SerializeField] private bool loadCharacters;
            [SerializeField] private bool loadPurchasing;
            [SerializeField] private bool registerIosPushNotificationsAtStartup;
#pragma warning restore 0649, CA2235

            public string CatalogVersion
            {
                get => this.catalogVersion;
                set => this.catalogVersion = value;
            }

            public List<string> FacebookPermissions
            {
                get => this.facebookPermissions;
                set => this.facebookPermissions = value;
            }

            public List<string> LoadTitleDataKeys
            {
                get => this.loadTitleDataKeys;
                set => this.loadTitleDataKeys = value;
            }

            public List<string> LoadStoresAtStartup
            {
                get => this.loadStoresAtStartup;
                set => this.loadStoresAtStartup = value;
            }

            public bool LoadInventory
            {
                get => this.loadInventory;
                set => this.loadInventory = value;
            }

            public bool LoadVirtualCurrency
            {
                get => this.loadVirtualCurrency;
                set => this.loadVirtualCurrency = value;
            }

            public bool LoadPlayerProfile
            {
                get => this.loadPlayerProfile;
                set => this.loadPlayerProfile = value;
            }

            public bool LoadCatalog
            {
                get => this.loadCatalog;
                set => this.loadCatalog = value;
            }

            public bool LoadCharacters
            {
                get => this.loadCharacters;
                set => this.loadCharacters = value;
            }

            public bool LoadPurchasing
            {
                get => this.loadPurchasing;
                set => this.loadPurchasing = value;
            }

            public bool RegisterIosPushNotificationsAtStartup
            {
                get => this.registerIosPushNotificationsAtStartup;
                set => this.registerIosPushNotificationsAtStartup = value;
            }
        }
    }
}

#endif
