namespace OGT
{
#if UNITY_EDITOR
    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    ////
    //// https://gist.github.com/wappenull/668a492c80f7b7fda0f7c7f42b3ae0b0
    ////
    public static class GameViewUtils
    {
        private static object s_GameViewSizes_instance;
        private static Type s_GameViewType;
        private static Type s_GameViewSizesType;
        private static Type s_GameViewSizeSingleType;
        private static MethodInfo s_GameView_SetCustomResolution;
        private static MethodInfo s_GameView_SizeSelectionCallback;
        private static MethodInfo s_GameViewSizes_GetGroup;

        static GameViewUtils()
        {
            s_GameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
            s_GameViewSizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
            s_GameViewSizeSingleType = typeof(ScriptableSingleton<>).MakeGenericType(s_GameViewSizesType);

            s_GameView_SetCustomResolution = s_GameViewType.GetMethod("SetCustomResolution", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            s_GameView_SizeSelectionCallback = s_GameViewType.GetMethod("SizeSelectionCallback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            s_GameViewSizes_GetGroup = s_GameViewSizesType.GetMethod("GetGroup");

            var instanceProp = s_GameViewSizeSingleType.GetProperty("instance");
            s_GameViewSizes_instance = instanceProp.GetValue(null, null);
        }

        public static Vector2 GetResolution()
        {
            var targetRenderSizePropertyInfo = s_GameViewType.GetProperty("targetRenderSize", BindingFlags.NonPublic | BindingFlags.Instance);
            var targetRenderSizeGetMethod = targetRenderSizePropertyInfo.GetGetMethod(true);

            return (Vector2)targetRenderSizeGetMethod.Invoke(GetMainGameView(), null);
        }

        public static void AddResolution(int width, int height, string name)
        {
            int foundIndex = FindSize(GetCurrentGroupType(), name);

            if (foundIndex >= 0)
            {
                return;
            }

            s_GameView_SetCustomResolution.Invoke(GetMainGameView(), new object[] { new Vector2(width, height), name });
        }

        public static void SetResolutionByName(string name)
        {
            TrySetSize(name);
        }

        public static EditorWindow GetMainGameView()
        {
            return EditorWindow.GetWindow(s_GameViewType);
        }

        /// <summary>
        /// Try to find and set game view size to specified query.
        /// Size must be already exists in game view setting.
        /// You must send the right game view (your current platform) in order to get the right result.
        /// </summary>
        /// <param name="sizeText">Query string such as 1280x720 or 16:9</param>
        private static bool TrySetSize(string sizeText)
        {
            GameViewSizeGroupType currentGroup = GetCurrentGroupType();
            int foundIndex = FindSize(currentGroup, sizeText);
            if (foundIndex < 0)
            {
                UnityEngine.Debug.LogError($"Size {sizeText} was not found in game view settings");
                return false;
            }

            SetSizeIndex(foundIndex);
            return true;
        }

        /// <summary>
        /// Set current gameview size to target resolution index.
        /// Index must be known beforehand.
        /// </summary>
        private static void SetSizeIndex(int index)
        {
            // Calling GameView.SizeSelectionCallback will also auto focus game view,
            // We will restore focus if it is something else
            EditorWindow currentWindow = EditorWindow.focusedWindow;
            SceneView lastSceneView = SceneView.lastActiveSceneView;

            EditorWindow gv = EditorWindow.GetWindow(s_GameViewType);
            s_GameView_SizeSelectionCallback.Invoke(gv, new object[] { index, null });

            // Hack, will mock re-active scene view, in case it was active,
            // Because EditorWindow.focusedWindow could now be inspector
            // If scene view and game view were in same docking group,
            // SizeSelectionCallback will switch to game view without knowing if user left scene view visible or not.
            // - If last active was actually game view, it should be corrected by currentWindow.Focus, no problem
            // - If last active is something else, like console for inspector, this will bring up scene view, should be no harm.
            // Remove this out if you do not want this behavior
            if (lastSceneView != null)
                lastSceneView.Focus();

            if (currentWindow != null)
                currentWindow.Focus();
        }

        /// <summary>
        /// Finding text could be fixed resoluation as WxH "1280x720"
        /// or ratio like W:H "16:9"
        /// </summary>
        private static int FindSize(GameViewSizeGroupType sizeGroupType, string text)
        {
            var group = GetGroup(sizeGroupType); // class GameViewSizeGroup
            var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
            var displayTexts = getDisplayTexts.Invoke(group, null) as string[];

            for (int i = 0; i < displayTexts.Length; i++)
            {
                string display = displayTexts[i];

                bool found = display.Contains(text);
                if (found)
                    return i;
            }

            return -1;
        }

        private static object GetGroup(GameViewSizeGroupType type)
        {
            return s_GameViewSizes_GetGroup.Invoke(s_GameViewSizes_instance, new object[] { (int)type });
        }

        private static GameViewSizeGroupType GetCurrentGroupType()
        {
#if UNITY_IOS
            return GameViewSizeGroupType.iOS;
#elif UNITY_ANDROID
            return GameViewSizeGroupType.Android;
#else
            return GameViewSizeGroupType.Standalone;
#endif
        }
    }
#endif
}
