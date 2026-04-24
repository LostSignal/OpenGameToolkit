//-----------------------------------------------------------------------
// <copyright file="PurchasingInitializationException.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY && USING_UNITY_PURCHASING && !UNITY_XBOXONE && !UNITY_LUMIN
#define PURCHASING_ENABLED
#endif

#if PURCHASING_ENABLED

// Implement standard exception constructors (Ignore this so the user is force to give a InitializationFailureReason)
#pragma warning disable CA1032

namespace OGT
{
    using System;
    using UnityEngine.Purchasing;

    public class PurchasingInitializationException : Exception
    {
        public PurchasingInitializationException(InitializationFailureReason failureReason)
            : base(failureReason.ToString())
        {
            this.FailureReason = failureReason;
        }

        public PurchasingInitializationException(InitializationFailureReason failureReason, Exception innerException)
            : base(failureReason.ToString(), innerException)
        {
            this.FailureReason = failureReason;
        }

        public InitializationFailureReason FailureReason { get; private set; }
    }
}

#endif
