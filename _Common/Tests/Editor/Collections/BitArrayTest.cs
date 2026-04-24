//-----------------------------------------------------------------------
// <copyright file="BitArrayTest.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.TestTools;
    using OGT;

    public class BitArrayTest
    {
        [Test]
        public void DuplicateAdd()
        {
            BitArray bitArray = new (0);
            bitArray.SetBit(0);
            bitArray.SetBit(0);
            bitArray.SetBit(0);
            bitArray.SetBit(0);
            bitArray.SetBit(0);

            Assert.True(bitArray.ToString() == "10000000");
            Assert.True(bitArray.IsBitSet(0));
            Assert.False(bitArray.IsBitSet(1));
        }

        [Test]
        public void MaxBitTest()
        {
            int max = 1000;

            BitArray bitArray1 = new (0);
            bitArray1.SetCapacity(max);

            BitArray bitArray2 = new (0);

            LogAssert.Expect(LogType.Error, BitArray.GrowError);
            bitArray2.SetBit(max);
            LogAssert.NoUnexpectedReceived();

            Assert.True(bitArray2.IsBitSet(max));
        }

        [Test]
        public void MaxBitExceededTest()
        {
            int max = 1001;

            LogAssert.Expect(LogType.Error, "BitArray.SetCapacity Failed, maxBitIndex 1001 exceeds MaxBitIndex 1000!");
            BitArray bitArray1 = new (0);
            bitArray1.SetCapacity(max);
            LogAssert.NoUnexpectedReceived();

            BitArray bitArray2 = new (0);

            LogAssert.Expect(LogType.Error, "BitArray.SetBit Failed, bitIndex 1001 exceeds MaxBitIndex 1000!");
            bitArray2.SetBit(max);
            LogAssert.NoUnexpectedReceived();

            Assert.False(bitArray2.IsBitSet(max));
        }

        [Test]
        public void TestSettingAndGrowing()
        {
            BitArray bitArray = new ();
            bitArray.SetCapacity(7);

            Assert.False(bitArray.IsBitSet(0));
            Assert.False(bitArray.IsBitSet(1));
            Assert.False(bitArray.IsBitSet(2));
            Assert.False(bitArray.IsBitSet(3));
            Assert.False(bitArray.IsBitSet(4));
            Assert.False(bitArray.IsBitSet(5));
            Assert.False(bitArray.IsBitSet(6));
            Assert.False(bitArray.IsBitSet(7));
            Assert.False(bitArray.IsBitSet(8));

            bitArray.SetBit(0);
            Assert.True(bitArray.ToString() == "10000000");

            bitArray.SetBit(1);
            Assert.True(bitArray.ToString() == "11000000");

            bitArray.SetBit(2);
            Assert.True(bitArray.ToString() == "11100000");

            bitArray.SetBit(3);
            Assert.True(bitArray.ToString() == "11110000");

            bitArray.SetBit(4);
            Assert.True(bitArray.ToString() == "11111000");

            bitArray.SetBit(5);
            Assert.True(bitArray.ToString() == "11111100");

            bitArray.SetBit(6);
            Assert.True(bitArray.ToString() == "11111110");

            bitArray.SetBit(7);
            Assert.True(bitArray.ToString() == "11111111");

            LogAssert.Expect(LogType.Error, BitArray.GrowError);
            bitArray.SetBit(8);
            Assert.True(bitArray.ToString() == "11111111 10000000");
            LogAssert.NoUnexpectedReceived();

            Assert.True(bitArray.IsBitSet(0));
            Assert.True(bitArray.IsBitSet(1));
            Assert.True(bitArray.IsBitSet(2));
            Assert.True(bitArray.IsBitSet(3));
            Assert.True(bitArray.IsBitSet(4));
            Assert.True(bitArray.IsBitSet(5));
            Assert.True(bitArray.IsBitSet(6));
            Assert.True(bitArray.IsBitSet(7));
            Assert.True(bitArray.IsBitSet(8));

            bitArray.Clear();

            Assert.False(bitArray.IsBitSet(0));
            Assert.False(bitArray.IsBitSet(1));
            Assert.False(bitArray.IsBitSet(2));
            Assert.False(bitArray.IsBitSet(3));
            Assert.False(bitArray.IsBitSet(4));
            Assert.False(bitArray.IsBitSet(5));
            Assert.False(bitArray.IsBitSet(6));
            Assert.False(bitArray.IsBitSet(7));
            Assert.False(bitArray.IsBitSet(8));
        }

        [Test]
        public void TestGrowingError()
        {
            BitArray bitArray = new ();
            bitArray.SetCapacity(0);

            LogAssert.Expect(LogType.Error, BitArray.GrowError);
            bitArray.SetBit(8);
            LogAssert.NoUnexpectedReceived();

            Assert.True(bitArray.ToString() == "00000000 10000000");
            Assert.True(bitArray.IsBitSet(8));
        }

        [Test]
        public void AddingNegative()
        {
            BitArray bitArray = new ();
            Assert.Throws<ArgumentException>(() => { bitArray.SetBit(-1); });
        }
    }
}
