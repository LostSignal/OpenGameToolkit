//-----------------------------------------------------------------------
// <copyright file="InteractablesManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace HavenXR
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using OGT;
    using UnityEngine;

    public sealed class InteractablesManager : Manager, InputHandler, IAwake
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;
        private CameraManager cameraManager;
        private InputManager inputManager;
        private int layer;

        protected override Task InitializeManager(Bootloader bootloader)
        {
            return Task.CompletedTask;
        }

        public void OnAwake(Bootloader bootloader)
        {
            int layerNumber = LayerMask.NameToLayer(Interactable.LayerName);

            if (layerNumber == -1)
            {
                Logger.LogFormat("Trying to use Interactables Manager without the \"{0}\" layer defined!  This system will not work.", Interactable.LayerName);
                return;
            }

            this.layer = 1 << layerNumber;
            this.cameraManager = bootloader.FindManager<CameraManager>();

            if (this.cameraManager == null)
            {
                Logger.LogError("Interactables Manager couldn't find CameraManager, will not work!");
                return;
            }

            this.inputManager = bootloader.FindManager<InputManager>();

            if (this.inputManager == null)
            {
                Logger.LogError("Interactables Manager couldn't find InputManager, will not work!");
                return;
            }

            this.inputManager.AddHandler(this);
        }

        private void OnDestroy()
        {
            if (this.inputManager != null)
            {
                this.inputManager.RemoveHandler(this);
            }
        }

        void InputHandler.HandleInputs(List<OGT.Input> touches, OGT.Input mouse, OGT.Input pen)
        {
            this.OnInput(mouse);
            this.OnInput(pen);

            for (int i = 0; i < touches.Count; i++)
            {
                this.OnInput(touches[i]);
            }
        }

        private void OnInput(OGT.Input input)
        {
            if (this.cameraManager == null || input == null || input.InputState != InputState.Pressed)
            {
                return;
            }

            var camera = this.cameraManager.CameraState.Camera;

            if (camera == null)
            {
                return;
            }

            Ray ray = camera.ScreenPointToRay(input.CurrentPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, this.layer))
            {
                Interactable interactable = hit.collider.gameObject.GetComponentInParent<Interactable>();

                if (interactable != null && interactable.HasInput == false)
                {
                    interactable.SetInputData(input, hit.collider, camera);
                }
                else if (interactable == null)
                {
                    Logger.LogErrorFormat(hit.collider, "GameObject {0} has a collider on the {1} layer, but not Interactable component!", hit.collider.gameObject.name, Interactable.LayerName);
                }
            }
        }
    }
}
