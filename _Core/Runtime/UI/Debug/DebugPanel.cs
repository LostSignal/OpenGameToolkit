//-----------------------------------------------------------------------
// <copyright file="DebugPanel.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using OGT.Backend;
    using TMPro;
    using UnityEngine;
    using UnityPipeline.Microsoft.CodeAnalysis.CSharp.Syntax;

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

        private int frameCount = 0;
        private float elapsedTime = 0f;
        private IBackend backend;

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

            // Updating FPS
            var fpsText = this.GetText(this.fps);
            if (fpsText != null)
            {
                Platform.OnUpdate += this.OnUpdate;
            }

            // Show user id onece logged in
            this.backend = bootloader.FindManager<IBackend>();
            if (this.backend != null)
            {
                this.backend.OnLogIn += this.UpdateUserId;
                this.UpdateUserId();
            }

            this.Showable.Show();
        }

        private void UpdateUserId()
        {
            var userIdText = this.GetText(this.userId);
            if (this.backend == null || userIdText == null)
            {
                return;
            }

            userIdText.text = this.backend.UserId;
        }

        private void OnUpdate(object sender, object args)
        {
            this.frameCount++;
            this.elapsedTime += Time.unscaledDeltaTime;

            if (this.elapsedTime >= 0.33f)
            {
                var fpsText = this.GetText(DebugLocation.TopRight);
                if (fpsText != null)
                {
                    var fps = (float)(this.frameCount / this.elapsedTime);
                    BetterStringBuilder.New().Append(fps, 1).Set(fpsText);
                }

                this.frameCount = 0;
                this.elapsedTime = 0f;
            }
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
