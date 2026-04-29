//-----------------------------------------------------------------------
// <copyright file="PlayFab.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_PLAYFAB

namespace OGT.Settings
{
    public class PlayFab : Settings
    {
        public string TitleId { get; set; }
        public string CatalogVersion { get; set; }
        public int? CloudScriptRevision { get; set; }
        public string SecretKey { get; set; }

        public override void ApplySettings() => this.ApplyPlayFabSettings();

        public override void ApplySettingsOnEnterPlayMode() => this.ApplyPlayFabSettings();

        private void ApplyPlayFabSettings()
        {
            global::PlayFab.PlayFabSettings.staticSettings.TitleId = this.TitleId;
            global::PlayFab.PlayFabSettings.staticSettings.DeveloperSecretKey = this.SecretKey;

            if (this.TitleId != null)
            {
                OGT.PlayFab.PlayFabRuntimeSettings.TitleId = this.TitleId;
            }

            if (this.CatalogVersion != null)
            {
                OGT.PlayFab.PlayFabRuntimeSettings.CatalogVersion = this.CatalogVersion;
            }

            if (this.CloudScriptRevision.HasValue)
            {
                OGT.PlayFab.PlayFabRuntimeSettings.CloudScriptRevision = this.CloudScriptRevision.Value;
            }
        }
    }
}

#endif
