//-----------------------------------------------------------------------
// <copyright file="GuidComponentDrawer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(GuidComponent))]
    public class GuidComponentDrawer : Editor
    {
        private GuidComponent guidComp;

        public override void OnInspectorGUI()
        {
            if (this.guidComp == null)
            {
                this.guidComp = (GuidComponent)this.target;
            }

            // Draw label
            EditorGUILayout.LabelField("Guid:", this.guidComp.GetGuid().ToString());

            EditorGUILayout.Space();

            if (GUILayout.Button("Copy to Clipboard"))
            {
                GUIUtility.systemCopyBuffer = this.guidComp.GetGuid().ToString();
            }
        }
    }
}
