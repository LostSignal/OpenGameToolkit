//-----------------------------------------------------------------------
// <copyright file="EnumPropertyToggleBinding.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using OGT.Properties;
    using UnityEngine;
    using UnityEngine.UI;

    public class EnumPropertyToggleBinding : GameBehavior, IAwake, IValidate
    {
#pragma warning disable 0649, 0044
        [Header("Binding Object")]
        [SerializeField] private Toggle toggle;

        [Header("Enum")]
        [SerializeField] private EnumProperty enumVariable;
        [SerializeField] private EnumValue enumValue;
#pragma warning restore 0649, 0044

        public void OnAwake(Bootloader bootloader)
        {
            this.enumVariable.OnChange += this.OnSettingChanged;

            if (this.toggle != null)
            {
                this.toggle.onValueChanged.AddListener(this.OnToggleValueChanged);
            }

            this.OnSettingChanged(default, this.enumVariable.Value);
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNull(this, this.enumVariable, nameof(this.enumVariable));
            report.AssertNotNull(this, this.toggle, nameof(this.toggle));
        }

        private void Awake() => ActivationManager.Register(this);

        private void OnToggleValueChanged(bool newValue)
        {
            if (newValue)
            {
                this.enumVariable.Value = this.enumValue;
            }
        }

        private void OnSettingChanged(EnumValue oldValue, EnumValue newValue)
        {
            if (this.toggle != null)
            {
                this.toggle.SetIsOnWithoutNotify(this.enumVariable.Value == this.enumValue);
            }
        }

        private void OnDestroy()
        {
            if (this.enumVariable == null)
            {
                return;
            }

            this.enumVariable.OnChange -= this.OnSettingChanged;

            if (this.toggle != null)
            {
                this.toggle.onValueChanged.RemoveListener(this.OnToggleValueChanged);
            }
        }
    }
}
