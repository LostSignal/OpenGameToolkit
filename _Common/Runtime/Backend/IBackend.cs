//-----------------------------------------------------------------------
// <copyright file="IBackend.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Backend
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IBackend
    {
        event Action OnLogIn;
        event Action OnReLogInRequired;
        event Action<string, int> OnCurrencyChanged;

        // Log in
        bool IsLoggedIn { get; }
        Task<LoginResult> AnonymousLogin(LoginInfo loginInfo);

        // UserData
        Task<string> GetUserData(string key);

        // Stores / Catalog / Inventory
        Task<List<StoreItem>> GetStoreItems(string storeId);
        Task<CatalogItem> GetCatalogItem(string itemId);
        Task<int> GetInventoryCount(string itemId);
        Task PurchaseStoreItem(StoreItem storeItem);
        Task<bool> CanAfford(StoreItem storeItem);

        // Virtual Currencies
        Task RefreshVirtualCurrency();
        void InternalAddVirtualCurrencyToInventory(string virtualCurrencyId, int amount);
        int GetCurrency(string currencyId);

        // Cloud Script
        Task<CloudScriptResult> ExecuteCloudScript(string functionName, object functionParameter = null);
        Task<CloudScriptResult<T>> ExecuteCloudScript<T>(string functionName, object functionParameter = null);
    }

    public class CloudScriptResult
    {
        public bool Success { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string JsonResult { get; set; }
    }

    public class CloudScriptResult<T> : CloudScriptResult
    {
        public T Result => JsonUtil.Deserialize<T>(this.JsonResult);
    }

    public class LoginInfo
    {
        public List<string> UserDataKeys { get; set; }

        public List<string> TitleDataKeys { get; set; }
    }

    public class StoreItem : IComparable<StoreItem>
    {
        public string ItemId { get; set; }

        public Dictionary<string, uint> RealCurrencyPrices { get; set; }

        public Dictionary<string, uint> VirtualCurrencyPrices { get; set; }

        public int GetVirtualCurrencyPrice(string virtualCurrencyId)
        {
            if (this.VirtualCurrencyPrices.TryGetValue(virtualCurrencyId, out uint cost))
            {
                return (int)cost;
            }

            return -1;
        }

        public int CompareTo(StoreItem other)
        {
            if (other == null || other.ItemId == null) return 1;
            if (ItemId == null) return -1;
            return ItemId.CompareTo(other.ItemId);
        }
    }

    public class CatalogItemBundleInfo
    {
        /// <summary>
        /// unique ItemId values for all items which will be added to the player inventory when the bundle is added
        /// </summary>
        //[Unordered]
        public List<string> BundledItems { get; set; }

        /// <summary>
        /// unique TableId values for all RandomResultTable objects which are part of the bundle (random tables will be resolved and
        /// add the relevant items to the player inventory when the bundle is added)
        /// </summary>
        //[Unordered]
        public List<string> BundledResultTables { get; set; }

        /// <summary>
        /// virtual currency types and balances which will be added to the player inventory when the bundle is added
        /// </summary>
        public Dictionary<string, uint> BundledVirtualCurrencies { get; set; }
    }

    /// <summary>
    /// A purchasable item from the item catalog
    /// </summary>
    public class CatalogItem : IComparable<CatalogItem>
    {
        /// <summary>
        /// defines the bundle properties for the item - bundles are items which contain other items, including random drop tables
        /// and virtual currencies
        /// </summary>
        public CatalogItemBundleInfo Bundle { get; set; }

        /// <summary>
        /// if true, then an item instance of this type can be used to grant a character to a user.
        /// </summary>
        public bool CanBecomeCharacter { get; set; }

        /// <summary>
        /// catalog version for this item
        /// </summary>
        public string CatalogVersion { get; set; }

        /// <summary>
        /// defines the consumable properties (number of uses, timeout) for the item
        /// </summary>
        //public CatalogItemConsumableInfo Consumable;

        /// <summary>
        /// defines the container properties for the item - what items it contains, including random drop tables and virtual
        /// currencies, and what item (if any) is required to open it via the UnlockContainerItem API
        /// </summary>
        //public CatalogItemContainerInfo Container;

        /// <summary>
        /// game specific custom data
        /// </summary>
        public string CustomData { get; set; }

        /// <summary>
        /// text name for the item, to show in-game
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// class to which the item belongs
        /// </summary>
        public string ItemClass { get; set; }

        /// <summary>
        /// unique identifier for this item
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// override prices for this item for specific currencies
        /// </summary>
        public Dictionary<string, uint> RealCurrencyPrices { get; set; }

        /// <summary>
        /// list of item tags
        /// </summary>
        //[Unordered]
        public List<string> Tags { get; set; }

        /// <summary>
        /// price of this item in virtual currencies and "RM" (the base Real Money purchase price, in USD pennies)
        /// </summary>
        public Dictionary<string, uint> VirtualCurrencyPrices { get; set; }

        public int CompareTo(CatalogItem other)
        {
            if (other == null || other.ItemId == null) return 1;
            if (ItemId == null) return -1;
            return ItemId.CompareTo(other.ItemId);
        }
    }

    public class LoginResult
    {
        public string PlayerId { get; set; }
        public string PlayerDisplayName { get; set; }
        public Dictionary<string, string> UserData { get; set; }
        public Dictionary<string, string> TitleData { get; set; }
    }
}

