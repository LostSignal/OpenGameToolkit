//-----------------------------------------------------------------------
// <copyright file="NetBuffer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    using System;
    using System.Runtime.CompilerServices;

    // A growable buffer class used by NetworkReader and NetworkWriter.
    // this is used instead of MemoryStream and BinaryReader/BinaryWriter to avoid allocations.
    public class NetBuffer
    {
        private static readonly OGTLogger Logger = new OGTLogger("Networking");

        private const int InitialSize = 64;
        private const float GrowthFactor = 1.5f;
        private const int BufferSizeWarning = 1024 * 1024 * 128;

        private byte[] byteBuffer;
        private uint position;

        public NetBuffer()
        {
            this.ResetBuffer(new byte[InitialSize]);
        }

        // This does NOT copy the buffer
        public NetBuffer(byte[] buffer)
        {
            this.ResetBuffer(buffer);
        }

        public byte[] RawBuffer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.byteBuffer;
        }

        public uint Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.position;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.byteBuffer.Length;
        }

        public void ResetBuffer(byte[] buffer)
        {
            this.byteBuffer = buffer;
            this.position = 0;
        }

        public byte ReadByte()
        {
            if (this.position >= this.byteBuffer.Length)
            {
                throw new IndexOutOfRangeException("NetworkReader:ReadByte out of range:" + this.ToString());
            }

            return this.byteBuffer[this.position++];
        }

        public void ReadBytes(byte[] buffer, uint count)
        {
            if (this.position + count > this.byteBuffer.Length)
            {
                throw new IndexOutOfRangeException("NetworkReader:ReadBytes out of range: (" + count + ") " + this.ToString());
            }

            for (ushort i = 0; i < count; i++)
            {
                buffer[i] = this.byteBuffer[this.position + i];
            }

            this.position += count;
        }

        public void WriteByte(byte value)
        {
            this.WriteCheckForSpace(1);
            this.byteBuffer[this.position] = value;
            this.position += 1;
        }

        public void WriteByte2(byte value0, byte value1)
        {
            this.WriteCheckForSpace(2);
            this.byteBuffer[this.position] = value0;
            this.byteBuffer[this.position + 1] = value1;
            this.position += 2;
        }

        public void WriteByte4(byte value0, byte value1, byte value2, byte value3)
        {
            this.WriteCheckForSpace(4);
            this.byteBuffer[this.position] = value0;
            this.byteBuffer[this.position + 1] = value1;
            this.byteBuffer[this.position + 2] = value2;
            this.byteBuffer[this.position + 3] = value3;
            this.position += 4;
        }

        public void WriteByte8(byte value0, byte value1, byte value2, byte value3, byte value4, byte value5, byte value6, byte value7)
        {
            this.WriteCheckForSpace(8);
            this.byteBuffer[this.position] = value0;
            this.byteBuffer[this.position + 1] = value1;
            this.byteBuffer[this.position + 2] = value2;
            this.byteBuffer[this.position + 3] = value3;
            this.byteBuffer[this.position + 4] = value4;
            this.byteBuffer[this.position + 5] = value5;
            this.byteBuffer[this.position + 6] = value6;
            this.byteBuffer[this.position + 7] = value7;
            this.position += 8;
        }

        // Every other Write() function in this class writes implicitly at the end-marker m_Pos.
        // this is the only Write() function that writes to a specific location within the buffer
        public void WriteBytesAtOffset(byte[] buffer, ushort targetOffset, ushort count)
        {
            uint newEnd = (uint)(count + targetOffset);

            this.WriteCheckForSpace((ushort)newEnd);

            if (targetOffset == 0 && count == buffer.Length)
            {
                buffer.CopyTo(this.byteBuffer, (int)this.position);
            }
            else
            {
                // CopyTo doesnt take a count :(
                for (int i = 0; i < count; i++)
                {
                    this.byteBuffer[targetOffset + i] = buffer[i];
                }
            }

            // Although this writes within the buffer, it could move the end-marker
            if (newEnd > this.position)
            {
                this.position = newEnd;
            }
        }

        public void WriteBytes(byte[] buffer, ushort count)
        {
            this.WriteCheckForSpace(count);

            if (count == buffer.Length)
            {
                buffer.CopyTo(this.byteBuffer, (int)this.position);
            }
            else
            {
                // CopyTo doesnt take a count :(
                for (int i = 0; i < count; i++)
                {
                    this.byteBuffer[this.position + i] = buffer[i];
                }
            }

            this.position += count;
        }

        public void FinishMessage()
        {
            // two shorts (size and msgType) are in header.
            ushort sz = (ushort)(this.position - (sizeof(ushort) * 2));
            this.byteBuffer[0] = (byte)(sz & 0xff);
            this.byteBuffer[1] = (byte)((sz >> 8) & 0xff);
        }

        public void SeekZero()
        {
            this.position = 0;
        }

        public void Replace(byte[] buffer)
        {
            this.byteBuffer = buffer;
            this.position = 0;
        }

        public override string ToString()
        {
            return string.Format("NetBuf sz:{0} pos:{1}", this.byteBuffer.Length, this.position);
        }

        internal ArraySegment<byte> AsArraySegment()
        {
            return new ArraySegment<byte>(this.byteBuffer, 0, (int)this.position);
        }

        private void WriteCheckForSpace(ushort count)
        {
            if (this.position + count < this.byteBuffer.Length)
            {
                return;
            }

            int newLen = (int)Math.Ceiling(this.byteBuffer.Length * GrowthFactor);

            while (this.position + count >= newLen)
            {
                newLen = (int)Math.Ceiling(newLen * GrowthFactor);

                if (newLen > BufferSizeWarning)
                {
                    Logger.LogWarning("NetworkBuffer has increased to {0} bytes!", (object)newLen.ToString());
                }
            }

            // Only do the copy once, even if newLen is increased multiple times
            byte[] tmp = new byte[newLen];
            this.byteBuffer.CopyTo(tmp, 0);
            this.byteBuffer = tmp;
        }
    }
}
