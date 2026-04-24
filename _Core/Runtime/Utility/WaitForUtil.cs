//-----------------------------------------------------------------------
// <copyright file="WaitForUtil.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using UnityEngine;

    public static class WaitForUtil
    {
        public static readonly WaitForEndOfFrame EndOfFrame = new ();
        private static readonly Dictionary<float, WaitForSeconds> WaitForSecondsCache = new ();
        private static readonly Dictionary<float, WaitForSecondsRealtime> WaitForSecondsRealtimeCache = new ();

        public static WaitForSeconds Seconds(float time)
        {
            if (WaitForSecondsCache.TryGetValue(time, out WaitForSeconds waitForSeconds) == false)
            {
                waitForSeconds = new WaitForSeconds(time);
                WaitForSecondsCache.Add(time, waitForSeconds);
            }

            return waitForSeconds;
        }

        public static WaitForSecondsRealtime RealtimeSeconds(float time)
        {
            if (WaitForSecondsRealtimeCache.TryGetValue(time, out WaitForSecondsRealtime waitForSecondsRealtime) == false)
            {
                waitForSecondsRealtime = new WaitForSecondsRealtime(time);
                WaitForSecondsRealtimeCache.Add(time, waitForSecondsRealtime);
            }

            return waitForSecondsRealtime;
        }
    }
}
