//-----------------------------------------------------------------------
// <copyright file="ShowIfAttribute.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using UnityEngine;

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class FoldoutGroupAttribute : PropertyAttribute
    {
        private readonly string foldoutGroupName;
        private readonly bool isFirstItem;

        public FoldoutGroupAttribute(string foldoutGroupName, bool isFirstItem = false)
        {
            this.foldoutGroupName = foldoutGroupName;
            this.isFirstItem = isFirstItem;
        }

        public string FoldoutGroupName => this.foldoutGroupName;

        public bool IsFirstItem => this.isFirstItem;
    }
}
