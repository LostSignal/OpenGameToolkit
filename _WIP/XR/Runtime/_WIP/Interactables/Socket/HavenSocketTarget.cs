#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenSocketTarget.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Haven
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.XR.Interaction.Toolkit;

    [AddComponentMenu("Haven XR/Socket/HXR Socket Target")]
    [RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable))]
    public class HavenSocketTarget : MonoBehaviour, IAwake, IValidate
    {
        private static readonly Dictionary<ulong, string> SocketTargetMap = new();

#pragma warning disable 0649
        [SerializeField] private Rigidbody interactableRigidbody;
        [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;
        [SerializeField] private string socketTargetName;
        [SerializeField] private UnityEvent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor> onSocketed;
#pragma warning restore 0649

        public static string GetSocketTargetName(UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable)
        {
            if (SocketTargetMap.TryGetValue(interactable.GetEntityId().ToULong(), out string socketTargetName))
            {
                return socketTargetName;
            }

            return null;
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNull(this, this.interactable, nameof(this.interactable));
            report.AssertNotNullOrEmpty(this, this.socketTargetName, nameof(this.socketTargetName));
        }

        public void OnAwake(Bootloader bootloader)
        {
#if USING_UNITY_XR_INTERACTION_TOOLKIT
            this.interactable.selectEntered.AddListener(this.SelectEntered);
#endif
        }

#if USING_UNITY_XR_INTERACTION_TOOLKIT
        public void DisableInteractable()
        {
            this.enabled = false;
            this.interactable.enabled = false;

            if (this.interactableRigidbody != null)
            {
                this.interactableRigidbody.useGravity = false;
                this.interactableRigidbody.isKinematic = true;
            }
        }

        private void Awake() => ActivationManager.Register(this);

        private void OnDestroy()
        {
            if (this.interactable && string.IsNullOrWhiteSpace(this.socketTargetName) == false)
            {
                this.interactable.selectEntered.RemoveListener(this.SelectEntered);
            }
        }

        private void OnEnable()
        {
            if (this.interactable && string.IsNullOrWhiteSpace(this.socketTargetName) == false)
            {
                SocketTargetMap.Add(this.interactable.GetEntityId().ToULong(), this.socketTargetName);
            }
        }

        private void OnDisable()
        {
            if (this.interactable && string.IsNullOrWhiteSpace(this.socketTargetName) == false)
            {
                SocketTargetMap.Remove(this.interactable.GetEntityId().ToULong());
            }
        }

        private void OnValidate()
        {
            EditorUtil.SetIfNull(this, ref this.interactable);
            EditorUtil.SetIfNull(this, ref this.interactableRigidbody);
        }

        private void SelectEntered(SelectEnterEventArgs selectEnterEventArgs)
        {
            var socketInteractor = selectEnterEventArgs.interactorObject as HavenSocket;

            if (socketInteractor == null || socketInteractor.SocketTargetName != this.socketTargetName)
            {
                return;
            }

            try
            {
                this.onSocketed?.Invoke(selectEnterEventArgs.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

#endif
    }
}
