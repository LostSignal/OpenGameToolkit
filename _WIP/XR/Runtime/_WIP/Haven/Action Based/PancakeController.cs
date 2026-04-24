#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="PancakeController.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !USING_UNITY_XR_INTERACTION_TOOLKIT
#pragma warning disable CS0414, IDE0051
#endif

namespace OGT.Haven
{
    using System.Collections;
    using OGT;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.XR;
    using UnityEngine.UI;

    //// NOTE [bgish]:  In ActionBasedControllerManager.cs in OnUpdateTeleportState function, need to delay the
    ////                TransitionState call by one frame if you want releasing a key to cancel and select teleport

    //// TODO [bgish]: Disable Right Hand XR Direct Interactor (Not usable in Pancake Mode)
    //// TODO [bgish]: When Enter Teleport Mode, make a global event so Teleport Anchors can reveal themselves (like Bigscreen)
    //// TODO [bgish]: Need to update TeleportAnchors to have a Seated vs Standing option
    //// TODO [bgish]: Need to figure out how to handle Seated vs Standing
    //// TODO [bgish]: Need turn off all to figure out how to handle Seated vs Standing

    public class PancakeController : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private InputActionAsset pancakeInputActions;
        [SerializeField] private bool disableControllerModels = true;
        [SerializeField] private bool disableLineRenderer = true;

        [Header("Mouse")]
        [SerializeField] private float moveSpeed = 0.5f;
        [SerializeField] private float shiftSpeed = 1.5f;
        [SerializeField] private float mouseSensitivity = 0.1f;

        [Header("Teleport")]
        [SerializeField] private Vector3 teleportControllerOffset = new Vector3(0.25f, -0.3f, 0.0f);
        [SerializeField] private Vector3 teleportLookAtForward = new Vector3(0.0f, 0.5f, 1.0f);
        [SerializeField] private float teleportVelocity = 6.5f;
        [SerializeField] private float teleportGravity = 9.8f;

