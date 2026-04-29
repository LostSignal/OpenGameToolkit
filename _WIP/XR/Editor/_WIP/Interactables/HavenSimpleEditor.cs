#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenSimpleEditor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace OGT.Haven
{
    using UnityEditor;

    [CustomEditor(typeof(HavenSimple))]
    public class HavenSimpleEditor : OGT.Editor
    {
        protected override void NewOnInspectorGUI()
        {
            this.DrawProperty(this.target, "havenSimpleSettings");
            this.DrawProperty(this.target, "disableRayGrab");
            this.DrawProperty(this.target, "m_Colliders");

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
