//-----------------------------------------------------------------------
// <copyright file="NetworkReaderExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using OGT.Networking;
    using UnityEngine;

    public static class NetworkReaderExtensions
    {
        public static Vector2 ReadVector2(this NetworkReader reader) => new(reader.ReadSingle(), reader.ReadSingle());

        public static Vector3 ReadVector3(this NetworkReader reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        public static Vector4 ReadVector4(this NetworkReader reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        public static Color ReadColor(this NetworkReader reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        public static Color32 ReadColor32(this NetworkReader reader) => new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());

        public static Quaternion ReadQuaternion(this NetworkReader reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        public static Rect ReadRect(this NetworkReader reader) => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        public static Plane ReadPlane(this NetworkReader reader) => new(reader.ReadVector3(), reader.ReadSingle());

        public static Ray ReadRay(this NetworkReader reader) => new(reader.ReadVector3(), reader.ReadVector3());
    }
}
