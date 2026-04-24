//-----------------------------------------------------------------------
// <copyright file="ILoggingProvider.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    public interface ILoggingProvider
    {
        void Log(LoggingChannel channel, LoggingLevel level, object context, string message);

        void LogException(LoggingChannel channel, object context, System.Exception exception);
    }
}
