//-----------------------------------------------------------------------
// <copyright file="FriendInvite.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY || USING_PLAYFAB

namespace Lost.CloudFunctions.Friends
{
    public class FriendInvite
    {
        public Friend Friend { get; set; }

        public bool IsRead { get; set; }
    }
}

#endif
