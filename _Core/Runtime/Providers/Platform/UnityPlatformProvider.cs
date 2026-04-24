//-----------------------------------------------------------------------
// <copyright file="UnityPlatformProvider.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Networking;

    //// NOTE [bgish]:  Windows Universal May Support System.IO.File class now
    //// TODO add events for pen and mouse detected, that way if someone uses a pen
    //// TODO controller too?  maybe only if InControl is detected?

    public class UnityPlatformProvider : IPlatformProvider
    {
        // TODO [bgish] - make sure <uses-permission android:name="android.permission.VIBRATE"/> is in the AndroidManifest.xml file
#if UNITY_ANDROID && !UNITY_EDITOR
        private static readonly AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        private static readonly AndroidJavaObject CurrentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        private static readonly AndroidJavaObject Vibrator = CurrentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
#endif

        private static readonly OGTLogger Logger = OGTLogger.OGT;
        private static bool isApplicationQuitting;

        private List<Func<Task>> shutdownTasks = new();

        public delegate void OnResetDelegate();

        public static event OnResetDelegate OnReset;

        private static UnityDispatcher dispatcher;

        public UnityPlatformProvider()
        {
            CreateDispatcher();
            Application.wantsToQuit += this.UnityWantsToQuit;
        }

        public event EventHandler OnBackButtonPressed
        {
            add => dispatcher.OnBackButtonPressed += value;
            remove => dispatcher.OnBackButtonPressed -= value;
        }

        public event EventHandler OnUpdate
        {
            add => dispatcher.OnUpdate += value;
            remove => dispatcher.OnUpdate -= value;
        }

        public event EventHandler OnLateUpdate
        {
            add => dispatcher.OnLateUpdate += value;
            remove => dispatcher.OnLateUpdate -= value;
        }

        public event EventHandler OnFixedUpdate
        {
            add => dispatcher.OnLateUpdate += value;
            remove => dispatcher.OnLateUpdate -= value;
        }

        public event EventHandler OnApplicationQuitting
        {
            add => dispatcher.OnApplicationQuitting += value;
            remove => dispatcher.OnApplicationQuitting -= value;
        }

        public event EventHandler<bool> OnApplicationFocusChanged
        {
            add => dispatcher.OnApplicationFocusChanged += value;
            remove => dispatcher.OnApplicationFocusChanged -= value;
        }

        public bool IsEditor => Application.isEditor;

        public bool IsDebugBuild => Debug.isDebugBuild;

        public bool IsApplicationQuitting => isApplicationQuitting;

        public static bool IsUnityCloudBuild
        {
            get
            {
#if UNITY_CLOUD_BUILD
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsPlayingOrEnteringPlaymode
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
#else
                return true;
#endif
            }
        }

        public static bool IsIosOrAndroid
        {
            get => GetCurrentDevicePlatform() switch
            {
                DevicePlatform.iOS => true,
                DevicePlatform.Android => true,
                _ => false,
            };
        }

        public static EditorPlatform CurrentEditorPlatform
        {
            get => Application.platform switch
            {
                RuntimePlatform.WindowsEditor => EditorPlatform.Windows,
                RuntimePlatform.OSXEditor => EditorPlatform.Mac,
                RuntimePlatform.LinuxEditor => EditorPlatform.Linux,
                _ => EditorPlatform.Unknown,
            };
        }

        public DevicePlatform CurrentDevicePlatform
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetCurrentDevicePlatform();
        }

        public void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void Vibrate(long milliseconds)
        {
            switch (CurrentDevicePlatform)
            {
                case DevicePlatform.Android:
#if UNITY_ANDROID && !UNITY_EDITOR
                    Vibrator.Call("vibrate", milliseconds);
#endif
                    break;

                case DevicePlatform.iOS:
#if UNITY_IOS
                    Handheld.Vibrate();
#endif
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public string GetCurrentCulture()
        {
            string unityCulture = GetUnityMapping();
            string dotNetCulture = CultureInfo.CurrentCulture.Name;

            if (unityCulture != null && unityCulture != dotNetCulture)
            {
                Logger.LogWarning($"Found Mismatch in Language Culture Unity: \'{unityCulture}\' and .Net: \'{dotNetCulture}\'");
            }

            return unityCulture ?? dotNetCulture;

            string GetUnityMapping()
            {
                return Application.systemLanguage switch
                {
                    SystemLanguage.Afrikaans => "af",
                    SystemLanguage.Arabic => "ar",
                    SystemLanguage.Basque => "eu",
                    SystemLanguage.Belarusian => "be",
                    SystemLanguage.Bulgarian => "bg",
                    SystemLanguage.Catalan => "ca",
                    SystemLanguage.Chinese => "zh",
                    SystemLanguage.Czech => "cs",
                    SystemLanguage.Danish => "da",
                    SystemLanguage.Dutch => "nl",
                    SystemLanguage.English => "en",
                    SystemLanguage.Estonian => "et",
                    SystemLanguage.Faroese => "fo",
                    SystemLanguage.Finnish => "fi",
                    SystemLanguage.French => "fr",
                    SystemLanguage.German => "de",
                    SystemLanguage.Greek => "el",
                    SystemLanguage.Hebrew => "he",
                    SystemLanguage.Icelandic => "is",
                    SystemLanguage.Indonesian => "id",
                    SystemLanguage.Italian => "it",
                    SystemLanguage.Japanese => "ja",
                    SystemLanguage.Korean => "ko",
                    SystemLanguage.Latvian => "lv",
                    SystemLanguage.Lithuanian => "lt",
                    SystemLanguage.Polish => "pl",
                    SystemLanguage.Portuguese => "pt",
                    SystemLanguage.Romanian => "ro",
                    SystemLanguage.Russian => "ru",
                    SystemLanguage.Slovak => "sk",
                    SystemLanguage.Slovenian => "sl",
                    SystemLanguage.Spanish => "es",
                    SystemLanguage.Swedish => "sv",
                    SystemLanguage.Thai => "th",
                    SystemLanguage.Turkish => "tr",
                    SystemLanguage.Ukrainian => "uk",
                    SystemLanguage.Vietnamese => "vi",
                    SystemLanguage.ChineseSimplified => "zh-Hans",
                    SystemLanguage.ChineseTraditional => "zh-Hant",
                    SystemLanguage.Hindi => "hi",
                    SystemLanguage.Hungarian => "hu",

                    // NOTE [bgish]: Unsure about best mapping for these (possibly just return null?)
                    SystemLanguage.SerboCroatian => "hr",
                    SystemLanguage.Norwegian => "nb",

                    SystemLanguage.Unknown => null,
                    _ => null,
                };
            }
        }

        public string GetStoreURL()
        {
            switch (this.CurrentDevicePlatform)
            {
                case DevicePlatform.Android:
                    return string.Format("market://details?id={0}", Application.identifier);

                case DevicePlatform.iOS:
                    return string.Format("itms-apps://itunes.apple.com/app/{0}", Application.identifier);

                default:
                    throw new NotImplementedException();
            }
        }

        public void OpenURL(string url) => Application.OpenURL(url);

        public void RateApp() => this.OpenURL(this.GetStoreURL());

        public void SendEmail(string email, string subject = null, string body = null)
        {
            string mailToUrl = "mailto:" + email;

            if (string.IsNullOrEmpty(subject) == false)
            {
                mailToUrl += "?subject=" + UnityWebRequest.EscapeURL(subject).Replace("+", "%20");
            }

            if (string.IsNullOrEmpty(body) == false)
            {
                mailToUrl += "?body=" + UnityWebRequest.EscapeURL(body).Replace("+", "%20");
            }

            this.OpenURL(mailToUrl);
        }

        public bool DoesLocalFileExist(string localFileName)
        {
            switch (CurrentDevicePlatform)
            {
                case DevicePlatform.iOS:
                case DevicePlatform.Android:
                case DevicePlatform.Windows:
                case DevicePlatform.Mac:
                case DevicePlatform.Linux:
                    {
                        try
                        {
                            return File.Exists(Path.Combine(Application.persistentDataPath, localFileName));
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"Error in Platform.DoesLocalFileExist({localFileName})");
                            Logger.LogException(ex);
                            return false;
                        }
                    }

                case DevicePlatform.WindowsUniversal:
                case DevicePlatform.WebGL:
                    {
                        return PlayerPrefs.HasKey(localFileName);
                    }

                case DevicePlatform.XboxOne:
                case DevicePlatform.XboxSeries:
                case DevicePlatform.PS4:
                case DevicePlatform.PS5:
                case DevicePlatform.MagicLeap:
                default:
                    {
                        throw new NotImplementedException();
                    }
            }
        }

        public int GetLocalFile(string localFileName, byte[] buffer)
        {
            switch (CurrentDevicePlatform)
            {
                case DevicePlatform.iOS:
                case DevicePlatform.Android:
                case DevicePlatform.Windows:
                case DevicePlatform.Mac:
                case DevicePlatform.Linux:
                    {
                        try
                        {
                            var path = Path.Combine(Application.persistentDataPath, localFileName);

                            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                int fileLength = (int)fileStream.Length;
                                int index = 0;
                                int count = fileLength;

                                Logger.AssertFormat(buffer.Length > fileLength, "Platform.GetLocalFile byte buffer is too small. Has {0} and needs {1}.", buffer.Length, fileLength);

                                while (count > 0)
                                {
                                    int n = fileStream.Read(buffer, index, count);

                                    if (n == 0)
                                    {
                                        throw new Exception("Unknown Read Error");
                                    }

                                    index += n;
                                    count -= n;
                                }

                                return count;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"Error in Platform.GetLocalFile({localFileName})");
                            Logger.LogException(ex);

                            return -1;
                        }
                    }

                case DevicePlatform.WindowsUniversal:
                case DevicePlatform.WebGL:
                    {
                        var bytes = Convert.FromBase64String(PlayerPrefs.GetString(localFileName));

                        Logger.AssertFormat(buffer.Length > bytes.Length, "Platform.GetLocalFile byte buffer is too small. Has {0} and needs {1}.", buffer.Length, bytes.Length);

                        Array.Copy(bytes, buffer, bytes.Length);

                        return bytes.Length;
                    }

                case DevicePlatform.XboxOne:
                case DevicePlatform.XboxSeries:
                case DevicePlatform.PS4:
                case DevicePlatform.PS5:
                case DevicePlatform.MagicLeap:
                default:
                    {
                        throw new NotImplementedException();
                    }
            }
        }

        public void SaveLocalFile(string localFileName, byte[] bytes)
        {
            SaveLocalFile(localFileName, bytes, 0, bytes.Length);
        }

        public void SaveLocalFile(string localFileName, byte[] bytes, int offset, int count)
        {
            switch (CurrentDevicePlatform)
            {
                case DevicePlatform.iOS:
                case DevicePlatform.Android:
                case DevicePlatform.Windows:
                case DevicePlatform.Mac:
                case DevicePlatform.Linux:
                    {
                        try
                        {
                            var path = Path.Combine(Application.persistentDataPath, localFileName);

                            using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
                            {
                                fileStream.Write(bytes, offset, count);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"Error in Platform.SaveLocalFile({localFileName})");
                            Logger.LogException(ex);
                        }

                        break;
                    }

                case DevicePlatform.WindowsUniversal:
                case DevicePlatform.WebGL:
                    {
                        PlayerPrefs.SetString(localFileName, Convert.ToBase64String(bytes));
                        PlayerPrefs.Save();

                        break;
                    }

                case DevicePlatform.XboxOne:
                case DevicePlatform.XboxSeries:
                case DevicePlatform.PS4:
                case DevicePlatform.PS5:
                case DevicePlatform.MagicLeap:
                default:
                    throw new NotImplementedException();
            }
        }

        public static string GetStreamingAssetsURL(string path)
        {
            return Application.platform == RuntimePlatform.Android ?
                Path.Combine(Application.streamingAssetsPath, path).Replace(@"\", "/") :
                "file://" + Path.Combine(Application.streamingAssetsPath, path).Replace(@"\", "/");
        }

        public float GetDeltaTime() => Time.deltaTime;

        public float GetPhysicsDeltaTime() => Time.fixedDeltaTime;

        public double GetTimeSinceStartup() => Time.realtimeSinceStartupAsDouble;

        [EditorEvents.OnExitPlayMode]
        public static void Reset()
        {
            try
            {
                OnReset?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        [EditorEvents.OnEnterPlayMode]
        private static void OnEnterPlayMode()
        {
            isApplicationQuitting = false;
            CreateDispatcher();
        }

        [EditorEvents.OnExitPlayMode]
        private static void OnExitPlayMode()
        {
            isApplicationQuitting = false;
            dispatcher = null;
        }

        private static void CreateDispatcher()
        {
            if (Application.isPlaying && dispatcher == null)
            {
                dispatcher = SingletonUtil.CreateSingleton<UnityDispatcher>("Unity Dispatcher");
                dispatcher.OnApplicationQuitting += (sender, eventArgs) => isApplicationQuitting = true;
            }
        }

        private static DevicePlatform GetCurrentDevicePlatform()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.IPhonePlayer:
                    return DevicePlatform.iOS;

                case RuntimePlatform.Android:
                    return DevicePlatform.Android;

                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return DevicePlatform.Windows;

                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return DevicePlatform.Mac;

                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxEditor:
                    return DevicePlatform.Linux;

                case RuntimePlatform.WSAPlayerX86:
                case RuntimePlatform.WSAPlayerX64:
                case RuntimePlatform.WSAPlayerARM:
                    return DevicePlatform.WindowsUniversal;

                case RuntimePlatform.GameCoreXboxOne:
                case RuntimePlatform.XboxOne:
                    return DevicePlatform.XboxOne;

                case RuntimePlatform.GameCoreXboxSeries:
                    return DevicePlatform.XboxSeries;

                case RuntimePlatform.WebGLPlayer:
                    return DevicePlatform.WebGL;

                case RuntimePlatform.PS4:
                    return DevicePlatform.PS4;

                case RuntimePlatform.PS5:
                    return DevicePlatform.PS5;

                case RuntimePlatform.tvOS:
                case RuntimePlatform.Switch:
                case RuntimePlatform.EmbeddedLinuxArm64:
                case RuntimePlatform.EmbeddedLinuxX64:
                case RuntimePlatform.LinuxServer:
                case RuntimePlatform.WindowsServer:
                case RuntimePlatform.OSXServer:
                default:
                    throw new NotImplementedException($"Platform {Application.platform} unsupported.");
            }
        }

        public void GetScreenWidthAndHeight(out uint width, out uint height)
        {
#if UNITY_EDITOR
            UnityEditor.PlayModeWindow.GetRenderingResolution(out width, out height);
#else
            width = (uint)Screen.width;
            height = (uint)Screen.height;
#endif
        }

        public void AddShutdownTask(Func<Task> shutdownTask)
        {
            if (shutdownTask == null)
            {
                throw new ArgumentNullException(nameof(shutdownTask));
            }

            this.shutdownTasks.Add(shutdownTask);
        }

        private bool UnityWantsToQuit()
        {
            if (this.shutdownTasks.IsNullOrEmpty())
            {
                return true;
            }

            Task.Run(StartAsyncShutdownTasks);

            return false;

            async void StartAsyncShutdownTasks()
            {
                Task[] tasks = new Task[this.shutdownTasks.Count];

                for (int i = 0; i < this.shutdownTasks.Count; i++)
                {
                    try
                    {
                        Debug.Log($"Starting Task Index {i}");
                        tasks[i] = this.shutdownTasks[i]?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex);
                        tasks[i] = Task.CompletedTask;
                    }
                }

                Debug.Log($"Wait for all tasks to complete...");
                await Task.WhenAll(tasks);
                Debug.Log($"All shutdown tasks complete!");

                Application.wantsToQuit -= this.UnityWantsToQuit;
                Application.Quit();
            }
        }
    }
}
