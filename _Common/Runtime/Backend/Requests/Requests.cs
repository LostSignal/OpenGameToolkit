//-----------------------------------------------------------------------
// <copyright file="Requests.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#pragma warning disable

namespace OGT
{
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
}
