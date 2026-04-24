//-----------------------------------------------------------------------
// <copyright file="DebugPurchaseItemRequest.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY || USING_PLAYFAB

namespace Lost.CloudFunctions.Debug
{
    public class DebugPurchaseItemRequest
    {
        public string ItemId { get; set; }
    }
}

#endif
