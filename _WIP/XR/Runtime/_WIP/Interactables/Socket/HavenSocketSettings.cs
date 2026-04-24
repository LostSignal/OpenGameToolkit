#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenSocketSettings.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Haven
{
    using System;
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;

    [Serializable]
    public class HavenSocketSettings
    {
        [Space]
        [SerializeField] private InteractionLayerMask interactionLayers = -1;
        [SerializeField] private bool keepSelectedTargetValid;
        [SerializeField] private float recycleDelayTime;
        [SerializeField] private float interactableHoverScale;
        [SerializeField] private bool showInteractableHoverMeshes;
        [SerializeField][Indent(1)] private Material interactableHoverMeshMaterial;
        [SerializeField][Indent(1)] private Material interactableCantHoverMeshMaterial;

        public void Apply(HavenSocket socket)
        {
#if USING_UNITY_XR_INTERACTION_TOOLKIT
            socket.interactionLayers = this.interactionLayers;
            socket.keepSelectedTargetValid = this.keepSelectedTargetValid;
            socket.recycleDelayTime = this.recycleDelayTime;
            socket.interactableHoverScale = this.interactableHoverScale;
            socket.showInteractableHoverMeshes = this.showInteractableHoverMeshes;
            socket.interactableHoverMeshMaterial = this.interactableHoverMeshMaterial;
            socket.interactableCantHoverMeshMaterial = this.interactableCantHoverMeshMaterial;
#endif
        }
    }
}