        [Header("Reticle")]
        [SerializeField] private Canvas reticleCanvas;
        [SerializeField] private Image reticleImage;
        [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor reticleRayInteractor;
        [SerializeField] private Color reticleEnabledColor = Color.green;
        [SerializeField] private Color reticleDisabledColor = Color.gray;
#pragma warning restore 0649

        //// // Head
        //// private TrackedPoseDriver headTrackedPoseDriver;
        //// private Transform headTransform;
        ////
        //// // Left Hand
        //// private Transform leftHand;
        //// private ActionBasedControllerManager leftHandActionManager;
        //// private XRRayInteractor leftHandTeleportXRRayInteractor;
        //// private ActionBasedController leftHandBase;
        //// private ActionBasedController leftHandTeleport;
        ////
        //// // Right Hand
        //// private Transform rightHand;
        //// private ActionBasedControllerManager rightHandActionManager;
        //// private XRRayInteractor rightHandTeleportXRRayInteractor;
        //// private ActionBasedController rightHandBase;
        //// private ActionBasedController rightHandTeleport;
        ////
        //// // Mouse Tracking
        //// private Vector2 initialMousePosotion;
        //// private bool isMouseDown;
        //// private float rotationX;
        //// private float rotationY;
        //// private bool hasFocus = true;
        ////
        //// private void Awake()
        //// {
        ////     // Making sure reticle is off by default
        ////     this.SelectExited(null);
        //// }
        ////
        //// private void Start()
        //// {
        ////     this.rotationX = Camera.main.transform.localRotation.eulerAngles.x;
        ////     this.rotationY = Camera.main.transform.localRotation.eulerAngles.y;
        ////
        ////     // Head
        ////     this.headTrackedPoseDriver = this.GetComponentInChildren<TrackedPoseDriver>();
        ////     this.headTrackedPoseDriver.enabled = false;
        ////     this.headTransform = this.headTrackedPoseDriver.transform;
        ////
        ////     var actionBasedControllerManagers = this.GetComponentsInChildren<ActionBasedControllerManager>();
        ////
        ////     // Left Hand
        ////     this.leftHand = actionBasedControllerManagers[0].transform;
        ////     this.leftHandActionManager = actionBasedControllerManagers[0];
        ////
        ////     var leftActionBasedControllers = this.leftHand.GetComponentsInChildren<ActionBasedController>(true);
        ////     this.leftHandBase = leftActionBasedControllers[0];
        ////     this.leftHandTeleport = leftActionBasedControllers[1];
        ////
        ////     // Right Hand
        ////     this.rightHand = actionBasedControllerManagers[1].transform;
        ////     this.rightHandActionManager = actionBasedControllerManagers[1];
        ////
        ////     var rightActionBasedControllers = this.rightHand.GetComponentsInChildren<ActionBasedController>(true);
        ////     this.rightHandBase = rightActionBasedControllers[0];
        ////     this.rightHandTeleport = rightActionBasedControllers[1];
        ////
        ////     // Left Hand XR Ray Interactor
        ////     this.leftHandTeleportXRRayInteractor = this.leftHandBase.GetComponent<XRRayInteractor>();
        ////
        ////     // Right Hand Teleport XR Ray Interactor
        ////     this.rightHandTeleportXRRayInteractor = this.rightHandTeleport.GetComponent<XRRayInteractor>();
        ////     this.rightHandTeleportXRRayInteractor.acceleration = this.teleportGravity;
        ////     this.rightHandTeleportXRRayInteractor.velocity = this.teleportVelocity;
        ////
        ////     // Overriding Input Actions to use Pancake Versions
        ////     if (this.pancakeInputActions != null)
        ////     {
        ////         this.rightHandActionManager.teleportModeActivate.Set(this.pancakeInputActions.FindAction("Teleport Activate"));
        ////         this.rightHandActionManager.teleportModeCancel.Set(this.pancakeInputActions.FindAction("Teleport Cancel"));
        ////         this.rightHandTeleport.selectAction = new InputActionProperty(this.pancakeInputActions.FindAction("Teleport Select"));
        ////
        ////         this.leftHandBase.selectAction = new InputActionProperty(this.pancakeInputActions.FindAction("Select"));
        ////         this.leftHandBase.activateAction = new InputActionProperty(this.pancakeInputActions.FindAction("Activate"));
        ////         this.leftHandBase.translateAnchorAction = new InputActionProperty(this.pancakeInputActions.FindAction("Translate Anchor"));
        ////     }
        ////
        ////     if (this.leftHandTeleportXRRayInteractor)
        ////     {
        ////         this.leftHandTeleportXRRayInteractor.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.Toggle;
        ////     }
        ////
        ////     // Turning off all the hand meshes
        ////     if (this.disableControllerModels)
        ////     {
        ////         this.leftHandBase.modelPrefab = null;
        ////         this.leftHandTeleport.modelPrefab = null;
        ////         this.rightHandBase.modelPrefab = null;
        ////         this.rightHandTeleport.modelPrefab = null;
        ////
        ////         DestoryControllerModel(this.leftHandBase);
        ////         DestoryControllerModel(this.leftHandTeleport);
        ////         DestoryControllerModel(this.rightHandBase);
        ////         DestoryControllerModel(this.rightHandTeleport);
        ////     }
        ////
        ////     // Making sure the main xr ray interactor doesn't have a line renderer
        ////     if (this.disableLineRenderer)
        ////     {
        ////         this.StartCoroutine(DisableLineRenderer());
        ////     }
        ////
        ////     // Setting up the reticle events
        ////     this.reticleRayInteractor.hoverEntered.AddListener(this.HoverEntered);
        ////     this.reticleRayInteractor.hoverExited.AddListener(this.HoverExited);
        ////     this.reticleRayInteractor.selectEntered.AddListener(this.SelectEntered);
        ////     this.reticleRayInteractor.selectExited.AddListener(this.SelectExited);
        ////
        ////     IEnumerator DisableLineRenderer()
        ////     {
        ////         yield return null;
        ////
        ////         var lineVisual = this.leftHandBase.GetComponent<XRInteractorLineVisual>();
        ////
        ////         if (lineVisual)
        ////         {
        ////             lineVisual.enabled = false;
        ////         }
        ////
        ////         var lineRenderer = this.leftHandBase.GetComponent<LineRenderer>();
        ////
        ////         if (lineRenderer)
        ////         {
        ////             lineRenderer.enabled = false;
        ////         }
        ////     }
        ////
        ////     void DestoryControllerModel(ActionBasedController controller)
        ////     {
        ////         for (int i = 0; i < controller.transform.childCount; i++)
        ////         {
        ////             var child = controller.transform.GetChild(i);
        ////
        ////             if (child.name.EndsWith(" Model"))
        ////             {
        ////                 child.DestroyAllChildren();
        ////             }
        ////         }
        ////     }
        //// }
        ////
        //// private void Update()
        //// {
        ////     this.UpdateHeadLook(this.headTransform);
        ////     this.UpdateMovement(this.transform, this.headTransform);
        ////     this.UpdateControllerPositionAndRotations();
        ////
        ////     // Moving the attach transform down a little so it's easy to view over the object your holding
        ////     if (this.leftHandTeleportXRRayInteractor)
        ////     {
        ////         var attachPosition = this.leftHandTeleportXRRayInteractor.attachTransform.localPosition;
        ////         attachPosition.y = -0.1f;
        ////         this.leftHandTeleportXRRayInteractor.attachTransform.localPosition = attachPosition;
        ////     }
        //// }
        ////
        //// private void OnApplicationFocus(bool focus)
        //// {
        ////     if (Time.realtimeSinceStartup > 1.0f)
        ////     {
        ////         this.hasFocus = focus;
        ////     }
        //// }
        ////
        //// private void UpdateHeadLook(Transform headTransform)
        //// {
        ////     Mouse mouse = Mouse.current;
        ////     Vector3 currentMousePosition = mouse.position.ReadValue();
        ////     Vector3 mouseDelta = mouse.delta.ReadValue();
        ////
        ////     if (mouse.rightButton.isPressed && this.isMouseDown == false)
        ////     {
        ////         this.isMouseDown = true;
        ////         this.initialMousePosotion = currentMousePosition;
        ////         Cursor.visible = false;
        ////     }
        ////     else if (mouse.rightButton.isPressed == false && this.isMouseDown)
        ////     {
        ////         this.isMouseDown = false;
        ////         Mouse.current.WarpCursorPosition(this.initialMousePosotion);
        ////         Cursor.visible = true;
        ////     }
        ////     else if (this.isMouseDown)
        ////     {
        ////         // TODO [bgish]: Look at the main camera's FOV and Camera.main.pixelRect to determin
        ////         //               how many degress we actually moved based on pixels moved.
        ////         this.rotationX -= mouseDelta.y * this.mouseSensitivity;
        ////         this.rotationY += mouseDelta.x * this.mouseSensitivity;
        ////         headTransform.localRotation = Quaternion.Euler(this.rotationX, this.rotationY, 0.0f);
        ////
        ////         // HACK [bgish]: Shouldn't have to do this every frame, but there is a bug in Unity right now
        ////         // https://forum.unity.com/threads/cant-hide-the-cursor.607762/?_ga=2.94768348.1728414185.1606752938-216733124.1587419502
        ////         Cursor.visible = false;
        ////
        ////         // Warping the mouse back to the center of the screen if it leaves the game view
        ////         if (currentMousePosition.x < 1 || currentMousePosition.x > Screen.width - 1 ||
        ////             currentMousePosition.y < 1 || currentMousePosition.y > Screen.height - 1)
        ////         {
        ////             if (this.hasFocus)
        ////             {
        ////                 Mouse.current.WarpCursorPosition(new Vector2(Screen.width / 2, Screen.height / 2));
        ////             }
        ////         }
        ////     }
        //// }
        ////
        //// private void UpdateMovement(Transform translationTransform, Transform rotationTransform)
        //// {
        ////     var keyboard = Keyboard.current;
        ////     var movementVector = Vector3.zero;
        ////     float speed = (keyboard.leftShiftKey.isPressed ? this.shiftSpeed : this.moveSpeed) * Time.deltaTime;
        ////
        ////     Vector3 forward = new Vector3(0, 0, 1);
        ////     Vector3 right = new Vector3(1, 0, 0);
        ////
        ////     // Forward / Back
        ////     if (keyboard.wKey.isPressed)
        ////     {
        ////         movementVector += forward;
        ////     }
        ////
        ////     if (keyboard.sKey.isPressed)
        ////     {
        ////         movementVector -= forward;
        ////     }
        ////
        ////     // Left / Right
        ////     if (keyboard.aKey.isPressed)
        ////     {
        ////         movementVector -= right;
        ////     }
        ////
        ////     if (keyboard.dKey.isPressed)
        ////     {
        ////         movementVector += right;
        ////     }
        ////
        ////     //// NOTE [bgish]: This probably needs to update the CameraOffset and not the movementVector
        ////     //// // Up / Down
        ////     //// if (keyboard.qKey.isPressed)
        ////     //// {
        ////     ////     movementVector -= Vector3.up;
        ////     //// }
        ////     ////
        ////     //// if (keyboard.eKey.isPressed)
        ////     //// {
        ////     ////     movementVector += Vector3.up;
        ////     //// }
        ////
        ////     Vector3 finalMovement = rotationTransform.rotation * movementVector;
        ////     finalMovement.y = 0;
        ////     translationTransform.position += finalMovement.normalized * speed;
        //// }
        ////
        //// private void UpdateControllerPositionAndRotations()
        //// {
        ////     this.leftHand.transform.rotation = this.headTransform.rotation;
        ////
        ////     this.rightHand.transform.SetPositionAndRotation(
        ////         this.headTransform.position + (this.headTransform.rotation * this.teleportControllerOffset),
        ////         this.headTransform.rotation * Quaternion.LookRotation(this.teleportLookAtForward));
        //// }
        ////
        //// private void HoverEntered(HoverEnterEventArgs hoverEnterEventArgs)
        //// {
        ////     this.reticleImage.color = this.reticleEnabledColor;
        //// }
        ////
        //// private void HoverExited(HoverExitEventArgs hoverExitEventArgs)
        //// {
        ////     this.reticleImage.color = this.reticleDisabledColor;
        //// }
        ////
        //// private void SelectEntered(SelectEnterEventArgs selectEnterEventArgs)
        //// {
        ////     this.reticleCanvas.enabled = false;
        //// }
        ////
        //// private void SelectExited(SelectExitEventArgs selectExitEventArgs)
        //// {
        ////     this.reticleCanvas.enabled = true;
        //// }
    }
}
