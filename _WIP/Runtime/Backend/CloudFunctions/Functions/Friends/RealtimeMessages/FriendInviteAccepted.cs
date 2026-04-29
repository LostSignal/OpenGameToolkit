//-----------------------------------------------------------------------
// <copyright file="FriendInviteAccepted.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY || USING_PLAYFAB

namespace Lost.CloudFunctions.Friends
{
    public sealed class FriendInviteAccepted : OGT.RealtimeMessage
    {
        public override string Type => nameof(FriendInviteAccepted);
    }
}

#endif
