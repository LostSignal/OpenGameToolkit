//-----------------------------------------------------------------------
// <copyright file="LabelWidthScope.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using UnityEditor;

    public sealed class LabelWidthScope : IDisposable
    {
        private readonly float originalWidth;

        public LabelWidthScope(float labelWidth)
        {
            this.originalWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = labelWidth;
        }

        public void Dispose()
        {
            EditorGUIUtility.labelWidth = this.originalWidth;
        }
    }
}
