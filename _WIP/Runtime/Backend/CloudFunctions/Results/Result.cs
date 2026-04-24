//-----------------------------------------------------------------------
// <copyright file="Result.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY || USING_PLAYFAB

namespace Lost.CloudFunctions
{
    using System;

    public class Result
    {
        protected Result()
        {
        }

        public bool Success { get; set; }

        public Exception Exception { get; set; }

        public static Result Ok()
        {
            return new Result
            {
                Success = true,
                Exception = null,
            };
        }

        public static Result Failure(Exception ex)
        {
            return new Result
            {
                Success = false,
                Exception = ex,
            };
        }
    }
}

#endif
