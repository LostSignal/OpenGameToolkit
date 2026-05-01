//-----------------------------------------------------------------------
// <copyright file="PlayFabRuntimeSettings.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.PlayFab
{
    public static class PlayFabRuntimeSettings
    {
        private static string titleId;
        private static string catalogVersion;
        private static int? cloudScriptRevision;

        public static string TitleId
        {
            get
            {
                if (titleId == null)
                {
                    titleId = RuntimeSettings.GetSetting<string>("PlayFab.TitleId");
                }

                return titleId;
            }

#if UNITY_EDITOR

            set
            {
                titleId = value;
                RuntimeSettings.SetSetting("PlayFab.TitleId", value);
            }

#endif
        }

        public static string CatalogVersion
        {
            get
            {
                if (catalogVersion == null)
                {
                    catalogVersion = RuntimeSettings.GetSetting<string>("PlayFab.CatalogVersion");
                }

                return catalogVersion;
            }

#if UNITY_EDITOR

            set
            {
                catalogVersion = value;
                RuntimeSettings.SetSetting("PlayFab.CatalogVersion", value);
            }

#endif
        }

        public static int CloudScriptRevision
        {
            get
            {
                if (cloudScriptRevision == null)
                {
                    cloudScriptRevision = RuntimeSettings.GetSetting<int>("PlayFab.CloudScriptRevision");
                }

                return cloudScriptRevision.Value;
            }

#if UNITY_EDITOR

            set
            {
                cloudScriptRevision = value;
                RuntimeSettings.SetSetting("PlayFab.CloudScriptRevision", value);
            }

#endif
        }

#if UNITY_6000_0_OR_NEWER
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetToDefaults()
        {
            titleId = null;
            catalogVersion = null;
            cloudScriptRevision = null;
        }
#endif
    }
}
