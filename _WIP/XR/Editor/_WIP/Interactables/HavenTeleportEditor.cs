#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenTeleportEditor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace OGT.Haven
{
    using UnityEditor;

    [CustomEditor(typeof(HavenTeleport)), CanEditMultipleObjects]
    public class HavenTeleportEditor : OGT.Editor
    {
        protected override void NewOnInspectorGUI()
        {
            this.DrawProperty(this.target, "m_CustomReticle");
            this.DrawProperty(this.target, "type");
            this.DrawProperty(this.target, "anchorTransform");
            this.DrawProperty(this.target, "matchAnchorOrientation");
            this.DrawProperty(this.target, "m_Colliders");

            this.Foldout("Events", () =>
            {
                this.DrawProperty(this.target, "onHoverStart");
                this.DrawProperty(this.target, "onHoverStop");
                this.DrawProperty(this.target, "onTeleport");
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
