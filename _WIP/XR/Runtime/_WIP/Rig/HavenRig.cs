//-----------------------------------------------------------------------
// <copyright file="HavenRig.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace OGT.Haven
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using OGT.XR;
    using OGT.Properties;
    using Unity.XR.CoreUtils;
    using UnityEngine;
    using UnityEngine.XR;
    using UnityEngine.XR.Interaction.Toolkit;

    public class HavenRig : MonoBehaviour, IAwake
    {
        private static readonly Vector3 Zero = new(0.0f, 0.0f, 0.0f);
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        private static HavenRig instance;

#pragma warning disable 0649
        [Header("Settings")]
        [SerializeField] private FloatProperty height;
        [SerializeField] private FloatProperty eyeHeight;
        [SerializeField] private EnumProperty sitStand;
        [SerializeField] private EnumProperty movementMode;

        [SerializeField] private BoolProperty enableStrafe;
        [SerializeField] private EnumProperty movementHand;
        [SerializeField] private EnumProperty movementSource;

        [SerializeField] private EnumProperty turnMode;
        [SerializeField] private BoolProperty allowTurnAround;
        [SerializeField] private FloatProperty snapTurnDegrees;
        [SerializeField] private FloatProperty continuousTurnSpeed;
        [SerializeField] private float movementSpeed;

        [Header("Providers")]
        [SerializeField] private HavenSnapTurnProvider snapTurnProvider;
        [SerializeField] private HavenContinuousTurnProvider continuousTurnProvider;
        [SerializeField] private HavenContinuousMoveProvider continuousMoveProvider;
        [SerializeField] private CharacterControllerDriver characterControllerDriver;
        [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider teleportationProvider;
        [SerializeField] private CharacterController characterController;

        [Header("Hands")]
        [SerializeField] private HavenHand leftHand;
        [SerializeField] private HavenHand rightHand;

        [Header("Transforms")]
        [SerializeField] private Transform headTransform;
        [SerializeField] private Transform leftHandTransform;
        [SerializeField] private Transform rightHandTransform;

        [Header("XR")]
        [SerializeField] private XROrigin xrOrigin;
        [SerializeField] private XRUtilManager xrUtilManager;
#pragma warning restore 0649

        private Dictionary<long, XRController> controllerCache = new();

        private bool isClimbingWithLeftHand;
        private bool isClimbingWithRightHand;

        private Vector3 leftHandPreviousPosition;
        private Vector3 rightHandPreviousPosition;

        private SitState currentSitStandState;

        private enum SitState
        {
            Uninitialized,
            Sit,
            Stand
        }

        public static HavenRig Instance
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => instance;
        }

        public float RigScale
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => 1.0f;
        }

        public UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationProvider TeleportationProvider
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.teleportationProvider;
        }

        public Transform HeadTransform
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.headTransform;
        }

        public Transform LeftHandTransform
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.leftHandTransform;
        }

        public Transform RightHandTransform
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.rightHandTransform;
        }

        public HavenHand LeftHand
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.leftHand;
        }

        public HavenHand RightHand
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.rightHand;
        }

        public HavenHand GetHand(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor)
        {
            return this.leftHand.HasInteractor(interactor) ? this.leftHand :
                this.rightHand.HasInteractor(interactor) ? this.rightHand :
                null;
        }

        public void SendHapticImpluse(UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor, float amplitude, float duration)
        {
            long entityId = interactor.GetEntityId();

            if (this.controllerCache.TryGetValue(entityId, out XRController controller) == false)
            {
                bool isHandInteractor = this.rightHand.HasInteractor(interactor) || this.leftHand.HasInteractor(interactor);

                if (isHandInteractor && interactor.TryGetComponent(out controller))
                {
                    this.controllerCache.Add(entityId, controller);
                }
                else
                {
                    this.controllerCache.Add(entityId, null);
                }
            }

            if (controller != null && controller.inputDevice.TryGetHapticCapabilities(out HapticCapabilities capabilities) && capabilities.supportsImpulse)
            {
                uint channel = 0;
                controller.inputDevice.SendHapticImpulse(channel, amplitude, duration);
            }
        }

        public void OnAwake(Bootloader bootloader)
        {
            if (instance != null)
            {
                Logger.LogError("Multiple Rigs Detected!", this);
                return;
            }

            instance = this;

            this.height.OnChange += this.OnSettingsChanged;
            this.eyeHeight.OnChange += this.OnSettingsChanged;
            this.sitStand.OnChange += this.OnSettingsChanged;
            this.movementMode.OnChange += this.OnSettingsChanged;
            this.enableStrafe.OnChange += this.OnSettingsChanged;
            this.movementHand.OnChange += this.OnSettingsChanged;
            this.movementSource.OnChange += this.OnSettingsChanged;
            this.turnMode.OnChange += this.OnSettingsChanged;
            this.allowTurnAround.OnChange += this.OnSettingsChanged;
            this.snapTurnDegrees.OnChange += this.OnSettingsChanged;
            this.continuousTurnSpeed.OnChange += this.OnSettingsChanged;

            this.xrUtilManager.OnXRDeviceChange += this.InitialSitStandUpdate;

            this.OnSettingsChanged();
            this.InitialSitStandUpdate();
        }

        private void Awake() => ActivationManager.Register(this);

        private void OnDestroy()
        {
            this.height.OnChange -= this.OnSettingsChanged;
            this.eyeHeight.OnChange -= this.OnSettingsChanged;
            this.sitStand.OnChange -= this.OnSettingsChanged;
            this.movementMode.OnChange -= this.OnSettingsChanged;
            this.enableStrafe.OnChange -= this.OnSettingsChanged;
            this.movementHand.OnChange -= this.OnSettingsChanged;
            this.movementSource.OnChange -= this.OnSettingsChanged;
            this.turnMode.OnChange -= this.OnSettingsChanged;
            this.allowTurnAround.OnChange -= this.OnSettingsChanged;
            this.snapTurnDegrees.OnChange -= this.OnSettingsChanged;
            this.continuousTurnSpeed.OnChange -= this.OnSettingsChanged;

            this.xrUtilManager.OnXRDeviceChange -= this.InitialSitStandUpdate;
        }

        private void Update()
        {
            this.UpdateClimbing();
        }

        private void UpdateClimbing()
        {
            if (this.isClimbingWithLeftHand || this.isClimbingWithRightHand)
            {
                Vector3 offset = Zero;

                if (this.isClimbingWithLeftHand)
                {
                    Vector3 leftHandPosition = this.leftHandTransform.position;
                    offset += this.leftHandPreviousPosition - leftHandPosition;
                    this.leftHandPreviousPosition = leftHandPosition;
                }

                if (this.isClimbingWithRightHand)
                {
                    Vector3 rightHandPosition = this.rightHandTransform.position;
                    offset += this.rightHandPreviousPosition - rightHandPosition;
                    this.rightHandPreviousPosition = rightHandPosition;
                }

                this.characterController.Move(offset);
                this.leftHandPreviousPosition += offset;
                this.rightHandPreviousPosition += offset;
            }
        }

        public void StartClimbing(Hand hand)
        {
            if (hand == Hand.Left)
            {
                this.isClimbingWithLeftHand = true;
                this.leftHandPreviousPosition = this.leftHandTransform.position;
            }
            else if (hand == Hand.Right)
            {
                this.isClimbingWithRightHand = true;
                this.rightHandPreviousPosition = this.rightHandTransform.position;
            }
            else
            {
                throw new NotImplementedException();
            }

            // Disable all other movement when in climbing mode
            this.continuousMoveProvider.enabled = false;
            this.continuousTurnProvider.enabled = false;
            this.snapTurnProvider.enabled = false;
        }

        public void StopClimbing(Hand hand)
        {
            if (hand == Hand.Left)
            {
                this.isClimbingWithLeftHand = false;
            }
            else if (hand == Hand.Right)
            {
                this.isClimbingWithRightHand = false;
            }
            else
            {
                throw new NotImplementedException();
            }

            if (this.isClimbingWithLeftHand == false && this.isClimbingWithRightHand == false)
            {
                this.UpdateProviderSettings(this.GetSettings());
            }
        }

        public void CalibrateByHeight()
        {
            CoroutineRunner.Instance.StartCoroutine(Coroutine());

            IEnumerator Coroutine()
            {
                var controllersState = XRControllersState.Instance;

                while (true)
                {
                    controllersState.UpdateValues();

                    float cameraHeight = Camera.main.transform.position.y;

                    this.eyeHeight.Value = cameraHeight;
                    this.height.Value = cameraHeight + 0.1143f;

                    if (controllersState.IsAnyButtonDown())
                    {
                        break;
                    }
                    else
                    {
                        yield return null;
                    }
                }
            }
        }

        public void CalibrateByArmSpan()
        {
            CoroutineRunner.Instance.StartCoroutine(Coroutine());

            IEnumerator Coroutine()
            {
                var controllersState = XRControllersState.Instance;

                while (true)
                {
                    controllersState.UpdateValues();

                    float armSpan = Vector3.Distance(controllersState.LeftPosition, controllersState.RightPosition);
                    float estimatedHeight = armSpan + 0.21336f;
                    float eyeHeight = armSpan + .09906f;

                    this.eyeHeight.Value = eyeHeight;
                    this.height.Value = estimatedHeight;

                    if (controllersState.IsAnyButtonDown())
                    {
                        break;
                    }
                    else
                    {
                        yield return null;
                    }
                }
            }
        }

        private void UpdateSitStand(bool forceUpdate = false)
        {
            SitState settingsSitState = this.sitStand.CurrentValueIndex == 0 ? SitState.Sit : SitState.Stand;

            if (this.currentSitStandState == settingsSitState && forceUpdate == false)
            {
                return;
            }

            this.currentSitStandState = settingsSitState;

            if (this.xrOrigin.CurrentTrackingOriginMode == TrackingOriginModeFlags.Device)
            {
                this.xrOrigin.CameraYOffset = this.eyeHeight.Value;
            }
            else if (this.xrOrigin.CurrentTrackingOriginMode == TrackingOriginModeFlags.Floor)
            {
                this.xrOrigin.CameraYOffset = 0.0f;

                float offset = settingsSitState == SitState.Sit ?
                    this.eyeHeight.Value - this.headTransform.localPosition.y :
                    0.0f;

                var offsetObjectTransform = this.xrOrigin.CameraFloorOffsetObject.transform;
                offsetObjectTransform.localPosition = offsetObjectTransform.localPosition.SetY(offset);
            }
        }

        private void InitialSitStandUpdate()
        {
            CoroutineRunner.Instance.StartCoroutine(Coroutine());

            IEnumerator Coroutine()
            {
                float currentY = this.headTransform.localPosition.y;

                while (currentY < 0.5f)
                {
                    yield return null;

                    currentY = this.headTransform.localPosition.y;
                }

                this.UpdateSitStand(true);
            }
        }


        private void OnSettingsChanged(bool oldValue, bool newValue) => this.OnSettingsChanged();
        private void OnSettingsChanged(float oldValue, float newValue) => this.OnSettingsChanged();
        private void OnSettingsChanged(EnumValue oldValue, EnumValue newValue) => this.OnSettingsChanged();

        private void OnSettingsChanged()
        {
            var settings = this.GetSettings();

            // Updating settings for all providers
            this.UpdateProviderSettings(settings);

            // Enabling / Disabling Teleport and Turn Around
            if (settings.MovementMode == MovementMode.ContinuousOnly)
            {
                this.leftHand.DisableTeleport = true;
                this.rightHand.DisableTeleport = true;
            }
            else if (settings.MovementMode == MovementMode.ContinuousAndTeleport)
            {
                this.leftHand.DisableTeleport = settings.MovementHand == Hand.Left;
                this.rightHand.DisableTeleport = settings.MovementHand == Hand.Right;
            }
            else if (settings.MovementMode == MovementMode.TeleportOnly)
            {
                this.leftHand.DisableTeleport = false;
                this.rightHand.DisableTeleport = false;
            }
            else
            {
                throw new NotImplementedException();
            }

            if (this.currentSitStandState != SitState.Uninitialized)
            {
                this.UpdateSitStand();
            }
        }

        private void UpdateProviderSettings(HavenRigSettings settings)
        {
            this.snapTurnProvider.UpdateSettings(settings, this.leftHand, this.rightHand);

            this.continuousTurnProvider.UpdateSettings(settings, this.leftHand, this.rightHand);

            this.continuousMoveProvider.UpdateSettings(
                settings,
                this.movementSpeed,
                this.headTransform,
                this.leftHand,
                this.rightHand);
        }

        private HavenRigSettings GetSettings()
        {
            return new HavenRigSettings
            {
                // General
                MovementMode = this.movementMode.CurrentValueIndex == 0 ? MovementMode.ContinuousAndTeleport :
                               this.movementMode.CurrentValueIndex == 1 ? MovementMode.ContinuousOnly :
                                MovementMode.TeleportOnly,

                // Movement
                AllowStrafe = this.enableStrafe.Value,

                MovementHand = this.movementHand.CurrentValueIndex == 0 ? Hand.Left : Hand.Right,

                MovementSource = this.movementSource.CurrentValueIndex == 0 ? ContinuousMovementSource.Head :
                                 this.movementSource.CurrentValueIndex == 1 ? ContinuousMovementSource.RightHand :
                                 ContinuousMovementSource.LeftHand,

                // Turning
                TurnMode = this.turnMode.CurrentValueIndex == 0 ? TurnMode.Snap : TurnMode.Continuous,

                ContinuousTurnSpeed = this.continuousTurnSpeed.Value,
                SnapTurnAmmount = this.snapTurnDegrees.Value,
                EnableTurnAround = this.allowTurnAround.Value,
            };
        }
    }
}

#endif
