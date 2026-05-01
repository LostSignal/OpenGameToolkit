//-----------------------------------------------------------------------
// <copyright file="OGTLogger.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class OGTLogger
    {
        private static readonly Dictionary<int, LoggingChannel> Channels = new Dictionary<int, LoggingChannel>();
        private static readonly List<ILoggingProvider> Providers = new List<ILoggingProvider>();
        private static LoggingLevel DefaultLoggingLevel = LoggingLevel.Info;

        public static readonly OGTLogger OGT = new("OGT");
        public static readonly OGTLogger OGTEditor = new("OGT Editor");
        public static readonly OGTLogger Audio = new("Audio");
        public static readonly OGTLogger Bootloader = new("Bootloader");
        public static readonly OGTLogger Validation = new("Validation");
        public static readonly OGTLogger EditorEvents = new("Editor Events");
        public static readonly OGTLogger Networking = new("Networking");

        private string channelName;
        private int channelHash;

#if UNITY_6000_0_OR_NEWER
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => DefaultLoggingLevel = LoggingLevel.Info;
#endif

        public OGTLogger(string channelName)
        {
            this.channelName = channelName;
            this.channelHash = this.channelName.GetHashCode();

            GetOrCreateLoggingChannel(this.channelName, this.channelHash, DefaultLoggingLevel);
        }

        public static IEnumerable<LoggingChannel> GetChannels()
        {
            foreach (var channel in Channels)
            {
                yield return channel.Value;
            }
        }

        public static LoggingLevel GetLoggingLevel(string channelName)
        {
            return Channels.TryGetValue(channelName.GetHashCode(), out LoggingChannel channel) ? channel.Level : DefaultLoggingLevel;
        }

        public static void SetDefaultLoggingLevel(LoggingLevel level)
        {
            DefaultLoggingLevel = level;
        }

        public static void SetLoggingLevel(string channelName, LoggingLevel loggingLevel)
        {
            var channel = GetOrCreateLoggingChannel(channelName, channelName.GetHashCode(), loggingLevel);
            channel.Level = loggingLevel;
        }

        public static void AddProvider(ILoggingProvider provider)
        {
            foreach (var currentProvider in Providers)
            {
                if (currentProvider.GetType() == provider.GetType())
                {
                    return;
                }
            }

            Providers.Add(provider);
        }

        public void Log(string message)
        {
            this.Log(this.channelHash, LoggingLevel.Info, null, message);
        }

        public void Log(string message, object context)
        {
            this.Log(this.channelHash, LoggingLevel.Info, context, message);
        }

        public void LogAssertion(string message, object context)
        {
            this.Log(this.channelHash, LoggingLevel.Assert, context, message);
        }

        public void LogError(string message)
        {
            this.Log(this.channelHash, LoggingLevel.Error, null, message);
        }

        public void LogError(string message, object context)
        {
            this.Log(this.channelHash, LoggingLevel.Error, context, message);
        }

        public void LogWarning(string message)
        {
            this.Log(this.channelHash, LoggingLevel.Warning, null, message);
        }

        public void LogWarning(string message, object context)
        {
            this.Log(this.channelHash, LoggingLevel.Warning, context, message);
        }

        public void Assert(bool condition, string message)
        {
            if (condition == false)
            {
                this.Log(this.channelHash, LoggingLevel.Assert, null, message);
            }
        }

        public void Assert(bool condition, string message, object context)
        {
            if (condition == false)
            {
                this.Log(this.channelHash, LoggingLevel.Assert, context, message);
            }
        }

        public void LogException(System.Exception ex)
        {
            this.LogException(this.channelHash, null, ex);
        }

        public void LogException(System.Exception ex, object context)
        {
            this.LogException(this.channelHash, context, ex);
        }

        public void LogFormat(string message, object arg1)
        {
            this.Log(this.channelHash, LoggingLevel.Info, null, message, arg1);
        }

        public void LogFormat(string message, object arg1, object arg2)
        {
            this.Log(this.channelHash, LoggingLevel.Info, null, message, arg1, arg2);
        }

        public void LogFormat(string message, object arg1, object arg2, object arg3)
        {
            this.Log(this.channelHash, LoggingLevel.Info, null, message, arg1, arg2, arg3);
        }

        public void LogFormat(string message, object arg1, object arg2, object arg3, object arg4)
        {
            this.Log(this.channelHash, LoggingLevel.Info, null, message, arg1, arg2, arg3, arg4);
        }

        public void LogFormat(object context, string message, object arg1, object arg2, object arg3, object arg4)
        {
            this.Log(this.channelHash, LoggingLevel.Info, context, message, arg1, arg2, arg3, arg4);
        }

        public void LogWarningFormat(string message, object arg1)
        {
            this.Log(this.channelHash, LoggingLevel.Warning, null, message, arg1);
        }

        public void LogWarningFormat(string message, object arg1, object arg2)
        {
            this.Log(this.channelHash, LoggingLevel.Warning, null, message, arg1, arg2);
        }

        public void LogWarningFormat(object context, string message, object arg1)
        {
            this.Log(this.channelHash, LoggingLevel.Warning, context, message, arg1);
        }

        public void LogWarningFormat(object context, string message, object arg1, object arg2)
        {
            this.Log(this.channelHash, LoggingLevel.Warning, context, message, arg1, arg2);
        }

        public void LogErrorFormat(string message, object arg1)
        {
            this.Log(this.channelHash, LoggingLevel.Error, null, message, arg1);
        }

        public void LogErrorFormat(string message, object arg1, object arg2)
        {
            this.Log(this.channelHash, LoggingLevel.Error, null, message, arg1, arg2);
        }

        public void LogErrorFormat(string message, object arg1, object arg2, object arg3)
        {
            this.Log(this.channelHash, LoggingLevel.Error, null, message, arg1, arg2, arg3);
        }

        public void LogErrorFormat(object context, string message, object arg1)
        {
            this.Log(this.channelHash, LoggingLevel.Error, context, message, arg1);
        }

        public void LogErrorFormat(object context, string message, object arg1, object arg2)
        {
            this.Log(this.channelHash, LoggingLevel.Error, context, message, arg1, arg2);
        }

        public void LogErrorFormat(object context, string message, object arg1, object arg2, object arg3)
        {
            this.Log(this.channelHash, LoggingLevel.Error, context, message, arg1, arg2, arg3);
        }

        public void AssertFormat(bool condition, string message, object arg1)
        {
            if (condition == false)
            {
                this.Log(this.channelHash, LoggingLevel.Assert, null, message, arg1);
            }
        }

        public void AssertFormat(bool condition, string message, object arg1, object arg2)
        {
            if (condition == false)
            {
                this.Log(this.channelHash, LoggingLevel.Assert, null, message, arg1, arg2);
            }
        }

        public void AssertFormat(bool condition, object context, string message, object arg1)
        {
            if (condition == false)
            {
                this.Log(this.channelHash, LoggingLevel.Assert, context, message, arg1);
            }
        }

        public void AssertFormat(bool condition, object context, string message, object arg1, object arg2)
        {
            if (condition == false)
            {
                this.Log(this.channelHash, LoggingLevel.Assert, context, message, arg1, arg2);
            }
        }

        // ----------------------------------------------------------------------

        private void Log(int channelHash, LoggingLevel level, object context, string message)
        {
            if (this.ShouldLog(channelHash, level, out LoggingChannel channel))
            {
                this.Log(channel, level, context, message);
            }
        }

        private void Log(int channelHash, LoggingLevel level, object context, string message, object param1)
        {
            if (this.ShouldLog(channelHash, level, out LoggingChannel channel))
            {
                this.Log(channel, level, context, string.Format(message, param1));
            }
        }

        private void Log(int channelHash, LoggingLevel level, object context, string message, object param1, object param2)
        {
            if (this.ShouldLog(channelHash, level, out LoggingChannel channel))
            {
                this.Log(channel, level, context, string.Format(message, param1, param2));
            }
        }

        private void Log(int channelHash, LoggingLevel level, object context, string message, object param1, object param2, object param3)
        {
            if (this.ShouldLog(channelHash, level, out LoggingChannel channel))
            {
                this.Log(channel, level, context, string.Format(message, param1, param2, param3));
            }
        }

        private void Log(int channelHash, LoggingLevel level, object context, string message, object param1, object param2, object param3, object param4)
        {
            if (this.ShouldLog(channelHash, level, out LoggingChannel channel))
            {
                this.Log(channel, level, context, string.Format(message, param1, param2, param3, param4));
            }
        }

        private void LogException(int channelHash, object context, Exception exception)
        {
            if (this.ShouldLog(channelHash, LoggingLevel.Exception, out LoggingChannel channel) == false)
            {
                return;
            }

            for (int i = 0; i < Providers.Count; i++)
            {
                try
                {
                    Providers[i].LogException(channel, context, exception);
                }
                catch
                {
                }
            }
        }

        private void Log(LoggingChannel channel, LoggingLevel level, object context, string message)
        {
            for (int i = 0; i < Providers.Count; i++)
            {
                try
                {
                    Providers[i].Log(channel, level, context, message);
                }
                catch
                {
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ShouldLog(int channelHash, LoggingLevel loggingLevel, out LoggingChannel channel)
        {
            return Channels.TryGetValue(channelHash, out channel) && loggingLevel >= channel.Level;
        }

        private static LoggingChannel GetOrCreateLoggingChannel(string channelName, int channelHash, LoggingLevel loggingLevel)
        {
            if (Channels.TryGetValue(channelHash, out LoggingChannel channel) == false)
            {
                channel = new LoggingChannel
                {
                    Name = channelName,
                    Hash = channelHash,
                    Level = loggingLevel,
                };

                Channels.Add(channelHash, channel);
            }

            return channel;
        }
    }
}
