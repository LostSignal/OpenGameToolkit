//-----------------------------------------------------------------------
// <copyright file="NetworkWriterExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using OGT.Networking;
    using UnityEngine;

    public static class NetworkWriterExtensions
    {
        public static void Write(this NetworkWriter writer, Vector2 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
        }

        public static void Write(this NetworkWriter writer, Vector3 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
        }

        public static void Write(this NetworkWriter writer, Vector4 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
            writer.Write(value.w);
        }

        public static void Write(this NetworkWriter writer, Color value)
        {
            writer.Write(value.r);
            writer.Write(value.g);
            writer.Write(value.b);
            writer.Write(value.a);
        }

        public static void Write(this NetworkWriter writer, Color32 value)
        {
            writer.Write(value.r);
            writer.Write(value.g);
            writer.Write(value.b);
            writer.Write(value.a);
        }

        public static void Write(this NetworkWriter writer, Quaternion value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
            writer.Write(value.w);
        }

        public static void Write(this NetworkWriter writer, Rect value)
        {
            writer.Write(value.xMin);
            writer.Write(value.yMin);
            writer.Write(value.width);
            writer.Write(value.height);
        }

        public static void Write(this NetworkWriter writer, Plane value)
        {
            writer.Write(value.normal);
            writer.Write(value.distance);
        }

        public static void Write(this NetworkWriter writer, Ray value)
        {
            writer.Write(value.direction);
            writer.Write(value.origin);
        }
    }
}
