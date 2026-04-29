//-----------------------------------------------------------------------
// <copyright file="BetterStringBuilderExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    public static class BetterStringBuilderExtensions
    {
        public static void Set(this BetterStringBuilder builder, TMPro.TMP_Text text)
        {
            text.SetCharArray(builder.CurrentCharBuffer, 0, builder.CurrentCharBufferLength);
        }
    }
}
