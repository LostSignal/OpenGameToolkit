//-----------------------------------------------------------------------
// <copyright file="AnalyticsManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class AnalyticsManager : Manager
    {
        private static readonly OGTLogger logger = new OGTLogger("Analytics");

        private List<IAnalyticsProvider> analyticsProviders = new List<IAnalyticsProvider>();
        private string cachedAppVersion;
        private string cachedPlatform;
        private string cachedDeviceModel;
        private string cachedDeviceType;
        private string cachedOperatingSystem;
        private string cachedSystemLanguage;
        private bool cachedIsEditor;

        protected override async Task InitializeManager(Bootloader bootloader)
        {
            // Caching these values so that we don't have to call them every time we send an event.
            this.cachedIsEditor = Platform.IsEditor;

            //// TODO [bgish]: Move all these to the Platform class so we don't have Unity specific code in here.
            this.cachedAppVersion = UnityEngine.Application.version;
            this.cachedPlatform = UnityEngine.Application.platform.ToString();
            this.cachedDeviceModel = UnityEngine.SystemInfo.deviceModel;
            this.cachedDeviceType = UnityEngine.SystemInfo.deviceType.ToString();
            this.cachedOperatingSystem = UnityEngine.SystemInfo.operatingSystem;
            this.cachedSystemLanguage = UnityEngine.Application.systemLanguage.ToString();

            foreach (var provider in this.GetChildrenOfType<IAnalyticsProvider>())
            {
                if (provider.IsSupported == false)
                {
                    logger.Log($"Skipping Analytics Provider: {provider.GetType().Name} (Not Supported)");
                    continue;
                }

                logger.Log($"Initializing Analytics Provider: {provider.GetType().Name}");
                await provider.Initialize();

                this.analyticsProviders.Add(provider);
            }

            logger.Log("AnalyticsManager Initialized");
        }

        //// TODO [bgish]: This method needs to copy the data and store it in a queue to be processed.
        ////               Processing should be immediately, but if there is no internet, then it should wait
        ////               for it to reach a certain size and then save to disk till internet is restored.
        ////               Also, if the manager has not finished initializing, it should also be batching
        ////               these and not sending them until it's fully initialized.
        public void Send(string eventName, Dictionary<string, object> data)
        {
            var eventData = this.BuildEventData(data);

            if (logger.IsLoggingEnabled)
            {
                logger.Log($"Sending Event '{eventName}': {JsonUtil.Serialize(eventData)}");
            }

            foreach (var provider in this.analyticsProviders)
            {
                provider.Send(eventName, eventData);
            }
        }

        private Dictionary<string, object> BuildEventData(Dictionary<string, object> data)
        {
            var eventData = data != null
                ? new Dictionary<string, object>(data)
                : new Dictionary<string, object>();

            this.AddBaseEventValue(eventData, "app_version", this.cachedAppVersion);
            this.AddBaseEventValue(eventData, "platform", this.cachedPlatform);
            this.AddBaseEventValue(eventData, "device_model", this.cachedDeviceModel);
            this.AddBaseEventValue(eventData, "device_type", this.cachedDeviceType);
            this.AddBaseEventValue(eventData, "operating_system", this.cachedOperatingSystem);
            this.AddBaseEventValue(eventData, "system_language", this.cachedSystemLanguage);
            this.AddBaseEventValue(eventData, "is_editor", this.cachedIsEditor);
            this.AddBaseEventValue(eventData, "timestamp_utc", DateTime.UtcNow.ToString("O"));

            return eventData;
        }

        private void AddBaseEventValue(Dictionary<string, object> eventData, string key, object value)
        {
            if (eventData.ContainsKey(key) == false)
            {
                eventData[key] = value;
            }
        }

        public override void OnManagerDestroyed()
        {
            base.OnManagerDestroyed();

            foreach (var provider in this.analyticsProviders)
            {
                provider.Flush();
            }
        }
    }
}