/*
public class BackendManager : Manager
{
    //public event Action OnLogin;

    public bool IsLoggedIn { get; private set; }
    public string PlayerId { get; private set; }
    public string DisplayName { get; private set; }
    public string SessionTicket { get; private set; }

    protected override Task InitializeManager(Bootloader bootloader)
    {
        throw new NotImplementedException();
    }

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
*/

/*

public interface IBackend
{
    event EventHandler<RealtimeMessage> OnRealtimeMessageReceived;

    // -------------------- Login / Presence --------------------

    // Login with Username / Password
    // Sign up with Username / Password
    // Login with device id

    // Update This users Presence to LoggedIn
    // Sends FriendPresenceUpdated to all Friends that care
    Task<LoginResult> Login(LoginRequest request);

    // Update This users Presence to LoggedOut
    // Sends FriendPresenceUpdated to all Friends care
    Task<LogoutResult> Logout(LogoutRequest request);

    // Sends FriendPresenceUpdated to all Friends
    Task<UpdatePresenceResult> UpdatePresence(UpdatePresenceRequest request);

    Task<GetFriendsPresenceResult> GetFriendsPresence(GetFriendsPresenceRequest request);

    // -------------------- General --------------------
    Task<ChangeDisplayNameResult> ChangeDisplayName(ChangeDisplayNameRequest request);

    // -------------------- Friends --------------------
    Task<GetFriendsResult> GetFriends(GetFriendsRequest request);

    Task<GetFriendInvitesResult> GetFriendInvites(GetFriendInvitesRequest request);

    // Sends FriendInviteAccepted to other user
    Task<AcceptFriendInviteResult> AcceptFriendInvite(AcceptFriendInviteRequest request);

    // Sends FriendInviteRejected to other user
    Task<RejectFriendInviteResult> RejectFriendInvite(RejectFriendInviteRequest request);

    // TODO [bgish]: MarkFriendInvitesRead
    // TODO [bgish]: SendFriendInvite (Sends FriendInviteRecieved to other user)
    // TODO [bgish]: RemoveFriend (Sends RemovedAsFriend to other user)

    // TODO [bgish]: FindFriendsByDisplayName
    // TODO [bgish]: FindFriendsByEmail
    // TODO [bgish]: FindFriendsByPlatformId

    // -------------------- Inbox --------------------
    Task<SendInboxItemResult> SendInboxItem(SendInboxItemRequest request);

    Task<GetInboxItemsResult> GetInboxItems(GetInboxItemsRequest request);

    Task<RemoveInboxItemsResult> RemoveInboxItems(RemoveInboxItemsRequest request);

    Task<MarkInboxItemsReadResult> MarkInboxItemsRead(MarkInboxItemsReadRequest request);

    // -------------------- Party --------------------

    // StartParty (Player Ids)
    // SendPartyInvite (Sends RecievedPartyInvite message)

    // GetPartyInvites
    // MarkPartyInvitesRead
    // AcceptPartyInvite (Sends PlayerJoinedParty message)
    // RejectPartyInvite

    // -------------------- Rooms --------------------
    // TODO [bgish]: CreateOrJoinPublicRoom (Just needs 4/5 digit room key and anyone can join)

    // -------------------- Logging --------------------
    // Upload Logs

    // -------------------- Perf --------------------
    // Upload Perf Data

    //// ---------------------------
    //// Characters
    //// Catalog / Inventory / Purchasing
    //// Virtual Currency
    //// Daily Reward
    //// Hourly Reward
    //// Ad Reward
    //// Leaderboard
    //// GetAppVersion
    //// Game Data (aka Title Data)
    //// User Data
    //// Stats
}

*/


