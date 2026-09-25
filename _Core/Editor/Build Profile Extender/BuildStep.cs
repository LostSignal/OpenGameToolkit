//-----------------------------------------------------------------------
// <copyright file="BuildStep.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BuildProfile
{
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEngine;
        
    public abstract class BuildStep : ScriptableObject
    {
        public static readonly OGTLogger Logger = new OGTLogger("Build Step");

        public virtual void OnBuildProfileSelected()
        {
        }

        public virtual void OnPrepareForBuild(BuildPlayerContext buildPlayerContext)
        {
        }

        public virtual void OnPreprocessBuild(BuildProfileExtender.Environment environment, BuildTarget target)
        {
        }

        public virtual void OnPostprocessBuild(BuildProfileExtender.Environment environment, BuildTarget target, string path)
        {
        }
    }
}
