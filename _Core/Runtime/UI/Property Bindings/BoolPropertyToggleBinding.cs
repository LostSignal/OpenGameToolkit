//-----------------------------------------------------------------------
// <copyright file="BoolPropertyToggleBinding.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using OGT.Properties;
    using UnityEngine;
    using UnityEngine.UI;

    public class BoolPropertyToggleBinding : GameBehavior, IAwake, IValidate
    {
#pragma warning disable 0649, 0044
        [SerializeField] private BoolProperty boolProperty;

        [Header("Toggle Binding Object")]
        [SerializeField] private Toggle toggle;
        [SerializeField] private bool toggleOnValue;
#pragma warning restore 0649, 0044

        public void OnAwake(Bootloader bootloader)
        {
            this.boolProperty.OnChange += this.OnSettingChanged;

            if (this.toggle != null)
            {
                this.toggle.onValueChanged.AddListener(this.OnToggleValueChanged);
            }

            this.OnSettingChanged(false, this.boolProperty.Value);
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNull(this, this.boolProperty, nameof(this.boolProperty));
            report.AssertNotNull(this, this.toggle, nameof(this.toggle));
        }

        private void OnToggleValueChanged(bool newValue)
        {
            bool boolVariableValue = newValue ? this.toggleOnValue : !this.toggleOnValue;

            if (this.boolProperty.Value != boolVariableValue)
            {
                this.boolProperty.Value = boolVariableValue;
            }
        }

        private void OnSettingChanged(bool oldValue, bool newValue)
        {
            if (this.toggle != null)
            {
                this.toggle.SetIsOnWithoutNotify(this.boolProperty.Value == this.toggleOnValue);
            }
        }

        private void OnDestroy()
        {
            if (this.boolProperty == null)
            {
                return;
            }

            this.boolProperty.OnChange -= this.OnSettingChanged;

            if (this.toggle != null)
            {
                this.toggle.onValueChanged.RemoveListener(this.OnToggleValueChanged);
            }
        }
    }
}
