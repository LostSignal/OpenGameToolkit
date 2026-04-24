//-----------------------------------------------------------------------
// <copyright file="RoomInfo.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY || USING_PLAYFAB

namespace Lost.CloudFunctions
{
    using System.Collections.Generic;

    public class RoomInfo
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public List<string> ImageUrls { get; set; }
    }
}

#endif
