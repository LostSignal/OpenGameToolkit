//-----------------------------------------------------------------------
// <copyright file="DebugPanel.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using TMPro;
    using UnityEngine;

    public enum DebugLocation
    {
        None,
        TopLeft,
        TopCenter,
        TopRight,
        BottomLeft,
        BottomCenter,
        BottomRight,
    }

    public class DebugPanel : PanelLogic
    {
        [Header("Texts")]
        [SerializeField] private TMP_Text topLeftText;
        [SerializeField] private TMP_Text topCenterText;
        [SerializeField] private TMP_Text topRightText;
        [SerializeField] private TMP_Text bottomLeftText;
        [SerializeField] private TMP_Text bottomCenterText;
        [SerializeField] private TMP_Text bottomRightText;

        [Header("Standard Information")]
        [SerializeField] private DebugLocation versionString = DebugLocation.None;
        [SerializeField] private DebugLocation fps = DebugLocation.None;
        [SerializeField] private DebugLocation userId = DebugLocation.None;

        public override void OnAwake(Bootloader bootloader)
        {
            base.OnAwake(bootloader);

            if (RuntimeSettings.IsOgtDevelopmentBuild == false)
            {
                return;
            }

            // Updating Version String
            var versionStringText = this.GetText(this.versionString);

            if (versionStringText != null)
            {
                versionStringText.text = Platform.VersionString;
            }

            //// TODO [bgish]: Show user id onece logged in
            //// TODO [bgish]: Show FPS by hooking into the Platform.OnUpdate

            this.Showable.Show();
        }

        private TMP_Text GetText(DebugLocation debugLocation)
        {
            return debugLocation switch 
            {
                DebugLocation.TopLeft => this.topLeftText,
                DebugLocation.TopCenter => this.topCenterText,
                DebugLocation.TopRight => this.topRightText,
                DebugLocation.BottomLeft => this.bottomLeftText,
                DebugLocation.BottomCenter => this.bottomCenterText,
                DebugLocation.BottomRight => this.bottomRightText,
                _ => null
            };
        }
    }
}
