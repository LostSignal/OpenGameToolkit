//-----------------------------------------------------------------------
// <copyright file="ReleaseLocator.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using UnityEngine;

    public static class ReleaseLocator
    {
        public const string ReleasesResourcesName = "release";

        public static Release GetCurrentReleaseFromResources()
        {
            var jsonAsset = Resources.Load<TextAsset>(ReleasesResourcesName);
            return JsonUtil.Deserialize<Release>(jsonAsset.text);
        }
    }
}

#endif
