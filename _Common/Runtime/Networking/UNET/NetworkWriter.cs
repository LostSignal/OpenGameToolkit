//-----------------------------------------------------------------------
// <copyright file="NetworkWriter.cs" company="Lost Signal LLC">
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

    public class NetworkWriter
    {
        private const int MaxStringLength = 1024 * 32;

        private static readonly Encoding Encoding = new UTF8Encoding();
        private static readonly byte[] StringWriteBuffer = new byte[MaxStringLength];
        private static UIntFloat floatConverter;

#if UNITY_6000_0_OR_NEWER
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => floatConverter = default;
#endif

        private readonly NetBuffer netBuffer;

        public NetworkWriter()
        {
            this.netBuffer = new NetBuffer();
        }

        public NetworkWriter(byte[] buffer)
        {
            this.netBuffer = new NetBuffer(buffer);
        }

        public NetworkWriter(NetBuffer netBuffer)
        {
            this.netBuffer = netBuffer;
        }

        public byte[] RawBuffer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return this.netBuffer.RawBuffer; }
        }

        public short Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (short)this.netBuffer.Position; }
        }

        public void ResetBuffer(byte[] buffer)
        {
            this.netBuffer.ResetBuffer(buffer);
        }

        public byte[] ToArray()
        {
            var newArray = new byte[this.netBuffer.AsArraySegment().Count];
            Array.Copy(this.netBuffer.AsArraySegment().Array, newArray, this.netBuffer.AsArraySegment().Count);
            return newArray;
        }

        public byte[] AsArray()
        {
            return this.AsArraySegment().Array;
        }

        // http://sqlite.org/src4/doc/trunk/www/varint.wiki
        public void WritePackedUInt32(UInt32 value)
        {
            if (value <= 240)
            {
                this.Write((byte)value);
                return;
            }

            if (value <= 2287)
            {
                this.Write((byte)(((value - 240) / 256) + 241));
                this.Write((byte)((value - 240) % 256));
                return;
            }

            if (value <= 67823)
            {
                this.Write((byte)249);
                this.Write((byte)((value - 2288) / 256));
                this.Write((byte)((value - 2288) % 256));
                return;
            }

            if (value <= 16777215)
            {
                this.Write((byte)250);
                this.Write((byte)(value & 0xFF));
                this.Write((byte)((value >> 8) & 0xFF));
                this.Write((byte)((value >> 16) & 0xFF));
                return;
            }

            // All other values of uint
            this.Write((byte)251);
            this.Write((byte)(value & 0xFF));
            this.Write((byte)((value >> 8) & 0xFF));
            this.Write((byte)((value >> 16) & 0xFF));
            this.Write((byte)((value >> 24) & 0xFF));
        }

        public void WritePackedUInt64(UInt64 value)
        {
            if (value <= 240)
            {
                this.Write((byte)value);
                return;
            }

            if (value <= 2287)
            {
                this.Write((byte)(((value - 240) / 256) + 241));
                this.Write((byte)((value - 240) % 256));
                return;
            }

            if (value <= 67823)
            {
                this.Write((byte)249);
                this.Write((byte)((value - 2288) / 256));
                this.Write((byte)((value - 2288) % 256));
                return;
            }

            if (value <= 16777215)
            {
                this.Write((byte)250);
                this.Write((byte)(value & 0xFF));
                this.Write((byte)((value >> 8) & 0xFF));
                this.Write((byte)((value >> 16) & 0xFF));
                return;
            }

            if (value <= 4294967295)
            {
                this.Write((byte)251);
                this.Write((byte)(value & 0xFF));
                this.Write((byte)((value >> 8) & 0xFF));
                this.Write((byte)((value >> 16) & 0xFF));
                this.Write((byte)((value >> 24) & 0xFF));
                return;
            }

            if (value <= 1099511627775)
            {
                this.Write((byte)252);
                this.Write((byte)(value & 0xFF));
                this.Write((byte)((value >> 8) & 0xFF));
                this.Write((byte)((value >> 16) & 0xFF));
                this.Write((byte)((value >> 24) & 0xFF));
                this.Write((byte)((value >> 32) & 0xFF));
                return;
            }

            if (value <= 281474976710655)
            {
                this.Write((byte)253);
                this.Write((byte)(value & 0xFF));
                this.Write((byte)((value >> 8) & 0xFF));
                this.Write((byte)((value >> 16) & 0xFF));
                this.Write((byte)((value >> 24) & 0xFF));
                this.Write((byte)((value >> 32) & 0xFF));
                this.Write((byte)((value >> 40) & 0xFF));
                return;
            }

            if (value <= 72057594037927935)
            {
                this.Write((byte)254);
                this.Write((byte)(value & 0xFF));
                this.Write((byte)((value >> 8) & 0xFF));
                this.Write((byte)((value >> 16) & 0xFF));
                this.Write((byte)((value >> 24) & 0xFF));
                this.Write((byte)((value >> 32) & 0xFF));
                this.Write((byte)((value >> 40) & 0xFF));
                this.Write((byte)((value >> 48) & 0xFF));
                return;
            }

            // All others
            {
                this.Write((byte)255);
                this.Write((byte)(value & 0xFF));
                this.Write((byte)((value >> 8) & 0xFF));
                this.Write((byte)((value >> 16) & 0xFF));
                this.Write((byte)((value >> 24) & 0xFF));
                this.Write((byte)((value >> 32) & 0xFF));
                this.Write((byte)((value >> 40) & 0xFF));
                this.Write((byte)((value >> 48) & 0xFF));
                this.Write((byte)((value >> 56) & 0xFF));
            }
        }

        public void Write(char value)
        {
            this.netBuffer.WriteByte((byte)value);
        }

        public void Write(byte value)
        {
            this.netBuffer.WriteByte(value);
        }

        public void Write(sbyte value)
        {
            this.netBuffer.WriteByte((byte)value);
        }

        public void Write(short value)
        {
            this.netBuffer.WriteByte2((byte)(value & 0xff), (byte)((value >> 8) & 0xff));
        }

        public void Write(ushort value)
        {
            this.netBuffer.WriteByte2((byte)(value & 0xff), (byte)((value >> 8) & 0xff));
        }

        public void Write(int value)
        {
            // little endian...
            this.netBuffer.WriteByte4(
                (byte)(value & 0xff),
                (byte)((value >> 8) & 0xff),
                (byte)((value >> 16) & 0xff),
                (byte)((value >> 24) & 0xff));
        }

        public void Write(uint value)
        {
            this.netBuffer.WriteByte4(
                (byte)(value & 0xff),
                (byte)((value >> 8) & 0xff),
                (byte)((value >> 16) & 0xff),
                (byte)((value >> 24) & 0xff));
        }

        public void Write(long value)
        {
            this.netBuffer.WriteByte8(
                (byte)(value & 0xff),
                (byte)((value >> 8) & 0xff),
                (byte)((value >> 16) & 0xff),
                (byte)((value >> 24) & 0xff),
                (byte)((value >> 32) & 0xff),
                (byte)((value >> 40) & 0xff),
                (byte)((value >> 48) & 0xff),
                (byte)((value >> 56) & 0xff));
        }

        public void Write(ulong value)
        {
            this.netBuffer.WriteByte8(
                (byte)(value & 0xff),
                (byte)((value >> 8) & 0xff),
                (byte)((value >> 16) & 0xff),
                (byte)((value >> 24) & 0xff),
                (byte)((value >> 32) & 0xff),
                (byte)((value >> 40) & 0xff),
                (byte)((value >> 48) & 0xff),
                (byte)((value >> 56) & 0xff));
        }

        public void Write(float value)
        {
            floatConverter.FloatValue = value;
            this.Write(floatConverter.IntValue);
        }

        public void Write(double value)
        {
            floatConverter.DoubleValue = value;
            this.Write(floatConverter.LongValue);
        }

        public void Write(decimal value)
        {
            Int32[] bits = decimal.GetBits(value);
            this.Write(bits[0]);
            this.Write(bits[1]);
            this.Write(bits[2]);
            this.Write(bits[3]);
        }

        public void Write(string value)
        {
            if (value == null)
            {
                this.netBuffer.WriteByte2(0, 0);
                return;
            }

            int len = Encoding.GetByteCount(value);

            if (len >= MaxStringLength)
            {
                throw new IndexOutOfRangeException($"Serialize(string) too long: {value.Length}");
            }

            this.Write((ushort)len);
            int numBytes = Encoding.GetBytes(value, 0, value.Length, StringWriteBuffer, 0);
            this.netBuffer.WriteBytes(StringWriteBuffer, (ushort)numBytes);
        }

        public void Write(bool value)
        {
            if (value)
            {
                this.netBuffer.WriteByte(1);
            }
            else
            {
                this.netBuffer.WriteByte(0);
            }
        }

        public void Write(Position3D position)
        {
            this.Write(position.X);
            this.Write(position.Y);
            this.Write(position.Z);
        }

        public void Write(Rotation rotation)
        {
            this.Write(rotation.X);
            this.Write(rotation.Y);
            this.Write(rotation.Z);
            this.Write(rotation.W);
        }

        public void Write(byte[] bytes, int count)
        {
            if (count > UInt16.MaxValue)
            {
                // TODO [bgish]: Add this back in some form?
                // if (LogFilter.logError) { OGT.Logger.LogError("NetworkWriter Write: buffer is too large (" + count + ") bytes. The maximum buffer size is 64K bytes."); }
                return;
            }

            this.netBuffer.WriteBytes(bytes, (UInt16)count);
        }

        public void Write(byte[] bytes, int offset, int count)
        {
            if (count > UInt16.MaxValue)
            {
                // TODO [bgish]: Add this back in some form?
                // if (LogFilter.logError) { OGT.Logger.LogError("NetworkWriter Write: buffer is too large (" + count + ") bytes. The maximum buffer size is 64K bytes."); }
                return;
            }

            this.netBuffer.WriteBytesAtOffset(bytes, (ushort)offset, (ushort)count);
        }

        public void WriteBytesAndSize(byte[] bytes, int count)
        {
            if (bytes == null || count == 0)
            {
                this.Write((UInt16)0);
                return;
            }

            if (count > UInt16.MaxValue)
            {
                // TODO [bgish]: Add this back in some form?
                // if (LogFilter.logError) { OGT.Logger.LogError("NetworkWriter WriteBytesAndSize: buffer is too large (" + count + ") bytes. The maximum buffer size is 64K bytes."); }
                return;
            }

            this.Write((UInt16)count);
            this.netBuffer.WriteBytes(bytes, (UInt16)count);
        }

        // NOTE: this will write the entire buffer.. including trailing empty space!
        public void WriteBytesFull(byte[] bytes)
        {
            if (bytes == null)
            {
                this.Write((UInt16)0);
                return;
            }

            if (bytes.Length > UInt16.MaxValue)
            {
                // TODO [bgish]: Add this back in some form?
                // if (LogFilter.logError) { OGT.Logger.LogError("NetworkWriter WriteBytes: buffer is too large (" + buffer.Length + ") bytes. The maximum buffer size is 64K bytes."); }
                return;
            }

            this.Write((UInt16)bytes.Length);
            this.netBuffer.WriteBytes(bytes, (UInt16)bytes.Length);
        }

        public void Write(Message msg)
        {
            msg.Serialize(this);
        }

        public void SeekZero()
        {
            this.netBuffer.SeekZero();
        }

        public void StartMessage(short msgType)
        {
            this.SeekZero();

            // two bytes for size, will be filled out in FinishMessage
            this.netBuffer.WriteByte2(0, 0);

            // two bytes for message type
            this.Write(msgType);
        }

        public void FinishMessage()
        {
            // writes correct size into space at start of buffer
            this.netBuffer.FinishMessage();
        }

        internal ArraySegment<byte> AsArraySegment()
        {
            return this.netBuffer.AsArraySegment();
        }
    }
}
