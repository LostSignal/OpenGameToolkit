//-----------------------------------------------------------------------
// <copyright file="StringInputBox.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections;
    using TMPro;
    using UnityEngine;
    
    //// TODO [bgish]: Make sure to move the Content Object up if TouchScreenKeyboard.visible is true

    public class NewStringInputBox : PanelLogic
    {
#pragma warning disable 0649
        [Header("StringInputBox")]
        [SerializeField] private ModalWidget modalWidget;
        [SerializeField] private InputFieldWidget inputFieldWidget;
        [SerializeField] private ButtonWidget confirmButtonWidget;
        [SerializeField] private ButtonWidget declineButtonWidget;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private RectTransform content;
        [SerializeField] private float slideUpDownTimeInSeconds = 1.0f;
#pragma warning restore 0649

        private Action<string> onComplete;
        private Coroutine virtualKeyboardCoroutine;
        private string startingText;
        private float centerY = 0.0f;
        private float upperY = 0.0f;

        public void Show(string title, string body, string startingText, Action<string> onComplete, int maxCharacterCount = -1)
        {
            if (maxCharacterCount < 0)
            {
                this.inputFieldWidget.InputField.characterLimit = int.MaxValue;
            }

            this.onComplete = onComplete;

            this.startingText = string.IsNullOrWhiteSpace(startingText) ? string.Empty : startingText;
            this.inputFieldWidget.InputField.text = this.startingText;
            this.modalWidget.TitleText.text = title;
            this.bodyText.text = body;
            
            this.Panel.Show();
        }

        public override void Validate(ValidationReport report, bool isSceneObject)
        {
            base.Validate(report, isSceneObject);
            report.AssertNotNull(this.modalWidget, this, nameof(this.modalWidget));
            report.AssertNotNull(this.inputFieldWidget, this, nameof(this.inputFieldWidget));
            report.AssertNotNull(this.confirmButtonWidget, this, nameof(this.confirmButtonWidget));
            report.AssertNotNull(this.declineButtonWidget, this, nameof(this.declineButtonWidget));
            report.AssertNotNull(this.bodyText, this, nameof(this.bodyText));
            report.AssertNotNull(this.content, this, nameof(this.content));
        }

        public override void OnAwake(Bootloader bootloader)
        {
            base.OnAwake(bootloader);

            var rectTransform = this.transform as RectTransform;
            this.upperY = rectTransform.sizeDelta.y / 4.0f;

            this.confirmButtonWidget.Button.onClick.AddListener(this.OkButtonClicked);
            this.declineButtonWidget.Button.onClick.AddListener(this.CancelButtonPressed);

            this.Showable.OnShowStart.AddListener(this.OnShow);
            this.Showable.OnHideEnd.AddListener(this.OnHide);
            this.Panel.OnBackButtonPressed.AddListener(this.OnBackButtonPressed);
        }

        private void OnShow()
        {
            // Whenever we show it, make sure it's centered
            this.content.transform.localPosition = this.content.transform.localPosition.SetY(this.centerY);

            this.StopVirtualKeyboardCoroutine();
            this.StartVirtualKeyboardCoroutine();
        }

        private void OnHide()
        {
            this.StopVirtualKeyboardCoroutine();
        }

        private void OnBackButtonPressed()
        {
            this.CancelButtonPressed();
        }

        private IEnumerator VirtualKeyboardCoroutine()
        {
            while (true)
            {
                bool isTouchScreenVisible = false;

#if UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WSA_10_0
                isTouchScreenVisible = TouchScreenKeyboard.visible;
#endif

                float desiredY = isTouchScreenVisible ? this.upperY : this.centerY;
                float currentY = this.content.transform.localPosition.y;
                float distanceFromDesiredY = Mathf.Abs(desiredY - currentY);

                if (distanceFromDesiredY > 0.1f)
                {
                    float direction = desiredY < currentY ? -1.0f : 1.0f;
                    float speed = direction * (this.upperY - this.centerY) / this.slideUpDownTimeInSeconds;
                    float movement = speed * Time.deltaTime;

                    if (Mathf.Abs(movement) > distanceFromDesiredY)
                    {
                        this.content.transform.localPosition = this.content.transform.localPosition.SetY(desiredY);
                    }
                    else
                    {
                        this.content.transform.localPosition = this.content.transform.localPosition.AddToY(movement);
                    }
                }

                yield return null;
            }
        }

        private void CancelButtonPressed()
        {
            this.onComplete?.Invoke(null);
            this.Panel.Hide();
        }

        private void OkButtonClicked()
        {
            this.onComplete?.Invoke(this.inputFieldWidget.InputField.text);
            this.Panel.Hide();
        }

        private void StartVirtualKeyboardCoroutine()
        {
            if (TouchScreenKeyboard.isSupported)
            {
                this.virtualKeyboardCoroutine = this.StartCoroutine(this.VirtualKeyboardCoroutine());
            }
        }

        private void StopVirtualKeyboardCoroutine()
        {
            if (this.virtualKeyboardCoroutine != null)
            {
                this.StopCoroutine(this.virtualKeyboardCoroutine);
                this.virtualKeyboardCoroutine = null;
            }
        }
    }
}
