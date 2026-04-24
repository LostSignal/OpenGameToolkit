//-----------------------------------------------------------------------
// <copyright file="FloatConversion.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    internal class FloatConversion
    {
        public static float ToSingle(uint value)
        {
            UIntFloat uf = default;
            uf.IntValue = value;
            return uf.FloatValue;
        }

        public static double ToDouble(ulong value)
        {
            UIntFloat uf = default;
            uf.LongValue = value;
            return uf.DoubleValue;
        }

        public static decimal ToDecimal(ulong value1, ulong value2)
        {
            UIntDecimal uf = default;
            uf.LongValue1 = value1;
            uf.LongValue2 = value2;
            return uf.DecimalValue;
        }
    }
}
