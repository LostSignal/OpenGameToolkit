//-----------------------------------------------------------------------
// <copyright file="Languages.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Localization
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public static class Languages
    {
        public static readonly Language English = new("English", "English", "en", ",", ".", "Yes", "No");
        public static readonly Language Vietnamese = new("Vietnamese", "Tiếng Việt", "vi", ".", ",", "Có", "Không");

        private static ReadOnlyCollection<Language> languages;
        private static ReadOnlyCollection<string> languageNames;

        public static ReadOnlyCollection<Language> AllLanguages
        {
            get
            {
                if (languages == null)
                {
                    languages = new ReadOnlyCollection<Language>(new List<Language>
                    {
                        English,
                        Vietnamese,
                    });
                }

                return languages;
            }
        }

        public static ReadOnlyCollection<string> AllIsoLanguageNames
        {
            get
            {
                if (languageNames == null)
                {
                    List<string> names = new();

                    for (int i = 0; i < AllLanguages.Count; i++)
                    {
                        names.Add(AllLanguages[i].IsoLanguageName);
                    }

                    languageNames = new ReadOnlyCollection<string>(names);
                }

                return languageNames;
            }
        }
    }
}
