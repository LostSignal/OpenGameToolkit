//-----------------------------------------------------------------------
// <copyright file="DebugGiveCurrencyRequest.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY || USING_PLAYFAB

namespace Lost.CloudFunctions.Debug
{
    public class DebugGiveCurrencyRequest
    {
        public string CurrencyCode { get; set; }

        public int Amount { get; set; }
    }
}

#endif
