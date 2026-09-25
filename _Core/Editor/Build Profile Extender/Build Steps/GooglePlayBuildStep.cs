//-----------------------------------------------------------------------
// <copyright file="GooglePlayBuildStep.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BuildProfile
{
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Google Play", menuName = "OGT/Build Steps/Google Play")]
    public class GooglePlayBuildStep : BuildStep
    {
        [ReadOnly] [SerializeField] private bool splitApplicationBinary = true;
        [ReadOnly] [SerializeField] private bool isUnityDevelopmentBuild = false;
        [SerializeField] private string keystoreLocation;
        [SerializeField] private SecretString keyAliasPassword;
        [SerializeField] private SecretString keystorePassword;
        [SerializeField] private bool uploadToGooglePlay;

        [ShowIf(nameof(uploadToGooglePlay), true)]
        [SerializeField] private SecretString uploadKey;

        [ShowIf(nameof(uploadToGooglePlay), true)]
        [SerializeField] private string track = "internal";

        public override void OnPreprocessBuild(BuildProfileExtender.Environment environment, BuildTarget target)
        {
            if (target != BuildTarget.Android)
            {
                return;
            }

            Logger.Log($"[GooglePlayBuildStep] Setting PlayerSettings.Android.splitApplicationBinary to {this.splitApplicationBinary}");
            PlayerSettings.Android.splitApplicationBinary = this.splitApplicationBinary;

            Logger.Log($"[GooglePlayBuildStep] Setting EditorUserBuildSettings.development to {this.isUnityDevelopmentBuild}");
            EditorUserBuildSettings.development = this.isUnityDevelopmentBuild;

            Logger.Log($"[GooglePlayBuildStep] Setting PlayerSettings.Android.useCustomKeystore to true");
            PlayerSettings.Android.useCustomKeystore = true;

            Logger.Log($"[GooglePlayBuildStep] Setting PlayerSettings.Android.keystoreName to {this.keystoreLocation}");
            PlayerSettings.Android.keystoreName = this.keystoreLocation;

            Logger.Log($"[GooglePlayBuildStep] Setting PlayerSettings.Android.keyaliasPass");
            PlayerSettings.Android.keyaliasPass = this.keyAliasPassword.Value;

            Logger.Log($"[GooglePlayBuildStep] Setting PlayerSettings.Android.keystorePass");
            PlayerSettings.Android.keystorePass = this.keystorePassword.Value;
        }

        public override void OnPostprocessBuild(BuildProfileExtender.Environment environment, BuildTarget target, string path)
        {
            if (target != BuildTarget.Android || this.uploadToGooglePlay == false)
            {
                return;
            }

            Logger.Log("[GooglePlayBuildStep] GooglePlay Upload Not Implemented Yet, but Upload Key = " + this.uploadKey.Value);
            Logger.Log("[GooglePlayBuildStep] GooglePlay Upload Not Implemented Yet, but Track = " + this.track);
        }
    }
}
