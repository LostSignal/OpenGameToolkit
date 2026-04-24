//-----------------------------------------------------------------------
// <copyright file="Position3D.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    public struct Position3D
    {
        public static readonly Position3D Zero = new Position3D(0.0f, 0.0f, 0.0f);

        public float X;
        public float Y;
        public float Z;

        public Position3D(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }
}
