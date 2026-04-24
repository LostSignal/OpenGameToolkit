//-----------------------------------------------------------------------
// <copyright file="UnityTaskCanceledException.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;

    public class UnityTaskCanceledException : Exception
    {
        public UnityTaskCanceledException()
            : base()
        {
        }

        public UnityTaskCanceledException(string message)
            : base(message)
        {
        }

        public UnityTaskCanceledException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
