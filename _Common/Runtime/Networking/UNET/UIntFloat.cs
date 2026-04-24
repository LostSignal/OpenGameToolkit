//-----------------------------------------------------------------------
// <copyright file="UIntFloat.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    using System.Runtime.InteropServices;

    // -- helpers for float conversion --
    [StructLayout(LayoutKind.Explicit)]
    internal struct UIntFloat
    {
        [FieldOffset(0)]
        public float FloatValue;

        [FieldOffset(0)]
        public uint IntValue;

        [FieldOffset(0)]
        public double DoubleValue;

        [FieldOffset(0)]
        public ulong LongValue;
    }
}
