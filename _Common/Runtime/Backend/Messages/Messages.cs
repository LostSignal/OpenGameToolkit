//-----------------------------------------------------------------------
// <copyright file="Messages.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#pragma warning disable

namespace OGT
{
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
}
