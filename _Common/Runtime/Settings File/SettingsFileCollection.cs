//-----------------------------------------------------------------------
// <copyright file="SettingsFileCollection.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    [Serializable]
    public class SettingsFileCollection
    {
        [Newtonsoft.Json.JsonIgnore]
        public IEnumerable<ISettingsFile> Files => this.files;

        [Newtonsoft.Json.JsonProperty]
        [UnityEngine.SerializeField]
        private List<File> files;

        [Newtonsoft.Json.JsonProperty]
        [UnityEngine.SerializeField]
        private string defaultSettingsFileId;

        public ISettingsFile GetDefaultSettingFile()
        {
            return this.files?.FirstOrDefault(x => x.Id == this.defaultSettingsFileId);
        }

        public void SetDefaultSettingFile(ISettingsFile file)
        {
            this.defaultSettingsFileId = file.Id;
        }

        public bool IsDefaultSettingsFile(ISettingsFile file)
        {
            return this.defaultSettingsFileId == file.Id;
        }

        public ISettingsFile AddSettingsFile(string name) => this.AddSettingsFile(name, null, false);

        public ISettingsFile AddSettingsFile(string name, ISettingsFile parent) => this.AddSettingsFile(name, parent, false);

        public ISettingsFile AddSettingsFile(string name, bool isDefault) => this.AddSettingsFile(name, null, isDefault);

        public ISettingsFile AddSettingsFile(string name, ISettingsFile parent, bool isDefault)
        {
            this.files ??= new List<File>();

            var newSettingFile = new File
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                ParentId = parent?.Id,
            };

            this.files.Add(newSettingFile);

            if (isDefault)
            {
                this.defaultSettingsFileId = newSettingFile.Id;
            }

            return newSettingFile;
        }

        public string GetFullName(ISettingsFile file)
        {
            if (file == null)
            {
                return null;
            }

            var parent = GetSettingFileById(file.ParentId);
            var isDefaultText = IsDefaultSettingsFile(file) ? " (Default)" : string.Empty;

            return parent == null ? file.Name : GetFullName(parent) + "/" + file.Name + isDefaultText;
        }

        public int GetDepth(ISettingsFile file)
        {
            if (file == null)
            {
                return 0;
            }

            var parent = GetSettingFileById(file.ParentId);
            return parent == null ? 0 : 1 + GetDepth(parent);
        }

        public ISettingsFile GetSettingFileById(string id)
        {
            // NOTE [bgish]: Maybe one day do this lookup as a dictionary, but for now this should do just fine
            return this.files?.FirstOrDefault(x => x.Id == id);
        }

        public string GetCombinedSettingFileContent(ISettingsFile file)
        {
            var contentFiles = new List<string>();

            GetCombinedSettingFileContent(file, contentFiles);

            string combinedIniFile = IniSerializer.Combine(contentFiles);

            return combinedIniFile;

            void GetCombinedSettingFileContent(ISettingsFile currentFile, List<string> contentFiles)
            {
                var parent = GetSettingFileById(currentFile.ParentId);

                if (parent != null)
                {
                    GetCombinedSettingFileContent(parent, contentFiles);
                }


                contentFiles.Add(currentFile.Content);
            }
        }

        public string SerializeToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);


        public void GetAvailableParentNames(ISettingsFile settingsFile, List<string> parentNames)
        {
            parentNames.Clear();

            if (this.files == null)
            {
                return;
            }

            var settingsFileFullName = GetFullName(settingsFile) + "/";

            foreach (var file in this.files)
            {
                // Skip if it's ourself, or our parent
                if (file == settingsFile || settingsFile.ParentId == file.Id)
                {
                    continue;
                }

                var fileFullName = GetFullName(file);

                // Skip if it's a child of ours (no cycles!)
                if (fileFullName.StartsWith(settingsFileFullName))
                {
                    continue;
                }

                parentNames.Add(fileFullName);
            }

            bool IsChild(ISettingsFile possibleChild)
            {
                if (possibleChild == null)
                {
                    return false;
                }

                if (possibleChild == settingsFile)
                {
                    return true;
                }

                var parent = GetSettingFileById(possibleChild.ParentId);
                return IsChild(parent);
            }
        }

        public void Clear() => this.files?.Clear();

        [Serializable]
        private class File : ISettingsFile
        {
            [field: UnityEngine.SerializeField]
            public string ParentId { get; set; }

            [field: UnityEngine.SerializeField]
            public string Id { get; set; }

            [field: UnityEngine.SerializeField]
            public string Name { get; set; }

            [field: UnityEngine.SerializeField]
            public bool IsSelectable { get; set; }

            [field: UnityEngine.SerializeField]
            public string Content { get; set; }
        }
    }
}
