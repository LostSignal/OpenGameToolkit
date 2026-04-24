//-----------------------------------------------------------------------
// <copyright file="CombinationLock.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace HavenXR
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using OGT;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;

    public class CombinationLock : CircleInteractable
    {
        private readonly List<int> recentNumbers = new List<int>();

        #pragma warning disable 0649
        [Header("Combo Lock")]
        [SerializeField] private Transform comboLockTransform;
        [SerializeField] private int totalNumbers = 40;
        [SerializeField] private int[] unlockCombo = new int[] { 8, -30, 12 };

        [Tooltip("In Degrees Per Second")]
        [SerializeField] private float returnSpeed = 360.0f;

        [Header("Events")]
        [SerializeField] private UnityEvent numberChangedEvent;
        [SerializeField] private UnityEvent lockOpenedEvent;

        [Header("Effect")]
        [SerializeField] private TMP_Text effectText;
        [SerializeField] private Animator effectAnimator;
        #pragma warning restore 0649

        private float degreesPerNumber;
        private float halfDegreesPerNumber;
        private bool isLockSolved;
        private int lastNumber;
        private float comboLockRotation;

        // traking finger
        private bool isFingerDown;
        private float fingerStillTime;

        [NonSerialized]
        private float minimumPixelMovementSquared;

        private float ComboLockRotation
        {
            get
            {
                return this.comboLockRotation;
            }

            set
            {
                if (this.comboLockRotation != value)
                {
                    this.comboLockRotation = value;
                    this.comboLockTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, this.comboLockRotation);
                }
            }
        }

        private int CurrentNumber
        {
            get
            {
                float rotation = this.Rotation;

                if (rotation >= -this.halfDegreesPerNumber && rotation <= this.halfDegreesPerNumber)
                {
                    return 0;
                }
                else if (rotation > 0.0f)
                {
                    return (int)((rotation + this.halfDegreesPerNumber) / this.degreesPerNumber);
                }
                else
                {
                    return (int)((rotation - this.halfDegreesPerNumber) / this.degreesPerNumber);
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();

            this.degreesPerNumber = 360.0f / this.totalNumbers;
            this.halfDegreesPerNumber = this.degreesPerNumber / 2.0f;
            this.minimumPixelMovementSquared = OGT.Input.GetMinimumPixelMovementSquared();

            // Making sure the effect animator isn't on when we first start up
            if (this.effectAnimator != null)
            {
                this.effectAnimator.enabled = false;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (this.isFingerDown)
            {
                if (this.fingerStillTime < 0.25f)
                {
                    float desiredRotation = this.Rotation;
                    this.ComboLockRotation = Mathf.Lerp(this.ComboLockRotation, desiredRotation, 0.9f);
                }
                else
                {
                    float desiredRotation = this.CurrentNumber * this.degreesPerNumber;
                    this.ComboLockRotation = Mathf.Lerp(this.ComboLockRotation, desiredRotation, 0.1f);
                    this.Rotation = this.ComboLockRotation;
                }
            }
            else
            {
                float desiredRotation = this.CurrentNumber * this.degreesPerNumber;

                if (this.ComboLockRotation == desiredRotation)
                {
                    // do nothing
                }
                else if (Mathf.Abs(this.ComboLockRotation - desiredRotation) < 0.01)
                {
                    this.ComboLockRotation = desiredRotation;
                }
                else
                {
                    this.ComboLockRotation = Mathf.Lerp(this.ComboLockRotation, desiredRotation, 0.9f);
                }
            }
        }

        protected override void OnInput(OGT.Input input, Collider collider, Camera camera)
        {
            base.OnInput(input, collider, camera);

            this.isFingerDown = input.InputState != OGT.InputState.Released;

            float inputSqrMagnatude = (input.PreviousPosition - input.CurrentPosition).sqrMagnitude;

            if (inputSqrMagnatude < this.minimumPixelMovementSquared)
            {
                this.fingerStillTime += Time.deltaTime;
            }
            else if (inputSqrMagnatude >= this.minimumPixelMovementSquared || input.InputState == OGT.InputState.Released)
            {
                this.fingerStillTime = 0.0f;
            }

            this.Rotation = Mathf.Clamp(this.Rotation, -360.0f + this.degreesPerNumber, 360.0f - this.degreesPerNumber);

            if (this.lastNumber != this.CurrentNumber)
            {
                this.lastNumber = this.CurrentNumber;
                this.numberChangedEvent.SafeInvoke();
            }

            // the user let go, so record the number they left it on
            if (input.InputState == OGT.InputState.Released)
            {
                this.recentNumbers.Add(this.CurrentNumber);

                this.ShowTextEffect(this.CurrentNumber);

                while (this.recentNumbers.Count > this.unlockCombo.Length)
                {
                    this.recentNumbers.RemoveAt(0);
                }

                this.StartCoroutine(this.ReturnToCenterCoroutine());
            }
        }

        private IEnumerator ReturnToCenterCoroutine()
        {
            this.IsInteractable = false;

            while (true)
            {
                if (this.Rotation >= 0.0f)
                {
                    this.Rotation -= this.returnSpeed * Time.deltaTime;

                    if (this.Rotation < 0.0f)
                    {
                        this.Rotation = 0.0f;
                        break;
                    }
                }
                else
                {
                    this.Rotation += this.returnSpeed * Time.deltaTime;

                    if (this.Rotation > 0.0f)
                    {
                        this.Rotation = 0.0f;
                        break;
                    }
                }

                yield return null;
            }

            this.isLockSolved = this.IsLockSolved();

            if (this.isLockSolved)
            {
                this.lockOpenedEvent.SafeInvoke();
            }
            else
            {
                this.IsInteractable = true;
            }
        }

        private bool IsLockSolved()
        {
            if (this.recentNumbers.Count == this.unlockCombo.Length)
            {
                bool allEqual = true;

                for (int i = 0; i < this.unlockCombo.Length; i++)
                {
                    if (this.unlockCombo[i] != this.recentNumbers[i])
                    {
                        allEqual = false;
                    }
                }

                return allEqual;
            }

            return false;
        }

        private void ShowTextEffect(int number)
        {
            // TODO [bgish]: Fire off a Network Message so the other clients can fire the effect too

            // Setting the text
            if (this.effectText != null)
            {
                BetterStringBuilder.New()
                    .Append(Math.Abs(number))
                    .Append(" ")
                    .Append(number < 0 ? "Left" : "Right")
                    .Set(this.effectText);
            }

            // Playing the animator
            if (this.effectAnimator != null)
            {
                this.effectAnimator.enabled = true;
            }
        }
    }
}
