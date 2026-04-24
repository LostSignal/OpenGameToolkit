//-----------------------------------------------------------------------
// <copyright file="FacebookException.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY || USING_PLAYFAB

namespace OGT.PlayFab
{
    using System;

    public class FacebookException : Exception
    {
        public FacebookException()
        {
        }

        public FacebookException(string error)
            : base(error)
        {
            this.Error = error;
        }

        public FacebookException(string error, Exception innerException)
            : base(error, innerException)
        {
            this.Error = error;
        }

        public string Error { get; private set; }
    }
}

#endif
