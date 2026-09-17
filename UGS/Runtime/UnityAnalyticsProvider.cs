//-----------------------------------------------------------------------
// <copyright file="UnityAnalyticsProvider.cs" company="PieTrap">
//     Copyright (c) PieTrap. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
#if !USING_UGS_ANALYTICS

    // TODO [bgis]: Have a giant warning info box here that says "You are using the Unity Analytics Provider without the UGS Analytics package. Please install the UGS Analytics package to use this provider."
    public class UnityAnalyticsProvider : GameBehavior, IAnalyticsProvider
    {
        public bool IsSupported => false;
        public async System.Threading.Tasks.Task Initialize() => throw new System.NotImplementedException();
        public void Send(string eventName, System.Collections.Generic.Dictionary<string, object> data) => throw new System.NotImplementedException();
        public void Flush() => throw new System.NotImplementedException();
    }

#else

    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Unity.Services.Analytics;
    using Unity.Services.Authentication;
    using Unity.Services.Core;
    using UnityEngine;
    using UnityEngine.UnityConsent;

    public class UnityAnalyticsProvider : GameBehavior, IAnalyticsProvider
    {
        [SerializeField] private bool dontSendEventsInEditor = true;

        private static readonly OGTLogger logger = new OGTLogger("Analytics");

        private PropertyInfo isInitializedProperty;
        private bool isInitialized;

        public bool IsSupported => this.enabled;

        public async Task Initialize()
        {
            if (this.isInitialized)
            {
                return;
            }

            try
            {
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    await UnityServices.InitializeAsync();
                }

                if (AuthenticationService.Instance.IsSignedIn == false)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                EndUserConsent.SetConsentState(new ConsentState
                {
                    AnalyticsIntent = ConsentStatus.Granted,
                    AdsIntent = ConsentStatus.Granted,
                });

                this.isInitialized = true;

                logger.Log("Unity Gaming Services Analytics initialized.");
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to initialize Unity Analytics Provider: {ex}");
            }
        }

        public void Send(string eventName, Dictionary<string, object> data)
        {
            if (this.isInitialized == false || string.IsNullOrWhiteSpace(eventName))
            {
                return;
            }

            if (this.dontSendEventsInEditor && Application.isEditor)
            {
                return;
            }

            try
            {
                var analyticEvent = new CustomEvent(eventName);

                foreach (var kvp in data ?? new Dictionary<string, object>())
                {
                    analyticEvent.Add(kvp.Key, kvp.Value);
                }

                AnalyticsService.Instance.RecordEvent(analyticEvent);
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to send analytics event '{eventName}': {ex}");
            }
        }

        public void Flush()
        {
            if (this.isInitialized == false)
            {
                return;
            }

            try
            {
                if (this.isInitializedProperty == null)
                {
                    this.isInitializedProperty = typeof(AnalyticsService).GetProperty("IsInitialized", BindingFlags.Static | BindingFlags.NonPublic);
                }

                bool analyticsIsInitialized = (bool?)this.isInitializedProperty?.GetValue(null) == true;

                if (analyticsIsInitialized)
                {
                    AnalyticsService.Instance.Flush();
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to flush analytics events: {ex}");
            }
        }
    }

#endif
}
