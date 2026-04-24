//-----------------------------------------------------------------------
// <copyright file="PurchasingException.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY && USING_UNITY_PURCHASING && !UNITY_XBOXONE && !UNITY_LUMIN
#define PURCHASING_ENABLED
#endif

#if PURCHASING_ENABLED

// Implement standard exception constructors (Ignore this so the user is force to give a PurchaseFailureReason)
#pragma warning disable CA1032

namespace OGT
{
    using System;
    using UnityEngine.Purchasing;

    public class PurchasingException : Exception
    {
        public PurchasingException()
        {
        }

        public PurchasingException(PurchaseFailureReason failureReason)
            : base(failureReason.ToString())
        {
            this.FailureReason = failureReason;
        }

        public PurchasingException(PurchaseFailureReason failureReason, Exception innerException)
            : base(failureReason.ToString(), innerException)
        {
            this.FailureReason = failureReason;
        }

        public PurchaseFailureReason FailureReason { get; private set; }
    }
}

#endif
