//-----------------------------------------------------------------------
// <copyright file="CloudFunctionsUtil.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if USING_PLAYFAB

namespace Lost.CloudFunctions
{
    using UnityEngine;

    public static class CloudFunctionsUtil
    {
#if UNITY_EDITOR
        private const string EditorPrefsKey = "CloudFunctionsUseLocalhost";
        private const string UseLocalhostFuctionsPath = "Tools/Lost/Cloud Functions/Use Localhost Functions";
        private const string UsePlayFabFunctionsPath = "Tools/Lost/Cloud Functions/Use PlayFab Functions";

        public static bool UseLocalhostFunctions
        {
            get => UnityEditor.EditorPrefs.GetBool($"{Application.productName}.{EditorPrefsKey}", false);
            set => UnityEditor.EditorPrefs.SetBool($"{Application.productName}.{EditorPrefsKey}", value);
        }

        [UnityEditor.MenuItem(UsePlayFabFunctionsPath, false, 1)]
        private static void UsePlayFabFunctionsPathMenuItem()
        {
            UseLocalhostFunctions = false;
        }

        [UnityEditor.MenuItem(UsePlayFabFunctionsPath, true, 1)]
        private static bool UsePlayFabFunctionsPathMenuItemValidate()
        {
            UnityEditor.Menu.SetChecked(UsePlayFabFunctionsPath, !UseLocalhostFunctions);
            return true;
        }

        [UnityEditor.MenuItem(UseLocalhostFuctionsPath, false, 1)]
        private static void UseLocalhostFunctionsMenuItem()
        {
            UseLocalhostFunctions = true;
        }

        [UnityEditor.MenuItem(UseLocalhostFuctionsPath, true, 1)]
        private static bool UseLocalhostFunctionsMenuItemValidate()
        {
            UnityEditor.Menu.SetChecked(UseLocalhostFuctionsPath, UseLocalhostFunctions);
            return true;
        }

#else
        public static bool UseLocalhostFunctions => false;
#endif
    }
}

#endif
