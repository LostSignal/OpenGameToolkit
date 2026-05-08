namespace OGT
{
    using System;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Serialization;

    public class NewMessageBox : PanelLogic
    {
        public enum YesNoResult
        {
            Yes,
            No,
        }

        [SerializeField] private ModalWidget modalWidget;

        [FormerlySerializedAs("yesButtonWidget")]
        [SerializeField] private ButtonWidget leftButtonWidget;

        [FormerlySerializedAs("noButtonWidget")]
        [SerializeField] private ButtonWidget rightButtonWidget;

        [SerializeField] private ButtonWidget okButtonWidget;
        [SerializeField] private TMP_Text bodyText;

        public void ShowOk(string title, string message, Action okAction)
        {
            // Ok Button Setup
            this.okButtonWidget.gameObject.SetActive(true);
            this.okButtonWidget.Button.onClick.RemoveAllListeners();
            this.okButtonWidget.Button.onClick.AddListener(this.Panel.Hide);
            this.okButtonWidget.Button.onClick.AddListener(() => okAction?.Invoke());

            // Right / Left Button Setup
            this.leftButtonWidget.gameObject.SetActive(false);
            this.rightButtonWidget.gameObject.SetActive(false);

            // Title / Message Body Setup
            this.modalWidget.TitleText.text = title;
            this.bodyText.text = message;

            // Making sure we actually show the message box
            this.Panel.Show();
        }

        public void ShowCustomTwoButton(string title, string message, string leftButtonText, string rightButtonText, Action leftAction, Action rightAction)
        {
            // Left Button Setup
            this.leftButtonWidget.ButtonText.text = leftButtonText;
            this.leftButtonWidget.gameObject.SetActive(true);
            this.leftButtonWidget.Button.onClick.RemoveAllListeners();
            this.leftButtonWidget.Button.onClick.AddListener(this.Panel.Hide);
            this.leftButtonWidget.Button.onClick.AddListener(() => leftAction?.Invoke());

            // Right Button Setup
            this.rightButtonWidget.ButtonText.text = rightButtonText;
            this.rightButtonWidget.gameObject.SetActive(true);
            this.rightButtonWidget.Button.onClick.RemoveAllListeners();
            this.rightButtonWidget.Button.onClick.AddListener(this.Panel.Hide);
            this.rightButtonWidget.Button.onClick.AddListener(() => rightAction?.Invoke());

            // Ok Button Setup
            this.okButtonWidget.gameObject.SetActive(false);

            // Title / Message Body Setup
            this.modalWidget.TitleText.text = title;
            this.bodyText.text = message;

            // Making sure we actually show the message box
            this.Panel.Show();
        }

        public void ShowYesNo(string title, string message, Action yesAction, Action noAction)
        {
            ShowCustomTwoButton(
                title,
                message,
                Localization.Localization.CurrentLanguage.Yes,
                Localization.Localization.CurrentLanguage.No,
                yesAction,
                noAction);
        }

        public async Awaitable<YesNoResult> ShowYesNo(string title, string message)
        {
            YesNoResult result = YesNoResult.No;
            bool isDone = false;

            this.ShowYesNo(title, message,
                () =>
                {
                    isDone = true;
                    result = YesNoResult.Yes;
                },
                () =>
                {
                    isDone = true;
                    result = YesNoResult.No;
                });

            while (isDone == false)
            {
                await Awaitable.NextFrameAsync();
            }

            return result;
        }

        public async Awaitable ShowOk(string title, string message)
        {
            bool isDone = false;

            this.ShowOk(title, message, () => isDone = true);

            while (isDone == false)
            {
                await Awaitable.NextFrameAsync();
            }
        }
    }
}
