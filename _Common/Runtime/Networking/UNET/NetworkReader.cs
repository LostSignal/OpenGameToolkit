//-----------------------------------------------------------------------
// <copyright file="NetworkReader.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// NOTE [bgish]: This code was originally from Unity's UNet code, so keeping the types they used intact
#pragma warning disable SA1121 // Use built-in type alias

namespace OGT.Networking
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;

    public class NetworkReader
    {
        private const int MaxStringLength = 1024 * 32;
        private const int InitialStringBufferSize = 1024;

        private static readonly Encoding Encoding = new UTF8Encoding();

        private static byte[] stringReaderBuffer = new byte[InitialStringBufferSize];

        private readonly NetBuffer netBuffer;

#if UNITY_6000_0_OR_NEWER
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => stringReaderBuffer = new byte[InitialStringBufferSize];
#endif

        public NetworkReader()
        {
            this.netBuffer = new NetBuffer();
        }

        public NetworkReader(NetworkWriter writer)
        {
            this.netBuffer = new NetBuffer(writer.AsArray());
        }

        public NetworkReader(byte[] buffer)
        {
            this.netBuffer = new NetBuffer(buffer);
        }

        public NetworkReader(NetBuffer netBuffer)
        {
            this.netBuffer = netBuffer;
        }

        public byte[] RawBuffer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.netBuffer.RawBuffer;
        }

        public uint Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.netBuffer.Position;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.netBuffer.Length;
        }

        public void ResetBuffer(byte[] buffer)
        {
            this.netBuffer.ResetBuffer(buffer);
        }

        public void SeekZero()
        {
            this.netBuffer.SeekZero();
        }

        //// http://sqlite.org/src4/doc/trunk/www/varint.wiki
        //// NOTE: big endian.
        public UInt32 ReadPackedUInt32()
        {
            byte a0 = this.ReadByte();
            if (a0 < 241)
            {
                return a0;
            }

            byte a1 = this.ReadByte();
            if (a0 >= 241 && a0 <= 248)
            {
                return (UInt32)(240 + (256 * (a0 - 241)) + a1);
            }

            byte a2 = this.ReadByte();
            if (a0 == 249)
            {
                return (UInt32)(2288 + (256 * a1) + a2);
            }

            byte a3 = this.ReadByte();
            if (a0 == 250)
            {
                return a1 + (((UInt32)a2) << 8) + (((UInt32)a3) << 16);
            }

            byte a4 = this.ReadByte();
            if (a0 >= 251)
            {
                return a1 + (((UInt32)a2) << 8) + (((UInt32)a3) << 16) + (((UInt32)a4) << 24);
            }

            throw new IndexOutOfRangeException("ReadPackedUInt32() failure: " + a0);
        }

        public UInt64 ReadPackedUInt64()
        {
            byte a0 = this.ReadByte();
            if (a0 < 241)
            {
                return a0;
            }

            byte a1 = this.ReadByte();
            if (a0 >= 241 && a0 <= 248)
            {
                return 240 + (256 * (a0 - ((UInt64)241))) + a1;
            }

            byte a2 = this.ReadByte();
            if (a0 == 249)
            {
                return 2288 + (((UInt64)256) * a1) + a2;
            }

            byte a3 = this.ReadByte();
            if (a0 == 250)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16);
            }

            byte a4 = this.ReadByte();
            if (a0 == 251)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24);
            }

            byte a5 = this.ReadByte();
            if (a0 == 252)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32);
            }

            byte a6 = this.ReadByte();
            if (a0 == 253)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32) + (((UInt64)a6) << 40);
            }

            byte a7 = this.ReadByte();
            if (a0 == 254)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32) + (((UInt64)a6) << 40) + (((UInt64)a7) << 48);
            }

            byte a8 = this.ReadByte();
            if (a0 == 255)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32) + (((UInt64)a6) << 40) + (((UInt64)a7) << 48) + (((UInt64)a8) << 56);
            }

            throw new IndexOutOfRangeException("ReadPackedUInt64() failure: " + a0);
        }

        public byte ReadByte()
        {
            return this.netBuffer.ReadByte();
        }

        public sbyte ReadSByte()
        {
            return (sbyte)this.netBuffer.ReadByte();
        }

        public short ReadInt16()
        {
            ushort value = 0;
            value |= this.netBuffer.ReadByte();
            value |= (ushort)(this.netBuffer.ReadByte() << 8);
            return (short)value;
        }

        public ushort ReadUInt16()
        {
            ushort value = 0;
            value |= this.netBuffer.ReadByte();
            value |= (ushort)(this.netBuffer.ReadByte() << 8);
            return value;
        }

        public int ReadInt32()
        {
            uint value = 0;
            value |= this.netBuffer.ReadByte();
            value |= (uint)(this.netBuffer.ReadByte() << 8);
            value |= (uint)(this.netBuffer.ReadByte() << 16);
            value |= (uint)(this.netBuffer.ReadByte() << 24);
            return (int)value;
        }

        public uint ReadUInt32()
        {
            uint value = 0;
            value |= this.netBuffer.ReadByte();
            value |= (uint)(this.netBuffer.ReadByte() << 8);
            value |= (uint)(this.netBuffer.ReadByte() << 16);
            value |= (uint)(this.netBuffer.ReadByte() << 24);
            return value;
        }

        public long ReadInt64()
        {
            ulong value = 0;

            ulong other = this.netBuffer.ReadByte();
            value |= other;

            other = ((ulong)this.netBuffer.ReadByte()) << 8;
            value |= other;

            other = ((ulong)this.netBuffer.ReadByte()) << 16;
            value |= other;

            other = ((ulong)this.netBuffer.ReadByte()) << 24;
            value |= other;

            other = ((ulong)this.netBuffer.ReadByte()) << 32;
            value |= other;

            other = ((ulong)this.netBuffer.ReadByte()) << 40;
            value |= other;

            other = ((ulong)this.netBuffer.ReadByte()) << 48;
            value |= other;

            other = ((ulong)this.netBuffer.ReadByte()) << 56;
            value |= other;

            return (long)value;
        }

        public ulong ReadUInt64()
        {
            ulong value = 0;
            ulong other = this.netBuffer.ReadByte();
            value |= other;

            other = ((ulong)this.netBuffer.ReadByte()) << 8;
            value |= other;

            other = ((ulong)this.netBuffer.ReadByte()) << 16;
            value |= other;

            other = ((ulong)this.netBuffer.ReadByte()) << 24;
            value |= other;

            other = ((ulong)this.netBuffer.ReadByte()) << 32;
            value |= other;

            other = ((ulong)this.netBuffer.ReadByte()) << 40;
            value |= other;

            other = ((ulong)this.netBuffer.ReadByte()) << 48;
            value |= other;

            other = ((ulong)this.netBuffer.ReadByte()) << 56;
            value |= other;
            return value;
        }

        public decimal ReadDecimal()
        {
            Int32[] bits = new Int32[4];

            bits[0] = this.ReadInt32();
            bits[1] = this.ReadInt32();
            bits[2] = this.ReadInt32();
            bits[3] = this.ReadInt32();

            return new decimal(bits);
        }

        public float ReadSingle()
        {
            uint value = this.ReadUInt32();
            return FloatConversion.ToSingle(value);
        }

        public double ReadDouble()
        {
            ulong value = this.ReadUInt64();
            return FloatConversion.ToDouble(value);
        }

        public string ReadString()
        {
            UInt16 numBytes = this.ReadUInt16();

            if (numBytes == 0)
            {
                return string.Empty;
            }

            if (numBytes >= MaxStringLength)
            {
                throw new IndexOutOfRangeException("ReadString() too long: " + numBytes);
            }

            while (numBytes > stringReaderBuffer.Length)
            {
                stringReaderBuffer = new byte[stringReaderBuffer.Length * 2];
            }

            this.netBuffer.ReadBytes(stringReaderBuffer, numBytes);

            char[] chars = Encoding.GetChars(stringReaderBuffer, 0, numBytes);
            return new string(chars);
        }

        public char ReadChar()
        {
            return (char)this.netBuffer.ReadByte();
        }

        public bool ReadBoolean()
        {
            int value = this.netBuffer.ReadByte();
            return value == 1;
        }

        public Position3D ReadPosition3D()
        {
            return new Position3D
            {
                X = this.ReadSingle(),
                Y = this.ReadSingle(),
                Z = this.ReadSingle(),
            };
        }

        public Rotation ReadRotation()
        {
            return new Rotation
            {
                X = this.ReadSingle(),
                Y = this.ReadSingle(),
                Z = this.ReadSingle(),
                W = this.ReadSingle(),
            };
        }

        public void ReadBytes(byte[] bytes, int count)
        {
            if (count < 0)
            {
                throw new IndexOutOfRangeException("NetworkReader ReadBytes " + count);
            }
            else if (bytes.Length < count)
            {
                throw new IndexOutOfRangeException(string.Format("NetworkReader was given byte array of size {0} and needs to be at least {1}", bytes.Length, count));
            }

            this.netBuffer.ReadBytes(bytes, (uint)count);
        }

        public byte[] ReadBytes(int count)
        {
            if (count < 0)
            {
                throw new IndexOutOfRangeException("NetworkReader ReadBytes " + count);
            }

            byte[] value = new byte[count];
            this.netBuffer.ReadBytes(value, (uint)count);
            return value;
        }

        public byte[] ReadBytesAndSize()
        {
            ushort sz = this.ReadUInt16();
            if (sz == 0)
            {
                return Array.Empty<byte>();
            }

            return this.ReadBytes(sz);
        }

        public override string ToString() => this.netBuffer.ToString();

        public TMsg ReadMessage<TMsg>()
            where TMsg : Message, new()
        {
            var msg = new TMsg();
            msg.Deserialize(this);
            return msg;
        }

        public void Replace(byte[] buffer)
        {
            this.netBuffer.Replace(buffer);
        }
    }
}
