//-----------------------------------------------------------------------
// <copyright file="RosylnAssetPostprocessor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEditor;

    public class RosylnAssetPostprocessor : AssetPostprocessor
    {
        public static string OnGeneratedCSProject(string path, string content)
        {
            content = ProjectSettingsRosylnAnalyzers.Instance.AddAnalyzersToCSProjects(path, content);

            return content;
        }
    }
}
