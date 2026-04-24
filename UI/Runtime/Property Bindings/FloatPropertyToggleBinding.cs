//-----------------------------------------------------------------------
// <copyright file="FloatPropertyToggleBinding.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using OGT.Properties;
    using UnityEngine;
    using UnityEngine.UI;

    public class FloatPropertyToggleBinding : GameBehavior, IAwake, IValidate
    {
#pragma warning disable 0649, 0044
        [SerializeField] private FloatProperty floatVariable;

        [Header("Toggle Binding Object")]
        [SerializeField] private Toggle floatToggle;
        [SerializeField] private float floatToggleValue;
#pragma warning restore 0649, 0044

        public void OnAwake(Bootloader bootloader)
        {
            this.floatVariable.OnChange += this.OnSettingChanged;

            if (this.floatToggle != null)
            {
                this.floatToggle.onValueChanged.AddListener(this.OnToggleValueChanged);
            }

            this.OnSettingChanged(default, this.floatVariable.Value);
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNull(this, this.floatVariable, nameof(this.floatVariable));
            report.AssertNotNull(this, this.floatToggle, nameof(this.floatToggle));
        }

        private void OnSliderValueChanged(float newValue)
        {
            if (this.floatVariable.Value != newValue)
            {
                this.floatVariable.Value = newValue;
            }
        }

        private void OnToggleValueChanged(bool newValue)
        {
            if (newValue)
            {
                this.floatVariable.Value = this.floatToggleValue;
            }
        }

        private void OnSettingChanged(float oldValue, float newValue)
        {
            if (this.floatToggle != null)
            {
                bool isEqual = Mathf.Abs(this.floatVariable.Value - this.floatToggleValue) < 0.001f;
                this.floatToggle.SetIsOnWithoutNotify(isEqual);
            }
        }

        private void OnDestroy()
        {
            if (this.floatVariable == null)
            {
                return;
            }

            this.floatVariable.OnChange -= this.OnSettingChanged;

            if (this.floatToggle != null)
            {
                this.floatToggle.onValueChanged.RemoveListener(this.OnToggleValueChanged);
            }
        }
    }
}
