//-----------------------------------------------------------------------
// <copyright file="ForceFlatModeInEditor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.XR
{
    public static class ForceFlatModeInEditorUtil
    {
#if !UNITY_EDITOR

        public static bool ForceFlatModeInEditor => false;

#else

        private const string Key = "ForceFlatModeInEditor";

        public static bool ForceFlatModeInEditor
        {
            get => UnityEditor.EditorPrefs.GetBool($"{UnityEditor.PlayerSettings.applicationIdentifier}-{Key}", false);
            set => UnityEditor.EditorPrefs.SetBool($"{UnityEditor.PlayerSettings.applicationIdentifier}-{Key}", value);
        }

#endif
    }
}
