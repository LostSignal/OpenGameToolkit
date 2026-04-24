#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="XRControllerStateManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace HavenXR
{
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;

    public class XRControllerStateManager : MonoBehaviour
    {
#pragma warning disable 0649
        [Header("Interact / UI Controllers")]
        [SerializeField] private XRController interactXRController;
        [SerializeField] private XRController uiXRController;

        [Header("Teleport Controller")]
        [SerializeField] private XRController teleportXRController;
#pragma warning restore 0649

        private enum State
        {
            None,
            Interact,
            Teleport,
        }

        public void SetControllerModel(Transform controllerModelPrefab)
        {
            this.interactXRController.modelPrefab = controllerModelPrefab;
            this.teleportXRController.modelPrefab = controllerModelPrefab;
        }

        public void SetNoneState()
        {
            this.SetState(State.None);
        }

        public void SetInteractState()
        {
            this.SetState(State.Interact);
        }

        public void SetTeleportState()
        {
            this.SetState(State.Teleport);
        }

        private void SetState(State state)
        {
            bool teleport = state == State.Teleport;
            bool interact = state == State.Interact;

            // Teleport State
            this.teleportXRController.gameObject.SetActive(teleport);
            this.teleportXRController.enableInputActions = teleport;

            // Interact State
            this.interactXRController.gameObject.SetActive(interact);
            this.interactXRController.enableInputActions = interact;
            this.uiXRController.gameObject.SetActive(interact);
            this.uiXRController.enableInputActions = interact;
        }
    }
}

#endif
