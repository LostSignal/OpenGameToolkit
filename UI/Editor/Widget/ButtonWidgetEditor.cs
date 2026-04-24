
namespace OGT
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(ButtonWidget))]
    public class ButtonWidgetEditor : OGT.Editor
    {
        protected override void NewOnInspectorGUI()
        {
            this.DrawDefaultInspector();

            GUILayout.Space(20);

            var buttonWidget = this.target as ButtonWidget;
            var buttonText = buttonWidget.ButtonText;

            if (buttonText != null)
            {
                this.DrawProperty(buttonText, "text", "Button Text");
            }
        }
    }
}
