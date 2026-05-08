namespace OGT
{
    using System.Linq;
    using TMPro;
    using UnityEngine;

    public class ModalWidget : Widget, IValidate
    {
        [Header("UI Objects")]
        [SerializeField] private TMP_Text titleText;

        public TMP_Text TitleText => this.titleText;

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            this.EditorGetFirstComponentInChildren(ref this.titleText, true);

            report.AssertNotNull(this, this.titleText, nameof(this.titleText));
        }
    }
}
