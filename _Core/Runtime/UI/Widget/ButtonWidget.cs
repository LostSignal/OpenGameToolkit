namespace OGT
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class ButtonWidget : Widget, IValidate
    {
        [Header("UI Objects")]
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text text;

        public Button Button => this.button;

        public TMP_Text ButtonText => this.text;

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            this.EditorGetFirstComponentInChildren(ref this.button, true);
            this.EditorGetFirstComponentInChildren(ref this.text, true);

            report.AssertNotNull(this, this.button, nameof(this.button));
            report.AssertNotNull(this, this.text, nameof(this.text));
        }
    }
}
