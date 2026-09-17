//-----------------------------------------------------------------------
// <copyright file="IAnalyticsProvider.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    //// TODO [bgish]: Create a PlayFabAnalyticProvider and make sure it's loaded in bootloader
    //// TODO [bgish]: Create a UnityAnalyticProvider and make sure it's loaded in bootloader

    public interface IAnalyticsProvider
    {
        bool IsSupported { get; }

        Task Initialize();

        void Send(string eventName, Dictionary<string, object> data);

        void Flush();
    }
}
