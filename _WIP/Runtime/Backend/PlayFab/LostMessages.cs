//-----------------------------------------------------------------------
// <copyright file="LostMessages.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using UnityEngine;

    public static class LostMessages
    {
        public static Awaitable<YesNoResult> ShowUnableToConnectToServer()
        {
            return MessageBox.Instance.ShowYesNo("Server Error", "We're unable to connect to the server at the time.  Would you like to try again?");
        }

        public static void BootloaderLoggingIn()
        {
            //// Bootloader.UpdateLoadingText("Logging In...");
        }

        public static void BootloaderDownloadingCatalog()
        {
            //// Bootloader.UpdateLoadingText("Downloading Catalog...");
        }

        public static void BootloaderLoadingStores()
        {
            //// Bootloader.UpdateLoadingText("Loading Stores...");
        }

        public static void BootloaderInitializingPurchasing()
        {
            //// Bootloader.UpdateLoadingText("Initializing Purchasing...");
        }

        public static Awaitable<YesNoResult> GpsIsRequiredRetryOrQuitApp()
        {
            string title = "GPS Required";
            string body = "GPS is required to run this app, would you like to retry granting \n" +
                "permissions? If not we'll need to shut down the app.";

            return MessageBox.Instance.ShowYesNo(title, body);
        }
    }
}

#endif
