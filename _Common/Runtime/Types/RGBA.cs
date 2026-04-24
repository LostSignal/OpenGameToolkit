//-----------------------------------------------------------------------
// <copyright file="RGBA.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    public struct RGBA
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public RGBA(float r, float g, float b, float a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }
    }
}
