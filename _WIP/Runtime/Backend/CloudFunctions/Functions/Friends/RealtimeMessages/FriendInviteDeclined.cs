//-----------------------------------------------------------------------
// <copyright file="FriendInviteDeclined.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY || USING_PLAYFAB

namespace Lost.CloudFunctions.Friends
{
    public sealed class FriendInviteDeclined : OGT.RealtimeMessage
    {
        public override string Type => nameof(FriendInviteDeclined);
    }
}

#endif
