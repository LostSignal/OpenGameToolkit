#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="VRIKEditor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT && USING_UNITY_ANIMATION_RIGGING

namespace OGT
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(VRIK))]
    public class VRIKEditor : Editor
    {
        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Setup Rig"))
            {
                (this.target as VRIK).SetupRig();
            }

            if (GUILayout.Button("Reset Rig"))
            {
                (this.target as VRIK).ResetRig();
            }
        }
    }
}

#endif
