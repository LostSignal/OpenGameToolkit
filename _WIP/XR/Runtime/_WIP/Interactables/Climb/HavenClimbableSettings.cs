#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenClimbableSettings.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Haven
{
    using System;
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;

    [Serializable]
    public class HavenClimbableSettings
    {
        [Space]
        [SerializeField] private InteractionLayerMask interactionLayers = -1;
        [SerializeField] private GameObject customReticle;
        [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.InteractableSelectMode selectMode;

        public void Apply(HavenClimbable climb)
        {
#if USING_UNITY_XR_INTERACTION_TOOLKIT
            climb.interactionLayers = this.interactionLayers;
            climb.customReticle = this.customReticle;
            climb.selectMode = this.selectMode;
#endif
        }
    }
}
