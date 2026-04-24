//-----------------------------------------------------------------------
// <copyright file="Settings.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Settings
{
    using UnityEditor;
    using UnityEditor.Build.Reporting;

    public abstract class Settings
    {
        public static readonly OGTLogger Logger = new OGTLogger("Config");

        public virtual void ApplySettings()
        {
        }

        public virtual void ApplySettingsOnEnterPlayMode()
        {
        }

        public virtual void ApplySettingsPostAndroidBuild(string gradlePath)
        {
        }

        public virtual void ApplySettingsPostBuild(BuildReport buildReport)
        {
        }

        public virtual void ApplySettingOnBuildStarted()
        {
        }

        public virtual BuildPlayerOptions ApplyBuildPlayerOptions(BuildPlayerOptions options)
        {
            return options;
        }
    }
}
