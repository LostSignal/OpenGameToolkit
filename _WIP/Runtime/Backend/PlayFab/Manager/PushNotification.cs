//-----------------------------------------------------------------------
// <copyright file="PushNotification.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_PLAYFAB

namespace OGT
{
    public struct PushNotification
    {
        public string Title { get; set; }

        public string Body { get; set; }

        public string SoundName { get; set; }

        public int ApplicationIconBadgeNumber { get; set; }
    }
}

#endif
