//-----------------------------------------------------------------------
// <copyright file="BuildStep.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BuildProfile
{
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;
        
    public abstract class BuildStep : ScriptableObject
    {
        public static readonly OGTLogger Logger = new OGTLogger("Build Step");

        public virtual void OnBuildProfileSelected(UnityEditor.Build.Profile.BuildProfile buildProfile)
        {
        }

        public virtual void OnPrepareForBuild(UnityEditor.Build.Profile.BuildProfile buildProfile, BuildPlayerContext buildPlayerContext)
        {
        }

        public virtual void OnPreprocessBuild(UnityEditor.Build.Profile.BuildProfile buildProfile, BuildProfileExtender.Environment environment, BuildReport report)
        {
        }

        public virtual void OnPostprocessBuild(UnityEditor.Build.Profile.BuildProfile buildProfile, BuildProfileExtender.Environment environment, BuildReport report)
        {
        }
    }
}
