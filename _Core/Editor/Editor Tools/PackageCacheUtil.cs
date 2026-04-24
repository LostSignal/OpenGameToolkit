//-----------------------------------------------------------------------
// <copyright file="PackageCacheUtil.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.IO;
    using UnityEditor;

    public static class PackageCacheUtil
    {
        public static bool IsInPackageCache(UnityEngine.Object obj)
        {
            string assetPath = AssetDatabase.GetAssetOrScenePath(obj);
            return IsInPackageCache(assetPath);
        }

        public static bool IsInPackageCache(string path)
        {
            return Path.GetFullPath(path)
                .Replace("\\", "/")
                .Contains("/PackageCache/");
        }
    }
}