// ------------------------------------------ Messages -------------------------------------------
/*

    public abstract class RealtimeMessage
    {
        public abstract string Type { get; }
    }

    // -------------------- Login / Presence --------------------
    public class FriendPresenceUpdated : RealtimeMessage
    {
        public override string Type => nameof(FriendPresenceUpdated);
    }

    // -------------------- General --------------------

    public class DisplayNameUpdated : RealtimeMessage
    {
        public override string Type => nameof(FriendInviteRecieved);

        public string UserHexId { get; set; }

        public string NewDisplayName { get; set; }
    }


    // -------------------- Friends --------------------
    public class FriendInviteRecieved : RealtimeMessage
    {
        public override string Type => nameof(FriendInviteRecieved);
    }

    public class FriendInviteAccepted : RealtimeMessage
    {
        public override string Type => nameof(FriendInviteAccepted);
    }

    public class FriendInviteRejected : RealtimeMessage
    {
        public override string Type => nameof(FriendInviteRejected);
    }

    public class RemovedAsFriend : RealtimeMessage
    {
        public override string Type => nameof(RemovedAsFriend);
    }

    // -------------------- Inbox --------------------
    public class RecievedInboxItem : RealtimeMessage
    {
        public override string Type => nameof(RecievedInboxItem);

        public int InboxItemCount { get; set; }

        public int InboxItemUnreadCount { get; set; }
    }
*/


// ------------------------------------------ Requests -------------------------------------------

/*
   public class BaseRequest
    {
    }

    // -------------------- Login / Presence --------------------
    public class LoginRequest : BaseRequest
    {
    }

    public class LogoutRequest : BaseRequest
    {
    }

    public class UpdatePresenceRequest : BaseRequest
    {
    }

    public class GetFriendsPresenceRequest : BaseRequest
    {
    }

    // -------------------- General --------------------
    public class ChangeDisplayNameRequest
    {
        public string NewDisplayName { get; set; }
    }

    // -------------------- Friends --------------------
    public class GetFriendsRequest : BaseRequest
    {
    }

    public class GetFriendInvitesRequest : BaseRequest
    {
    }

    public class AcceptFriendInviteRequest : BaseRequest
    {
    }

    public class RejectFriendInviteRequest : BaseRequest
    {
    }

    // -------------------- Inbox --------------------
    public class SendInboxItemRequest : BaseRequest
    {
    }

    public class GetInboxItemsRequest : BaseRequest
    {
    }

    public class RemoveInboxItemsRequest : BaseRequest
    {
    }

    public class MarkInboxItemsReadRequest : BaseRequest
    {
    }

    // -------------------- Party --------------------

    // -------------------- Rooms --------------------
*/

// ------------------------------------------ Results -------------------------------------------

/*

    public class BaseResult
    {
    }

    // -------------------- Login / Presence --------------------
    // public class LoginResult : BaseResult
    // {
    //     long UserId { get; }
    //     string UserHexId { get; }
    //     string DisplayName { get; }
    //     string PubSubURL { get; }
    // }

    public class LogoutResult : BaseResult
    {
    }

    public class UpdatePresenceResult : BaseResult
    {
    }

    public class GetFriendsPresenceResult : BaseResult
    {
    }

    // -------------------- General --------------------
    public class ChangeDisplayNameResult : BaseResult
    {
    }

    // -------------------- Friends --------------------
    public class GetFriendsResult : BaseResult {}
    public class GetFriendInvitesResult : BaseResult {}
    public class AcceptFriendInviteResult : BaseResult {}
    public class RejectFriendInviteResult : BaseResult {}

    // -------------------- Inbox --------------------
    public class SendInboxItemResult : BaseResult {}

    public class GetInboxItemsResult : BaseResult
    {
        public List<InboxItem> InboxItems { get; set; }
    }

    public class InboxItem
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public string Contents { get; set; }

        public bool IsRead { get; set; }
    }

    public class RemoveInboxItemsResult : BaseResult {}
    public class MarkInboxItemsReadResult : BaseResult {}

    // -------------------- Party --------------------

    // -------------------- Rooms --------------------
*/

