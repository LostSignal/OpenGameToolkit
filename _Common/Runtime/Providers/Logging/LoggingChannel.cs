//-----------------------------------------------------------------------
// <copyright file="LoggingChannel.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    public class LoggingChannel
    {
        public string Name { get; set; }

        public int Hash { get; set; }

        public LoggingLevel Level { get; set; }
    }
}
