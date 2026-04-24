#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="ColorExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using OGT;
    using UnityEngine;

    public static class ColorExtensions
    {
        public static Color ToUnityColor(this RGBA c)
        {
            return new Color(c.R, c.G, c.B, c.A);
        }

        public static Color SetR(this Color lhs, float val)
        {
            lhs.r = val;
            return lhs;
        }

        public static Color SetG(this Color lhs, float val)
        {
            lhs.g = val;
            return lhs;
        }

        public static Color SetB(this Color lhs, float val)
        {
            lhs.b = val;
            return lhs;
        }

        public static Color SetA(this Color lhs, float val)
        {
            lhs.a = val;
            return lhs;
        }

        public static Color OffsetR(this Color lhs, float val)
        {
            lhs.r += val;
            return lhs;
        }

        public static Color OffsetG(this Color lhs, float val)
        {
            lhs.g += val;
            return lhs;
        }

        public static Color OffsetB(this Color lhs, float val)
        {
            lhs.b += val;
            return lhs;
        }

        public static Color OffsetA(this Color lhs, float val)
        {
            lhs.a += val;
            return lhs;
        }
    }
}
