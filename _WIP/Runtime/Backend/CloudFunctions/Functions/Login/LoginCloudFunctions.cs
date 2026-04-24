//-----------------------------------------------------------------------
// <copyright file="LoginCloudFunctions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY

namespace Lost.CloudFunctions.Login
{
    using System;
    using System.Threading.Tasks;

    public static class LoginCloudFunctions
    {
        [AnonymousCloudFunction("Login", "RequestLogin")]
        public static Task RequestLogin(CloudFunctionContext context, string email)
        {
            throw new NotImplementedException();
        }

        [AnonymousCloudFunction("Login", "AuthCodeLogin")]
        public static Task<AuthCodeLoginResult> AuthCodeLogin(CloudFunctionContext context, AuthCodeLoginRequest request)
        {
            throw new NotImplementedException();
        }
    }
}

#endif
