//-----------------------------------------------------------------------
// <copyright file="UnityPlatformInitializer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEngine;

    //// NOTE [bgish]:  Windows Universal May Support System.IO.File class now
    //// TODO add events for pen and mouse detected, that way if someone uses a pen
    //// TODO controller too?  maybe only if InControl is detected?

    public class UnityPlatformInitializer : MonoBehaviour, IProviderInitializer
    {
        public void Register()
        {
            RegisterProvider();
        }

        public static void RegisterProvider()
        {
            Platform.SetPlatformProvider(new UnityPlatformProvider());
        }
    }
}
