//-----------------------------------------------------------------------
// <copyright file="OGTToggleEditor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEditor;
    using UnityEditor.UI;

    [CustomEditor(typeof(OGTToggle))]
    public class OGTToggleEditor : ToggleEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            UIActionsEditor.DrawUIActionGUI(this.target);
        }
    }
}
