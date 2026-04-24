//-----------------------------------------------------------------------
// <copyright file="PurchasingInitializationTimeOutException.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY && USING_UNITY_PURCHASING && !UNITY_XBOXONE && !UNITY_LUMIN
#define PURCHASING_ENABLED
#endif

#if PURCHASING_ENABLED

namespace OGT
{
    using System;

    public class PurchasingInitializationTimeOutException : Exception
    {
        public PurchasingInitializationTimeOutException()
            : base()
        {
        }

        public PurchasingInitializationTimeOutException(string message)
            : base(message)
        {
        }

        public PurchasingInitializationTimeOutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

#endif
