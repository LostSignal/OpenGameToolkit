//-----------------------------------------------------------------------
// <copyright file="IndentAttribute.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using UnityEngine;

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class IndentAttribute : PropertyAttribute
    {
        private readonly int indentLevel;

        public IndentAttribute(int indentLevel)
        {
            this.indentLevel = indentLevel;
        }

        public int IndentLevel => this.indentLevel;
    }
}
