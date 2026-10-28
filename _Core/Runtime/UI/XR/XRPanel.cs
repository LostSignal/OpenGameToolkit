//-----------------------------------------------------------------------
// <copyright file="XRPanel.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.XR
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

#if USING_UNITY_XR_INTERACTION_TOOLKIT
    using UnityEngine.XR.Interaction.Toolkit.UI;
#endif

    //// Right now the PancakeDevice.Awake (line 47) sets "Cursor.lockState = CursorLockMode.Locked;"
    //// Should this be a part of the XRManager
    //// Whenever a Dialog is shown that has "RequireMouseInPancakeMode", it will turn the mouse back on?????

    //// XR Manager
    ////   Cursor / Mouse Icon

    //// XR Settings
    ////     bool DoesFollowUser
    ////         float canvasDistanceForwards;
    ////         float canvasDistanceUpwards = 0.0f;
    ////         float positionLerpSpeed = 2.0f;
    ////         float rotationLerpSpeed = 10.0f;
    ////
    ////     bool isManipulableByUser;
    ////         float min/max distance;
    ////
    ////     bool requiresMouseInPancakeMode;
    ////
    ////     bool staticWorldSpace;

    [RequireComponent(typeof(Panel))]
    public class XRPanel : GameBehavior, IAwake, IValidate
    {
#pragma warning disable 0649
        [SerializeField][ReadOnly] private Canvas panelCanvas;
        [SerializeField][ReadOnly] private Panel panel;
        [SerializeField] private XRPanelSettings settings;
        [SerializeField] private bool isGrabable;
        [SerializeField] private List<GameObject> disableObjectsInXrMode;
#pragma warning restore 0649

        public Canvas Canvas => this.panelCanvas;

        private Coroutine updatePositionCoroutine;
        private XRManager xrManager;
        private bool hasBeenGrabbed;


#if USING_UNITY_XR_INTERACTION_TOOLKIT
        private TrackedDeviceGraphicRaycaster trackedDeviceGraphicRaycaster;
        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable xrGrabInteractable;
#endif

        private void OnValidate()
        {
            this.EditorGetComponent(ref this.panel);
            this.EditorGetComponent(ref this.panelCanvas);

            if (this.settings == null)
            {
                this.settings = this.EditorGetAssetByGuid<XRPanelSettings>("b94163a381853194897edb2eabf51730");
                EditorUtil.SetDirty(this);
            }
        }

        public void OnAwake(Bootloader bootloader)
        {
            this.xrManager = bootloader.FindManager<XRManager>();
            this.xrManager.OnXRDeviceChange += () =>
            {
                // TODO [bgish]: Update this dialog to be in the correct mode (Pancake vs XR)
            };

            this.panel.Showable.OnShowEnd.AddListener(this.OnShow);
            this.panel.Showable.OnHideStart.AddListener(this.OnHide);
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNull(this, this.panel, "XRPanel this.panel is null.");
            report.AssertNotNull(this, this.panelCanvas, "XRPanel this.panelCanvas is null.");
            report.AssertNotNull(this, this.settings, "XRPanel this.settings is null.");
        }

        private void UpdatePosition()
        {
            var dialogCamera = this.panelCanvas.worldCamera;

            if (dialogCamera == null)
            {
                return;
            }

            if (dialogCamera.gameObject.activeInHierarchy == false || dialogCamera.enabled == false)
            {
                dialogCamera = Camera.main;
            }

            if (dialogCamera == null)
            {
                return;
            }

            var dialogCameraTransform = dialogCamera.transform;

            // Move the object CanvasDistance units in front of the camera
            float positionSpeed = Time.deltaTime * this.settings.PositionLerpSpeed;

            Vector3 positionTo =
                dialogCameraTransform.position +
                (dialogCameraTransform.forward * this.settings.CanvasDistanceForwards) +
                (dialogCameraTransform.up * this.settings.CanvasDistanceUpwards);

            this.transform.position = Vector3.SlerpUnclamped(this.transform.position, positionTo, positionSpeed);

            // Rotate the object to face the camera
            float rotattionSpeed = Time.deltaTime * this.settings.RotationLerpSpeed;
            Vector3 diff = this.transform.position - dialogCameraTransform.position;

            if (diff != Vector3.zero)
            {
                Quaternion rotationTo = Quaternion.LookRotation(diff);
                this.transform.rotation = Quaternion.Slerp(this.transform.rotation, rotationTo, rotattionSpeed);
            }
        }

        private void OnShow()
        {
            // Making sure the coroutine is stopped before starting it again
            this.StopUpdateCoroutine();

            // If the XRPanel is disabled, we don't want to do anything
            if (this.enabled == false)
            {
                return;
            }

            // Determing if we're in pancake mode so we can early out
            if (this.xrManager.IsFlatMode)
            {
                //// TODO [bgish]: figure out what mode it needs to be in and ensure that it's in that mode. 
                //// this.dialogCanvas.renderMode = this.panel.IsOverlayCamera ? RenderMode.ScreenSpaceOverlay : RenderMode.ScreenSpaceCamera;
                //// this.dialogCanvas.planeDistance = this.originalPlaneDistance;

                return;
            }

            // Disable any objects that should not be shown in XR mode
            if (this.disableObjectsInXrMode.IsNullOrEmpty() == false)
            {
                foreach (var disableObject in this.disableObjectsInXrMode)
                {
                    if (disableObject != null)
                    {
                        disableObject.SetActive(false);
                    }
                }
            }

            // Start the Coroutine
            this.StartUpdatePositionCoroutine();

            // Ensuring the dialog is in WorldSpace mode and at the correct distance from the camera
            if (this.panelCanvas.renderMode != RenderMode.WorldSpace)
            {
                this.panelCanvas.planeDistance = 0;

                // Remembering the size the screen space canvas was driven to, since switching to world space doesn't keep it
                var canvasRectTransform = (RectTransform)this.panelCanvas.transform;
                Vector2 canvasSize = canvasRectTransform.rect.size;
                Debug.Log(canvasSize.x + ", " + canvasSize.y);

                this.ExecuteAtEndOfFrame(() =>
                {
                    this.panelCanvas.renderMode = RenderMode.WorldSpace;
                    this.panelCanvas.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

                    // Making sure the dialog is directly in front of the camera at the settings canvas distance
                    this.panelCanvas.transform.position =
                        this.panelCanvas.worldCamera.transform.position +
                        (this.panelCanvas.worldCamera.transform.forward.normalized * this.settings.CanvasDistanceForwards);

                    this.ExecuteAtEndOfFrame(() =>
                    {
                        // Restoring that size so the world space canvas (and anything sized from its rect) keeps the same layout
                        canvasRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, canvasSize.x);
                        canvasRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, canvasSize.y);
                    });
                });
            }

