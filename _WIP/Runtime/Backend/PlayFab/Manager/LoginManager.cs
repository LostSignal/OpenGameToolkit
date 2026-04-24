//-----------------------------------------------------------------------
// <copyright file="LoginManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY && USING_PLAYFAB

namespace OGT.PlayFab
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using global::PlayFab;
    using global::PlayFab.ClientModels;
    using global::PlayFab.Internal;
    using UnityEngine;

    public enum LoginMethod
    {
        AnonymousDeviceId,
        UsernameEmail,
        FacebookOnly,
        Apple,
    }

    public class LoginManager
    {
        private const string AnonymousIdKey = "AnnonymousId";

        private static readonly OGTLogger Logger = new("PlayFab");

        private readonly List<string> facebookPermissions = new() { "public_profile", "email" };
        private readonly PlayFabManager playfabManager;

        private LoginResult loginResult;
        private bool forceRelogin;
        private string anonymousId;
        public string appleAccountToken;

        public LoginManager(PlayFabManager playfabManager)
        {
            this.playfabManager = playfabManager;

            playfabManager.GlobalPlayFabResultHandler += this.OnGlobalPlayFabResultHandler;
            PlayFabSettings.GlobalErrorHandler += this.PlayfabEvents_OnGlobalErrorEvent;
        }

        public string SessionTicket => this.loginResult?.SessionTicket;

        public bool IsDeviceIdLinked => this.IsCustomIdLinked(this.DeviceId);

        public string AnonymousId
        {
            get
            {
                if (string.IsNullOrEmpty(this.anonymousId))
                {
                    string playerPrefsAnonymousId = LostPlayerPrefs.GetString(AnonymousIdKey, null);

                    if (string.IsNullOrEmpty(playerPrefsAnonymousId))
                    {
                        this.anonymousId = Guid.NewGuid().ToString();
                        LostPlayerPrefs.SetString(AnonymousIdKey, this.anonymousId);
                        LostPlayerPrefs.Save();
                    }
                    else
                    {
                        this.anonymousId = playerPrefsAnonymousId;
                    }
                }

                return this.anonymousId;
            }
        }

        public string DeviceId
        {
            get { return this.AnonymousId; }
        }

        public bool IsLoggedIn
        {
            get { return this.forceRelogin == false && PlayFabClientAPI.IsClientLoggedIn(); }
        }

        public Dictionary<string, UserDataRecord> UserData => this.loginResult.InfoResultPayload.UserData;

        public string LastLoginEmail
        {
            get => LostPlayerPrefs.GetString("LogIn-LastLoginEmail", string.Empty);
            set => LostPlayerPrefs.SetString("LogIn-LastLoginEmail", value, true);
        }

        public bool AutoLoginWithDeviceId
        {
            get => LostPlayerPrefs.GetBool("LogIn-AutoLoginWithDeviceId", false);
            set => LostPlayerPrefs.SetBool("LogIn-AutoLoginWithDeviceId", value, true);
        }

        public bool HasEverLoggedIn
        {
            get => LostPlayerPrefs.GetBool("LogIn-HasEverLoggedIn", false);
            set => LostPlayerPrefs.SetBool("LogIn-HasEverLoggedIn", value, true);
        }

        public bool HasLinkedFacebook
        {
            get => LostPlayerPrefs.GetBool("LogIn-HasLinkedFacebook", false);
            set => LostPlayerPrefs.SetBool("LogIn-HasLinkedFacebook", value, true);
        }

#if USING_FACEBOOK_SDK
        public Facebook.Unity.ILoginResult FacebookLoginResult { get; private set; }
#endif

        public string GetEmailCustomId(string email)
        {
            string key = $"LogIn-{email}";

            if (LostPlayerPrefs.HasKey(key) == false)
            {
                LostPlayerPrefs.SetString(key, System.Guid.NewGuid().ToString(), true);
            }

            return LostPlayerPrefs.GetString(key, null);
        }

        public bool IsCustomIdLinked(string customId)
        {
            if (Application.isEditor || Platform.IsIosOrAndroid == false)
            {
                return this.loginResult?.InfoResultPayload?.AccountInfo?.CustomIdInfo?.CustomId == customId;
            }
            else if (Platform.CurrentDevicePlatform == DevicePlatform.iOS)
            {
                return this.loginResult?.InfoResultPayload?.AccountInfo?.IosDeviceInfo?.IosDeviceId == customId;
            }
            else if (Platform.CurrentDevicePlatform == DevicePlatform.Android)
            {
                return this.loginResult?.InfoResultPayload?.AccountInfo?.AndroidDeviceInfo?.AndroidDeviceId == customId;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public UnityTask<LoginResult> Login(LoginMethod loginMethod, GetPlayerCombinedInfoRequestParams combinedInfoRequestParams, List<string> facebookPermissions = null)
        {
            return UnityTask<LoginResult>.Run(Coroutine());

            IEnumerator<LoginResult> Coroutine()
            {
                while (this.IsLoggedIn == false)
                {
                    UnityTask<LoginResult> login = null;

                    if (this.HasLinkedFacebook)
                    {
                        login = this.LoginWithFacebook(false, combinedInfoRequestParams, facebookPermissions);
                    }
                    else if (loginMethod == LoginMethod.FacebookOnly)
                    {
                        login = this.LoginWithFacebook(true, combinedInfoRequestParams, facebookPermissions);
                    }
                    else if (loginMethod == LoginMethod.AnonymousDeviceId)
                    {
                        login = this.LoginWithDeviceId(true, this.DeviceId, combinedInfoRequestParams);
                    }
                    else if (loginMethod == LoginMethod.Apple)
                    {
                        Debug.LogError("APPLE LOGIN NOT IMPLEMENTED!");
                    }

                    if (login != null)
                    {
                        // Waiting for login to finish
                        while (login.IsDone == false)
                        {
                            yield return null;
                        }

                        // If we tried logging in with facebook, but the account doesn't exist, then remove account link and retry
                        if (this.HasLinkedFacebook && login.HasError)
                        {
                            var playfabException = login.Exception as PlayFabException;

                            if (playfabException?.Error?.Error == PlayFabErrorCode.AccountNotFound)
                            {
                                this.HasLinkedFacebook = false;
                                continue;
                            }
                        }

                        this.loginResult = login.Value;
                    }

                    if (loginMethod == LoginMethod.UsernameEmail)
                    {
                        this.LoginWithEmailAndPasswordDialog(combinedInfoRequestParams);

                        // Waiting for the login dialogs to finish logging in the user
                        while (this.loginResult == null)
                        {
                            yield return null;
                        }
                    }

                    if (this.IsLoggedIn == false)
                    {
                        // Waiting
                        float wait1 = Time.realtimeSinceStartup + 0.7f;
                        while (Time.realtimeSinceStartup < wait1)
                        {
                            yield return null;
                        }

                        var show = LostMessages.ShowUnableToConnectToServer();

                        while (show.IsRunning())
                        {
                            yield return null;
                        }

                        if (show.GetAwaiter().GetResult() == YesNoResult.No)
                        {
                            Platform.QuitApplication();
                        }

                        // Waiting
                        float wait2 = Time.realtimeSinceStartup + 0.25f;
                        while (Time.realtimeSinceStartup < wait2)
                        {
                            yield return null;
                        }
                    }
                }

                /*Analytics.AnalyticsEvent.Custom("player_logged_in", new Dictionary<string, object>
                {
                    { "playfab_id", this.loginResult?.InfoResultPayload?.AccountInfo?.PlayFabId },
                    { "is_facebook_linked",  string.IsNullOrEmpty(this.loginResult?.InfoResultPayload?.AccountInfo?.FacebookInfo?.FacebookId) == false },
                });*/

                yield return this.loginResult;
            }
        }

        public UnityTask<SendAccountRecoveryEmailResult> SendAccountRecoveryEmail(string email, string emailTemplateId)
        {
            return this.playfabManager.Do<SendAccountRecoveryEmailRequest, SendAccountRecoveryEmailResult>(
                new SendAccountRecoveryEmailRequest
                {
                    Email = email,
                    EmailTemplateId = emailTemplateId,
                },
                PlayFabClientAPI.SendAccountRecoveryEmailAsync);
        }

        public UnityTask<LoginResult> LoginWithEmailAddress(string email, string password, GetPlayerCombinedInfoRequestParams combinedInfoParams = null)
        {
            return this.playfabManager.Do<LoginWithEmailAddressRequest, LoginResult>(
                new LoginWithEmailAddressRequest
                {
                    Email = email,
                    Password = password,
                    InfoRequestParameters = this.GetCombinedInfoRequest(combinedInfoParams),
                },
                PlayFabClientAPI.LoginWithEmailAddressAsync);
        }

        public UnityTask<RegisterPlayFabUserResult> RegisterPlayFabUser(string email, string password, GetPlayerCombinedInfoRequestParams combinedInfoParams = null)
        {
            return this.playfabManager.Do<RegisterPlayFabUserRequest, RegisterPlayFabUserResult>(
                new RegisterPlayFabUserRequest
                {
                    Email = email,
                    Password = password,
                    InfoRequestParameters = this.GetCombinedInfoRequest(combinedInfoParams),
                    RequireBothUsernameAndEmail = false,
                },
                PlayFabClientAPI.RegisterPlayFabUserAsync);
        }

        public UnityTask<PlayFabResultCommon> LinkDeviceId(string deviceId)
        {
            return UnityTask<PlayFabResultCommon>.Run(LinkDeviceIdIterator());

            IEnumerator<PlayFabResultCommon> LinkDeviceIdIterator()
            {
                if (Application.isEditor || Platform.IsIosOrAndroid == false)
                {
                    var coroutine = this.playfabManager.DoIterator<LinkCustomIDRequest, LinkCustomIDResult>(new LinkCustomIDRequest { CustomId = deviceId }, PlayFabClientAPI.LinkCustomIDAsync);

                    while (coroutine.MoveNext())
                    {
                        yield return coroutine.Current as PlayFabResultCommon;
                    }
                }
                else if (Platform.CurrentDevicePlatform == DevicePlatform.iOS)
                {
                    var coroutine = this.playfabManager.DoIterator<LinkIOSDeviceIDRequest, LinkIOSDeviceIDResult>(
                        new LinkIOSDeviceIDRequest
                        {
                            DeviceId = deviceId,
                            DeviceModel = UnityEngine.SystemInfo.deviceModel,
                            OS = UnityEngine.SystemInfo.operatingSystem,
                        },
                        PlayFabClientAPI.LinkIOSDeviceIDAsync);

                    while (coroutine.MoveNext())
                    {
                        yield return coroutine.Current as PlayFabResultCommon;
                    }
                }
                else if (Platform.CurrentDevicePlatform == DevicePlatform.Android)
                {
                    var coroutine = this.playfabManager.DoIterator<LinkAndroidDeviceIDRequest, LinkAndroidDeviceIDResult>(
                        new LinkAndroidDeviceIDRequest
                        {
                            AndroidDeviceId = deviceId,
                            AndroidDevice = UnityEngine.SystemInfo.deviceModel,
                            OS = UnityEngine.SystemInfo.operatingSystem,
                        },
                        PlayFabClientAPI.LinkAndroidDeviceIDAsync);

                    while (coroutine.MoveNext())
                    {
                        yield return coroutine.Current as PlayFabResultCommon;
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public UnityTask<PlayFabResultCommon> UnlinkDeviceId(string deviceId)
        {
            return UnityTask<PlayFabResultCommon>.Run(UnlinkDeviceIdIterator());

            IEnumerator<PlayFabResultCommon> UnlinkDeviceIdIterator()
            {
                if (Application.isEditor || Platform.IsIosOrAndroid == false)
                {
                    var coroutine = this.playfabManager.Do<UnlinkCustomIDRequest, UnlinkCustomIDResult>(new UnlinkCustomIDRequest { CustomId = deviceId }, PlayFabClientAPI.UnlinkCustomIDAsync);

                    while (coroutine.MoveNext())
                    {
                        yield return coroutine.Current as PlayFabResultCommon;
                    }
                }
                else if (Platform.CurrentDevicePlatform == DevicePlatform.iOS)
                {
                    var coroutine = this.playfabManager.Do<UnlinkIOSDeviceIDRequest, UnlinkIOSDeviceIDResult>(new UnlinkIOSDeviceIDRequest { DeviceId = deviceId }, PlayFabClientAPI.UnlinkIOSDeviceIDAsync);

                    while (coroutine.MoveNext())
                    {
                        yield return coroutine.Current as PlayFabResultCommon;
                    }
                }
                else if (Platform.CurrentDevicePlatform == DevicePlatform.Android)
                {
                    var coroutine = this.playfabManager.Do<UnlinkAndroidDeviceIDRequest, UnlinkAndroidDeviceIDResult>(new UnlinkAndroidDeviceIDRequest { AndroidDeviceId = deviceId }, PlayFabClientAPI.UnlinkAndroidDeviceIDAsync);

                    while (coroutine.MoveNext())
                    {
                        yield return coroutine.Current as PlayFabResultCommon;
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public UnityTask<LinkFacebookAccountResult> LinkFacebook()
        {
            return UnityTask<LinkFacebookAccountResult>.Run(LinkFacebookCoroutine());

            IEnumerator<LinkFacebookAccountResult> LinkFacebookCoroutine()
            {
                var accessToken = this.GetFacebookAccessToken();

                while (accessToken.IsDone == false)
                {
                    yield return default;
                }

                var request = new LinkFacebookAccountRequest { AccessToken = accessToken.Value };

                var link = this.playfabManager.Do<LinkFacebookAccountRequest, LinkFacebookAccountResult>(request, PlayFabClientAPI.LinkFacebookAccountAsync);

                while (link.IsDone == false)
                {
                    yield return default;
                }

                yield return link.Value;
            }
        }

        public UnityTask<UnlinkFacebookAccountResult> UnlinkFacebook()
        {
            return this.playfabManager.Do<UnlinkFacebookAccountRequest, UnlinkFacebookAccountResult>(new UnlinkFacebookAccountRequest(), PlayFabClientAPI.UnlinkFacebookAccountAsync);
        }

        public void Logout()
        {
            this.AutoLoginWithDeviceId = false;
            this.LastLoginEmail = null;
            this.forceRelogin = true;
            this.loginResult = null;
        }

        public void LogOutOfFacebook()
        {
#if !USING_FACEBOOK_SDK
            throw new FacebookException("USING_FACEBOOK_SDK is not defined!  Check your AppSettings.");
#else
            if (Facebook.Unity.FB.IsLoggedIn)
            {
                Facebook.Unity.FB.LogOut();
            }
#endif
        }

        //// public IEnumerator SwitchToFacebookAccount()
        //// {
        ////     // TODO [bgish]: Need to handle errors!
        ////
        ////     this.LoginMethod = LoginMethod.Facebook;
        ////     yield return PF.Login.UnlinkDeviceId(PF.Login.DeviceId);
        ////     yield return this.Relogin();
        ////     yield return PF.Login.LinkDeviceId(PF.Login.DeviceId);
        //// }
        ////
        //// public IEnumerator Relogin()
        //// {
        ////     PF.Login.Logout();
        ////     this.IsServerInitialized = false;
        ////     yield return this.InitializeServer();
        ////     this.UpdateServerDebugInfo();
        //// }

        // private void LinkFacebookButtonClicked(Action onComplete)
        // {
        //     if (DialogManager.GetDialog<GameDialog>().CurrentGame != null)
        //     {
        //         TienLenMessages.ShowCantLinkFacebookWhileGameRunning();
        //         return;
        //     }
        //
        //     CoroutineRunner.Instance.StartCoroutine(LinkFacebookCoroutine());
        //
        //     IEnumerator LinkFacebookCoroutine()
        //     {
        //         if (this.linkFacebookCoroutineRunning)
        //         {
        //             yield break;
        //         }
        //
        //         this.linkFacebookCoroutineRunning = true;
        //
        //         var link = PF.Login.LinkFacebook();
        //
        //         yield return link;
        //
        //         if (link.HasError == false)
        //         {
        //             this.UpdateFacebook();
        //
        //             TienLenServer.Instance.LoginMethod = LoginMethod.Facebook;
        //
        //             yield return TienLenMessages.ShowFacebookLinkSucess();
        //         }
        //         else if (link.Exception is PlayFabException)
        //         {
        //             var playfabException = link.Exception as PlayFabException;
        //
        //             if (playfabException.Error.Error == PlayFab.PlayFabErrorCode.LinkedAccountAlreadyClaimed)
        //             {
        //                 var accountExists = TienLenMessages.ShowSwitchToFacebookAccount();
        //
        //                 yield return accountExists;
        //
        //                 if (accountExists.Value == YesNoResult.Yes)
        //                 {
        //                     yield return TienLenServer.Instance.SwitchToFacebookAccount();
        //                     this.UpdateFacebook();
        //                 }
        //             }
        //         }
        //
        //         this.linkFacebookCoroutineRunning = false;
        //
        //         onComplete?.Invoke();
        //     }
        // }

        //// private void UnlinkFacebookButtonClicked()
        //// {
        ////     var sideBarDialog = DialogManager.GetDialog<SideBarDialog>();
        ////
        ////     if (sideBarDialog.IsFriendsListShowing)
        ////     {
        ////         sideBarDialog.HideAll();
        ////     }
        ////
        ////     CoroutineRunner.Instance.StartCoroutine(UnlinkFacebookCoroutine());
        ////
        ////     IEnumerator UnlinkFacebookCoroutine()
        ////     {
        ////         if (this.unlinkFacebookCoroutineRunning)
        ////         {
        ////             yield break;
        ////         }
        ////
        ////         this.unlinkFacebookCoroutineRunning = true;
        ////
        ////         var unlinkFacebook = TienLenMessages.ShowDisconnectFromFacebook();
        ////
        ////         yield return unlinkFacebook;
        ////
        ////         if (unlinkFacebook.Value == YesNoResult.Yes)
        ////         {
        ////             PF.Login.LogoutOfFacebook();
        ////
        ////             var unlink = PF.Login.UnlinkFacebook();
        ////
        ////             yield return unlink;
        ////
        ////             if (unlink.HasError == false)
        ////             {
        ////                 TienLenServer.Instance.LoginMethod = LoginMethod.DeviceId;
        ////                 this.UpdateFacebook();
        ////                 yield return TienLenMessages.ShowFacebookUnlinkSuccess();
        ////             }
        ////         }
        ////
        ////         this.unlinkFacebookCoroutineRunning = false;
        ////     }
        //// }


        public UnityTask<LoginResult> LoginWithApple(bool createAccount, GetPlayerCombinedInfoRequestParams combinedInfoParams)
        {
            return UnityTask<LoginResult>.Run(this.LoginWithAppleCoroutine(createAccount, combinedInfoParams));
        }

        public UnityTask<EmptyResult> LinkAppleAccount(string token)
        {
            Debug.Log("=====> LinkAppleAccount token=" + token);
            appleAccountToken = token;
            return UnityTask<EmptyResult>.Run(this.LinkAppleAccountCoroutine(token));
        }

        public UnityTask<EmptyResponse> UnlinkAppleAccount()
        {
            Debug.Log("=====> UnlinkAppleAccount called");
            appleAccountToken = "";
            return this.playfabManager.Do<UnlinkAppleRequest, EmptyResponse>(new UnlinkAppleRequest(), PlayFabClientAPI.UnlinkAppleAsync);
        }

        private IEnumerator<LoginResult> LoginWithAppleCoroutine(bool createAccount, GetPlayerCombinedInfoRequestParams combinedInfoParams)
        {
            if (string.IsNullOrEmpty(appleAccountToken))
            {
                appleAccountToken = PlayerPrefs.GetString("SAVED_APPLE_ID_TOKEN", "");
            }

            var appleLoginRequest = new LoginWithAppleRequest
            {
                IdentityToken = appleAccountToken,
                CreateAccount = createAccount,
                InfoRequestParameters = this.GetCombinedInfoRequest(combinedInfoParams),
            };

            var link = this.playfabManager.Do<LoginWithAppleRequest, LoginResult>(appleLoginRequest, PlayFabClientAPI.LoginWithAppleAsync);

            Debug.Log("==>>> LoginWithAppleCoroutine -> called");
            while (link.IsDone == false)
            {
                yield return default(LoginResult);
            }

            yield return link.Value;
        }

        private IEnumerator<EmptyResult> LinkAppleAccountCoroutine(string token)
        {
            var request = new LinkAppleRequest { IdentityToken = token };

            var link = this.playfabManager.Do<LinkAppleRequest, EmptyResult>(request, PlayFabClientAPI.LinkAppleAsync);

            Debug.Log("LinkAppleAccountCoroutine -> called");
            while (link.IsDone == false)
            {
                yield return default(EmptyResult);
            }

            Debug.Log("LinkAppleAccountCoroutine -> value=" + link.Value);
            Debug.Log("LinkAppleAccountCoroutine -> hasError= " + link.HasError);
            yield return link.Value;
        }

        public void PrintFacebookInfo()
        {
#if USING_FACEBOOK_SDK
            Logger.Log("Facebook.Unity.FB.IsInitialized = " + Facebook.Unity.FB.IsInitialized);
            Logger.Log("Facebook.Unity.FB.IsLoggedIn = " + Facebook.Unity.FB.IsLoggedIn);

            var currentAccessToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            var permissionsList = currentAccessToken?.Permissions;
            var permissionsString = (permissionsList != null ? string.Join(", ", permissionsList) : string.Empty);

            Logger.Log("Facebook.Unity.AccessToken.CurrentAccessToken.ExpirationTime = " + currentAccessToken?.ExpirationTime);
            Logger.Log("Facebook.Unity.AccessToken.CurrentAccessToken.LastRefresh = " + currentAccessToken?.LastRefresh);
            Logger.Log("Facebook.Unity.AccessToken.CurrentAccessToken.Permissions = " + permissionsString);
            Logger.Log("Facebook.Unity.AccessToken.CurrentAccessToken.UserId = " + currentAccessToken?.UserId);
            Logger.Log("Facebook.Unity.AccessToken.CurrentAccessToken.TokenString = " + currentAccessToken?.TokenString);
#else
            Logger.Log("USING_FACEBOOK_SDK Not Set!");
#endif
        }

        private UnityTask<LoginResult> LoginWithDeviceId(bool createAccount, string deviceId, GetPlayerCombinedInfoRequestParams combinedInfoParams = null)
        {
            if (Application.isEditor || Platform.IsIosOrAndroid == false)
            {
                return this.playfabManager.Do<LoginWithCustomIDRequest, LoginResult>(
                    new LoginWithCustomIDRequest
                    {
                        CreateAccount = createAccount,
                        CustomId = deviceId,
                        InfoRequestParameters = this.GetCombinedInfoRequest(combinedInfoParams),
                    },
                    PlayFabClientAPI.LoginWithCustomIDAsync);
            }
            else if (Platform.CurrentDevicePlatform == DevicePlatform.iOS)
            {
                return this.playfabManager.Do<LoginWithIOSDeviceIDRequest, LoginResult>(
                    new LoginWithIOSDeviceIDRequest
                    {
                        CreateAccount = createAccount,
                        DeviceId = deviceId,
                        DeviceModel = SystemInfo.deviceModel,
                        OS = SystemInfo.operatingSystem,
                        InfoRequestParameters = this.GetCombinedInfoRequest(combinedInfoParams),
                    },
                    PlayFabClientAPI.LoginWithIOSDeviceIDAsync);
            }
            else if (Platform.CurrentDevicePlatform == DevicePlatform.Android)
            {
                return this.playfabManager.Do<LoginWithAndroidDeviceIDRequest, LoginResult>(
                    new LoginWithAndroidDeviceIDRequest
                    {
                        CreateAccount = createAccount,
                        AndroidDeviceId = deviceId,
                        AndroidDevice = SystemInfo.deviceModel,
                        OS = SystemInfo.operatingSystem,
                        InfoRequestParameters = this.GetCombinedInfoRequest(combinedInfoParams),
                    },
                    PlayFabClientAPI.LoginWithAndroidDeviceIDAsync);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private UnityTask<LoginResult> LoginWithFacebook(bool createAccount, GetPlayerCombinedInfoRequestParams combinedInfoParams, List<string> facebookPermissions)
        {
            return UnityTask<LoginResult>.Run(LoginWithFacebookCoroutine());

            IEnumerator<LoginResult> LoginWithFacebookCoroutine()
            {
#if !USING_FACEBOOK_SDK
#pragma warning disable CS0162
                throw new FacebookException("USING_FACEBOOK_SDK is not defined!  Check your AppSettings.");
#endif

                // Making sure all passed in facebook permissions are appended to the global list
                if (facebookPermissions != null)
                {
                    foreach (var facebookPermission in facebookPermissions)
                    {
                        this.facebookPermissions.AddIfUnique(facebookPermission);
                    }
                }

                var accessToken = this.GetFacebookAccessToken();

                while (accessToken.IsDone == false)
                {
                    yield return default;
                }

                var facebookLoginRequest = new LoginWithFacebookRequest
                {
                    AccessToken = accessToken.Value,
                    CreateAccount = createAccount,
                    InfoRequestParameters = this.GetCombinedInfoRequest(combinedInfoParams),
                };

                var facebookLogin = this.playfabManager.Do<LoginWithFacebookRequest, LoginResult>(facebookLoginRequest, PlayFabClientAPI.LoginWithFacebookAsync);

                while (facebookLogin.IsDone == false)
                {
                    yield return default;
                }

                yield return facebookLogin.Value;

#if !USING_FACEBOOK_SDK
#pragma warning restore CS0162
#endif
            }
        }

        private Coroutine LoginWithEmailAndPasswordDialog(GetPlayerCombinedInfoRequestParams infoRequestParams = null)
        {
            return CoroutineRunner.Instance.StartCoroutine(Coroutine());

            IEnumerator Coroutine()
            {
                if (this.HasEverLoggedIn)
                {
                    if (this.AutoLoginWithDeviceId && this.LastLoginEmail.IsNullOrWhitespace() == false)
                    {
                        var deviceId = this.GetEmailCustomId(this.LastLoginEmail);
                        var login = this.LoginWithDeviceId(false, deviceId, this.GetCombinedInfoRequest(infoRequestParams));

                        yield return login;

                        if (login.HasError)
                        {
                            DialogManager.GetDialog<LogInDialog>().Show(this, this.GetCombinedInfoRequest(infoRequestParams));
                        }
                        else if (this.IsLoggedIn)
                        {
                            yield break;
                        }
                        else
                        {
                            // NOTE [bgish]: This should never happen, but catching this case just in case
                            Logger.LogError("LoginHelper.LoginWithDeviceId failed, but didn't correctly report the error.");
                            DialogManager.GetDialog<LogInDialog>().Show(this, this.GetCombinedInfoRequest(infoRequestParams));
                        }
                    }
                    else
                    {
                        DialogManager.GetDialog<LogInDialog>().Show(this, this.GetCombinedInfoRequest(infoRequestParams));
                    }
                }
                else
                {
                    DialogManager.GetDialog<SignUpDialog>().Show(this, this.GetCombinedInfoRequest(infoRequestParams));
                }

                while (this.IsLoggedIn == false)
                {
                    yield return null;
                }
            }
        }

        private UnityTask<string> GetFacebookAccessToken()
        {
            return UnityTask<string>.Run(GetFacebookAccessTokenCoroutine());

            static IEnumerator<string> GetFacebookAccessTokenCoroutine()
            {
#if !USING_FACEBOOK_SDK
                throw new FacebookException("USING_FACEBOOK_SDK is not defined!  Check your AppSettings.");
#else

                if (Facebook.Unity.FB.IsInitialized == false)
                {
                    bool initializationFinished = false;

                    Facebook.Unity.FB.Init(() => { initializationFinished = true; });

                    // HACK [bgish]: If you call FB.Init, then immediately switch scenes, the facebook loading object
                    //               will get destroyed and you'll never finish initialization.  The will ensure the
                    //               object stays alive.
                    var facebookObjectName = "UnityFacebookSDKPlugin";
                    var facebookGameObject = GameObject.Find(facebookObjectName);

                    if (facebookGameObject != null)
                    {
                        GameObject.DontDestroyOnLoad(facebookGameObject);
                    }
                    else
                    {
                        Lost.Logger.LogWarning($"Unable to find facebook loading object {facebookObjectName}");
                    }

                    // Waiting for FB to initialize
                    while (initializationFinished == false)
                    {
                        yield return null;
                    }
                }

                if (Facebook.Unity.FB.IsInitialized == false)
                {
                    throw new FacebookException("Initialization Failed!");
                }
                else if (Facebook.Unity.FB.IsLoggedIn == false || PF.User.HasFacebookPermissions(this.FacebookPermissions) == false)
                {
                    this.FacebookLoginResult = null;

                    Facebook.Unity.FB.LogInWithReadPermissions(this.FacebookPermissions, (loginResult) => { this.FacebookLoginResult = loginResult; });

                    // Waiting for FB login to complete
                    while (this.FacebookLoginResult == null)
                    {
                        yield return null;
                    }

                    // Checking for errors
                    if (this.FacebookLoginResult.Cancelled)
                    {
                        throw new FacebookException("User Canceled");
                    }
                    else if (this.FacebookLoginResult.AccessToken == null)
                    {
                        throw new FacebookException("AccessToken is Null!");
                    }
                    else if (string.IsNullOrEmpty(this.FacebookLoginResult.AccessToken.TokenString))
                    {
                        throw new FacebookException("TokenString is Null! or Empty!");
                    }
                }

                yield return Facebook.Unity.AccessToken.CurrentAccessToken.TokenString;
#endif
            }
        }

        private GetPlayerCombinedInfoRequestParams GetCombinedInfoRequest(GetPlayerCombinedInfoRequestParams request)
        {
            request ??= new GetPlayerCombinedInfoRequestParams();
            request.GetUserVirtualCurrency = true;
            request.GetUserInventory = true;
            request.GetUserAccountInfo = true;
            request.GetTitleData = request.TitleDataKeys?.Count > 0;

            return request;
        }

        private void OnGlobalPlayFabResultHandler(PlayFabRequestCommon request, PlayFabResultCommon result)
        {
            if (result is LoginResult loginResult)
            {
                this.loginResult = loginResult;
                this.forceRelogin = false;
                this.HasEverLoggedIn = true;
            }
        }

        private void PlayfabEvents_OnGlobalErrorEvent(PlayFabError error)
        {
            if (error.Error == PlayFabErrorCode.InvalidSessionTicket)
            {
                this.forceRelogin = true;
            }
        }
    }
}

#endif
