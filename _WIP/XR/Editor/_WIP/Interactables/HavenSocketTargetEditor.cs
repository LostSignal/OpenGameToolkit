#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenSocketTargetEditor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace OGT.Haven
{
    using UnityEditor;

    [CustomEditor(typeof(HavenSocketTarget))]
    public class HavenSocketTargetEditor : OGT.Editor
    {
        protected override void NewOnInspectorGUI()
        {
            this.DrawProperty(this.target, "interactable");
            this.DrawProperty(this.target, "interactableRigidbody");
            this.DrawProperty(this.target, "socketTargetName");

            this.Foldout("Events", () =>
            {
                this.DrawProperty(this.target, "onSocketed");
            });
        }
    }
}

#endif
