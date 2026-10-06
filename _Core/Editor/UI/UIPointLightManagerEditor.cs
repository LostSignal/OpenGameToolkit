//-----------------------------------------------------------------------
// <copyright file="UIPointLightManagerEditor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEditor;

    [CustomEditor(typeof(UIPointLightManager))]
    public class UIPointLightManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                ((UIPointLightManager)this.target).UpdateLightingAndShadows();
            }
        }
    }
}
