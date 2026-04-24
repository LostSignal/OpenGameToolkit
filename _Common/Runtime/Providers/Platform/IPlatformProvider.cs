//-----------------------------------------------------------------------
// <copyright file="IPlatformProvider.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Threading.Tasks;

    public interface IPlatformProvider
    {
        bool IsEditor { get; }

        bool IsDebugBuild { get; }

        bool IsApplicationQuitting { get; }

        event EventHandler OnBackButtonPressed;

        event EventHandler OnUpdate;

        event EventHandler OnLateUpdate;

        event EventHandler OnFixedUpdate;

        event EventHandler OnApplicationQuitting;

        event EventHandler<bool> OnApplicationFocusChanged;

        DevicePlatform CurrentDevicePlatform { get; }

        void QuitApplication();

        void Vibrate(long milliseconds);

        string GetCurrentCulture();

        string GetStoreURL();

        void OpenURL(string url);

        void SendEmail(string email, string subject, string body);

        void RateApp();

        bool DoesLocalFileExist(string localFileName);

        int GetLocalFile(string localFileName, byte[] buffer);

        void SaveLocalFile(string localFileName, byte[] bytes);

        void SaveLocalFile(string localFileName, byte[] bytes, int offset, int count);

        float GetDeltaTime();

        float GetPhysicsDeltaTime();

        double GetTimeSinceStartup();

        void GetScreenWidthAndHeight(out uint width, out uint height);

        void AddShutdownTask(Func<Task> shutdownTask);
    }
}
