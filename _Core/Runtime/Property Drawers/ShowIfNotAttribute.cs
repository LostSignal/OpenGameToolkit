//-----------------------------------------------------------------------
// <copyright file="ShowIfNotAttribute.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using UnityEngine;

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class ShowIfNotAttribute : PropertyAttribute
    {
        private readonly string conditionFieldName;
        private readonly object compareValue;

        public ShowIfNotAttribute(string conditionFieldName, object compareValue)
        {
            this.conditionFieldName = conditionFieldName;
            this.compareValue = compareValue;
        }

        public string ConditionFieldName => this.conditionFieldName;

        public object CompareValue => this.compareValue;
    }
}
