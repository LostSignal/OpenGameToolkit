//-----------------------------------------------------------------------
// <copyright file="ScreenSizeManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using System.Threading.Tasks;
    using UnityEngine;

    public sealed class ScreenSizeManager : Manager
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

#pragma warning disable 0649
        [SerializeField] private bool limitMobileScreenSize;
        [SerializeField] private int maxScreenSize = 1920;
#pragma warning restore 0649

        protected override Task InitializeManager(Bootloader bootloader)
        {
            if (this.limitMobileScreenSize == false)
            {
                return Task.CompletedTask;
            }

            // TODO [bgish]: These should be questions for the Platform class, or platform Manager
            bool isMobilePlatform = Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
            bool isMobileVrDevice = isMobilePlatform && SystemInfo.deviceName.ToLower().Contains("quest");

            if (isMobilePlatform && isMobileVrDevice == false)
            {
                bool isLandscape = Screen.width > Screen.height;

                if (isLandscape && Screen.width > maxScreenSize)
                {
                    float aspectRatio = Screen.height / (float)Screen.width;
                    int newHeight = (int)(maxScreenSize * aspectRatio);
                    int newWidth = maxScreenSize;

                    Logger.LogFormat("Resizing Screen From {0}x{1} To {2}x{3}", Screen.width, Screen.height, newWidth, newHeight);
                    Screen.SetResolution(newWidth, newHeight, true);
                }
                else if (isLandscape == false && Screen.height > maxScreenSize)
                {
                    float aspectRatio = Screen.width / (float)Screen.height;
                    int newHeight = maxScreenSize;
                    int newWidth = (int)(maxScreenSize * aspectRatio);

                    Logger.LogFormat("Resizing Screen From {0}x{1} To {2}x{3}", Screen.width, Screen.height, newWidth, newHeight);
                    Screen.SetResolution(newWidth, newHeight, true);
                }
            }

            return Task.CompletedTask;
        }
    }
}

#endif
