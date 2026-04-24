//-----------------------------------------------------------------------
// <copyright file="PlayFabCloudScriptException.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY || USING_PLAYFAB

namespace OGT.PlayFab
{
    using System;

    public class PlayFabCloudScriptException : Exception
    {
        public PlayFabCloudScriptException()
        {
        }

        public PlayFabCloudScriptException(string error)
            : base(error)
        {
            this.Error = error;
        }

        public PlayFabCloudScriptException(string error, Exception innerException)
            : base(error, innerException)
        {
            this.Error = error;
        }

        public string Error { get; private set; }
    }
}

#endif
