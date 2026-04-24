//-----------------------------------------------------------------------
// <copyright file="BootloaderDialog.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class BootloaderDialog : DialogLogic
    {
#pragma warning disable 0649
        [SerializeField] private AspectRatioFitter aspectRatioFitter;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private Image progressBar;
#pragma warning disable 0649

        public override void OnAwake(Bootloader bootloader)
        {
            base.OnAwake(bootloader);

            //// Bootloader.ProgressTextUpdate += this.OnProgressTextUpdate;
            //// Bootloader.ProgressUpdated += this.OnProgressUpdated;
        }

        private void OnProgressTextUpdate(string newText)
        {
            if (this.progressText != null)
            {
                this.progressText.text = newText;
            }
        }

        private void OnProgressUpdated(float newProgress)
        {
            if (this.progressBar != null)
            {
                this.progressBar.fillAmount = newProgress;
            }
        }

        //// public void Show()
        //// {
        ////     this.StopShow();
        ////     this.showCoroutine = this.StartCoroutine(Coroutine());
        ////
        ////     IEnumerator Coroutine()
        ////     {
        ////         this.IsShown = true;
        ////
        ////         yield return WaitForUtil.Seconds(0.5f);
        ////
        ////         this.DisableAspectRatioFitter();
        ////
        ////
        ////     }
        //// }
        ////
        //// public void Hide()
        //// {
        ////     this.StopShow();
        ////
        ////     this.IsShown = false;
        ////     Bootloader.ProgressTextUpdate -= this.OnProgressTextUpdate;
        ////     Bootloader.ProgressUpdated -= this.OnProgressUpdated;
        //// }
        ////
        //// private void StopShow()
        //// {
        ////     if (this.showCoroutine != null)
        ////     {
        ////         this.StopCoroutine(this.showCoroutine);
        ////         this.showCoroutine = null;
        ////     }
        //// }
        ////
        //// private void DisableAspectRatioFitter()
        //// {
        ////     if (this.aspectRatioFitter != null && Application.isEditor == false)
        ////     {
        ////         this.aspectRatioFitter.enabled = false;
        ////     }
        //// }
    }
}

#endif
