#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="ForceFlatModeInEditor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_UNITY_XR_INTERACTION_TOOLKIT

namespace OGT.XR
{
    public static class ForceFlatModeInEditorUtil
    {
        private const string Key = "ForceFlatModeInEditor";
        private const string ForceFlatModeInEditorPath = "Tools/XR Util/Force Flat Mode In Editor";

#if !UNITY_EDITOR
        public static bool ForceFlatModeInEditor => false;
#else

        public static bool ForceFlatModeInEditor
        {
            get => UnityEditor.EditorPrefs.GetBool($"{UnityEditor.PlayerSettings.applicationIdentifier}-{Key}", false);
            set => UnityEditor.EditorPrefs.SetBool($"{UnityEditor.PlayerSettings.applicationIdentifier}-{Key}", value);
        }

        [UnityEditor.MenuItem(ForceFlatModeInEditorPath, false, -1)]
        public static void SetForceFlatModeInEditor()
        {
            ForceFlatModeInEditor = !ForceFlatModeInEditor;
        }

        [UnityEditor.MenuItem(ForceFlatModeInEditorPath, true, -1)]
        private static bool SetForceFlatModeEditorValidate()
        {
            UnityEditor.Menu.SetChecked(ForceFlatModeInEditorPath, ForceFlatModeInEditor);
            return true;
        }

#endif
    }
}

#endif
