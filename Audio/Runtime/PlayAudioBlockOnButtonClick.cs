//-----------------------------------------------------------------------
// <copyright file="PlayAudioBlockOnButtonClick.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Button))]
    public class PlayAudioBlockOnButtonClick : GameBehavior, IValidate, IAwake
    {
#pragma warning disable 0649
        [SerializeField] private AudioBlock audioBlock;
        [SerializeField] private bool playSoundFromButtonPosition;

        [HideInInspector]
        [SerializeField] private Button button;
#pragma warning restore 0649

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            this.EditorGetComponent(ref this.button, true);

            report.AssertNotNull(this, this.audioBlock, nameof(this.audioBlock));
        }

        public void OnAwake(Bootloader bootloader)
        {
            this.button.onClick.AddListener(this.OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            if (this.audioBlock == null)
            {
                return;
            }
            else if (this.playSoundFromButtonPosition)
            {
                this.audioBlock.PlayOneShot(this.button.transform.position);
            }
            else
            {
                this.audioBlock.PlayOneShot();
            }
        }
    }
}
