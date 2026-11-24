
//-----------------------------------------------------------------------
// <copyright file="MetaStoreBuildStep.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BuildProfile
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;

    //// Uploads the built APK to a Meta Horizon Store release channel using one of Meta's command line tools:
    ////
    ////   Meta VR CLI (default)  - "metavr store dist upload".  Uses the login from "metavr auth login", so there are
    ////                            no secrets to manage.  Best for building on your own machine.
    ////                            Install: npx metavr@latest init
    ////
    ////   OVR Platform Utility   - "ovr-platform-util upload-quest-build".  Authenticates with the App Secret (or a
    ////                            user token), so it works on headless build machines.
    ////                            Install: https://developers.meta.com/horizon/resources/publish-reference-platform-command-line-utility
    ////
    //// NOTE: Uploading only puts the build on a release channel.  Submitting the app for store review is still
    ////       done by hand in the Meta Horizon Developer Dashboard.
    [CreateAssetMenu(fileName = "Meta Store Upload", menuName = "OGT/Build Steps/Upload to Meta Store")]
    public class MetaStoreBuildStep : BuildStep
    {
        private const string LogPrefix = "[MetaStoreBuildStep]";
        private const string MetaVrExecutableName = "metavr";
        private const string OvrPlatformUtilExecutableName = "ovr-platform-util";

        public enum AgeGroup
        {
            TeensAndAdults,
            MixedAges,
            Children,
        }

        [Header("Upload")]
        [Tooltip("Turn off to keep this step on the Build Profile without uploading.")]
        [SerializeField] private bool uploadToMetaStore = true;

        [Tooltip("The App ID from the Meta Horizon Developer Dashboard (Development > API).")]
        [SerializeField] private string appId;

        [Tooltip("Release channel to upload to.  Meta VR CLI: ALPHA, BETA, RC or LIVE.  OVR Platform Utility: the channel name, or 'store' for the production channel.")]
        [SerializeField] private string channel = "ALPHA";

        [Tooltip("Off = the build is uploaded as a draft and must be published to the channel from the Developer Dashboard.  On = the build goes live on the channel as soon as the upload finishes.")]
        [SerializeField] private bool publishImmediately = false;

        [Tooltip("Release notes for this build.  Supports {version}, {versionCode} and {environment}.")]
        [TextArea(2, 6)]
        [SerializeField] private string releaseNotes = "{version} ({versionCode})";

        [Tooltip("Minutes to wait for the upload before killing it and failing the build.")]
        [SerializeField] private int timeoutMinutes = 60;

        [Header("Tool")]
        [Tooltip("Off = Meta VR CLI (metavr), which uses your 'metavr auth login'.  On = OVR Platform Utility (ovr-platform-util), which uses the App Secret below and works on headless build machines.")]
        [SerializeField] private bool useOvrPlatformUtil = false;

        [Tooltip("Optional full path to the tool's executable.  Leave empty to find it on the PATH.")]
        [SerializeField] private string executablePath;

        [Tooltip("The App Secret (Developer Dashboard > Development > API), or a user token if 'App Secret Is User Token' is on.")]
        [ShowIf(nameof(useOvrPlatformUtil), true)]
        [SerializeField] private SecretString appSecret;

        [Tooltip("On if the secret above is a user token (Developer Dashboard > Account settings) instead of the App Secret.")]
        [ShowIf(nameof(useOvrPlatformUtil), true)]
        [SerializeField] private bool appSecretIsUserToken = false;

        [Tooltip("The age group the build is self-certified for.  Required by OVR Platform Utility.")]
        [ShowIf(nameof(useOvrPlatformUtil), true)]
        [SerializeField] private AgeGroup ageGroup = AgeGroup.TeensAndAdults;

        public override void OnPostprocessBuild(UnityEditor.Build.Profile.BuildProfile buildProfile, BuildProfileExtender.Environment environment, BuildReport report)
        {
            var summary = report.summary;

            if (summary.platform != BuildTarget.Android || this.uploadToMetaStore == false)
            {
                return;
            }

            if ((summary.options & BuildOptions.Development) != 0)
            {
                Logger.LogWarning($"{LogPrefix} This is a Development Build. Meta only allows debuggable APKs on private release channels.");
            }

            // Saved before uploading so a failed upload can be retried with the "Upload Last Build" button
            EditorPrefs.SetString(LastApkKey, Path.GetFullPath(summary.outputPath));

            this.Upload(summary.outputPath, environment.ToString());
        }

        // EditorPrefs are shared by every project on this machine, so the key has to be per project
        private static string LastApkKey => $"OGT.MetaStoreBuildStep.LastApk.{Application.dataPath.GetHashCode()}";

        // Uploads the APK from the last Android build, so the upload can be tested or retried without doing a full build
        [InspectorButton("Upload Last Build")]
        private void UploadLastBuild()
        {
            var apkPath = EditorPrefs.GetString(LastApkKey, string.Empty);

            // Builds made before this step was added to the profile never saved their location
            if (string.IsNullOrEmpty(apkPath) || File.Exists(apkPath) == false)
            {
                apkPath = EditorUserBuildSettings.GetBuildLocation(BuildTarget.Android);
            }

            if (string.IsNullOrEmpty(apkPath) || File.Exists(apkPath) == false || apkPath.EndsWith(".apk", StringComparison.OrdinalIgnoreCase) == false)
            {
                EditorUtility.DisplayDialog("Upload to Meta Store", "Could not find the APK from the last Android build. Make a build first, or use 'Upload Existing APK...' from this asset's context menu to pick one.", "OK");
                return;
            }

            this.ConfirmAndUpload(apkPath);
        }

        // Same as above, but lets you pick the APK
        [ContextMenu("Upload Existing APK...")]
        private void UploadExistingApk()
        {
            var lastApk = EditorPrefs.GetString(LastApkKey, string.Empty);
            var directory = string.IsNullOrEmpty(lastApk) ? string.Empty : Path.GetDirectoryName(lastApk);
            var apkPath = EditorUtility.OpenFilePanel("Select the APK to upload to the Meta Store", directory, "apk");

            if (string.IsNullOrEmpty(apkPath))
            {
                return;
            }

            EditorPrefs.SetString(LastApkKey, Path.GetFullPath(apkPath));
            this.ConfirmAndUpload(apkPath);
        }

        private void ConfirmAndUpload(string apkPath)
        {
            apkPath = Path.GetFullPath(apkPath);

            var message = $"Upload '{apkPath}' (built {File.GetLastWriteTime(apkPath):g}) to channel '{this.channel}' of app {this.appId}{(this.publishImmediately ? " and PUBLISH it immediately" : " as a draft")}?\n\nThe editor will be unresponsive until the upload finishes.";

            if (EditorUtility.DisplayDialog("Upload to Meta Store", message, "Upload", "Cancel") == false)
            {
                return;
            }

            try
            {
                this.Upload(apkPath, "Manual");
                EditorUtility.DisplayDialog("Upload to Meta Store", "Upload complete.", "OK");
            }
            catch (BuildFailedException ex)
            {
                Logger.LogError(ex.Message);
                EditorUtility.DisplayDialog("Upload to Meta Store", "Upload failed, see the Console for details.", "OK");
            }
        }

        private void Upload(string apkPath, string environmentName)
        {
            if (string.IsNullOrWhiteSpace(this.appId))
            {
                throw new BuildFailedException($"{LogPrefix} App ID is not set!");
            }

            if (string.IsNullOrWhiteSpace(this.channel))
            {
                throw new BuildFailedException($"{LogPrefix} Channel is not set!");
            }

            apkPath = Path.GetFullPath(apkPath);

            if (apkPath.EndsWith(".apk", StringComparison.OrdinalIgnoreCase) == false)
            {
                throw new BuildFailedException($"{LogPrefix} Meta only accepts APK files, but the build output is '{apkPath}'. Turn off 'Build App Bundle (Google Play)' and 'Export Project'.");
            }

            if (File.Exists(apkPath) == false)
            {
                throw new BuildFailedException($"{LogPrefix} Build output '{apkPath}' does not exist");
            }

            // Unity writes the expansion file next to the APK when "Split Application Binary" is on
            var obbPath = Path.ChangeExtension(apkPath, ".main.obb");

            if (File.Exists(obbPath) == false)
            {
                obbPath = null;
            }

            var notes = (this.releaseNotes ?? string.Empty)
                .Replace("{version}", Application.version)
                .Replace("{versionCode}", PlayerSettings.Android.bundleVersionCode.ToString())
                .Replace("{environment}", environmentName)
                .Trim();

            string secretToMask = null;

            var arguments = this.useOvrPlatformUtil
                ? this.GetOvrPlatformUtilArguments(apkPath, obbPath, notes, out secretToMask)
                : this.GetMetaVrArguments(apkPath, obbPath, notes);

            var executable = this.GetExecutable();

            Logger.Log($"{LogPrefix} Uploading '{apkPath}'{(obbPath != null ? " + OBB" : string.Empty)} to channel '{this.channel}' of app {this.appId}");

            var exitCode = this.Run(executable, arguments, secretToMask);

            if (exitCode != 0)
            {
                throw new BuildFailedException($"{LogPrefix} Upload failed, '{Path.GetFileName(executable)}' exited with code {exitCode}. See the log above for details.");
            }

            Logger.Log(this.publishImmediately
                ? $"{LogPrefix} Build Step Complete: build is published to channel '{this.channel}'"
                : $"{LogPrefix} Build Step Complete: build uploaded as a draft for channel '{this.channel}', publish it from the Developer Dashboard");
        }

        private List<string> GetMetaVrArguments(string apkPath, string obbPath, string notes)
        {
            var arguments = new List<string>
            {
                "store", "dist", "upload",
                "--app-id", this.appId.Trim(),
                "--channel", this.channel.Trim(),
                "--apk", apkPath,
                "--color", "never",
            };

            if (obbPath != null)
            {
                arguments.Add("--obb");
                arguments.Add(obbPath);
            }

            if (string.IsNullOrEmpty(notes) == false)
            {
                arguments.Add("--notes");
                arguments.Add(notes);
            }

            // metavr uploads as a draft unless told otherwise
            if (this.publishImmediately)
            {
                arguments.Add("--publish");
            }

            return arguments;
        }

        private List<string> GetOvrPlatformUtilArguments(string apkPath, string obbPath, string notes, out string secret)
        {
            if (this.appSecret == null || this.appSecret.HasKey == false)
            {
                throw new BuildFailedException($"{LogPrefix} App Secret is not set!");
            }

            if (this.appSecret.TryGetValue(out secret) == false || string.IsNullOrWhiteSpace(secret))
            {
                throw new BuildFailedException($"{LogPrefix} App Secret '{this.appSecret.SecretKey}' has no value.");
            }

            secret = secret.Trim();

            var arguments = new List<string>
            {
                "upload-quest-build",
                "--age-group", GetAgeGroupName(this.ageGroup),
                "--app-id", this.appId.Trim(),
                this.appSecretIsUserToken ? "--token" : "--app-secret", secret,
                "--apk", apkPath,
                "--channel", this.channel.Trim(),
                "--disable-progress-bar",
            };

            if (obbPath != null)
            {
                arguments.Add("--obb");
                arguments.Add(obbPath);
            }

            if (string.IsNullOrEmpty(notes) == false)
            {
                // ovr-platform-util wants newlines encoded as a literal \n
                arguments.Add("--notes");
                arguments.Add(notes.Replace("\r\n", "\n").Replace("\n", "\\n"));
            }

            // ovr-platform-util publishes to the channel unless told otherwise
            if (this.publishImmediately == false)
            {
                arguments.Add("--draft");
            }

            return arguments;
        }

        private static string GetAgeGroupName(AgeGroup ageGroup)
        {
            switch (ageGroup)
            {
                case AgeGroup.MixedAges: return "MIXED_AGES";
                case AgeGroup.Children: return "CHILDREN";
                default: return "TEENS_AND_ADULTS";
            }
        }

        private string GetExecutable()
        {
            if (string.IsNullOrWhiteSpace(this.executablePath) == false)
            {
                var customPath = Environment.ExpandEnvironmentVariables(this.executablePath.Trim());

                if (File.Exists(customPath) == false)
                {
                    throw new BuildFailedException($"{LogPrefix} Executable Path '{customPath}' does not exist");
                }

                return customPath;
            }

            var executableName = this.useOvrPlatformUtil ? OvrPlatformUtilExecutableName : MetaVrExecutableName;
            var fileName = executableName + (Application.platform == RuntimePlatform.WindowsEditor ? ".exe" : string.Empty);

            // The editor's PATH is captured when Unity Hub starts, so a tool installed after that isn't on it yet.
            // Also searching the PATH as it is saved right now, so a fresh install works without restarting anything.
            var searchPaths = new List<string>
            {
                Environment.GetEnvironmentVariable("PATH"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".metavr", "bin"),
            };

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                searchPaths.Add(Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User));
                searchPaths.Add(Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine));
            }

            foreach (var directory in searchPaths.Where(x => string.IsNullOrEmpty(x) == false).SelectMany(x => x.Split(Path.PathSeparator)))
            {
                try
                {
                    var candidate = Path.Combine(Environment.ExpandEnvironmentVariables(directory.Trim().Trim('"')), fileName);

                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }
                }
                catch (ArgumentException)
                {
                    // Malformed PATH entry
                }
            }

            return executableName;
        }

        private int Run(string executable, List<string> arguments, string secretToMask)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = executable,
                Arguments = string.Join(" ", arguments.Select(QuoteArgument)),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            Logger.Log($"{LogPrefix} Running: {executable} {string.Join(" ", arguments.Select(x => x == secretToMask ? "********" : QuoteArgument(x)))}");

            using var process = new Process { StartInfo = startInfo };

            // NOTE: These fire on thread pool threads while the main thread is blocked in WaitForExit below
            process.OutputDataReceived += (sender, e) => LogOutput(e.Data, secretToMask);
            process.ErrorDataReceived += (sender, e) => LogOutput(e.Data, secretToMask);

            try
            {
                process.Start();
            }
            catch (Win32Exception ex)
            {
                var install = this.useOvrPlatformUtil
                    ? "Download OVR Platform Utility from https://developers.meta.com/horizon/resources/publish-reference-platform-command-line-utility"
                    : "Install the Meta VR CLI with 'npx metavr@latest init'";

                // NOTE: The exception message contains the full command line, secret included
                var message = string.IsNullOrEmpty(secretToMask) ? ex.Message : ex.Message.Replace(secretToMask, "********");

                throw new BuildFailedException($"{LogPrefix} Could not start '{executable}' ({message}). {install},then either add it to the PATH and restart Unity and Unity Hub, or set Executable Path on this build step.");
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var timeout = TimeSpan.FromMinutes(Math.Max(1, this.timeoutMinutes));

            if (process.WaitForExit((int)timeout.TotalMilliseconds) == false)
            {
                try
                {
                    process.Kill();
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"{LogPrefix} Failed to kill the upload process: {ex.Message}");
                }

                throw new BuildFailedException($"{LogPrefix} Upload timed out after {timeout.TotalMinutes} minutes");
            }

            // Flushes any output that is still buffered
            process.WaitForExit();

            return process.ExitCode;
        }

        // Quotes an argument so it survives the standard command line parsing rules (spaces, quotes, trailing backslashes)
        private static string QuoteArgument(string argument)
        {
            if (argument.Length > 0 && argument.IndexOfAny(new[] { ' ', '\t', '\n', '\r', '"' }) < 0)
            {
                return argument;
            }

            var quoted = new StringBuilder("\"");
            var backslashes = 0;

            foreach (var c in argument)
            {
                if (c == '\\')
                {
                    backslashes++;
                    continue;
                }

                // Backslashes are only special when they come right before a quote
                quoted.Append('\\', c == '"' ? (backslashes * 2) + 1 : backslashes);
                quoted.Append(c);
                backslashes = 0;
            }

            quoted.Append('\\', backslashes * 2);
            quoted.Append('"');

            return quoted.ToString();
        }

        private static void LogOutput(string line, string secretToMask)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            if (string.IsNullOrEmpty(secretToMask) == false)
            {
                line = line.Replace(secretToMask, "********");
            }

            Logger.Log($"{LogPrefix} {line}");
        }
    }
}
