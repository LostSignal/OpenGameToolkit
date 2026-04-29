//-----------------------------------------------------------------------
// <copyright file="Android.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Settings
{
    using System.IO;

    public class Android : Settings
    {
        // Keystore Settings
        public bool? UseCustomKeystore { get; set; }
        public string KeystoreFile { get; set; }
        public string KeystoreFilePassword { get; set; }
        public string KeystoreAliasName { get; set; }
        public string KeystoreAliasePassword { get; set; }

        // Gradle Settings
        public bool? OverrideGradleProperties { get; set; }
        public bool? UseAndroidX { get; set; }
        public bool? EnableJetifier { get; set; }

        public override void ApplySettings()
        {
            if (this.UseCustomKeystore == true)
            {
                UnityEditor.PlayerSettings.Android.useCustomKeystore = this.UseCustomKeystore.Value;
                UnityEditor.PlayerSettings.Android.keystoreName = this.KeystoreFile;
                UnityEditor.PlayerSettings.Android.keystorePass = this.KeystoreFilePassword;
                UnityEditor.PlayerSettings.Android.keyaliasName = this.KeystoreAliasName;
                UnityEditor.PlayerSettings.Android.keyaliasPass = this.KeystoreAliasePassword;
            }
        }

        public override void ApplySettingsPostAndroidBuild(string gradlePath)
        {
            if (this.OverrideGradleProperties != true)
            {
                return;
            }

            ////
            //// https://stackoverflow.com/questions/54186051/is-there-a-way-to-change-the-gradle-properties-file-in-unity
            ////
            string gradlePropertiesFile = gradlePath + "/gradle.properties";

            if (File.Exists(gradlePropertiesFile))
            {
                File.Delete(gradlePropertiesFile);
            }

            StreamWriter writer = File.CreateText(gradlePropertiesFile);
            writer.WriteLine("org.gradle.jvmargs=-Xmx4096M");

            if (this.UseAndroidX != null)
            {
                writer.WriteLine("android.useAndroidX=" + this.UseAndroidX.ToString().ToLower());
            }

            if (this.EnableJetifier != null)
            {
                writer.WriteLine("android.enableJetifier=" + this.EnableJetifier.ToString().ToLower());
            }

            writer.Flush();
            writer.Close();
        }
    }
}
