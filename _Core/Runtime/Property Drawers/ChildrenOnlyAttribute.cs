//-----------------------------------------------------------------------
// <copyright file="ChildrenOnlyAttribute.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using UnityEngine;

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class ChildrenOnlyAttribute : PropertyAttribute
    {
    }
}
