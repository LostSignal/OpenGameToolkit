//-----------------------------------------------------------------------
// <copyright file="TitleNewsManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY && USING_PLAYFAB

namespace OGT.PlayFab
{
    public class TitleNewsManager
    {
        private readonly PlayFabManager playfabManager;

        public TitleNewsManager(PlayFabManager playfabManager)
        {
            this.playfabManager = playfabManager;
        }
    }
}

#endif
