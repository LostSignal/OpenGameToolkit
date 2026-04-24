#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenSocketEditor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace OGT.Haven
{
    using UnityEditor;

    [CustomEditor(typeof(HavenSocket))]
    public class HavenSocketEditor : OGT.Editor
    {
        protected override void NewOnInspectorGUI()
        {
            this.DrawProperty(this.target, "havenSocketSettings");
            this.DrawProperty(this.target, "m_SocketActive");
            this.DrawProperty(this.target, "m_StartingSelectedInteractable");
            this.DrawProperty(this.target, "m_AttachTransform");

            this.Foldout("Advanced", () =>
            {
                this.DrawProperty(this.target, "onlyAllowSpecificSocketTarget");
                this.DrawProperty(this.target, "socketTargetName");
                this.DrawProperty(this.target, "disableInteractorAndInteractableOnSocketed");
            });

            this.Foldout("Events", () =>
            {
                this.DrawProperty(this.target, "onSocketed");
            });

            this.Foldout("Unity XRIT Events", () =>
            {
                this.DrawProperty(this.target, "m_HoverEntered");
                this.DrawProperty(this.target, "m_HoverExited");
                this.DrawProperty(this.target, "m_SelectEntered");
                this.DrawProperty(this.target, "m_SelectExited");
            });
        }
    }
}

#endif
