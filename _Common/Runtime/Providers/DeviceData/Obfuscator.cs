//-----------------------------------------------------------------------
// <copyright file="Obfuscator.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;

    public static class Obfuscator
    {
        private const int ByteObfuscatorLength = 4 * 1024; // 4 kilobytes
        private const int ByteSeed = 830378379;

        private static readonly List<byte> ByteObfuscatorList = new (ByteObfuscatorLength);

        static Obfuscator()
        {
            var random = new System.Random(ByteSeed);

            for (int i = 0; i < ByteObfuscatorLength; i++)
            {
                ByteObfuscatorList.Add((byte)random.Next(0, 255));
            }
        }

        public static string ObfuscateString(string sourceString)
        {
            byte[] sourceStringBytes = System.Text.Encoding.UTF8.GetBytes(sourceString);

            for (int i = 0; i < sourceStringBytes.Length; i++)
            {
                sourceStringBytes[i] ^= ByteObfuscatorList[i % ByteObfuscatorLength];
            }

            return Convert.ToBase64String(sourceStringBytes);
        }

        public static string DeobfuscateString(string obfuscatedString)
        {
            byte[] obfuscatedStringBytes = Convert.FromBase64String(obfuscatedString);

            for (int i = 0; i < obfuscatedStringBytes.Length; i++)
            {
                obfuscatedStringBytes[i] ^= ByteObfuscatorList[i % ByteObfuscatorLength];
            }

            return System.Text.Encoding.UTF8.GetString(obfuscatedStringBytes);
        }
    }
}
