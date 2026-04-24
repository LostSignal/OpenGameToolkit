//-----------------------------------------------------------------------
// <copyright file="UnityEventExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using OGT;
    using UnityEngine.Events;

    public static class UnityEventExtensions
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        public static void SafeInvoke(this UnityEvent unityEvent)
        {
            if (unityEvent != null)
            {
                try
                {
                    unityEvent.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
            }
        }
    }
}
