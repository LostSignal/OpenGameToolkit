//-----------------------------------------------------------------------
// <copyright file="ResultT.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !UNITY || USING_PLAYFAB

namespace Lost.CloudFunctions
{
    using System;

    public class ResultT<T> : Result
    {
        public T ResultObject { get; set; }

        public static ResultT<T> Ok(T resultObject)
        {
            return new ResultT<T>
            {
                Success = true,
                Exception = null,
                ResultObject = resultObject,
            };
        }

        public static new ResultT<T> Failure(Exception ex)
        {
            return new ResultT<T>
            {
                Success = false,
                Exception = ex,
                ResultObject = default,
            };
        }
    }
}

#endif
