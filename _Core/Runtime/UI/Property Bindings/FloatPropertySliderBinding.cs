//-----------------------------------------------------------------------
// <copyright file="FloatPropertySliderBinding.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using OGT.Properties;
    using UnityEngine;
    using UnityEngine.UI;

    public class FloatPropertySliderBinding : GameBehavior, IAwake, IValidate
    {
#pragma warning disable 0649
        [SerializeField] private FloatProperty floatVariable;

        [Header("Slider Binding Object")]
        [SerializeField] private Slider floatSlider;
#pragma warning restore 0649

        public void OnAwake(Bootloader bootloader)
        {
            this.floatVariable.OnChange += this.OnSettingChanged;

            if (this.floatSlider != null)
            {
                this.floatSlider.onValueChanged.AddListener(this.OnSliderValueChanged);
                this.floatSlider.minValue = this.floatVariable.Min;
                this.floatSlider.maxValue = this.floatVariable.Max;
            }

            this.OnSettingChanged(default, this.floatVariable.Value);
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNull(this, this.floatVariable, nameof(this.floatVariable));
            report.AssertNotNull(this, this.floatSlider, nameof(this.floatSlider));
        }

        private void OnSliderValueChanged(float newValue)
        {
            if (this.floatVariable.Value != newValue)
            {
                this.floatVariable.Value = newValue;
            }
        }

        private void OnSettingChanged(float oldValue, float newValue)
        {
            if (this.floatSlider != null)
            {
                this.floatSlider.SetValueWithoutNotify(this.floatVariable.Value);
            }
        }

        private void OnDestroy()
        {
            if (this.floatVariable == null)
            {
                return;
            }

            this.floatVariable.OnChange -= this.OnSettingChanged;

            if (this.floatSlider != null)
            {
                this.floatSlider.onValueChanged.RemoveListener(this.OnSliderValueChanged);
            }
        }
    }
}
