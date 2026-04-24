//-----------------------------------------------------------------------
// <copyright file="DebugCloudFunctions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY

namespace Lost.CloudFunctions.Debug
{
    using System;
    using System.Threading.Tasks;
    using global::PlayFab;
    using global::PlayFab.ServerModels;

    public static class DebugCloudFunctions
    {
        [DevCloudFunction("Debug", "PurchaseItem")]
        public static Task PurchaseItem(CloudFunctionContext context, DebugPurchaseItemRequest request)
        {
            throw new NotImplementedException();
        }

        [DevCloudFunction("Debug", "GiveCurrency")]
        public static Task GiveCurrency(CloudFunctionContext context, DebugGiveCurrencyRequest request)
        {
            throw new NotImplementedException();
        }

        [DevCloudFunction("Debug", "DeleteUser")]
        public static async Task DeleteUser(CloudFunctionContext context, string playfabId)
        {
            await PlayFabServerAPI.DeletePlayerAsync(new DeletePlayerRequest
            {
                AuthenticationContext = context.TitleAuthenticationContext,
                PlayFabId = playfabId,
            });
        }
    }
}

#endif
