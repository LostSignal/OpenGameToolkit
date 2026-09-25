//-----------------------------------------------------------------------
// <copyright file="LabelWidthAttribute.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Overrides the inspector label width for the decorated field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class LabelWidthAttribute : PropertyAttribute
    {
        private readonly float labelWidth;

        /// <param name="labelWidth">Label width in pixels.</param>
        public LabelWidthAttribute(float labelWidth)
        {
            this.labelWidth = labelWidth;
        }

        public float LabelWidth => this.labelWidth;
    }
}
