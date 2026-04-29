//-----------------------------------------------------------------------
// <copyright file="ColorUtil.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.Text;

    public static class ColorUtil
    {
        private static readonly Dictionary<char, int> HexToDecimal = new()
        {
            { '0', 0 }, { '1', 1 }, { '2', 2 }, { '3', 3 }, { '4', 4 },
            { '5', 5 }, { '6', 6 }, { '7', 7 }, { '8', 8 }, { '9', 9 },
            { 'a', 10 }, { 'A', 10 },
            { 'b', 11 }, { 'B', 11 },
            { 'c', 12 }, { 'C', 12 },
            { 'd', 13 }, { 'D', 13 },
            { 'e', 14 }, { 'E', 14 },
            { 'f', 15 }, { 'F', 15 },
        };

        private static readonly char[] DecimalToHex = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        public static RGBA ParseColorHexString(string colorHexString)
        {
            float r = 1.0f;
            float g = 1.0f;
            float b = 1.0f;
            float a = 1.0f;

            if (colorHexString.Length == 3)
            {
                r = HexToDecimal[colorHexString[0]] << 4 | HexToDecimal[colorHexString[0]];
                g = HexToDecimal[colorHexString[1]] << 4 | HexToDecimal[colorHexString[1]];
                b = HexToDecimal[colorHexString[2]] << 4 | HexToDecimal[colorHexString[2]];
            }
            else if (colorHexString.Length == 6)
            {
                r = HexToDecimal[colorHexString[0]] << 4 | HexToDecimal[colorHexString[1]];
                g = HexToDecimal[colorHexString[2]] << 4 | HexToDecimal[colorHexString[3]];
                b = HexToDecimal[colorHexString[4]] << 4 | HexToDecimal[colorHexString[5]];
            }
            else if (colorHexString.Length == 8)
            {
                r = HexToDecimal[colorHexString[0]] << 4 | HexToDecimal[colorHexString[1]];
                g = HexToDecimal[colorHexString[2]] << 4 | HexToDecimal[colorHexString[3]];
                b = HexToDecimal[colorHexString[4]] << 4 | HexToDecimal[colorHexString[5]];
                a = HexToDecimal[colorHexString[6]] << 4 | HexToDecimal[colorHexString[7]];
            }

            return new RGBA(r, g, b, a);
        }

        public static string ConvertToHexString(RGBA color)
        {
            int r = (int)(color.R * 255.0);
            int g = (int)(color.G * 255.0);
            int b = (int)(color.B * 255.0);
            int a = (int)(color.A * 255.0);

            if (color.A == 1.0f)
            {
                var builder = new StringBuilder(7);

                return builder
                    .Append(DecimalToHex[r >> 4])
                    .Append(DecimalToHex[r & 15])
                    .Append(DecimalToHex[g >> 4])
                    .Append(DecimalToHex[g & 15])
                    .Append(DecimalToHex[b >> 4])
                    .Append(DecimalToHex[b & 15])
                    .ToString();
            }
            else
            {
                var builder = new StringBuilder(9);

                return builder
                    .Append(DecimalToHex[r >> 4])
                    .Append(DecimalToHex[r & 15])
                    .Append(DecimalToHex[g >> 4])
                    .Append(DecimalToHex[g & 15])
                    .Append(DecimalToHex[b >> 4])
                    .Append(DecimalToHex[b & 15])
                    .Append(DecimalToHex[a >> 4])
                    .Append(DecimalToHex[a & 15])
                    .ToString();
            }
        }
    }
}
