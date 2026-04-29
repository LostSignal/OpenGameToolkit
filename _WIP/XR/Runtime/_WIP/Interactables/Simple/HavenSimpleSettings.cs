#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenSimpleSettings.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


namespace OGT.Haven
{
    using System;
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;

    [Serializable]
    public class HavenSimpleSettings
    {
        [Space]
        [SerializeField] private InteractionLayerMask interactionLayers = -1;
        [SerializeField] private GameObject customReticle;
        [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.InteractableSelectMode selectMode;

        public void Apply(HavenSimple simple)
        {
#if USING_UNITY_XR_INTERACTION_TOOLKIT
            simple.interactionLayers = this.interactionLayers;
            simple.customReticle = this.customReticle;
            simple.selectMode = this.selectMode;
#endif
        }
    }
}
