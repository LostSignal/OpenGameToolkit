namespace OGT
{
    using System.Linq;
    using TMPro;
    using UnityEngine;

    public class InputFieldWidget : Widget, IValidate
    {
        [Header("UI Objects")]
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text placeholderText;
        [SerializeField] private TMP_Text inputText;

        public TMP_InputField InputField => this.inputField;

        public TMP_Text PlaceholderText => this.placeholderText;

        public TMP_Text InputText => this.inputText;

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            this.EditorGetFirstComponentInChildren(ref this.inputField, true);
            this.EditorGetFirstComponentInChildren(ref this.placeholderText, true);
            this.EditorGetFirstComponentInChildren(ref this.inputText, true);

            report.AssertNotNull(this, this.inputField, nameof(this.inputField));
            report.AssertNotNull(this, this.placeholderText, nameof(this.placeholderText));
            report.AssertNotNull(this, this.inputText, nameof(this.inputText));
        }
    }
}
