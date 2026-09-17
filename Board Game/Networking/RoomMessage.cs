//-----------------------------------------------------------------------
// <copyright file="RoomMessage.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BoardGame.Networking
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using OGT.Networking;

    /// <summary>
    /// Base class for every message exchanged through a <see cref="MessageRoom"/>. The wire layout is
    /// [short id][long senderUserId][subclass payload]. Subclasses must call the base Serialize/Deserialize first.
    /// </summary>
    public abstract class RoomMessage : Message
    {
        private const int ChunkBytes = 16 * 1024;
        private const int NullLength = -1;

        /// <summary>
        /// Gets or sets the room user that sent this message. Stamped automatically by <see cref="MessageRoom.SendMessage"/>.
        /// </summary>
        public string SenderUserId { get; set; }

        public override string GetTypeName() => this.GetType().Name;

        public override void Serialize(NetworkWriter writer)
        {
            base.Serialize(writer);
            writer.Write(this.SenderUserId);
        }

        public override void Deserialize(NetworkReader reader)
        {
            base.Deserialize(reader);
            this.SenderUserId = reader.ReadString();
        }

        /// <summary>
        /// Writes a string of any length (NetworkWriter.Write(string) is limited to 32 KB). Null is preserved.
        /// </summary>
        public static void WriteText(NetworkWriter writer, string text)
        {
            WriteLargeBytes(writer, text == null ? null : Encoding.UTF8.GetBytes(text));
        }

        public static string ReadText(NetworkReader reader)
        {
            byte[] bytes = ReadLargeBytes(reader);
            return bytes == null ? null : Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Writes a byte array of any length (the NetworkWriter byte[] writes are limited to 64 KB). Null is preserved.
        /// </summary>
        public static void WriteLargeBytes(NetworkWriter writer, byte[] bytes)
        {
            if (bytes == null)
            {
                writer.Write(NullLength);
                return;
            }

            writer.Write(bytes.Length);

            int offset = 0;

            while (offset < bytes.Length)
            {
                int count = Math.Min(ChunkBytes, bytes.Length - offset);

                if (offset == 0 && count == bytes.Length)
                {
                    writer.Write(bytes, count);
                }
                else
                {
                    // NetworkWriter.Write(byte[], offset, count) writes at a destination offset, so copy the chunk out instead
                    byte[] chunk = new byte[count];
                    Buffer.BlockCopy(bytes, offset, chunk, 0, count);
                    writer.Write(chunk, count);
                }

                offset += count;
            }
        }

        public static byte[] ReadLargeBytes(NetworkReader reader)
        {
            int length = reader.ReadInt32();

            if (length == NullLength)
            {
                return null;
            }

            if (length < 0)
            {
                throw new InvalidOperationException($"Invalid byte array length {length}");
            }

            return length == 0 ? Array.Empty<byte>() : reader.ReadBytes(length);
        }

        public static void WriteByteArrayList(NetworkWriter writer, List<byte[]> list)
        {
            writer.Write(list?.Count ?? 0);

            if (list == null)
            {
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                WriteLargeBytes(writer, list[i]);
            }
        }

        public static void ReadByteArrayList(NetworkReader reader, List<byte[]> list)
        {
            list.Clear();

            int count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                list.Add(ReadLargeBytes(reader));
            }
        }
    }
}
