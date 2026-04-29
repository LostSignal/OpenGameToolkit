#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenInteractableUtil.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace OGT.Haven
{
    using UnityEngine;


    public static class HavenInteractableUtil
    {
        public static void SetupInteractable(UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable)
        {
            BaseSetup(interactable);
            LostLayers.SetInteractable(interactable.colliders);
        }

        public static void SetupTeleport(UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable)
        {
            BaseSetup(interactable);
            LostLayers.SetTeleport(interactable.colliders);
        }

        private static void BaseSetup(UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable)
        {
            if (Application.isPlaying)
            {
                return;
            }

            // Always make sure the we're not saving a reference to the interaction manager
            if (interactable.interactionManager != null)
            {
                interactable.interactionManager = null;
                EditorUtil.SetDirty(interactable);
            }

            // Auto populating a collider if it already exists
            if (interactable.colliders.Count == 0)
            {
                var colliders = interactable.GetComponentsInChildren<Collider>();

                if (colliders.Length > 0)
                {
                    foreach (var collider in colliders)
                    {
                        interactable.colliders.Add(collider);
                    }

                    EditorUtil.SetDirty(interactable);
                }
            }
        }
    }
}

#endif
