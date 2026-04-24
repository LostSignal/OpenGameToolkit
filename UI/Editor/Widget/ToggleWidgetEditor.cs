
namespace OGT
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(ToggleWidget))]
    public class ToggleWidgetEditor : OGT.Editor
    {
        protected override void NewOnInspectorGUI()
        {
            this.DrawDefaultInspector();

            GUILayout.Space(20);

            var toggleWidget = this.target as ToggleWidget;
            var toggleText = toggleWidget.ToggleText;

            if (toggleText != null)
            {
                this.DrawProperty(toggleText, "text", "Toggle Text");
            }
        }
    }
}
