//-----------------------------------------------------------------------
// <copyright file="CameraManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public sealed class CameraManager : Manager, IUpdate
    {
#if UNITY_6000_0_OR_NEWER
        [UnityEngine.SerializeField] private UnityEngine.Camera mainCamera;
#endif

        private CameraState cameraState;

        public CameraState CameraState
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.cameraState;
        }

        public int UpdateOrder => -1;

        public void OnUpdate(float deltaTime)
        {
#if UNITY_6000_0_OR_NEWER
            if (this.cameraState.Camera == null)
            {
                var camera = this.mainCamera != null && this.mainCamera.isActiveAndEnabled ?
                    this.mainCamera :
                    null;

                if (camera == null)
                {
                    camera = UnityEngine.Camera.main;
                }

                if (camera != null)
                {
                    var cameraTransform = camera.transform;
                    var fov = camera.fieldOfView;

                    this.cameraState = new CameraState
                    {
                        Exists = true,
                        Camera = camera,
                        Transform = cameraTransform,
                        Position = cameraTransform.position,
                        Forward = cameraTransform.forward,
                        EulerRotation = cameraTransform.eulerAngles,
                        FieldOfView = fov,
                        CosOfFOV = UnityEngine.Mathf.Cos(fov * UnityEngine.Mathf.Deg2Rad),
                    };
                }
                else
                {
                    this.cameraState = new CameraState { Exists = false };
                }
            }
            else
            {
                // The camera is still valid, so lets just update the camera postion/forward
                this.cameraState.Position = this.cameraState.Transform.position;
                this.cameraState.Forward = this.cameraState.Transform.forward;
                this.cameraState.EulerRotation = this.cameraState.Transform.eulerAngles;
                this.cameraState.FieldOfView = this.cameraState.Camera.fieldOfView;
            }
#endif
        }

        protected override Task InitializeManager(Bootloader bootloader)
        {
            this.OnUpdate(0.0f);
            return Task.CompletedTask;
        }
    }
}
