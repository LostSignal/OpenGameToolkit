//-----------------------------------------------------------------------
// <copyright file="NewSpinnerBox.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using TMPro;
    using UnityEngine;

    public class NewSpinnerBox : PanelLogic
    {
#pragma warning disable 0649, 0044
        [SerializeField] private ModalWidget modalWidget;
        [SerializeField] private TMP_Text bodyText;
#pragma warning restore 0649, 0044

        public void Show(string title, string body)
        {
            this.modalWidget.TitleText.text = title;
            this.UpdateBodyText(body);
            this.Panel.Show();
        }

        public void UpdateBodyText(string body)
        {
            this.bodyText.text = body;
        }

        public override void Validate(ValidationReport report, bool isSceneObject)
        {
            base.Validate(report, isSceneObject);
            report.AssertNotNull(this.modalWidget, this, nameof(this.modalWidget));
            report.AssertNotNull(this.bodyText, this, nameof(this.bodyText));
        }
    }
}
