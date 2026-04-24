#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="EnumFlagAttribute.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// https://wiki.unity3d.com/index.php/EnumFlagPropertyDrawer

namespace OGT
{
    using UnityEngine;

    public class EnumFlagAttribute : PropertyAttribute
    {
        public string enumName;

        public EnumFlagAttribute()
        {
        }

        public EnumFlagAttribute(string name)
        {
            this.enumName = name;
        }
    }
}
