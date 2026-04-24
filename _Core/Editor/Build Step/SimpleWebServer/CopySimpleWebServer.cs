//-----------------------------------------------------------------------
// <copyright file="CopySimpleWebServerBuildStep.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.IO;
    using UnityEditor;
    using UnityEditor.Build.Profile;
    using UnityEditor.Build.Reporting;

    public class CopySimpleWebServerBuildStep : PostBuildStep
    {
        public override string Name => "Copy SimpleWebServer";

        public override void Run(BuildProfile buildProfile, BuildReport report)
        {
            var pathToBuiltProject = report.summary.outputPath;

            // Making sure we have a simple web server to run the game with
            var simpleWebServerExePath = Path.Combine(pathToBuiltProject, "SimpleWebServer.exe");
            if (File.Exists(simpleWebServerExePath) == false)
            {
                var simpleWebServerAssetGuid = "d9dcef8d7b6850a42b19ba9c6e3a0938";
                var simpleWebServerAssetPath = AssetDatabase.GUIDToAssetPath(simpleWebServerAssetGuid);
                File.WriteAllBytes(simpleWebServerExePath, File.ReadAllBytes(simpleWebServerAssetPath));
            }
        }
    }
}
