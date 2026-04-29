#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="Interactable.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace HavenXR
{
    using System.Collections.Generic;
    using OGT;
    using UnityEngine;

    public abstract class Interactable : MonoBehaviour, InputHandler, IAwake
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;
        public static readonly Vector3 InvalidVector = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        public static readonly string LayerName = "Interactable";
        private static readonly List<Transform> Children = new List<Transform>();

        private InputManager inputManager;
        private Camera currentCamera;
        private OGT.Input currentInput;
        private Collider currentCollider;
        private bool isInteractable = true;

        public bool HasInput
        {
            get { return this.currentInput != null; }
        }

        public bool IsInteractable
        {
            get
            {
                return this.isInteractable;
            }

            set
            {
                this.isInteractable = value;

                if (this.isInteractable == false)
                {
                    this.ResetInputData();
                }
            }
        }

        public void SetInputData(OGT.Input input, Collider collider, Camera camera)
        {
            if (input.InputState != InputState.Pressed || this.currentInput != null || this.isInteractable == false)
            {
                return;
            }

            this.currentCamera = camera;
            this.currentInput = input;
            this.currentCollider = collider;
            this.inputManager.AddHandler(this);

            this.OnInput(input, collider, camera);
        }

        void InputHandler.HandleInputs(List<OGT.Input> touches, OGT.Input mouse, OGT.Input pen)
        {
            this.OnInput(this.currentInput, this.currentCollider, this.currentCamera);

            if (this.currentInput != null && this.currentInput.InputState == InputState.Released)
            {
                this.ResetInputData();
            }
        }

        protected abstract void OnInput(OGT.Input input, Collider collider, Camera camera);

        protected virtual void Awake()
        {
            ActivationManager.Register(this);
        }

        public void OnAwake(Bootloader bootloader)
        {
            this.inputManager = bootloader.FindManager<InputManager>();
            this.Initialize();
        }

        private void Initialize()
        {
            int interactableLayer = LayerMask.NameToLayer(LayerName);

            this.GetComponentsInChildren<Transform>(true, Children);

            bool hasInteractable = false;

            for (int i = 0; i < Children.Count; i++)
            {
                hasInteractable |= Children[i].gameObject.layer == interactableLayer;
            }

            if (hasInteractable == false)
            {
                Logger.LogErrorFormat(this, "Interactable \"{0}\" does not have an collider on the \"{1}\" layer and will not work!", this.name, LayerName);
            }

            Children.Clear();
        }

        private void ResetInputData()
        {
            this.currentCamera = null;
            this.currentCollider = null;
            this.currentInput = null;
            this.inputManager.RemoveHandler(this);
        }
    }
}
