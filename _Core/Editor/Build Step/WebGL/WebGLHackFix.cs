using System.IO;
using System.Linq;
using OGT;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;

public class WebGLHackFix : PostBuildStep
{
    public override string Name => "WebGL Hack Fix";

    ////
    //// https://discussions.unity.com/t/cannot-set-properties-of-undefined-setting-1-when-running-a-unitywebrequest/873817
    ////
    public override void Run(BuildProfile buildProfile, BuildReport report)
    {
        var frameworkJsFile = Directory.EnumerateFiles(Path.Combine(report.summary.outputPath, "Build"), "*.framework.js", SearchOption.TopDirectoryOnly).FirstOrDefault();

        if (frameworkJsFile != null)
        {
            Logger.Log($"Updating File '{frameworkJsFile}' to fix WebGL Bug");

            var fileContents = File.ReadAllText(frameworkJsFile);
            var oldLine = "var wr = {requestInstances:{},nextRequestId:1,loglevel:2};";
            var newLine = "var wr = {requestInstances:{},nextRequestId:1,loglevel:2, abortControllers:[], requests:[], timer:[], responses:[]};";

            if (fileContents.Contains(oldLine) == false)
            {
                throw new BuildFailedException("Can't find broken WebGL framework.js line!");
            }

            try
            {
                File.WriteAllText(frameworkJsFile, fileContents.Replace(oldLine, newLine));
            }
            catch
            {
                throw new BuildFailedException("Unable to fix WebGL WebRequest bug!");
            }
        }
    }
}
