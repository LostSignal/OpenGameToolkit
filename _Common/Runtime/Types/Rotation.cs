//-----------------------------------------------------------------------
// <copyright file="Rotation.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    public struct Rotation
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public Rotation(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }
    }
}
