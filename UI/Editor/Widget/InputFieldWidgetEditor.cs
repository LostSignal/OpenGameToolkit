namespace OGT
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(InputFieldWidget))]
    public class InputFieldWidgetEditor : OGT.Editor
    {
        protected override void NewOnInspectorGUI()
        {
            this.DrawDefaultInspector();

            GUILayout.Space(20);

            var inputFieldWidget = this.target as InputFieldWidget;

            var placeholderText = inputFieldWidget.PlaceholderText;
            if (placeholderText != null)
            {
                this.DrawProperty(placeholderText, "text", "Placeholder Text");
            }

            var inputFieldText = inputFieldWidget.InputText;
            if (inputFieldText != null)
            {
                this.DrawProperty(inputFieldText, "text", "Input Text");
            }
        }
    }
}
