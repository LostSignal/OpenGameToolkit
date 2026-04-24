//-----------------------------------------------------------------------
// <copyright file="Results.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#pragma warning disable

using System.Collections.Generic;

namespace OGT
{
    public class BaseResult
    {
    }

    // -------------------- Login / Presence --------------------
    public class LoginResult : BaseResult
    {
        long UserId { get; }
        string UserHexId { get; }
        string DisplayName { get; }
        string PubSubURL { get; }
    }

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
    public class GetFriendsResult : BaseResult { }
    public class GetFriendInvitesResult : BaseResult { }
    public class AcceptFriendInviteResult : BaseResult { }
    public class RejectFriendInviteResult : BaseResult { }

    // -------------------- Inbox --------------------
    public class SendInboxItemResult : BaseResult { }
    
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

    public class RemoveInboxItemsResult : BaseResult { }
    public class MarkInboxItemsReadResult : BaseResult { }

    // -------------------- Party --------------------

    // -------------------- Rooms --------------------
}
