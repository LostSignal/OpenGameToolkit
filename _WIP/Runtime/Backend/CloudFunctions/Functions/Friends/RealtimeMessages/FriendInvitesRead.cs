//-----------------------------------------------------------------------
// <copyright file="FriendInvitesRead.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY || USING_PLAYFAB

namespace Lost.CloudFunctions.Friends
{
    public sealed class FriendInvitesRead : OGT.RealtimeMessage
    {
        public override string Type => nameof(FriendInvitesRead);
    }
}

#endif
