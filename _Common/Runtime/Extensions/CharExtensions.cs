#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="CharExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    public static class CharExtensions
    {
        public static bool IsDigit(this char c)
        {
            return c >= '0' && c <= '9';
        }
    }
}
