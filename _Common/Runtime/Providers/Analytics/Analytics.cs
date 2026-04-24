//-----------------------------------------------------------------------
// <copyright file="Analytics.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;

    public static class Analytics
    {
        private static List<IAnalyticsProvider> analyticsProviders = new(5);

        public static void Send(string eventName, Dictionary<string, object> data)
        {
            foreach (var provider in analyticsProviders)
            {
                try
                {
                    provider.Send(eventName, data);
                }
                catch
                {
                    // TODO [brgish]: Properly report an error
                }
            }
        }

        public static void AddAnalyticsProvider(IAnalyticsProvider provider)
        {
            analyticsProviders.AddIfUnique(provider);
        }
    }
}
