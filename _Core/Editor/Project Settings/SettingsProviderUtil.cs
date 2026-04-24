//-----------------------------------------------------------------------
// <copyright file="SettingsProviderUtil.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using UnityEditor;

    public static class SettingsProviderUtil
    {
        public static SettingsProvider GetSettingsProvider(UnityEngine.Object settingsObject, string windowPath)
        {
            var keywords = GetSearchKeywordsFromSerializedProperties(settingsObject);
            var provider = AssetSettingsProvider.CreateProviderFromObject(windowPath, settingsObject, keywords);

            provider.inspectorUpdateHandler += () =>
            {
                if (provider != null &&
                    provider.settingsEditor != null &&
                    provider.settingsEditor.serializedObject != null &&
                    provider.settingsEditor.serializedObject.UpdateIfRequiredOrScript())
                {
                    provider.Repaint();
                }
            };

            return provider;

            static List<string> GetSearchKeywordsFromSerializedProperties(UnityEngine.Object settingsObject)
            {
                var results = new List<string>();
                var serializedObject = new SerializedObject(settingsObject);
                var property = serializedObject.GetIterator();

                // TODO [bgish]: This returns too much, can I only get properties that belong to the LostLibrarySettings class?
                while (property.Next(true))
                {
                    results.AddIfUnique(property.displayName.ToLowerInvariant());
                }

                return results;
            }
        }
    }
}
