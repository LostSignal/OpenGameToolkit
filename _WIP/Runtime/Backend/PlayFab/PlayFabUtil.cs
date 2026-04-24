//-----------------------------------------------------------------------
// <copyright file="PlayFabUtil.cs" company="Full Circle Games">
//     Copyright (c) Full Circle Games. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY || USING_PLAYFAB

using System.Threading.Tasks;

namespace OGT.PlayFab
{
    public static class PlayFabUtil
    {
        public static Task<global::PlayFab.PlayFabResult<global::PlayFab.AuthenticationModels.GetEntityTokenResponse>> GetTitleEntityTokenAsync()
        {
            return global::PlayFab.PlayFabAuthenticationAPI.GetEntityTokenAsync(new global::PlayFab.AuthenticationModels.GetEntityTokenRequest
            {
                // NOTE [bgish]: If we don't send in an empty auth context, it will use the players, which won't work
                AuthenticationContext = new global::PlayFab.PlayFabAuthenticationContext(),
            });
        }
    }
}

#endif
