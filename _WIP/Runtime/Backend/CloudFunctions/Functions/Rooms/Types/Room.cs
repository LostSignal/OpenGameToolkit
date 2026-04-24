//-----------------------------------------------------------------------
// <copyright file="Room.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY || USING_PLAYFAB

namespace Lost.CloudFunctions
{
    public enum RoomVisibility
    {
        Members,
        OrgMembers,
        Public,
    }

    public class Room
    {
        public string Id { get; set; }

        public RoomInfo Info { get; set; }

        public string OwnerId { get; set; }

        public string OrgId { get; set; }

        public RoomVisibility Visibility { get; set; }
    }
}

#endif
