//-----------------------------------------------------------------------
// <copyright file="PlatformVersionString.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using TMPro;
    using UnityEngine;

    public class PlatformVersionString : GameBehavior, IAwake
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        [SerializeField] private TMP_Text text;

        public void OnAwake(Bootloader bootloader)
        {
            if (this.text == null)
            {
                Logger.LogError("PlatformVersionString: TMP_Text is null!");
                return;
            }

            this.text.text = Platform.VersionString;
        }
    }
}
