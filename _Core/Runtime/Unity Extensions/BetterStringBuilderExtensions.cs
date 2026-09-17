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

        public static BetterStringBuilder AppendColorAsHex(this BetterStringBuilder builder, UnityEngine.Color color)
        {
            int r = (int)(color.r * 255.0);
            int g = (int)(color.g * 255.0);
            int b = (int)(color.b * 255.0);
            int a = (int)(color.a * 255.0);

            return builder
                .Append(ColorUtil.DecimalToHex[r >> 4])
                .Append(ColorUtil.DecimalToHex[r & 15])
                .Append(ColorUtil.DecimalToHex[g >> 4])
                .Append(ColorUtil.DecimalToHex[g & 15])
                .Append(ColorUtil.DecimalToHex[b >> 4])
                .Append(ColorUtil.DecimalToHex[b & 15])
                .Append(ColorUtil.DecimalToHex[a >> 4])
                .Append(ColorUtil.DecimalToHex[a & 15]);
        }
    }
}
