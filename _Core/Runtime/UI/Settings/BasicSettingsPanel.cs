//-----------------------------------------------------------------------
// <copyright file="BasicSettingsPanel.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;

    public class BasicSettingsPanel : PanelLogic
    {
        [Header("Debug Info")]
        [SerializeField] private Button versionButton;
        [SerializeField] private GameObject debugInfoGameObject;
        [SerializeField] private TMP_Text debugInfoText;

        private List<float> clickTimes = new List<float>();
        private float lastClickTime;

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
            BetterStringBuilder.New()
                .AppendLine($"Version: {Platform.VersionString}")
                .AppendLine("Development Build: " + Debug.isDebugBuild)
                .AppendLine("OGT Development Build: " + RuntimeSettings.IsOgtDevelopmentBuild)
                .AppendLine("Build Profile Name: " + RuntimeSettings.BuildProfileName)
                .AppendLine("Branch Name: " + RuntimeSettings.SourceControlBranchName)
                .AppendLine("Changeset: " + RuntimeSettings.SourceControlChangeset)
                .AppendLine($"Platform: {Application.platform}")
                .AppendLine($"OS: {SystemInfo.operatingSystem}")
                .AppendLine($"Device Model: {SystemInfo.deviceModel}")
                .AppendLine($"Device Type: {SystemInfo.deviceType}")
                .AppendLine($"Graphics Device Name: {SystemInfo.graphicsDeviceName}")
                .AppendLine($"Graphics Device Type: {SystemInfo.graphicsDeviceType}")
                .AppendLine($"Graphics Memory Size: {SystemInfo.graphicsMemorySize} MB")
                .AppendLine($"Processor Type: {SystemInfo.processorType}")
                .AppendLine($"Processor Count: {SystemInfo.processorCount}")
                .AppendLine($"System Memory Size: {SystemInfo.systemMemorySize} MB")
                .Set(this.debugInfoText);
        }
    }
}
