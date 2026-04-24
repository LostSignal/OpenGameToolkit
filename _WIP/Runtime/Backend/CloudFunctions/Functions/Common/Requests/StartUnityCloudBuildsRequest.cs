//-----------------------------------------------------------------------
// <copyright file="StartUnityCloudBuildsRequest.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY || USING_PLAYFAB

namespace Lost.CloudFunctions.Common
{
    using System.Collections.Generic;

    public class StartUnityCloudBuildsRequest
    {
        public string BasicAuth { get; set; }

        public string Org { get; set; }

        public string Project { get; set; }

        public int SecondsBetweenBuilds { get; set; } = 5 * 60;

        public List<string> BuildTargets { get; set; }
    }
}

#endif
