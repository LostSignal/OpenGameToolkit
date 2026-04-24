//-----------------------------------------------------------------------
// <copyright file="PlayFabException.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY || USING_PLAYFAB

// Implement standard exception constructors (Ignore this so the user is force to give a PlayFabError)
#pragma warning disable CA1032

namespace OGT.PlayFab
{
    using System;
    using global::PlayFab;

    public class PlayFabException : Exception
    {
        public PlayFabException()
        {
        }

        public PlayFabException(PlayFabError error)
            : base(error.ToString())
        {
            this.Error = error;
        }

        public PlayFabException(PlayFabError error, Exception innerException)
            : base(error.ToString(), innerException)
        {
            this.Error = error;
        }

        public PlayFabError Error { get; private set; }
    }
}

#endif
