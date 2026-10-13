//-----------------------------------------------------------------------
// <copyright file="UnityLoggingProvider.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class UnityLoggingProvider : ILoggingProvider
    {
        private const string LoggingChannelsSettingsName = "OGT.LoggingChannels";

        public UnityLoggingProvider()
        {
            var channels = RuntimeSettings.GetSetting<List<Channel>>(LoggingChannelsSettingsName) ?? new List<Channel>
            {
                new Channel { Name = "Validation", Level = LoggingLevel.Warning },
                new Channel { Name = "Editor Events", Level = LoggingLevel.Warning },
            };

            channels.ForEach(x => OGTLogger.SetLoggingLevel(x.Name, x.Level));
        }

        [EditorEvents.OnEnterPlayMode]
        [EditorEvents.OnExitPlayMode]
        [EditorEvents.OnExitEditor]
        public static void SaveChannels()
        {
            RuntimeSettings.SetSetting(LoggingChannelsSettingsName, OGTLogger.GetChannels().Select(x => new Channel { Name = x.Name, Level = x.Level }).OrderBy(x => x.Name).ToList());
        }

        public static List<Channel> GetSavedChannels()
        {
            return RuntimeSettings.GetSetting<List<Channel>>(LoggingChannelsSettingsName) ?? new List<Channel>();
        }

        public static void SetSavedChannelLevel(string channelName, LoggingLevel level)
        {
            OGTLogger.SetLoggingLevel(channelName, level);

            var channels = GetSavedChannels();
            var channel = channels.FirstOrDefault(x => x.Name == channelName);

            if (channel == null)
            {
                channels.Add(new Channel { Name = channelName, Level = level });
                channels = channels.OrderBy(x => x.Name).ToList();
            }
            else
            {
                channel.Level = level;
            }

            RuntimeSettings.SetSetting(LoggingChannelsSettingsName, channels);
        }

        public void Log(LoggingChannel channel, LoggingLevel level, object context, string message)
        {
            switch (level)
            {
                case LoggingLevel.Info:
                {
                    UnityEngine.Debug.Log($"{channel.Name}: {message}", context as UnityEngine.Object);
                    break;
                }

                case LoggingLevel.Warning:
                {
                    UnityEngine.Debug.LogWarning($"{channel.Name}: {message}", context as UnityEngine.Object);
                    break;
                }

                case LoggingLevel.Error:
                {
                    UnityEngine.Debug.LogError($"{channel.Name}: {message}", context as UnityEngine.Object);
                    break;
                }

                case LoggingLevel.Assert:
                {
                    UnityEngine.Debug.LogAssertion($"{channel.Name}: {message}", context as UnityEngine.Object);
                    break;
                }

                default:
                    //// TODO [bgish]: Log an Error!
                    break;
            }
        }

        public void LogException(LoggingChannel channel, object context, System.Exception exception)
        {
            Debug.LogException(exception, context as UnityEngine.Object);
        }

        public class Channel
        {
            public string Name { get; set; }

            public LoggingLevel Level { get; set; }
        }
    }
}
