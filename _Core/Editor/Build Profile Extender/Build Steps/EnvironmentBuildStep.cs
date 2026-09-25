//-----------------------------------------------------------------------
// <copyright file="EnvironmentBuildStep.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BuildProfile
{
    using System.IO;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Environment Build Step", menuName = "OGT/Build Steps/Environment Build Step")]
    public class EnvironmentBuildStep : BuildStep
    {
        public enum WasmCodeOptimization
        {
            RuntimeSpeed,
            RuntimeSpeedLTO,
            DiskSize,
            DiskSizeLTO,
            BuildTimes
        }

        [Header("Unity Debug Settings")]
        [SerializeField] private bool isUnityDevelopmentBuild = true;
        [SerializeField] private bool allowConnectProfiler = true;
        [SerializeField] private bool allowDebugging = true;
        [SerializeField] private bool allowDeepProfiler = true;
        [SerializeField] private bool isOgtDevelopmentBuild = true;
        [SerializeField] private bool enablePipelineRuntimeInBuild = true;

        [Header("WebGL")]
#pragma warning disable CS0414
        [SerializeField] private WasmCodeOptimization codeOptimization = WasmCodeOptimization.BuildTimes;
#pragma warning restore CS0414

        [SerializeField] private bool copySimpleWebServer = true;
        [SerializeField] private bool fileNamesAsHashes = true;
        [SerializeField] private bool dataCaching = false;

        public override void OnPreprocessBuild(BuildProfileExtender.Environment environment, BuildTarget target)
        {
            this.SetBuildSettings();
            this.SetWebGLSettings(target);

            if (this.isOgtDevelopmentBuild)
            {
                Logger.Log("Enable OGT Development Build is not implemented yet");
            }
            
            if (this.enablePipelineRuntimeInBuild)
            {
                Logger.Log("Enable Pipeline Runtime in Build is not implemented yet");
            }
        }

        public override void OnPostprocessBuild(BuildProfileExtender.Environment environment, BuildTarget target, string path)
        {
            this.CopySimpleWebServer(target, path);
        }

        private void SetBuildSettings()
        {
            Logger.Log($"[EnvironmentBuildStep] Setting EditorUserBuildSettings.development to {this.isUnityDevelopmentBuild}");
            EditorUserBuildSettings.development = this.isUnityDevelopmentBuild;

            Logger.Log($"[EnvironmentBuildStep] Setting EditorUserBuildSettings.connectProfiler to {this.allowConnectProfiler}");
            EditorUserBuildSettings.connectProfiler = this.allowConnectProfiler;

            Logger.Log($"[EnvironmentBuildStep] Setting EditorUserBuildSettings.allowDebugging to {this.allowDebugging}");
            EditorUserBuildSettings.allowDebugging = this.allowDebugging;

            Logger.Log($"[EnvironmentBuildStep] Setting EditorUserBuildSettings.buildWithDeepProfilingSupport to {this.allowDeepProfiler}");
            EditorUserBuildSettings.buildWithDeepProfilingSupport = this.allowDeepProfiler;
        }

        private void SetWebGLSettings(BuildTarget target)
        {
            if (target != BuildTarget.WebGL)
            {
                return;
            }

            #if UNITY_WEBGL
            Logger.Log($"[EnvironmentBuildStep] Setting UnityEditor.WebGL.UserBuildSettings.codeOptimization to {this.codeOptimization}");
            if ((int)UnityEditor.WebGL.UserBuildSettings.codeOptimization != (int)this.codeOptimization)
            {
                UnityEditor.WebGL.UserBuildSettings.codeOptimization = (UnityEditor.WebGL.WasmCodeOptimization)(int)this.codeOptimization;
            }
            #endif

            Logger.Log($"[EnvironmentBuildStep] Setting PlayerSettings.WebGL.nameFilesAsHashes to {this.fileNamesAsHashes}");
            if (PlayerSettings.WebGL.nameFilesAsHashes != this.fileNamesAsHashes)
            {
                PlayerSettings.WebGL.nameFilesAsHashes = this.fileNamesAsHashes;
            }

            Logger.Log($"[EnvironmentBuildStep] Setting PlayerSettings.WebGL.dataCaching to {this.dataCaching}");
            if (PlayerSettings.WebGL.dataCaching != this.dataCaching)
            {                
                PlayerSettings.WebGL.dataCaching = this.dataCaching;
            }
        }

        private void CopySimpleWebServer(BuildTarget target, string path)
        {
            if (target != BuildTarget.WebGL || this.copySimpleWebServer == false)
            {
                return;
            }

            // Making sure we have a simple web server to run the game with
            var simpleWebServerExePath = Path.Combine(path, "SimpleWebServer.exe");

            if (File.Exists(simpleWebServerExePath) == false)
            {
                var simpleWebServerAssetGuid = "d9dcef8d7b6850a42b19ba9c6e3a0938";
                var simpleWebServerAssetPath = AssetDatabase.GUIDToAssetPath(simpleWebServerAssetGuid);
                File.WriteAllBytes(simpleWebServerExePath, File.ReadAllBytes(simpleWebServerAssetPath));
            }
        }
    }
}
