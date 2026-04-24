//-----------------------------------------------------------------------
// <copyright file="SpinnerBox.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using Text = TMPro.TMP_Text;

    public class SpinnerBox : DialogLogic
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

#pragma warning disable 0649, 0044
        [Header("SpinnerBox")]
        [SerializeField] private Text title;
        [SerializeField] private Text body;
        [SerializeField] private Button cancelButton;
#pragma warning restore 0649, 0044

        private Action cancelButtonAction;

        public static SpinnerBox Instance
        {
            get => DialogManager.GetDialog<SpinnerBox>();
        }

        public void Show(string title, string body)
        {
            this.PrivateShow(title, body, false, null);
        }

        public void ShowWithCancel(string title, string body, Action cancelButtonAction)
        {
            this.PrivateShow(title, body, true, cancelButtonAction);
        }

        public void UpdateBodyText(string body)
        {
            this.body.text = body;
        }

        public override void OnAwake(Bootloader bootloader)
        {
            base.OnAwake(bootloader);

#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                return;
            }
#endif

            Logger.Assert(this.body != null, "SpinnerBox didn't specify body text", this);
            Logger.Assert(this.cancelButton != null, "SpinnerBox didn't specify cancelButton", this);

            this.cancelButton.onClick.AddListener(this.CancelButtonClicked);
        }

        private void PrivateShow(string title, string body, bool showCancelButton, Action cancelButtonAction)
        {
            if (this.Dialog.IsShowing)
            {
                Logger.LogError("SpinnerBox.Show called while already showing.  SpinnerBox may not function correctly.", this);
            }

            if (this.title != null)
            {
                this.title.text = title;
            }

            this.UpdateBodyText(body);
            this.cancelButton.gameObject.SetActive(showCancelButton);
            this.cancelButtonAction = cancelButtonAction;

            this.Dialog.Show();
        }

        private void CancelButtonClicked()
        {
            if (this.cancelButtonAction != null)
            {
                this.cancelButtonAction.Invoke();
                this.cancelButtonAction = null;
            }

            this.Dialog.Hide();
        }
    }
}

#endif
