//-----------------------------------------------------------------------
// <copyright file="Platform.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public static class Platform
    {
        private static IPlatformProvider platformProvider = null;

#if UNITY_6000_0_OR_NEWER
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => platformProvider = null;
#endif

        public static bool IsEditor =>
            platformProvider.IsEditor;

        public static bool IsDebugBuild =>
            platformProvider.IsDebugBuild;

        public static bool IsApplicationQuitting =>
            platformProvider == null ? false : platformProvider.IsApplicationQuitting;

        public static event EventHandler OnBackButtonPressed
        {
            add => platformProvider.OnBackButtonPressed += value;
            remove => platformProvider.OnBackButtonPressed -= value;
        }

        public static event EventHandler OnUpdate
        {
            add => platformProvider.OnUpdate += value;
            remove => platformProvider.OnUpdate -= value;
        }

        public static event EventHandler OnLateUpdate
        {
            add => platformProvider.OnLateUpdate += value;
            remove => platformProvider.OnLateUpdate -= value;
        }

        public static event EventHandler OnFixedUpdate
        {
            add => platformProvider.OnFixedUpdate += value;
            remove => platformProvider.OnFixedUpdate -= value;
        }

        public static event EventHandler OnApplicationQuitting
        {
            add => platformProvider.OnApplicationQuitting += value;
            remove => platformProvider.OnApplicationQuitting -= value;
        }

        public static event EventHandler<bool> OnApplicationFocusChanged
        {
            add => platformProvider.OnApplicationFocusChanged += value;
            remove => platformProvider.OnApplicationFocusChanged -= value;
        }

        public static bool IsIosOrAndroid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var platform = platformProvider.CurrentDevicePlatform;
                return platform == DevicePlatform.iOS || platform == DevicePlatform.Android;
            }
        }

        public static DevicePlatform CurrentDevicePlatform =>
            platformProvider.CurrentDevicePlatform;

        public static void QuitApplication() =>
            platformProvider.QuitApplication();

        public static void Vibrate(long milliseconds) =>
            platformProvider.Vibrate(milliseconds);

        public static string GetCurrentISOLanguageId() =>
            platformProvider.GetCurrentCulture();

        public static string GetStoreURL() =>
            platformProvider.GetStoreURL();

        public static void OpenURL(string url) =>
            platformProvider.OpenURL(url);

        public static void SendEmail(string email, string subject = null, string body = null) =>
            platformProvider.SendEmail(email, subject, body);

        public static void RateApp() =>
            platformProvider.RateApp();

        public static bool DoesLocalFileExist(string localFileName) =>
            platformProvider.DoesLocalFileExist(localFileName);

        public static int GetLocalFile(string localFileName, byte[] buffer) =>
            platformProvider.GetLocalFile(localFileName, buffer);

        public static void SaveLocalFile(string localFileName, byte[] bytes) =>
            platformProvider.SaveLocalFile(localFileName, bytes);

        public static void SaveLocalFile(string localFileName, byte[] bytes, int offset, int count) =>
            platformProvider.SaveLocalFile(localFileName, bytes, offset, count);

        public static float GetDeltaTime() => platformProvider.GetDeltaTime();

        public static float GetPhysicsDeltaTime() => platformProvider.GetPhysicsDeltaTime();

        public static double GetTimeSinceStartup() => platformProvider.GetTimeSinceStartup();

        public static void GetScreenWidthAndHeight(out uint width, out uint height) => platformProvider.GetScreenWidthAndHeight(out width, out height);

        public static void AddShutdownTask(Func<Task> shutdownTask) => platformProvider.AddShutdownTask(shutdownTask);

        public static void SetPlatformProvider(IPlatformProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (platformProvider != null && platformProvider.GetType() == provider.GetType())
            {
                return;
            }

            platformProvider = provider;
        }
    }
}
