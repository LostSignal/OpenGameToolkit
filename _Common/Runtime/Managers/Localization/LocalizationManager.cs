//-----------------------------------------------------------------------
// <copyright file="LocalizationManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using OGT.Localization;

    public sealed class LocalizationManager : Manager
    {
        private LocalizationTable localizationTable;

        public string GetLocalization(Language language, string key)
        {
            if (language == Languages.English)
            {
                return this.localizationTable.Entries[key].English;
            }
            else if (language == Languages.Vietnamese)
            {
                return this.localizationTable.Entries[key].Vietnamese;
            }

            return null;
        }

        protected override Task InitializeManager(Bootloader bootloader)
        {
            var localizationAsset = UnityEngine.Resources.Load<UnityEngine.TextAsset>("Localization");

            if (localizationAsset != null)
            {
                this.localizationTable = Newtonsoft.Json.JsonConvert.DeserializeObject<LocalizationTable>(localizationAsset.text);
            }

            return Task.CompletedTask;
        }

        private class LocalizationTable
        {
            public Dictionary<string, Entry> Entries { get; set; }

            public class Entry
            {
                public string Description { get; set; }
                public string English { get; set; }
                public string Vietnamese { get; set; }
            }
        }
    }
}
