//-----------------------------------------------------------------------
// <copyright file="IBackend.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Threading.Tasks;

    //// public class BackendManager
    //// {
    ////     // Takes an IBackend
    ////     // You Login through this and it decides if it shoudl cache results or not?
    ////     //
    ////     // Gets List of Friends, Then over over time GetFriendsPresence for every friend
    ////     // (in chuncks), also registers for FriendPresenceUpdated event
    ////     //
    ////     // Implements INotifyPropertyChanged and has properties for things like UnreadMailCount,
    ////     // FriendInventationCount, etc
    ////     //
    ////     //
    ////     //
    ////     //
    ////     //
    //// }

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
}
