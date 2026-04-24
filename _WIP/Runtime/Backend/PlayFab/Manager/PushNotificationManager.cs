//-----------------------------------------------------------------------
// <copyright file="PushNotificationManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY && USING_PLAYFAB

namespace OGT.PlayFab
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public delegate void OnReceivePushNotificationDelegate();

    public class PushNotificationManager
    {
        //// TODO [bgish]: Have an option to register at startup, or from a call
        ////               that way we can prompt the user "We'd like to send you
        ////               xyz, is that cool? If yes, then register (for iOS).
        ////               If it's android just register, maybe have options for
        ////               iOS and android

        private const string HasRegisteredUserForPushNotificationsKey = "HasRegisteredUserForPushNotifications";
        private const float RetryWaitTime = 1.0f;
        private const int RetryCountMax = 10;

        private readonly Queue<PushNotification> pushNotifications = new Queue<PushNotification>();
        private readonly PlayFabManager playfabManager;

#if (UNITY_ANDROID && USING_ANDROID_FIREBASE_MESSAGING) || UNITY_IOS
        private string deviceToken = null;
#endif

        public PushNotificationManager(PlayFabManager playfabManager)
        {
            this.playfabManager = playfabManager;
        }

        public void RegisterPushNotificationsWithPlayFab()
        {
            // TODO [bgish]: Implement
        }

#pragma warning disable
        public event OnReceivePushNotificationDelegate OnReceivePushNotification;
#pragma warning restore

        public bool HasPushNotifications
        {
            get { return this.pushNotifications.Count > 0; }
        }

        public UnityTask<bool> RegisterForPushNotifications()
        {
#if UNITY_ANDROID && USING_ANDROID_FIREBASE_MESSAGING
            return UnityTask<bool>.Run(this.RegisterForAndroidPushNotificationsCoroutine());
#elif UNITY_IOS
            return UnityTask<bool>.Run(this.RegisterForIosPushNotificationsCoroutine());
#else
            return UnityTask<bool>.Run(this.UnsupportedPlatfromCoroutine());
#endif
        }

        public PushNotification GetAndRemoveLatestPushNotification()
        {
            return this.pushNotifications.Dequeue();
        }

        public void ScheduleLocalPushNotification(string title, string message, DateTime dateTime)
        {
#if UNITY_ANDROID && USING_ANDROID_FIREBASE_MESSAGING
            this.ScheduleLocalAndroidPushNotification(title, message, dateTime);
#elif UNITY_IOS
            this.ScheduleLocalIosPushNotification(title, message, dateTime);
#endif
        }

#if UNITY_ANDROID && USING_ANDROID_FIREBASE_MESSAGING

        private void ScheduleLocalAndroidPushNotification(string title, string message, DateTime dateTime)
        {
        }

        private IEnumerator<bool> RegisterForAndroidPushNotificationsCoroutine()
        {
            Firebase.Messaging.FirebaseMessaging.TokenReceived += this.OnTokenReceived;
            Firebase.Messaging.FirebaseMessaging.MessageReceived += this.OnMessageReceived;

            int retryCount = 0;
            while (this.deviceToken == null)
            {
                retryCount++;

                if (retryCount >= RetryCountMax)
                {
                    yield break;
                }

                // Waiting before we retry again...
                float waitTime = 0.0f;
                while (waitTime < RetryWaitTime)
                {
                    yield return default(bool);
                    waitTime += UnityEngine.Time.deltaTime;
                }
            }

            var register = PF.Do(new PlayFab.ClientModels.AndroidDevicePushNotificationRegistrationRequest { DeviceToken = this.deviceToken });

            while (register.IsDone == false)
            {
                yield return default(bool);
            }

            if (register.HasError)
            {
                var playfabError = register.Exception as PlayFabException;

                if (playfabError != null)
                {
                    Lost.Logger.Log("Error Registering for Android Push Notifications!");
                    Lost.Logger.Log(playfabError.Error.Error);
                    Lost.Logger.Log(playfabError.Error.ErrorMessage);
                    Lost.Logger.Log(playfabError.Error.ErrorDetails);
                }
            }
            else
            {
                // Saving that we've registed this user for PushNotifications with PlayFab successfully
                LostPlayerPrefs.GetString(HasRegisteredUserForPushNotificationsKey, PF.User.PlayFabId);


                Lost.Logger.Log("Push Notification Registration Successful!");
                yield return true;
            }

            yield return true;
        }

        private void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
        {
            this.deviceToken = token.Token;
        }

        private void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
        {
            if (e.Message.Notification != null)
            {
                this.pushNotifications.Enqueue(new PushNotification
                {
                    Title = e.Message.Notification.Title,
                    Body = e.Message.Notification.Body,
                    SoundName = null,
                    ApplicationIconBadgeNumber = -1,
                });

                this.OnReceivePushNotification?.Invoke();
            }
        }

#endif

#if UNITY_IOS

        private void ScheduleLocalIosPushNotification(string title, string message, DateTime dateTime)
        {
            UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(new UnityEngine.iOS.LocalNotification()
            {
                alertAction = title,
                alertBody = message,
                fireDate = dateTime,
            });
        }

        private IEnumerator<bool> RegisterForIosPushNotificationsCoroutine()
        {
            var notifications = UnityEngine.iOS.NotificationType.Alert | UnityEngine.iOS.NotificationType.Badge | UnityEngine.iOS.NotificationType.Sound;
            UnityEngine.iOS.NotificationServices.RegisterForNotifications(notifications, true);

            int retryCount = 0;
            while (this.deviceToken == null)
            {
                if (this.deviceToken == null && UnityEngine.iOS.NotificationServices.deviceToken != null)
                {
                    this.deviceToken = System.BitConverter.ToString(UnityEngine.iOS.NotificationServices.deviceToken).Replace("-", "").ToLower();
                }

                retryCount++;

                if (retryCount >= RetryCountMax)
                {
                    yield break;
                }

                // Waiting before we retry again...
                float waitTime = 0.0f;
                while (waitTime < RetryWaitTime)
                {
                    yield return default(bool);
                    waitTime += UnityEngine.Time.deltaTime;
                }
            }

            // if we got here and still no deviceToken, then we timed out
            if (this.deviceToken == null)
            {
                Lost.Logger.LogError("PlayFabIOSPushHandler timed out waiting for the deviceToken.");

                // cleaning up this iOS push notification handler so it doesn't take up any cycles
                yield break;
            }

            string registeredUser = LostPlayerPrefs.GetString(HasRegisteredUserForPushNotificationsKey, null);

            // Checking if we're already successfully registed this user with PlayFab for push notifications
            if (registeredUser == PF.User.PlayFabId)
            {
                CoroutineRunner.Instance.StartCoroutine(this.CheckForIosPushNotificationsCoroutine());
                yield return true;
                yield break;
            }

            var register = PF.Do(new PlayFab.ClientModels.RegisterForIOSPushNotificationRequest { DeviceToken = this.deviceToken });

            while (register.IsDone == false)
            {
                yield return default(bool);
            }

            if (register.HasError)
            {
                var playfabError = register.Exception as PlayFabException;

                if (playfabError != null)
                {
                    Lost.Logger.Log("Error Registering for iOS Push Notifications!");
                    Lost.Logger.Log(playfabError.Error.Error);
                    Lost.Logger.Log(playfabError.Error.ErrorMessage);
                    Lost.Logger.Log(playfabError.Error.ErrorDetails);
                }
            }
            else
            {
                // Saving that we've registed this user for PushNotifications with PlayFab successfully
                LostPlayerPrefs.GetString(HasRegisteredUserForPushNotificationsKey, PF.User.PlayFabId);

                CoroutineRunner.Instance.StartCoroutine(this.CheckForIosPushNotificationsCoroutine());

                Lost.Logger.Log("Push Notification Registration Successful!");
                yield return true;
            }
        }

        private System.Collections.IEnumerator CheckForIosPushNotificationsCoroutine()
        {
            float currentWaitTime = 0.0f;
            float queryWaitTime = 3.0f;

            while (true)
            {
                currentWaitTime += UnityEngine.Time.deltaTime;

                // Early out if haven't met the query time
                if (currentWaitTime < queryWaitTime)
                {
                    yield return null;
                }

                // Reset the timer
                currentWaitTime = 0.0f;

                // https://forum.unity3d.com/threads/using-the-new-notification-system.127016/
                if (UnityEngine.iOS.NotificationServices.remoteNotificationCount != 0)
                {
                    // iterating over all the remote notifications
                    for (int i = 0; i < UnityEngine.iOS.NotificationServices.remoteNotificationCount; i++)
                    {
                        var remote = UnityEngine.iOS.NotificationServices.GetRemoteNotification(i);

                        this.pushNotifications.Enqueue(new PushNotification
                        {
                            Title = remote.alertTitle,
                            Body = remote.alertBody,
                            SoundName = remote.soundName,
                            ApplicationIconBadgeNumber = remote.applicationIconBadgeNumber,
                        });

                        this.OnReceivePushNotification?.Invoke();
                    }

                    // clear all messages
                    UnityEngine.iOS.NotificationServices.ClearRemoteNotifications();
                    UnityEngine.iOS.NotificationServices.ClearLocalNotifications();
                }
            }
        }

#endif

        private IEnumerator<bool> UnsupportedPlatfromCoroutine()
        {
            yield return true;
        }
    }
}

#endif
