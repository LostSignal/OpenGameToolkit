namespace OGT
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class ToggleWidget : Widget
    {
        [Header("UI Objects")]
        [SerializeField] private Toggle toggle;
        [SerializeField] private TMP_Text toggleLabel;

        public Toggle Toggle => this.toggle;

        public TMP_Text ToggleText => this.toggleLabel;

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            this.EditorGetFirstComponentInChildren(ref this.toggle, true);
            this.EditorGetFirstComponentInChildren(ref this.toggleLabel, true);

            report.AssertNotNull(this, this.toggle, nameof(this.toggle));
            report.AssertNotNull(this, this.toggleLabel, nameof(this.toggleLabel));
        }
    }
}
