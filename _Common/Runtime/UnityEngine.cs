//-----------------------------------------------------------------------
// <copyright file="UnityEngine.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#pragma warning disable

#if !UNITY_6000_0_OR_NEWER

namespace UnityEngine
{
    public class SerializeFieldAttribute : System.Attribute
    {
    }

    public class HideInInspectorAttribute : System.Attribute
    {
    }

    public class HeaderAttribute : System.Attribute
    {
        public HeaderAttribute(string name)
        {
        }
    }
}

#endif
