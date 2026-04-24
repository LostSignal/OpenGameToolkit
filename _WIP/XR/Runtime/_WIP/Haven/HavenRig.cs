#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenRig.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if !USING_UNITY_XR_INTERACTION_TOOLKIT
#pragma warning disable CS0414, IDE0051
#endif

/*namespace OGT.Haven
{
    using System.Collections;
    using OGT;
    using OGT.XR;
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;
    using global::Unity.XR.CoreUtils;

    public class HavenRig : MonoBehaviour
    {
        public const string TelportLayer = "Teleport";

        private static HavenRig instance;

#pragma warning disable 0649
        [Header("Rigs")]
        [SerializeField] private XROrigin genericXRRig;
        [SerializeField] private Camera rigCamera;
        [SerializeField] private Transform leftController;
        [SerializeField] private Transform rightController;
        [SerializeField] private PancakeController pancakeController;
#pragma warning restore 0649

        // [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "Must register for events.")]
        // static HavenRig()
        // {
        //     Platform.OnReset += Reset;
        //
        //     static void Reset()
        //     {
        //         instance = null;
        //     }
        // }

        public Camera RigCamera => this.rigCamera;

        public Transform LeftController => this.leftController;

        public Transform RightController => this.rightController;

        public float RigScale => this.genericXRRig.transform.localScale.x;

        public static IEnumerator WaitForRig()
        {
            while (instance == null)
            {
                yield return null;
            }
        }

        public static HavenRig GetRig()
        {
            return instance;
        }

        public void SetScale(float scale)
        {
            this.genericXRRig.transform.localScale = new Vector3(scale, scale, scale);
        }

        private void Awake()
        {
            // XRManager.OnInitialized += this.Initialize;

            //// TODO [bgish]: Make sure all the Teleport Interactors have thier interaction layer set to HavenRig.TeleportLayer
        }

        private void OnEnable()
        {
            if (instance != null)
            {
                // Lost.Logger.LogError("HavenRig is already registered, this HavenRig will be ignored!", this);
                return;
            }

            instance = this;
        }

        private void OnDisable()
        {
            instance = null;
        }

        private void Initialize()
        {
            var device = XRManager.Instance.CurrentDevice;

            if (device.XRType == XRType.VRHeadset)
            {
                // Do Nothing
            }
            else if (device.XRType == XRType.Pancake)
            {
                this.pancakeController.enabled = true;

                // For some reason Pancake doesn't properly set the camera Y offset, so doing it manually
                this.genericXRRig.CameraFloorOffsetObject.transform.localPosition =
                    this.genericXRRig.CameraFloorOffsetObject.transform.localPosition.SetY(this.genericXRRig.CameraYOffset);
            }
            else if (device.XRType == XRType.ARHanheld)
            {
                // ...
            }
            else if (device.XRType == XRType.ARHeadset)
            {
                // ...
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
*/
