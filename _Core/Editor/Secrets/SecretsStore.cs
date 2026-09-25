//-----------------------------------------------------------------------
// <copyright file="SecretsStore.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Unity.Scripting.LifecycleManagement;
    using UnityEngine;

    //// NOTE [bgish]: Secrets are lazily loaded from ProjectSettings/OGTSecrets.json the first time they're
    ////               requested and cached in a static dictionary.  AutoStaticsCleanup makes sure that cache
    ////               is thrown away when entering play mode without a domain reload.
    ////
    //// TODO [bgish]: This file is plain JSON for now.  Eventually it will be an encrypted blob that only
    ////               decrypts when the correct environment variable is present.
    [AutoStaticsCleanup]
    public static class SecretsStore
    {
        public const string SecretsFilePath = "ProjectSettings/OGTSecrets.json";

        private static readonly OGTLogger Logger = OGTLogger.OGTEditor;

        private static Dictionary<string, string> secrets;

        public static bool IsLoaded => secrets != null;

        public static int Count => GetSecrets().Count;

        public static IEnumerable<string> GetKeys()
        {
            return GetSecrets().Keys.OrderBy(x => x, StringComparer.Ordinal);
        }

        public static IReadOnlyDictionary<string, string> GetAll()
        {
            return GetSecrets();
        }

        public static bool Contains(string key)
        {
            return string.IsNullOrEmpty(key) == false && GetSecrets().ContainsKey(key);
        }

        public static bool TryGetValue(string key, out string value)
        {
            value = null;
            return string.IsNullOrEmpty(key) == false && GetSecrets().TryGetValue(key, out value);
        }

        public static string GetValue(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Logger.LogWarning("[SecretsStore] Tried to get a secret with an empty key.");
                return null;
            }

            if (GetSecrets().TryGetValue(key, out string value))
            {
                return value;
            }

            Logger.LogWarning($"[SecretsStore] Secret '{key}' was not found in {SecretsFilePath}.");
            return null;
        }

        public static void Set(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Secret key can't be null or whitespace.", nameof(key));
            }

            GetSecrets()[key] = value ?? string.Empty;
        }

        public static bool Remove(string key)
        {
            return string.IsNullOrEmpty(key) == false && GetSecrets().Remove(key);
        }

        public static void ReplaceAll(IEnumerable<KeyValuePair<string, string>> newSecrets)
        {
            var replacement = new Dictionary<string, string>();

            foreach (var pair in newSecrets)
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                {
                    throw new ArgumentException("Secret key can't be null or whitespace.", nameof(newSecrets));
                }

                replacement[pair.Key] = pair.Value ?? string.Empty;
            }

            secrets = replacement;
        }

        public static void Reload()
        {
            secrets = null;
            GetSecrets();
        }

        public static void Save()
        {
            var file = new SecretsFile();

            foreach (var key in GetKeys())
            {
                file.secrets.Add(new SecretEntry { key = key, value = secrets[key] });
            }

            var directoryName = Path.GetDirectoryName(SecretsFilePath);

            if (string.IsNullOrEmpty(directoryName) == false && Directory.Exists(directoryName) == false)
            {
                Directory.CreateDirectory(directoryName);
            }

            File.WriteAllText(SecretsFilePath, JsonUtility.ToJson(file, true));
        }

        private static Dictionary<string, string> GetSecrets()
        {
            if (secrets != null)
            {
                return secrets;
            }

            secrets = new Dictionary<string, string>();

            if (File.Exists(SecretsFilePath) == false)
            {
                return secrets;
            }

            try
            {
                var file = JsonUtility.FromJson<SecretsFile>(File.ReadAllText(SecretsFilePath));

                if (file?.secrets != null)
                {
                    foreach (var entry in file.secrets)
                    {
                        if (string.IsNullOrWhiteSpace(entry.key))
                        {
                            continue;
                        }

                        secrets[entry.key] = entry.value ?? string.Empty;
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.LogError($"[SecretsStore] Failed to read {SecretsFilePath}, treating it as empty.");
                Logger.LogException(exception);
            }

            return secrets;
        }

        // JsonUtility can't serialize a Dictionary directly, so the file is a flat list of key/value entries.
        [Serializable]
        private class SecretsFile
        {
            public List<SecretEntry> secrets = new List<SecretEntry>();
        }

        [Serializable]
        private class SecretEntry
        {
            public string key;
            public string value;
        }
    }
}