#if USING_UNITY_XR_INTERACTION_TOOLKIT
            if (this.trackedDeviceGraphicRaycaster == null)
            {
                this.trackedDeviceGraphicRaycaster = EditorUtil.GetOrAddComponent<TrackedDeviceGraphicRaycaster>(this);
            }

            if (this.isGrabable)
            {
                if (this.xrGrabInteractable == null)
                {
                    this.xrGrabInteractable = EditorUtil.GetOrAddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>(this);
                    this.xrGrabInteractable.selectEntered.AddListener((interactor) => this.hasBeenGrabbed = true);

                    this.xrGrabInteractable.attachEaseInTime = -1.0f;
                    this.xrGrabInteractable.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.Instantaneous;
                    this.xrGrabInteractable.throwOnDetach = false;
                    this.xrGrabInteractable.forceGravityOnDetach = false;

                    this.xrGrabInteractable.trackPosition = true;
                    this.xrGrabInteractable.smoothPosition = true;
                    this.xrGrabInteractable.smoothPositionAmount = 3.0f;
                    this.xrGrabInteractable.tightenPosition = 0.0f;

                    this.xrGrabInteractable.trackRotation = true;
                    this.xrGrabInteractable.smoothRotation = true;
                    this.xrGrabInteractable.smoothRotationAmount = 0.5f;
                    this.xrGrabInteractable.tightenRotation = 0.0f;

                    var rigidBody = this.GetComponent<Rigidbody>();
                    rigidBody.useGravity = false;
                    rigidBody.isKinematic = true;
                }
            }
#endif
        }

        private void OnHide()
        {
            this.StopUpdateCoroutine();
            this.hasBeenGrabbed = false;
        }

        private void StartUpdatePositionCoroutine()
        {
            this.StopUpdateCoroutine();

            this.updatePositionCoroutine = CoroutineRunner.Instance.StartCoroutine(Coroutine());

            IEnumerator Coroutine()
            {
                while (true)
                {
                    if (this.hasBeenGrabbed == false)
                    {
                        this.UpdatePosition();
                    }

                    yield return null;
                }
            }
        }

        private void StopUpdateCoroutine()
        {
            if (this.updatePositionCoroutine != null)
            {
                CoroutineRunner.Instance.StopCoroutine(this.updatePositionCoroutine);
                this.updatePositionCoroutine = null;
            }
        }
    }
}
