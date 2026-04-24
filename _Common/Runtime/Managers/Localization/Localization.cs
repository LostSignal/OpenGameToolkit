//-----------------------------------------------------------------------
// <copyright file="Localization.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Localization
{
    using System.Collections.Generic;
    using OGT;

    public static class Localization
    {
        private const string CurrentLanguageKey = "CurrentLanguage";

        private static LanguageChangedDelegate languagedChanged;
        private static Language currentLanguage = null;

        public delegate void LanguageChangedDelegate();

        public static event LanguageChangedDelegate OnLanguagedChanged
        {
            add => languagedChanged += value;
            remove => languagedChanged -= value;
        }

        public static Language CurrentLanguage
        {
            get
            {
                SetCurrentLanguage();
                return currentLanguage;
            }

            set
            {
                if (value != null && currentLanguage != value)
                {
                    currentLanguage = value;

                    UpdateBetterStringBuilder();
                    SaveCurrentLangauge();
                    UpdateI2Localization();

                    languagedChanged?.Invoke();
                }
            }
        }

        // TODO [bgish]: Get this from the AppConfig Runtime properties
        public static Language DefaultLanguage
        {
            get { return Languages.English; }
        }

        // TODO [bgish]: Get this from the AppConfig Runtime properties
        public static IEnumerable<Language> GetSupportedLanguages()
        {
            yield return Languages.English;
            yield return Languages.Vietnamese;
        }

        public static string GetTranslation(string localizationKey)
        {
            throw new System.NotImplementedException();
            //// return LocalizationManager.Instance.GetLocalization(currentLanguage, localizationKey);
        }

        public static string GetThousandsSeperator()
        {
            return CurrentLanguage.ThousandsSeparator;
        }

        public static string GetDecimalPointSeperator()
        {
            return CurrentLanguage.DecimalSeparator;
        }

        private static void SetCurrentLanguage()
        {
            if (currentLanguage != null)
            {
                return;
            }

            string currentLanguageName = LostPlayerPrefs.GetString(CurrentLanguageKey, null);

            if (currentLanguageName == null)
            {
                // This is the first time running the app, so the language has never been
                // set, so determining the language based on the device's SystemLanguage.
                foreach (var language in GetSupportedLanguages())
                {
                    if (language.LanguageCode == Platform.GetCurrentISOLanguageId())
                    {
                        currentLanguage = language;
                        UpdateBetterStringBuilder();
                        SaveCurrentLangauge();
                        UpdateI2Localization();
                        return;
                    }
                }
            }
            else
            {
                // Looking up the Language by the name we last saved in LostPlayerPrefs
                foreach (var language in GetSupportedLanguages())
                {
                    if (language.IsoLanguageName == currentLanguageName)
                    {
                        currentLanguage = language;
                        UpdateBetterStringBuilder();
                        UpdateI2Localization();
                        return;
                    }
                }
            }

            // If we got here, then we failed to find the right language, so returning the default
            currentLanguage = DefaultLanguage;
            UpdateBetterStringBuilder();
            SaveCurrentLangauge();
            UpdateI2Localization();
        }

        private static void SaveCurrentLangauge()
        {
            LostPlayerPrefs.SetString(CurrentLanguageKey, CurrentLanguage.IsoLanguageName);
            LostPlayerPrefs.Save();
        }

        // TODO [bgish]: When Lost Localization is far enough along, remove this
        private static void UpdateI2Localization()
        {
#if USING_I2_LOCALIZATION
            // I2.Loc.LocalizationManager.CurrentLanguage = Localization.CurrentLanguage.IsoLanguageName;
#endif
        }

        private static void UpdateBetterStringBuilder()
        {
            BetterStringBuilder.SetDecimalPointSeperator(currentLanguage.DecimalSeparator);
            BetterStringBuilder.SetThousandsSeperator(currentLanguage.ThousandsSeparator);
        }
    }
}
