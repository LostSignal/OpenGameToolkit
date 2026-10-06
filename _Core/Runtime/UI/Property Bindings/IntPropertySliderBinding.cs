//-----------------------------------------------------------------------
// <copyright file="IntPropertySliderBinding.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using OGT.Properties;
    using UnityEngine;
    using UnityEngine.UI;

    public class IntPropertySliderBinding : GameBehavior, IAwake, IValidate
    {
#pragma warning disable 0649
        [SerializeField] private IntProperty intVariable;

        [Header("Slider Binding Object")]
        [SerializeField] private Slider intSlider;
#pragma warning restore 0649

        public void OnAwake(Bootloader bootloader)
        {
            this.intVariable.OnChange += this.OnSettingChanged;

            if (this.intSlider != null)
            {
                this.intSlider.onValueChanged.AddListener(this.OnSliderValueChanged);
                this.intSlider.wholeNumbers = true;
                this.intSlider.minValue = this.intVariable.Min;
                this.intSlider.maxValue = this.intVariable.Max;
            }

            this.OnSettingChanged(default, this.intVariable.Value);
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNull(this, this.intVariable, nameof(this.intVariable));
            report.AssertNotNull(this, this.intSlider, nameof(this.intSlider));
        }

        private void OnSliderValueChanged(float newValue)
        {
            if (this.intVariable.Value != newValue)
            {
                this.intVariable.Value = (int)newValue;
            }
        }

        private void OnSettingChanged(int oldValue, int newValue)
        {
            if (this.intSlider != null)
            {
                this.intSlider.SetValueWithoutNotify(this.intVariable.Value);
            }
        }

        private void OnDestroy()
        {
            if (this.intVariable == null)
            {
                return;
            }

            this.intVariable.OnChange -= this.OnSettingChanged;

            if (this.intSlider != null)
            {
                this.intSlider.onValueChanged.RemoveListener(this.OnSliderValueChanged);
            }
        }
    }
}
