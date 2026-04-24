#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenGrabbableEditor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace OGT.Haven
{
    using UnityEditor;

    [CustomEditor(typeof(HavenGrabbable))]
    public class HavenGrabbableEditor : OGT.Editor
    {
        protected override void NewOnInspectorGUI()
        {
            this.DrawProperty(this.target, "havenGrabbableSettings");
            this.DrawProperty(this.target, "isOffsetGrabbable");
            this.DrawProperty(this.target, "disableRayGrab");
            this.DrawProperty(this.target, "m_AttachTransform");
            this.DrawProperty(this.target, "m_Colliders");

            this.Foldout("Events", () =>
            {
                this.DrawProperty(this.target, "onHoverStart");
                this.DrawProperty(this.target, "onHoverStop");

                this.DrawProperty(this.target, "onGrabStart");
                this.DrawProperty(this.target, "onGrabStop");

                this.DrawProperty(this.target, "onUseStart");
                this.DrawProperty(this.target, "onUseStop");
            });

            this.Foldout("Unity XRIT Events", () =>
            {
                this.DrawProperty(this.target, "m_HoverEntered");
                this.DrawProperty(this.target, "m_HoverExited");
                this.DrawProperty(this.target, "m_SelectEntered");
                this.DrawProperty(this.target, "m_SelectExited");
                this.DrawProperty(this.target, "m_Activated");
                this.DrawProperty(this.target, "m_Deactivated");
            });
        }
    }
}

#endif
