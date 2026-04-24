//-----------------------------------------------------------------------
// <copyright file="UnityLoggingInitializer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    public class UnityLoggingInitializer : GameBehavior, IProviderInitializer
    {
        public void Register()
        {
            RegisterProvider();
        }

        public static void RegisterProvider()
        {
            OGTLogger.AddProvider(new UnityLoggingProvider());
        }
    }
}
