//-----------------------------------------------------------------------
// <copyright file="Language.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Localization
{
    public class Language
    {
        public Language(string isoLanguageName, string nativeLanguageName, string languageCode, string thousandsSeparator, string decimalSeparator, string yes, string no)
        {
            this.IsoLanguageName = isoLanguageName;
            this.NativeLanguageName = nativeLanguageName;
            this.LanguageCode = languageCode;
            this.ThousandsSeparator = thousandsSeparator;
            this.DecimalSeparator = decimalSeparator;
            this.Yes = yes;
            this.No = no;
        }

        public string IsoLanguageName { get; private set; }

        public string NativeLanguageName { get; private set; }

        public string LanguageCode { get; private set; }

        public string ThousandsSeparator { get; private set; }

        public string DecimalSeparator { get; private set; }

        public string Yes { get; private set; }

        public string No { get; private set; }
    }
}
