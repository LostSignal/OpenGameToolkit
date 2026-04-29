namespace OGT
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(ModalWidget))]
    public class ModalWidgetEditor : OGT.Editor
    {
        protected override void NewOnInspectorGUI()
        {
            this.DrawDefaultInspector();

            GUILayout.Space(20);

            var modalWidget = this.target as ModalWidget;
            var modalTitleText = modalWidget.TitleText;

            if (modalTitleText != null)
            {
                this.DrawProperty(modalTitleText, "text", "Title Text");
            }
        }
    }
}
