//-----------------------------------------------------------------------
// <copyright file="BasicSettingsPanel.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.Text;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class BasicSettingsPanel : PanelLogic
    {
        private const string HeaderColor = "#1E6FA8";
        private const string WarningColor = "#D03030";

        [Header("Debug Info")]
        [SerializeField] private Button versionButton;
        [SerializeField] private GameObject debugInfoGameObject;
        [SerializeField] private TMP_Text debugInfoText;

        private readonly List<float> clickTimes = new List<float>();
        private readonly StringBuilder builder = new StringBuilder(4096);

        public override void OnAwake(Bootloader bootloader)
        {
            base.OnAwake(bootloader);

            this.versionButton.onClick.AddListener(this.OnVersionButtonClicked);
        }

        private void OnVersionButtonClicked()
        {
            this.clickTimes.Add(Time.realtimeSinceStartup);

            while (this.clickTimes.Count > 3)
            {
                this.clickTimes.RemoveAt(0);
            }

            if (this.clickTimes.Count == 3 && this.clickTimes[2] - this.clickTimes[0] <= 1.0f)
            {
                this.SetDebugInfo();
                this.debugInfoGameObject.SetActive(!this.debugInfoGameObject.activeSelf);
                this.clickTimes.Clear();
            }
        }

        private void SetDebugInfo()
        {
            StringBuilder sb = this.builder;
            sb.Clear();

            // Resolution / orientation summary
            var currentResolution = Screen.currentResolution;
            int width = Screen.width;
            int height = Screen.height;
            var reported = Screen.orientation;
            float dpi = Screen.dpi;

            bool reportedPortrait = reported == ScreenOrientation.Portrait || reported == ScreenOrientation.PortraitUpsideDown;
            bool reportedLandscape = reported == ScreenOrientation.LandscapeLeft || reported == ScreenOrientation.LandscapeRight;
            bool sizePortrait = height >= width;
            string mismatch = (reportedPortrait && !sizePortrait) || (reportedLandscape && sizePortrait)
                ? $"  <color={WarningColor}><b>MISMATCH</b></color>"
                : string.Empty;

            AppendHeader(sb, "Build");
            sb.AppendLine($"  version: {Platform.VersionString}");
            sb.AppendLine($"  unity app version: {Application.version}");

            if (Application.platform == RuntimePlatform.Android)
            {
                sb.AppendLine($"  android build number: {UnityPlatformProvider.GetAndroidBuildNumber()}");
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                sb.AppendLine($"  ios build number: {UnityPlatformProvider.GetIOSBuildNumber()}");
            }

            sb.AppendLine($"  development build: {Debug.isDebugBuild}");
            sb.AppendLine($"  ogt development build: {RuntimeSettings.IsOgtDevelopmentBuild}");
            sb.AppendLine($"  build profile: {RuntimeSettings.BuildProfileName}");
            sb.AppendLine($"  branch: {RuntimeSettings.SourceControlBranchName}");
            sb.AppendLine($"  changeset: {RuntimeSettings.SourceControlChangeset}");
            sb.AppendLine($"  unity: {Application.unityVersion}");

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                sb.AppendLine($"  url: {Application.absoluteURL}");
            }            

            AppendHeader(sb, "Screen");
            sb.AppendLine($"  screen.orientation: <b>{reported}</b>");
            sb.AppendLine($"  by size (w vs h): <b>{(sizePortrait ? "Portrait" : "Landscape")}</b>{mismatch}");
            sb.AppendLine($"  aspect ratio: <b>{GetAspectString(width, height)}</b>");
            sb.AppendLine($"  width x height: {width} x {height}");
            sb.AppendLine($"  currentResolution: {currentResolution.width} x {currentResolution.height} @ {currentResolution.refreshRateRatio.value:F1}Hz");
            sb.AppendLine($"  dpi: {dpi:F1}  (est. devicePixelRatio: {(dpi > 0 ? (dpi / 96.0f).ToString("F2") : "?")})");
            sb.AppendLine($"  fullScreen: {Screen.fullScreen}  mode: {Screen.fullScreenMode}");
            sb.AppendLine($"  safeArea: {FormatRect(Screen.safeArea)}");
            sb.AppendLine($"  cutouts: {Screen.cutouts.Length}  resolutions reported: {Screen.resolutions.Length}");
            sb.AppendLine($"  autorotate P/PUD/LL/LR: {Screen.autorotateToPortrait}/{Screen.autorotateToPortraitUpsideDown}/{Screen.autorotateToLandscapeLeft}/{Screen.autorotateToLandscapeRight}");

            Display display = Display.main;
            AppendHeader(sb, "Display.main");
            sb.AppendLine($"  system: {display.systemWidth} x {display.systemHeight}");
            sb.AppendLine($"  rendering: {display.renderingWidth} x {display.renderingHeight}");

            Camera camera = Camera.main;
            AppendHeader(sb, "Camera.main");
            if (camera != null)
            {
                sb.AppendLine($"  name: {camera.name}");
                sb.AppendLine($"  pixel: {camera.pixelWidth} x {camera.pixelHeight}  aspect: {camera.aspect:F3}");
                sb.AppendLine($"  viewport rect: {FormatRect(camera.rect)}");
                sb.AppendLine(camera.orthographic
                    ? $"  orthographic size: {camera.orthographicSize:F2}"
                    : $"  vertical FOV: {camera.fieldOfView:F1}  horizontal FOV: {Camera.VerticalToHorizontalFieldOfView(camera.fieldOfView, camera.aspect):F1}");
            }
            else
            {
                sb.AppendLine("  (none tagged MainCamera)");
            }

            AppendHeader(sb, "Quality");
            sb.AppendLine($"  quality level: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
            sb.AppendLine($"  targetFrameRate: {Application.targetFrameRate}  vSync: {QualitySettings.vSyncCount}");

            AppendHeader(sb, "Device");
            sb.AppendLine($"  platform: {Application.platform}  mobile: {Application.isMobilePlatform}");
            sb.AppendLine($"  os: {SystemInfo.operatingSystem}");
            sb.AppendLine($"  model: {SystemInfo.deviceModel}  type: {SystemInfo.deviceType}");
            sb.AppendLine($"  cpu: {SystemInfo.processorType} ({SystemInfo.processorCount} cores)");
            sb.AppendLine($"  system memory: {SystemInfo.systemMemorySize} MB");
            sb.AppendLine($"  gpu: {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsDeviceType})");
            sb.AppendLine($"  gpu version: {SystemInfo.graphicsDeviceVersion}");
            sb.AppendLine($"  gpu memory: {SystemInfo.graphicsMemorySize} MB  max texture: {SystemInfo.maxTextureSize}");

            this.debugInfoText.SetText(sb);
        }

        private static void AppendHeader(StringBuilder sb, string title)
        {
            sb.AppendLine();
            sb.AppendLine($"<color={HeaderColor}><size=115%><b>{title}</b></size></color>");
        }

        private static string FormatRect(Rect rect)
        {
            return $"({rect.x:F0}, {rect.y:F0}, {rect.width:F0} x {rect.height:F0})";
        }

        private static string GetAspectString(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                return "?";
            }

            int a = width;
            int b = height;
            while (b != 0)
            {
                (a, b) = (b, a % b);
            }

            int aspectWidth = width / a;
            int aspectHeight = height / a;

            // Reduced ratios like 1080:2340 -> 6:13 are fine, but 412:915 isn't helpful, so fall back to a decimal
            return aspectWidth <= 32 && aspectHeight <= 32
                ? $"{aspectWidth}:{aspectHeight}"
                : $"{(float)width / height:F3}";
        }
    }
}
