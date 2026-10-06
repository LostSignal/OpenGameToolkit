//-----------------------------------------------------------------------
// <copyright file="ToggleFullscreenButton.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using OGT;
    using UnityEngine;
    using UnityEngine.UI;

    public class ToggleFullscreenButton : GameBehavior, IAwake
    {
        [SerializeField] private Button fullscreenButton;
        [SerializeField] private Image fullscreenImage;
        [SerializeField] private Sprite enterFullscreenSprite;
        [SerializeField] private Sprite exitFullscreenSprite;

        public void OnAwake(Bootloader bootloader)
        {
            if (Application.isEditor ||
                Application.platform == RuntimePlatform.WebGLPlayer ||
                Application.platform == RuntimePlatform.WindowsPlayer ||
                Application.platform == RuntimePlatform.OSXPlayer ||
                Application.platform == RuntimePlatform.LinuxPlayer)
            {
                this.fullscreenButton.onClick.AddListener(this.ToggleFullscreen);
                this.UpdateUI(Screen.fullScreen);
            }
            else
            {
                this.fullscreenButton.gameObject.SetActive(false);
            }
        }

        private void ToggleFullscreen()
        {
            this.UpdateUI(!Screen.fullScreen);
            Screen.fullScreen = !Screen.fullScreen;
        }

        private void OnEnable() => this.UpdateUI(Screen.fullScreen);

        private void UpdateUI(bool isFullscreen) => this.fullscreenImage.sprite = isFullscreen ? this.exitFullscreenSprite : this.enterFullscreenSprite;
    }
}
