//-----------------------------------------------------------------------
// <copyright file="UIntDecimal.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    internal struct UIntDecimal
    {
        [FieldOffset(0)]
        public ulong LongValue1;

        [FieldOffset(8)]
        public ulong LongValue2;

        [FieldOffset(0)]
        public decimal DecimalValue;
    }
}
