//-----------------------------------------------------------------------
// <copyright file="DebugMenuManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using System;
    using System.Collections;
    using System.Threading.Tasks;

#if USING_PLAYFAB
    using global::PlayFab.ClientModels;
    using global::PlayFab.Internal;
    using OGT.CloudBuild;
    using OGT.PlayFab;
#endif

    using UnityEngine;

    [Serializable]
    public class DebugMenuManagerSettings
    {
        [SerializeField] private bool developmentBuildsOnly = true;

        [Header("Settings")]
        [SerializeField] private DebugMenu.DebugMenuSettings settings = new();

        [Header("Overlay Options")]
        [SerializeField] private bool showAppVersionInLowerLeftKey = true;
        [SerializeField] private bool showPlayFabIdInLowerRight = true;

        [Header("Debug Menu Options")]
        [SerializeField] private bool showTestAd = true;
        [SerializeField] private bool showToggleFps = true;
        [SerializeField] private bool showPrintAdsInfo = true;
        [SerializeField] private bool addRebootButton = true;

        public bool DevelopmentBuildsOnly => this.developmentBuildsOnly;

        public DebugMenu.DebugMenuSettings DebugMenuSettings => this.settings;

        public bool ShowAppVersionInLowerLeftKey => this.showAppVersionInLowerLeftKey;

        public bool ShowPlayFabIdInLowerRight => this.showPlayFabIdInLowerRight;

        public bool ShowTestAd => this.showTestAd;

        public bool ShowToggleFps => this.showToggleFps;

        public bool ShowPrintAdsInfo => this.showPrintAdsInfo;

        public bool AddRebootButton => this.addRebootButton;
    }

    public sealed class DebugMenuManager : Manager
    {
#pragma warning disable 0649
        [SerializeField] private DebugMenuManagerSettings settings;
#pragma warning restore 0649

        private string versionAndCommitId;

        protected override Task InitializeManager(Bootloader bootloader)
        {
            if (this.settings.DevelopmentBuildsOnly == false || Application.isEditor || Debug.isDebugBuild)
            {
                CoroutineRunner.Instance.StartCoroutine(InitializeSettings(bootloader));
            }

            return Task.CompletedTask;

            IEnumerator InitializeSettings(Bootloader bootloader)
            {
                //// yield return DialogManager.WaitForInitialization();

                var debugMenu = DialogManager.GetDialog<DebugMenu>();

                debugMenu.SetSettings(this.settings.DebugMenuSettings);

                if (this.settings.ShowAppVersionInLowerLeftKey)
                {
                    if (this.versionAndCommitId == null)
                    {
                        var version = Application.version;
                        var commitId = CloudBuildManifest.Find()?.ScmCommitId;
                        this.versionAndCommitId = commitId == null ? version : string.Format($"{version} ({commitId})");
                    }

                    debugMenu.SetText(Corner.LowerLeft, this.versionAndCommitId);
                }

                if (this.settings.ShowPlayFabIdInLowerRight)
                {
#if USING_PLAYFAB
                    var playfabManager = bootloader.FindManager<PlayFabManager>();

                    if (playfabManager != null)
                    {
                        playfabManager.OnInitialize += () =>
                        {
                            if (playfabManager.Login.IsLoggedIn)
                            {
                                var debugMenu = DialogManager.GetDialog<DebugMenu>();
                                var playfabId = playfabManager.Login.IsLoggedIn ? playfabManager.User.PlayFabId : "Login Error!";
                                debugMenu.SetText(Corner.LowerRight, playfabId);
                            }
                        };
                    }
#endif
                }

                if (this.settings.ShowTestAd)
                {
                    //// throw new NotImplementedException();
                    //// debugMenu.AddItem("Show Test Ad", ShowTestAd);
                }

                if (this.settings.ShowToggleFps)
                {
                    debugMenu.AddItem("Toggle FPS", ToggleFps);
                }

                if (this.settings.ShowPrintAdsInfo)
                {
                    //// throw new NotImplementedException();
                    //// debugMenu.AddItem("Print Ads Info", PrintAdsInfo);
                }

                if (this.settings.AddRebootButton)
                {
                    //// throw new NotImplementedException();

                    //// Not sure where bootloader will live so commenting out for now
                    //// debugMenu.AddItem("Reboot", Bootloader.Reboot);
                }

                debugMenu.Dialog.Show();

                yield break;
            }
        }

        private static void ToggleFps()
        {
            DialogManager.GetDialog<DebugMenu>().ToggleFPS();
        }
    }
}

#endif
