namespace OGT
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(ScriptableObject), editorForChildClasses: true, isFallback = false)]
    public class DefaultScriptableObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            DefaultMonoBehaviourEditor.DrawDefaultContent(this.target);
        }
    }
}
