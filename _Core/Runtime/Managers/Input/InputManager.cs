//-----------------------------------------------------------------------
// <copyright file="InputManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;

    public sealed class InputManager : Manager, IAwake, IUpdate
    {
        private const int InputCacheSize = 20;

        private static readonly OGTLogger Logger = OGTLogger.OGT;
        private readonly List<InputHandler> handlers = new List<InputHandler>();
        private readonly List<Input> fingerInputs = new List<Input>(10);
        private readonly Dictionary<int, Input> fingerIdToInputMap = new Dictionary<int, Input>();
        private readonly HashSet<int> activeFingerIdsCache = new HashSet<int>();
        private readonly List<Input> inputCache = new List<Input>(InputCacheSize);

#pragma warning disable 0649
        [SerializeField] private bool useTouchInput;
        [SerializeField] private bool useMouseInput;
        [SerializeField] private bool usePenInput;
#pragma warning restore 0649

        private int inputIdCounter = 0;
        private Input mouseInput = null;
        private Input penInput = null;

        public static InputManager Instance
        {
            get
            {
                Debug.LogError("InputManager.Instance no longer supported");
                return GameObject.FindAnyObjectByType<Bootloader>().FindManager<InputManager>();
            }
        }

        protected override Task InitializeManager(Bootloader bootloader)
        {
            return Task.CompletedTask;
        }

        public void AddHandler(InputHandler handler)
        {
            if (handler != null && this.handlers.Contains(handler) == false)
            {
                this.handlers.Add(handler);
            }
        }

        public void RemoveHandler(InputHandler handler)
        {
            this.handlers.Remove(handler);
        }

        public void OnAwake(Bootloader bootloader)
        {
            if (this.useTouchInput)
            {
                UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
            }

            // Populating the input cache
            for (int i = 0; i < InputCacheSize; i++)
            {
                this.inputCache.Add(new Input());
            }
        }

        public void OnUpdate(float deltaTime)
        {
            if (this.useTouchInput)
            {
                this.UpdateTouchInputs();
            }

            if (this.useMouseInput)
            {
                this.UpdateMouseInput();
            }

            if (this.usePenInput)
            {
                this.UpdatePenInput();
            }

            // Sending inputs to all registered handlers
            for (int i = 0; i < this.handlers.Count; i++)
            {
                this.handlers[i].HandleInputs(this.fingerInputs, this.mouseInput, this.penInput);
            }
        }

        private void UpdateTouchInputs()
        {
            Logger.Assert(this.fingerInputs.Count == this.fingerIdToInputMap.Count, "Finger Inputs list and map don't match!");

            // Remove all inputs that have been marked as released
            for (int i = this.fingerInputs.Count - 1; i >= 0; i--)
            {
                if (this.fingerInputs[i].InputState == InputState.Released)
                {
                    Input input = this.fingerInputs[i];
                    this.RecycleInput(input);
                    this.fingerInputs.RemoveAt(i);
                    this.fingerIdToInputMap.Remove(input.UnityFingerId);
                }
            }

            this.activeFingerIdsCache.Clear();

            // Going through all the unity touch inputs and either creating/updating Lost.Inputs
            for (int i = 0; i < UnityEngine.InputSystem.EnhancedTouch.Touch.activeFingers.Count; i++)
            {
                var finger = UnityEngine.InputSystem.EnhancedTouch.Touch.activeFingers[i];
                var fingerId = finger.currentTouch.touchId;
                var position = finger.screenPosition;

                this.activeFingerIdsCache.Add(fingerId);

                if (this.fingerIdToInputMap.TryGetValue(fingerId, out Input input))
                {
                    input.Update(position);
                }
                else
                {
                    Input newInput = this.GetNewInput(fingerId, InputType.Touch, InputButton.Left, position);
                    this.fingerInputs.Add(newInput);
                    this.fingerIdToInputMap.Add(newInput.UnityFingerId, newInput);
                }
            }

            // Testing if any of the Lost.Inputs no longer have their unity counterparts and calling Done() on them if that's the case
            for (int i = 0; i < this.fingerInputs.Count; i++)
            {
                Input input = this.fingerInputs[i];

                if (this.activeFingerIdsCache.Contains(input.UnityFingerId) == false)
                {
                    input.Done();
                }
            }
        }

        private void UpdateMouseInput()
        {
            var mouse = UnityEngine.InputSystem.Mouse.current;

            if (mouse == null)
            {
                return;
            }

            Vector3 mousePosition = mouse.position.ReadValue();
            bool isLeftButtonDown = mouse.leftButton.isPressed;
            bool isRightButtonDown = mouse.rightButton.isPressed;
            bool isMiddleButtonDown = mouse.middleButton.isPressed;

            // Defaulting the mouse input to be in the hover state
            if (this.mouseInput == null || this.mouseInput.InputState == InputState.Released)
            {
                this.RecycleInput(this.mouseInput);
                this.mouseInput = this.GetNewInput(-1, InputType.Mouse, InputButton.None, mousePosition);
                this.mouseInput.UpdateHover(mousePosition);
            }

            if (this.mouseInput.InputState == InputState.Hover)
            {
                InputButton inputButton = InputButton.None;

                if (isLeftButtonDown)
                {
                    inputButton = InputButton.Left;
                }
                else if (isRightButtonDown)
                {
                    inputButton = InputButton.Right;
                }
                else if (isMiddleButtonDown)
                {
                    inputButton = InputButton.Middle;
                }

                if (inputButton != InputButton.None)
                {
                    this.RecycleInput(this.mouseInput);
                    this.mouseInput = this.GetNewInput(-1, InputType.Mouse, inputButton, mousePosition);
                }
                else
                {
                    this.mouseInput.UpdateHover(mousePosition);
                }
            }
            else
            {
                if (this.mouseInput.InputButton == InputButton.Left)
                {
                    if (isLeftButtonDown)
                    {
                        this.mouseInput.Update(mousePosition);
                    }
                    else
                    {
                        this.mouseInput.Done();
                    }
                }
                else if (this.mouseInput.InputButton == InputButton.Right)
                {
                    if (isRightButtonDown)
                    {
                        this.mouseInput.Update(mousePosition);
                    }
                    else
                    {
                        this.mouseInput.Done();
                    }
                }
                else if (this.mouseInput.InputButton == InputButton.Middle)
                {
                    if (isMiddleButtonDown)
                    {
                        this.mouseInput.Update(mousePosition);
                    }
                    else
                    {
                        this.mouseInput.Done();
                    }
                }
                else
                {
                    Logger.LogError("UpdateMouseInput found an invalid InputButton type!!!", this);
                }
            }
        }

        private void UpdatePenInput()
        {
            var pen = UnityEngine.InputSystem.Pen.current;

            if (pen == null)
            {
                return;
            }

            this.penInput ??= new Input();

            if (pen.press.isPressed)
            {
                this.penInput.Update(pen.position.ReadValue());
            }
            else
            {
                this.penInput.Done();
            }
        }

        private void RecycleInput(Input input)
        {
            if (input != null)
            {
                this.inputCache.Add(input);
            }
        }

        private Input GetNewInput(int unityFingerId, InputType inputType, InputButton inputButton, Vector2 position)
        {
            Logger.Assert(this.inputCache.Count != 0, "InputManager's input cache has run out!  Figure out why we're leaking inputs.");

            int lastIndex = this.inputCache.Count - 1;
            Input lastInput = this.inputCache[lastIndex];
            this.inputCache.RemoveAt(lastIndex);

            lastInput.Reset(this.inputIdCounter++, unityFingerId, inputType, inputButton, position);

            return lastInput;
        }
    }
}
