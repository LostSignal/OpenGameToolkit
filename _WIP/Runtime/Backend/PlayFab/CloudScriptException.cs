//-----------------------------------------------------------------------
// <copyright file="CloudScriptException.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY || USING_PLAYFAB

namespace OGT.PlayFab
{
    using System;

    public class CloudScriptException : Exception
    {
        public CloudScriptException()
        {
        }

        public CloudScriptException(string cloudScriptError)
            : base(cloudScriptError)
        {
            this.CloudScriptError = cloudScriptError;
        }

        public CloudScriptException(string cloudScriptError, Exception innerException)
            : base(cloudScriptError, innerException)
        {
            this.CloudScriptError = cloudScriptError;
        }

        public string CloudScriptError { get; private set; }
    }
}

#endif
