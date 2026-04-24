//-----------------------------------------------------------------------
// <copyright file="ShowIfNotPropertyDrawer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEditor;

    [CustomPropertyDrawer(typeof(ShowIfNotAttribute))]
    public class ShowIfNotPropertyDrawer : ShowIfBasePropertyDrawer
    {
        protected override string GetConditionFieldName() => ((ShowIfNotAttribute)this.attribute).ConditionFieldName;

        protected override object GetCompareValue() => ((ShowIfNotAttribute)this.attribute).CompareValue;

        protected override bool CompareBool(bool b1, bool b2) => b1 != b2;

        protected override bool CompareString(string str1, string str2) => str1 != str2;
    }
}
