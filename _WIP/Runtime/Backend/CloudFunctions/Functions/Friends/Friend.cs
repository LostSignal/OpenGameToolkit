//-----------------------------------------------------------------------
// <copyright file="Friend.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY || USING_PLAYFAB

namespace Lost.CloudFunctions.Friends
{
    public class Friend
    {
        public string PlayFabId { get; set; }

        public string DisplayName { get; set; }
    }
}

#endif
