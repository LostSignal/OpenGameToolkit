//-----------------------------------------------------------------------
// <copyright file="IntPropertyToggleBinding.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using OGT.Properties;
    using UnityEngine;
    using UnityEngine.UI;

    public class IntPropertyToggleBinding : GameBehavior, IAwake, IValidate
    {
#pragma warning disable 0649, 0044
        [SerializeField] private IntProperty integerVariable;

        [Header("Toggle Binding Object")]
        [SerializeField] private Toggle intToggle;
        [SerializeField] private int intToggleValue;
#pragma warning restore 0649, 0044

        public void OnAwake(Bootloader bootloader)
        {
            this.integerVariable.OnChange += this.OnSettingChanged;

            if (this.intToggle != null)
            {
                this.intToggle.onValueChanged.AddListener(this.OnToggleValueChanged);
            }

            this.OnSettingChanged(default, this.integerVariable.Value);
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNull(this, this.integerVariable, nameof(this.integerVariable));
            report.AssertNotNull(this, this.intToggle, nameof(this.intToggle));
        }

        private void OnToggleValueChanged(bool newValue)
        {
            if (newValue)
            {
                this.integerVariable.Value = this.intToggleValue;
            }
        }

        private void OnSettingChanged(int oldValue, int newValue)
        {
            if (this.intToggle != null)
            {
                this.intToggle.SetIsOnWithoutNotify(this.integerVariable.Value == this.intToggleValue);
            }
        }

        private void OnDestroy()
        {
            if (this.integerVariable == null)
            {
                return;
            }

            this.integerVariable.OnChange -= this.OnSettingChanged;

            if (this.intToggle != null)
            {
                this.intToggle.onValueChanged.RemoveListener(this.OnToggleValueChanged);
            }
        }
    }
}
