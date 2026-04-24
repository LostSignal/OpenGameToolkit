//-----------------------------------------------------------------------
// <copyright file="AuthCodeLoginResult.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY || USING_PLAYFAB

namespace Lost.CloudFunctions.Login
{
    public class AuthCodeLoginResult
    {
        public string ClientSessionsString { get; set; }
    }
}

#endif
