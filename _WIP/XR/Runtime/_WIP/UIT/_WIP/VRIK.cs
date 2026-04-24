//-----------------------------------------------------------------------
// <copyright file="VRIK.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

//// How to make a Body in VR - PART 1 (https://www.youtube.com/watch?v=tBYl-aSxUe0&t=368s)
//// NOTE [bgish]: As the avatar looks down at their feet, need to have them bend over a little bit


//// NOTE [bgish]: There is a bug in StyleCop that thinks that Local Functions need this prefix, so turning it off.
#pragma warning disable SA1101

#if USING_UNITY_XR_INTERACTION_TOOLKIT && USING_UNITY_ANIMATION_RIGGING

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using OGT.Haven;
    using OGT.XR;
    using UnityEngine;
    using UnityEngine.Animations.Rigging;
    using UnityEngine.InputSystem;

    public class VRIK : MonoBehaviour, IStart
    {
        private enum AvatarType
        {
            None,
            ReadyPlayerMe,
            Synty,
        }

#pragma warning disable 0649
        [Header("Required")]
        [SerializeField] private AvatarType avatarType;
        [SerializeField] private Transform root;
        [SerializeField] private Animator rigAnimator;
        [SerializeField] private Transform centerEye;
        [SerializeField] private bool calibrateAvatar;

        [Header("Auto Populated")]
        [SerializeField] private Vector3 headBodyOffset;
        [SerializeField] private MultiRotationConstraint[] backBends;
        [SerializeField] private BlendConstraint leftShoulder;
        [SerializeField] private BlendConstraint rightShoulder;
        [SerializeField] private float upBackBendWeight = 0.75f;
        [SerializeField] private float downBackBendWeight = 1.0f;
        [SerializeField] private Transform headConstraint;
        [SerializeField] private float turnSmoothness;
        [SerializeField] private VRMap head;
        [SerializeField] private VRMap leftHand;
        [SerializeField] private VRMap rightHand;
#pragma warning restore 0649

        private HavenRig rig;

        public void SetupRig()
        {
            if (this.root == null)
            {
                this.root = this.transform;
            }

            this.ResetRig();

            var rigNameToTransform = new Dictionary<string, Transform>();
            var humanNameToBoneName = new Dictionary<string, string>();

            // Making a quick dictionary lookup for transforms and their names
            foreach (var transform in this.root.GetComponentsInChildren<Transform>())
            {
                rigNameToTransform.Add(transform.name, transform);
            }

            // Setting up the Bone Renderer
            var boneTransforms = new List<Transform>();
            foreach (var humanBone in this.rigAnimator.avatar.humanDescription.human)
            {
                humanNameToBoneName.Add(humanBone.humanName, humanBone.boneName);
                boneTransforms.Add(rigNameToTransform[humanBone.boneName]);
            }

#if UNITY_EDITOR
            var boneRenderer = GetOrAddComponentToGameObject<BoneRenderer>(this.gameObject);
            boneRenderer.transforms = boneTransforms.ToArray();
#endif

            // Setting up Rig
            var vrConstraints = GetOrCreateChildGameObject(this.root.gameObject, "VR Constraints");
            var rig = GetOrAddComponentToGameObject<Rig>(vrConstraints);

            // Setting up Rig Builder
            var rigBuilder = GetOrAddComponentToGameObject<RigBuilder>(this.gameObject);

            if (rigBuilder.layers == null || rigBuilder.layers.Count == 0 || rigBuilder.layers[0].rig != rig)
            {
                rigBuilder.layers = new List<RigLayer> { new RigLayer(rig) };
            }

            // Setting Two Bone IK Constraints (Arms)
            SetupTwoBoneContraint(vrConstraints, "Right Arm IK", "RightUpperArm", "RightLowerArm", "RightHand", this.rightHand);
            SetupTwoBoneContraint(vrConstraints, "LeftArm IK", "LeftUpperArm", "LeftLowerArm", "LeftHand", this.leftHand);

            // Setting up Head Constraint
            var headBone = rigNameToTransform[humanNameToBoneName["Head"]];

            var headConstraint = GetOrCreateChildGameObject(vrConstraints, "Head Constraint");
            headConstraint.transform.SetPositionAndRotation(headBone.position, headBone.rotation);

            var multiParentConstraint = GetOrAddComponentToGameObject<MultiParentConstraint>(headConstraint);
            multiParentConstraint.data.constrainedObject = headBone;
            multiParentConstraint.data.sourceObjects = new WeightedTransformArray
            {
                new WeightedTransform { transform = headConstraint.transform, weight = 1.0f },
            };

            // Storing Head Constraint for Start/LateUpdate function
            this.headConstraint = headConstraint.transform;
            this.head.RigTarget = this.headConstraint;

            switch (this.avatarType)
            {
                case AvatarType.ReadyPlayerMe:
                    {
                        this.head.TrackingPositionOffset = new Vector3(0.0f, 0.0f, 0.0f);
                        this.head.TrackingRotationOffset = new Vector3(0.0f, 0.0f, 0.0f);

                        this.leftHand.TrackingPositionOffset = new Vector3(-0.02f, 0.0f, -0.1f);
                        this.leftHand.TrackingRotationOffset = new Vector3(165.2f, 254.57f, -81.2f);

                        this.rightHand.TrackingPositionOffset = new Vector3(0.02f, 0.0f, -0.1f);
                        this.rightHand.TrackingRotationOffset = new Vector3(-165.2f, -254.57f, 81.2f);

                        break;
                    }
                case AvatarType.Synty:
                    {
                        this.head.TrackingPositionOffset = new Vector3();
                        this.head.TrackingRotationOffset = new Vector3();
                        this.leftHand.TrackingPositionOffset = new Vector3();
                        this.leftHand.TrackingRotationOffset = new Vector3();
                        this.rightHand.TrackingPositionOffset = new Vector3();
                        this.rightHand.TrackingRotationOffset = new Vector3();
                        break;
                    }
                default:
                    {
                        this.head.TrackingPositionOffset = new Vector3();
                        this.head.TrackingRotationOffset = new Vector3();
                        this.leftHand.TrackingPositionOffset = new Vector3();
                        this.leftHand.TrackingRotationOffset = new Vector3();
                        this.rightHand.TrackingPositionOffset = new Vector3();
                        this.rightHand.TrackingRotationOffset = new Vector3();
                        break;
                    }
            }

            void SetupTwoBoneContraint(GameObject parent, string gameObjectName, string rootName, string midName, string tipName, VRMap vrMap)
            {
                var armIk = GetOrCreateChildGameObject(parent, gameObjectName);
                var armIkConstraint = GetOrAddComponentToGameObject<TwoBoneIKConstraint>(armIk);

                armIkConstraint.data.root = rigNameToTransform[humanNameToBoneName[rootName]];
                armIkConstraint.data.mid = rigNameToTransform[humanNameToBoneName[midName]];
                armIkConstraint.data.tip = rigNameToTransform[humanNameToBoneName[tipName]];

                // Setup Target
                var target = GetOrCreateChildGameObject(armIk, "Target");
                target.transform.SetPositionAndRotation(armIkConstraint.data.tip.position, armIkConstraint.data.tip.rotation);
                armIkConstraint.data.target = target.transform;

                // Setup Hint
                Vector3 rootToMid = armIkConstraint.data.root.position - armIkConstraint.data.mid.position;
                Vector3 tipToMid = armIkConstraint.data.tip.position - armIkConstraint.data.mid.position;
                Vector3 average = (rootToMid.normalized + tipToMid.normalized) / -2.0f;
                Vector3 hintPosition = armIkConstraint.data.mid.position + (average * 0.1f);

                var hint = GetOrCreateChildGameObject(armIk, "Hint");
                hint.transform.position = hintPosition;
                armIkConstraint.data.hint = hint.transform;

                // Updating VR Map object
                vrMap.RigTarget = target.transform;
            }

            T GetOrAddComponentToGameObject<T>(GameObject gameObject)
                where T : Component
            {
                var component = gameObject.GetComponent<T>();

                if (component == null)
                {
                    return gameObject.AddComponent<T>();
                }

                return component;
            }

            GameObject GetOrCreateChildGameObject(GameObject gameObject, string name)
            {
                var childTransform = gameObject.transform.Find(name);

                if (childTransform == null)
                {
                    childTransform = new GameObject(name).transform;
                    childTransform.SetParent(gameObject.transform);
                    childTransform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    childTransform.localScale = Vector3.one;
                }

                return childTransform.gameObject;
            }
        }

        public void ResetRig()
        {
            if (this.root != null)
            {
                var constraints = this.root.Find("VR Constraints");

                if (constraints)
                {
                    GameObject.DestroyImmediate(constraints.gameObject);
                }
            }

            this.headConstraint = null;
            this.turnSmoothness = 2.0f;
            this.head = new VRMap();
            this.leftHand = new VRMap();
            this.rightHand = new VRMap();

            var boneRenderer = this.GetComponent<BoneRenderer>();
            var rigBuilder = this.GetComponent<RigBuilder>();

            if (boneRenderer)
            {
                GameObject.DestroyImmediate(boneRenderer);
            }

            if (rigBuilder)
            {
                GameObject.DestroyImmediate(rigBuilder);
            }
        }

        public void OnStart()
        {
            if (XRManager.Instance.CurrentDevice.XRType == XRType.VRHeadset)
            {
                this.rig = HavenRig.Instance;
                this.enabled = true;
            }
        }

        private void Awake()
        {
            ActivationManager.Register(this);
            this.enabled = false;
        } 

        private HeadOffsets headOffsets = new HeadOffsets();
        private bool isDoneCalibrating = false;

        private void LateUpdate()
        {
            var cameraForward = Camera.main.transform.forward;
            var headForward = cameraForward.SetY(0).normalized;
            var dot = Mathf.Clamp01(Vector3.Dot(headForward, cameraForward));
            var isLookingUp = cameraForward.y >= 0;

            if (this.calibrateAvatar && this.headOffsets.IsCalibrating)
            {
                this.headOffsets.Add(dot, isLookingUp, Camera.main.transform.position);
                this.isDoneCalibrating = true;
                
                //// this.head.TrackingPositionOffset = new Vector3(0.0f, 0.0f, -this.centerEye.localPosition.z);
                //// this.leftHand.TrackingPositionOffset = new Vector3(0.0f, 0.0f, -this.centerEye.localPosition.z);
                //// this.rightHand.TrackingPositionOffset = new Vector3(0.0f, 0.0f, -this.centerEye.localPosition.z);
            
                return;
            }
            
            if (this.isDoneCalibrating)
            {
                this.isDoneCalibrating = false;
                this.headBodyOffset = new Vector3(0.0f, -this.headOffsets.GetCameraHeight(), -this.centerEye.localPosition.z);
            }

            var currentOffset = this.headOffsets.GetHeadOffset(dot, isLookingUp);

            this.transform.position = this.head.RigTarget.position + (this.headBodyOffset - currentOffset);

            //// this.transform.forward = Vector3.Lerp(
            ////     this.transform.forward,
            ////     this.rig.HeadTransform.forward.SetY(0).normalized,
            ////     Time.deltaTime * this.turnSmoothness);

            this.head.Map(this.rig.HeadTransform, new Vector3());
            this.leftHand.Map(this.rig.LeftHandTransform, new Vector3());
            this.rightHand.Map(this.rig.RightHandTransform, new Vector3());

            // Remapping -1, 1 to 0, 1 (where 0 is looking down and 1 is looking up)
            float t = (Mathf.Clamp(cameraForward.y, -1.0f, 1.0f) + 1.0f) / 2.0f;
            float weight = Mathf.Lerp(this.downBackBendWeight, this.upBackBendWeight, t);

            for (int i = 0; i < this.backBends.Length; i++)
            {
                this.backBends[i].weight = weight;
            }
        }

        [Serializable]
        private class VRMap
        {
#pragma warning disable 0649, CA2235
            [SerializeField] private Transform rigTarget;
            [SerializeField] private Vector3 trackingPositionOffset;
            [SerializeField] private Vector3 trackingRotationOffset;
#pragma warning restore 0649, CA2235

            public Transform RigTarget
            {
                get => this.rigTarget;
                set => this.rigTarget = value;
            }

            public Vector3 TrackingPositionOffset
            {
                get => this.trackingPositionOffset;
                set => this.trackingPositionOffset = value;
            }

            public Vector3 TrackingRotationOffset
            {
                get => this.trackingRotationOffset;
                set => this.trackingRotationOffset = value;
            }

            public void Map(Transform targetTransform, Vector3 currentOffset)
            {
                this.rigTarget.SetPositionAndRotation(
                    targetTransform.TransformPoint(this.trackingPositionOffset + currentOffset),
                    targetTransform.rotation * Quaternion.Euler(this.trackingRotationOffset));
            }
        }

        private class HeadOffsets
        {
            // Index 0 = Straight Up, Index[100] = Straight Ahead
            private HeadPosition[] upHeadPositions = new HeadPosition[100];

            // Index 0 = Straight Down, Index[100] = Straight Ahead
            private HeadPosition[] downHeadPositions = new HeadPosition[100];

            public bool IsCalibrating { get; private set; } = true;

            private int elementsSet = 0;

            public void Add(float dot, bool up, Vector3 offset)
            {
                int index = this.GetIndex(dot);

                if (index >= 100)
                {
                    return;
                }

                if (up)
                {
                    if (this.upHeadPositions[index].HasBeenSet == false)
                    {
                        this.elementsSet++;
                    }

                    this.upHeadPositions[index] = new HeadPosition { HasBeenSet = true, Offset = offset };
                }
                else
                {
                    if (this.downHeadPositions[index].HasBeenSet == false)
                    {
                        this.elementsSet++;
                    }

                    this.downHeadPositions[index] = new HeadPosition { HasBeenSet = true, Offset = offset };
                }

                if (this.IsCalibrating && this.elementsSet >= 195)
                {
                    this.IsCalibrating = false;
                    this.CleanupData();
                }
            }

            public Vector3 GetHeadOffset(float dot, bool up)
            {
                //// TODO [bgish]: Get the two closest indexes and interpolate between them

                var index = this.GetIndex(dot);
                var forwardPosition = this.upHeadPositions[99].Offset;
                var currentPosition = up ? this.upHeadPositions[index].Offset : this.downHeadPositions[index].Offset;

                return currentPosition - forwardPosition;
            }

            public float GetCameraHeight()
            {
                return this.upHeadPositions[99].Offset.y;
            }

            private void CleanupData()
            {
                //// TODO [bgish]: IMPLEMENT!
            }

            private int GetIndex(float dot) => (int)(dot * 100.0f);

            public struct HeadPosition
            {
                public bool HasBeenSet;
                public Vector3 Offset;
            }
        }
    }
}

#endif
