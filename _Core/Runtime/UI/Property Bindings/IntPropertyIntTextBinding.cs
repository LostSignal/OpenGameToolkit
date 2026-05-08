//-----------------------------------------------------------------------
// <copyright file="IntPropertyIntTextBinding.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using OGT.Properties;
    using UnityEngine;

    public class IntPropertyIntTextBinding : MonoBehaviour, IAwake, IValidate
    {
#pragma warning disable 0649, 0044
        [SerializeField] private IntProperty integerVariable;

        [Header("Binding Objects")]
        [SerializeField] private IntText intText;
        [SerializeField] private TextUpdateType intTextUpdateType;
#pragma warning restore 0649, 0044

        public void OnAwake(Bootloader bootloader)
        {
            this.integerVariable.OnChange += this.OnSettingChanged;

            this.OnSettingChanged(default, this.integerVariable.Value);
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNull(this, this.integerVariable, nameof(this.integerVariable));
            report.AssertNotNull(this, this.intText, nameof(this.intText));
        }

        private void Awake() => ActivationManager.Register(this);

        private void OnSettingChanged(int oldValue, int newValue)
        {
            if (this.intText != null)
            {
                this.intText.UpdateValue(this.integerVariable.Value, this.intTextUpdateType);
            }
        }

        private void OnDestroy()
        {
            if (this.integerVariable == null)
            {
                return;
            }

            this.integerVariable.OnChange -= this.OnSettingChanged;
        }
    }
}
