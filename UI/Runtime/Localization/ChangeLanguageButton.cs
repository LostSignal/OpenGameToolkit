//-----------------------------------------------------------------------
// <copyright file="ChangeLanguageButton.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Localization
{
    using OGT;
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Button))]
    public class ChangeLanguageButton : MonoBehaviour
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

#pragma warning disable 0649
        [ReadOnly]
        [SerializeField] private Button button;
        [SerializeField] private string isoLanguageName;
#pragma warning restore 0649

        private void OnValidate()
        {
            this.EditorGetFirstComponentInChildren(ref this.button, true);
        }

        private void Awake()
        {
            this.button.onClick.AddListener(this.Clicked);
        }

        private void Clicked()
        {
            foreach (var language in Localization.GetSupportedLanguages())
            {
                if (language.IsoLanguageName == this.isoLanguageName)
                {
                    Localization.CurrentLanguage = language;
                    return;
                }
            }

            Logger.LogErrorFormat(this, "ChangeLanguage.ChangeLanguageTo couldn't find supported language {0}!", this.isoLanguageName);
        }
    }
}
