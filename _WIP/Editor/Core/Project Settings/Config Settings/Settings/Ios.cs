//-----------------------------------------------------------------------
// <copyright file="Ios.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Settings
{
    using UnityEditor.Build.Reporting;

    public class Ios : Settings
    {
        public enum IOSPushNotificationType
        {
            Development,
            Production,
        }

        // BitCode
        public bool? DisableIOSBitCode { get; set; }

        // Push Notifications
        public IOSPushNotificationType? IosPushNotificationType { get; set; }

        // Provisioning
        public string ProvisitionProfile { get; set; }  // Relative path
        public string TeamId { get; set; }              // Can be found here https://developer.apple.com/account/#/membership
        public string P12File { get; set; }             // Relative path
        public string P12Password { get; set; }

        public override void ApplySettingsPostBuild(BuildReport buildReport)
        {
            if (buildReport.summary.platform != UnityEditor.BuildTarget.iOS)
            {
                return;
            }

            var buildPath = buildReport.summary.outputPath;

            if (this.DisableIOSBitCode == true)
            {
                DisableBitCode(buildPath);
            }

            if (this.IosPushNotificationType != null)
            {
                EnableIOSPushNotifications(buildPath);
            }

            void DisableBitCode(string buildPath)
            {
#if UNITY_IOS
                AppSettings.Logger.Log("Disabling BitCode...");

                string projectPath = buildPath + "/Unity-iPhone.xcodeproj/project.pbxproj";

                var pbxProject = new UnityEditor.iOS.Xcode.PBXProject();
                pbxProject.ReadFromFile(projectPath);

                string target = pbxProject.TargetGuidByName("Unity-iPhone");
                pbxProject.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

                pbxProject.WriteToFile(projectPath);
#endif
            }

            // Majority of this code was thanks to the com.unity.mobile.notifications package
            void EnableIOSPushNotifications(string buildPath)
            {
#if UNITY_IOS
                AppSettings.Logger.Log("Enabling iOS Push Notifications...");

                // Turning on push notifications (release/development)
                var projectPath = buildPath + "/Unity-iPhone.xcodeproj/project.pbxproj";
                var project = new UnityEditor.iOS.Xcode.PBXProject();
                project.ReadFromString(System.IO.File.ReadAllText(projectPath));

                // Push Notification Capability
                var manager = new UnityEditor.iOS.Xcode.ProjectCapabilityManager(
                    projectPath,
                    "Entitlements.entitlements",
                    targetGuid: project.GetUnityMainTargetGuid()
                );
                manager.AddPushNotifications(this.IosPushNotificationType == IOSPushNotificationType.Development);
                manager.WriteToFile();

                // Making sure Uses Remote Notifications is on
                var preprocessorPath = buildPath + "/Classes/Preprocessor.h";
                var preprocessor = System.IO.File.ReadAllText(preprocessorPath);
                preprocessor = preprocessor.Replace("UNITY_USES_REMOTE_NOTIFICATIONS 0", "UNITY_USES_REMOTE_NOTIFICATIONS 1");

                System.IO.File.WriteAllText(preprocessorPath, preprocessor);
#endif
            }
        }
    }
}
