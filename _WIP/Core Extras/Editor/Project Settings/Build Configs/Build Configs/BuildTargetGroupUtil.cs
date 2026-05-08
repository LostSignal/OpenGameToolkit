//-----------------------------------------------------------------------
// <copyright file="BuildTargetGroupUtil.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using UnityEditor.Build;

    public static class BuildTargetGroupUtil
    {
        private static List<NamedBuildTarget> validGroups = new()
        {
            NamedBuildTarget.Standalone,
            NamedBuildTarget.Standalone,
            NamedBuildTarget.iOS,
            NamedBuildTarget.Android,
            NamedBuildTarget.WebGL,
            NamedBuildTarget.PS4,
            NamedBuildTarget.XboxOne,
            NamedBuildTarget.tvOS,
            NamedBuildTarget.NintendoSwitch,
        };

        public static List<NamedBuildTarget> GetValid()
        {
            return validGroups;
        }
    }
}
