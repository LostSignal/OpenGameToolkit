//-----------------------------------------------------------------------
// <copyright file="ActionExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;

    public static class ActionExtensions
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        public static void SafeInvoke(this Action action)
        {
            if (action != null)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
            }
        }
    }
}
