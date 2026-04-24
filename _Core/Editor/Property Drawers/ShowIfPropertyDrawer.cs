//-----------------------------------------------------------------------
// <copyright file="ShowIfPropertyDrawer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEditor;

    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfPropertyDrawer : ShowIfBasePropertyDrawer
    {
        protected override string GetConditionFieldName() => ((ShowIfAttribute)this.attribute).ConditionFieldName;

        protected override object GetCompareValue() => ((ShowIfAttribute)this.attribute).CompareValue;

        protected override bool CompareBool(bool b1, bool b2) => b1 == b2;

        protected override bool CompareString(string str1, string str2) => str1 == str2;
    }
}
