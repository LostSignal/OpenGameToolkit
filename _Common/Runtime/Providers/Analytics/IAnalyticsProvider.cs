//-----------------------------------------------------------------------
// <copyright file="IAnalyticsProvider.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;

    //// TODO [bgish]: Create a PlayFabAnalyticProvider and make sure it's loaded in bootloader
    //// TODO [bgish]: Create a UnityAnalyticProvider and make sure it's loaded in bootloader

    public interface IAnalyticsProvider
    {
        void Send(string eventName, Dictionary<string, object> data);
    }
}
