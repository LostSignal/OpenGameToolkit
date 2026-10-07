//-----------------------------------------------------------------------
// <copyright file="SoundToggleIcon.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEngine;
    using TMPro;

    public class SoundToggleIcon : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Toggle toggle;
        [SerializeField] private UnityEngine.UI.Image icon;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Sprite soundOnSprite;
        [SerializeField] private Sprite soundOffSprite;

        private void OnEnable()
        {
            if (this.toggle != null)
            {
                this.toggle.onValueChanged.AddListener(this.UpdateState);
                this.UpdateState(this.toggle.isOn);
            }
        }

        private void OnDisable()
        {
            if (this.toggle != null)
                this.toggle.onValueChanged.RemoveListener(this.UpdateState);
        }

        private void UpdateState(bool isOn)
        {
            if (this.icon != null)
                this.icon.sprite = isOn ? this.soundOnSprite : this.soundOffSprite;
            if (this.label != null)
                this.label.text = isOn ? "Sound on" : "Sound off";
        }
    }
}
