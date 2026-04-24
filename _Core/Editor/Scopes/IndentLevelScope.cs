//-----------------------------------------------------------------------
// <copyright file="IndentLevelScope.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using UnityEditor;

    public class IndentLevelScope : IDisposable
    {
        private readonly int originalIndent;

        public IndentLevelScope(int increaseIndentBy)
        {
            this.originalIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = this.originalIndent + increaseIndentBy;
        }

        void IDisposable.Dispose()
        {
            EditorGUI.indentLevel = this.originalIndent;
        }
    }
}
