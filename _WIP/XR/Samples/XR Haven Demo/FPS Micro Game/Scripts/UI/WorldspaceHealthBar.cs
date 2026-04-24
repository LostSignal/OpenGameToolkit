#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="ValidationError.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

////
//// TODO [bgish]: Need to update this class to use the UpdateManager, and perhaps
////               make sure it doesn't update if the object isn't in view.
////

#if true //UNITY

namespace Lost
{
    using UnityEngine;
    using UnityEngine.UI;
    using OGT;

    public class WorldspaceHealthBar : MonoBehaviour, IAwake
    {
#pragma warning disable 0649
        [Tooltip("Health component to track")]
        [SerializeField] private Health health;

        [Tooltip("Image component displaying health left")]
        [SerializeField] private Image healthBarImage;

        [Tooltip("The floating healthbar pivot transform")]
        [SerializeField] private Transform healthBarPivot;

        [Tooltip("Whether the health bar is visible when at full health or not")]
        [SerializeField] private bool hideIfHealthIsFull = true;
#pragma warning restore 0649

        private CameraManager cameraManager;

        public void OnAwake(Bootloader bootloader)
        {
            this.cameraManager = bootloader.FindManager<CameraManager>();
            this.health.onHealthChanged += this.UpdateHealth;
            this.UpdateHealth();
        }

        private void OnDestroy()
        {
            this.health.onHealthChanged -= this.UpdateHealth;
        }

        private void Update()
        {
            var cameraState = this.cameraManager.CameraState;

            if (cameraState.Exists)
            {
                // Rotate health bar to face the camera/player
                this.healthBarPivot.LookAt(cameraState.Position);
            }
        }

        private void UpdateHealth()
        {
            // Update health bar value
            this.healthBarImage.fillAmount = this.health.CurrentHealth / this.health.MaxHealth;

            // Hide health bar if needed
            if (this.hideIfHealthIsFull)
            {
                this.healthBarPivot.gameObject.SetActive(this.healthBarImage.fillAmount < 0.99f);
            }
        }
    }
}

#endif
