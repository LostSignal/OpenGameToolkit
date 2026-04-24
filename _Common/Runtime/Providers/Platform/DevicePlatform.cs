//-----------------------------------------------------------------------
// <copyright file="DevicePlatform.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Diagnostics.CodeAnalysis;

    [System.Flags]
    public enum DevicePlatform
    {
        None = 0,
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Doesn't make sense for platforms that are suppose to start with lowercase letters.")]
        iOS = 1 << 0,
        Android = 1 << 1,
        Windows = 1 << 2,
        Mac = 1 << 3,
        Linux = 1 << 4,
        WindowsUniversal = 1 << 5,
        XboxOne = 1 << 6,
        WebGL = 1 << 7,
        MagicLeap = 1 << 8,
        XboxSeries = 1 << 9,
        PS4 = 1 << 10,
        PS5 = 1 << 11,
    }
}
