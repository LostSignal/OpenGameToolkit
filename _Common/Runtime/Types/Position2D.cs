//-----------------------------------------------------------------------
// <copyright file="Position2D.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    public struct Position2D
    {
        public static readonly Position2D Zero = new Position2D(0.0f, 0.0f);

        public float X;
        public float Y;

        public Position2D(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
