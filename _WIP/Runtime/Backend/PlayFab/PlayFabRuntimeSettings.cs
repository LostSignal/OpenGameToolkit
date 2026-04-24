//-----------------------------------------------------------------------
// <copyright file="PlayFabRuntimeSettings.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.PlayFab
{
    public static class PlayFabRuntimeSettings
    {
        public static string TitleId
        {
            get => RuntimeSettings.GetSetting<string>("PlayFab.TitleId");
            set => RuntimeSettings.SetSetting("PlayFab.TitleId", value);
        }

        public static string CatalogVersion
        {
            get => RuntimeSettings.GetSetting<string>("PlayFab.CatalogVersion");
            set => RuntimeSettings.SetSetting("PlayFab.CatalogVersion", value);
        }

        public static int CloudScriptRevision
        {
            get => RuntimeSettings.GetSetting<int>("PlayFab.CloudScriptRevision");
            set => RuntimeSettings.SetSetting("PlayFab.CloudScriptRevision", value);
        }
    }
}
