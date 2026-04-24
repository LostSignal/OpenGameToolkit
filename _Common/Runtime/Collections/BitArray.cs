//-----------------------------------------------------------------------
// <copyright file="BitArray.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Text;
    using UnityEngine;

    [Serializable]
    public class BitArray
    {
        public const string GrowError = "BitArray had to grow in size to accomidate incoming bits.";
        public const int NumberOfBits = sizeof(byte) * 8;
        public const int MaxBitIndex = 1000;
        public const int MaxArrayLength = (MaxBitIndex / NumberOfBits) + 1;

        private static readonly OGTLogger Logger = OGTLogger.OGT;

        #pragma warning disable 0649
        [SerializeField] private byte[] bits;
        #pragma warning restore 0649

        public BitArray()
        {
        }

        public BitArray(int maxBitIndex)
        {
            this.SetCapacity(maxBitIndex);
        }

        public void SetCapacity(int maxBitIndex)
        {
            if (maxBitIndex > MaxBitIndex)
            {
                Logger.LogError($"BitArray.SetCapacity Failed, maxBitIndex {maxBitIndex} exceeds MaxBitIndex {MaxBitIndex}!");
                return;
            }

            int arrayLength = (maxBitIndex / NumberOfBits) + 1;
            this.EnsureArraySize(arrayLength, true);
        }

        public void Clear()
        {
            if (this.bits != null)
            {
                for (int i = 0; i < this.bits.Length; i++)
                {
                    this.bits[i] = 0;
                }
            }
        }

        public bool IsEmpty()
        {
            if (this.bits != null)
            {
                for (int i = 0; i < this.bits.Length; i++)
                {
                    if (this.bits[i] != 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void SetBits(byte[] newBits)
        {
            if (newBits == null)
            {
                return;
            }

            if (newBits.Length > MaxArrayLength)
            {
                Logger.LogError($"BitArray.SetBits Failed, newBits length {newBits.Length} exceeds MaxArrayLength {MaxArrayLength}!");
                return;
            }

            this.EnsureArraySize(newBits.Length);

            for (int i = 0; i < newBits.Length; i++)
            {
                this.bits[i] = newBits[i];
            }
        }

        public bool IsBitSet(int bitIndex)
        {
            if (this.bits == null)
            {
                return false;
            }

            int arrayIndex = bitIndex / NumberOfBits;

            if (arrayIndex >= this.bits.Length)
            {
                return false;
            }
            else
            {
                int bitToCheck = 1 << (bitIndex % NumberOfBits);
                return (this.bits[arrayIndex] & bitToCheck) != 0;
            }
        }

        public void SetBit(int bitIndex)
        {
            if (bitIndex < 0)
            {
                throw new ArgumentException("BitArray can not set negative bit indices");
            }

            if (bitIndex > MaxBitIndex)
            {
                Logger.LogError($"BitArray.SetBit Failed, bitIndex {bitIndex} exceeds MaxBitIndex {MaxBitIndex}!");
                return;
            }

            int index = bitIndex / NumberOfBits;

            this.EnsureArraySize(index + 1);

            if (index < this.bits.Length)
            {
                int bitToSet = 1 << (bitIndex % NumberOfBits);
                this.bits[index] = (byte)(this.bits[index] | bitToSet);
            }
            else
            {
                Logger.LogError("BitArray.SetBit was unable to set the bit because the index is greater than the length!");
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            for (int i = 0; i < this.bits.Length; i++)
            {
                string binary = Convert.ToString(this.bits[i], 2);

                for (int j = binary.Length - 1; j >= 0; j--)
                {
                    builder.Append(binary[j]);
                }

                for (int k = 0; k < 8 - binary.Length; k++)
                {
                    builder.Append("0");
                }

                if (i != this.bits.Length - 1)
                {
                    builder.Append(" ");
                }
            }

            return builder.ToString();
        }

        public byte[] ToArray() => this.bits;

        private void EnsureArraySize(int neededLength, bool suppressErrors = false)
        {
            bool didGrow = false;

            if (this.bits == null)
            {
                didGrow = true;
                this.bits = new byte[neededLength];
            }
            else if (this.bits.Length < neededLength)
            {
                didGrow = true;
                Array.Resize(ref this.bits, neededLength);
            }

            if (didGrow && suppressErrors == false)
            {
                Logger.LogError(GrowError);
            }
        }
    }
}
