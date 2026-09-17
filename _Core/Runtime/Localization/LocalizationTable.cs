//-----------------------------------------------------------------------
// <copyright file="LocalizationTable.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Localization Table", menuName = "OGT/Localization Table")]
    public class LocalizationTable : Resource
    {
        private static Dictionary<string, LocalizationTable> tableRegistry = new();

        [SerializeField] private string tableId;
        [SerializeField] private List<Entry> entries = new();

        private Dictionary<string, string> entryLookup;

        public string TableId => this.tableId;

        public string GetValue(string key)
        {
            this.InitializeLookup();
            return this.entryLookup.TryGetValue(key, out var value) ? value : $"[{key}]";
        }

        private void InitializeLookup()
        {
            if (this.entryLookup == null)
            {
                this.entryLookup = new Dictionary<string, string>();

                foreach (var entry in this.entries)
                {
                    this.entryLookup[entry.Key] = entry.Value;
                }
            }
        }

#if UNITY_EDITOR
        public static LocalizationTable GetTableById(string tableId)
        {
            if (string.IsNullOrEmpty(tableId))
                return null;

            if (tableRegistry.TryGetValue(tableId, out var table))
                return table;

            return null;
        }

        public static void RegisterTable(LocalizationTable table)
        {
            if (table == null || string.IsNullOrEmpty(table.tableId))
                return;

            tableRegistry[table.tableId] = table;
        }

        public static void UnregisterTable(LocalizationTable table)
        {
            if (table == null || string.IsNullOrEmpty(table.tableId))
                return;

            if (tableRegistry.TryGetValue(table.tableId, out var registered) && registered == table)
            {
                tableRegistry.Remove(table.tableId);
            }
        }

        public static IEnumerable<string> GetAllRegisteredTableIds() => tableRegistry.Keys;

        public static IEnumerable<LocalizationTable> GetAllRegisteredTables() => tableRegistry.Values;

        public string EditorTableId
        {
            get => this.tableId;
            set => this.tableId = value;
        }

        private void OnEnable()
        {
            RegisterTable(this);
        }

        private void OnDisable()
        {
            UnregisterTable(this);
        }

        public IReadOnlyList<Entry> Entries => this.entries;

        public IEnumerable<string> GetAllKeys()
        {
            foreach (var entry in this.entries)
            {
                yield return entry.Key;
            }
        }

        public void AddEntry(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                return;

            // Check if key already exists
            foreach (var entry in this.entries)
            {
                if (entry.Key == key)
                    return;
            }

            this.entries.Add(new Entry { Key = key, Value = value });
            this.entryLookup = null; // Reset lookup
        }

        public void RemoveEntry(string key)
        {
            for (int i = this.entries.Count - 1; i >= 0; i--)
            {
                if (this.entries[i].Key == key)
                {
                    this.entries.RemoveAt(i);
                    this.entryLookup = null; // Reset lookup
                    return;
                }
            }
        }

        public void UpdateEntry(string key, string newValue)
        {
            foreach (var entry in this.entries)
            {
                if (entry.Key == key)
                {
                    entry.Value = newValue;
                    this.entryLookup = null; // Reset lookup
                    return;
                }
            }
        }

#endif

        [Serializable]
        public class Entry
        {
            [SerializeField] private string key;
            [SerializeField] private long lastModified;
            [SerializeField] private string value;

            public string Key
            {
                get => this.key;

#if UNITY_EDITOR
                set => this.key = value;
#endif
            }

            public string Value
            {
                get => this.value;

#if UNITY_EDITOR
                set
                {
                    if (this.value != value)
                    {
                        this.value = value;
                        this.lastModified = DateTime.UtcNow.ToBinary();
                    }
                }
#endif
            }

            public DateTime LastModified => DateTime.FromBinary(this.lastModified);
        }
    }
}
