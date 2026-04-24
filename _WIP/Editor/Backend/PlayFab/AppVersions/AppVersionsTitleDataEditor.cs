#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="AppVersionsTitleDataEditor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_PLAYFAB

namespace OGT.PlayFab
{
    using UnityEditor;

    [CustomEditor(typeof(AppVersionsTitleData))]
    public class AppVersionsTitleDataEditor : TitleDataEditor<AppVersionsTitleDataEditorWindow>
    {
        public override string WindowName => "App Verions";
    }

    public class AppVersionsTitleDataEditorWindow : TitleDataEditorWindow<AppVersionsData, AppVersionsTitleData>
    {
        protected override void DrawData(AppVersionsData item, SerializedObject serializedObject, SerializedProperty serializedProperty)
        {
            using (new LabelWidthScope(200.0f))
            {
                EditorGUILayout.PropertyField(serializedProperty, true);
            }
        }
    }
}

#endif
