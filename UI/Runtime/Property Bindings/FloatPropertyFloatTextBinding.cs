//-----------------------------------------------------------------------
// <copyright file="FloatPropertyFloatTextBinding.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using OGT.Properties;
    using UnityEngine;

    public class FloatPropertyFloatTextBinding : GameBehavior, IAwake, IValidate
    {
#pragma warning disable 0649, 0044
        [SerializeField] private FloatProperty floatVariable;

        [Header("Float Text Binding Object")]
        [SerializeField] private FloatText floatText;
        [SerializeField] private TextUpdateType floatTextUpdateType;
#pragma warning restore 0649, 0044

        public void OnAwake(Bootloader bootloader)
        {
            this.floatVariable.OnChange += this.OnSettingChanged;

            this.OnSettingChanged(default, this.floatVariable.Value);
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNull(this, this.floatVariable, nameof(this.floatVariable));
            report.AssertNotNull(this, this.floatText, nameof(this.floatText));
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
            if (this.floatText != null)
            {
                this.floatText.UpdateValue(this.floatVariable.Value, this.floatTextUpdateType);
            }
        }

        private void OnDestroy()
        {
            if (this.floatVariable == null)
            {
                return;
            }

            this.floatVariable.OnChange -= this.OnSettingChanged;
        }
    }
}
