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
    using UnityEngine;

    public sealed class LocalizationManager : Manager
    {
        [SerializeField] private List<LocalizationTable> localizationTables;

        private Dictionary<string, LocalizationTable> localizationTablesById;

        public string GetLocalizedString(LocalizedString localizedString)
        {
            if (localizedString.IsValid == false)
            {
                Debug.LogError("Tried to lookup invalid Localization String!");
                return null;
            }

            if (this.localizationTablesById.TryGetValue(localizedString.TableId, out var table))
            {
                return table.GetValue(localizedString.Id);
            }

            Debug.LogError($"Found unknown Localization Table Id {localizedString.TableId}");
            return null;
        }

        protected override Task InitializeManager(Bootloader bootloader)
        {
            // ------------------ Old Depricated System (only used by Tien Len) ------------------
            var localizationAsset = Resources.Load<TextAsset>("Localization");

            if (localizationAsset != null)
            {
                this.depricatedLocalizationTable = Newtonsoft.Json.JsonConvert.DeserializeObject<DepricatedLocalizationTable>(localizationAsset.text);
            }

            // ------------------------------------ New System ------------------------------------
            this.localizationTablesById = new Dictionary<string, LocalizationTable>(this.localizationTables.Count);

            foreach (var table in this.localizationTables)
            {
                this.localizationTablesById.Add(table.TableId, table);
            }

            return Task.CompletedTask;
        }

        //// -------------------------------- Depricated System ------------------------------------

        private DepricatedLocalizationTable depricatedLocalizationTable;

        public string DepricatedGetLocalization(Language language, string key)
        {
            if (language == Languages.English)
            {
                return this.depricatedLocalizationTable.Entries[key].English;
            }
            else if (language == Languages.Vietnamese)
            {
                return this.depricatedLocalizationTable.Entries[key].Vietnamese;
            }

            return null;
        }

        private class DepricatedLocalizationTable
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
