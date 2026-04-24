//-----------------------------------------------------------------------
// <copyright file="ApkInstallTool.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    //// TODO [bgish]: Save last APK uploaded to EditorPrefs
    //// TODO [bgish]: Save last device used to  EditorPrefs and re-select if it's around

    public class ApkInstallTool : EditorWindow
    {
        private static readonly OGTLogger Logger = OGTLogger.OGTEditor;

        private string[] devices;
        private int selectedDeviceIndex = -1;
        private string apk;
        private bool reinstall;

        [MenuItem("Tools/OGT/Tools/APK Install Tool", priority = MenuItemPriorities.Tools + 0)]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<ApkInstallTool>(false, "APK Install Tool");
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Refresh Devices List"))
            {
                this.devices = this.GetCurrentDevices();
            }

            if (this.devices?.Length > 0)
            {
                this.selectedDeviceIndex = EditorGUILayout.Popup(this.selectedDeviceIndex, this.devices);
            }
            else
            {
                return;
            }

            if (this.selectedDeviceIndex < 0 || this.selectedDeviceIndex > this.devices.Length)
            {
                return;
            }

            if (GUILayout.Button(string.IsNullOrEmpty(this.apk) ? "Select APK" : "Change APK"))
            {
                var directory = ".";

                if (File.Exists(this.apk))
                {
                    directory = Path.GetDirectoryName(this.apk);
                }

                this.apk = EditorUtility.OpenFilePanel("Select APK", directory, "apk");
            }

            if (string.IsNullOrEmpty(this.apk) == false)
            {
                EditorGUILayout.LabelField(this.apk);
            }

            this.reinstall = EditorGUILayout.Toggle("Re-Install", this.reinstall);

            if (File.Exists(this.apk) && GUILayout.Button("Install"))
            {
                this.InstallAPK(this.reinstall);
            }
        }

        private string[] GetCurrentDevices()
        {
            var result = new List<string>();

            var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = this.GetAdbPath(),
                Arguments = "devices",
                RedirectStandardOutput = true,
                UseShellExecute = false,
            });

            string line = process.StandardOutput.ReadLine();
            while (line != null)
            {
                if (line.EndsWith("\tdevice"))
                {
                    result.Add(line.Substring(0, line.Length - 7));
                }

                line = process.StandardOutput.ReadLine();
            }

            return result.ToArray();
        }

        private void InstallAPK(bool reinstall)
        {
            string reinstallFlag = reinstall ? "-r" : string.Empty;

            var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = this.GetAdbPath(),
                Arguments = $"-s {this.devices[this.selectedDeviceIndex]} install {reinstallFlag} \"{this.apk}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
            });

            Logger.Log(process.StandardOutput.ReadToEnd());
        }

        private string GetAdbPath()
        {
            var editorFolder = Path.GetDirectoryName(EditorApplication.applicationPath);
            return (editorFolder + "/Data/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb").Replace(@"\", @"/");
        }
    }
}
