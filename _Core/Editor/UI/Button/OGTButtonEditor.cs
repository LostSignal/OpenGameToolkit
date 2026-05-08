//-----------------------------------------------------------------------
// <copyright file="OGTButtonEditor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEditor;
    using UnityEditor.UI;

    [CustomEditor(typeof(OGTButton))]
    public class OGTButtonEditor : ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            UIActionsEditor.DrawUIActionGUI(this.target);
        }
    }
}
