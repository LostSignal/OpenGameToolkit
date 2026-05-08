//-----------------------------------------------------------------------
// <copyright file="StringPropertyTextBinding.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using OGT.Properties;
    using TMPro;
    using UnityEngine;

    public class StringPropertyTextBinding : GameBehavior, IAwake, IValidate
    {
#pragma warning disable 0649
        [SerializeField] private StringProperty stringProperty;

        [Header("Binding Objects")]
        [SerializeField] private TMP_Text text;
#pragma warning restore 0649

        public void OnAwake(Bootloader bootloader)
        {
            this.stringProperty.OnChange += this.OnSettingChanged;

            this.OnSettingChanged(null, this.stringProperty.Value);
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNull(this, this.stringProperty, nameof(this.stringProperty));
            report.AssertNotNull(this, this.text, nameof(this.text));
        }

        private void OnSettingChanged(string oldValue, string newValue)
        {
            if (this.text != null)
            {
                BetterStringBuilder.New().Append(this.stringProperty.Value).Set(this.text);
            }
        }

        private void OnDestroy()
        {
            if (this.stringProperty == null)
            {
                return;
            }

            this.stringProperty.OnChange -= this.OnSettingChanged;
        }
    }
}
