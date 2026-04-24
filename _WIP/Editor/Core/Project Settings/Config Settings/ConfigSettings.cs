//-----------------------------------------------------------------------
// <copyright file="ConfigSettings.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Settings
{
    using System.Collections.Generic;
    using UnityEngine;

    public class ConfigSettings : ProjectSettingsBase<ConfigSettings>
    {
        private const string SettingFilePath = "./Library/SettingFileConfigId.txt";

#pragma warning disable 0649
        [SerializeField] private SettingsFileCollection settingsFiles;
#pragma warning restore 0649

        public override string AssetName => nameof(ConfigSettings);

        public SettingsFileCollection SettingsFiles
        {
            get
            {
                if (this.settingsFiles == null)
                {
                    this.LoadDefaults();
                }

                return this.settingsFiles;
            }
        }

        public static void SetActiveConfigById(string guid)
        {
            System.IO.File.WriteAllText(SettingFilePath, guid);
            ConfigSettings.Instance.Refresh();
        }

        public List<object> GetActiveSettingsFileObjects()
        {
            var activeSettingsFile = this.GetActiveSettingsFile();
            var iniFile = this.SettingsFiles.GetCombinedSettingFileContent(activeSettingsFile);
            return IniSerializer.DeserializeIni(iniFile);
        }

        private ISettingsFile GetActiveSettingsFile()
        {
            var activeConfigId = System.IO.File.Exists(SettingFilePath) ? System.IO.File.ReadAllText(SettingFilePath) : null;

            if (activeConfigId == null)
            {
                var defaultSettingsFile = this.SettingsFiles.GetDefaultSettingFile();
                activeConfigId = defaultSettingsFile?.Id;
                SetActiveConfigById(activeConfigId);
            }

            var activeConfig = this.SettingsFiles.GetSettingFileById(activeConfigId);

            if (activeConfig == null)
            {
                if (this.SettingsFiles.GetDefaultSettingFile() == null)
                {
                    this.LoadDefaults();
                }

                activeConfig = this.SettingsFiles.GetDefaultSettingFile();

                if (activeConfig != null)
                {
                    SetActiveConfigById(activeConfig.Id);
                }
            }

            return activeConfig;
        }

        public void Refresh()
        {
            // TODO [bgish]: Generate File Menu Options
            ConfigSettingsExecuter.ApplySettings();
        }

        public override void LoadDefaults()
        {
            this.settingsFiles ??= new SettingsFileCollection();
            this.settingsFiles.Clear();

            var root = this.settingsFiles.AddSettingsFile("Root");
            root.IsSelectable = false;
            root.Content = "";

            var dev = this.settingsFiles.AddSettingsFile("Dev", root, true);
            dev.IsSelectable = true;
            dev.Content = "";

            var live = this.settingsFiles.AddSettingsFile("Live", root);
            live.IsSelectable = true;
            live.Content = "";

            EditorUtil.SetDirty(this);
            this.Save();
        }

        // [EditorEvents.OnDomainReload]
        // private static void OnDomainReload()
        // {
        //     if (Instance.UsingBuildConfigs == false)
        //     {
        //         return;
        //     }
        // 
        //     // Recording defines before we possibly alter them
        //     List<string> definesBefore = new();
        //     BuildTargetGroupUtil.GetValid().ForEach(x => definesBefore.Add(PlayerSettings.GetScriptingDefineSymbols(x)));
        // 
        //     EditorBuildConfigDefinesHelper.UpdateProjectDefines();
        // 
        //     // Recording defines after we've possibly altered them
        //     List<string> definesAfter = new();
        //     BuildTargetGroupUtil.GetValid().ForEach(x => definesAfter.Add(PlayerSettings.GetScriptingDefineSymbols(x)));
        // 
        //     // checking to see if the scripting defines have changed
        //     bool forceRecompile = definesBefore.Count != definesAfter.Count;
        // 
        //     if (forceRecompile == false)
        //     {
        //         for (int i = 0; i < definesBefore.Count; i++)
        //         {
        //             if (definesBefore[i] != definesAfter[i])
        //             {
        //                 forceRecompile = true;
        //                 break;
        //             }
        //         }
        //     }
        // 
        //     if (forceRecompile)
        //     {
        //         // TODO [bgish]: Is this neccessary, if so implement
        //     }
        // 
        //     WriteRuntimeConfigFile();
        // 
        //     // TODO [bgish]: Write out the MenuItems class? (force recompile if new)
        // }

        // [EditorEvents.OnEnterPlayMode]
        // private static void OnEnterPlayMode()
        // {
        //     if (Instance.UsingBuildConfigs == false)
        //     {
        //         return;
        //     }
        // 
        //     WriteRuntimeConfigFile();
        // }

        // private static void WriteRuntimeConfigFile()
        // {
        //     if (Instance.UsingBuildConfigs == false)
        //     {
        //         return;
        //     }
        // 
        //     RuntimeBuildConfig.Reset();
        // 
        //     BuildConfig activeConfig = Instance.ActiveBuildConfig;
        // 
        //     if (activeConfig == null)
        //     {
        //         return;
        //     }
        // 
        //     // Collecting all the runtime config values
        //     var runtimeConfigValues = new Dictionary<string, string>();
        // 
        //     foreach (var settings in GetActiveConfigSettings())
        //     {
        //         settings.GetRuntimeConfigSettings(activeConfig, runtimeConfigValues);
        //     }
        // 
        //     // Generating the runtime config object and serializing to json
        //     var runtimeConfig = new RuntimeBuildConfig(activeConfig.Id, activeConfig.SafeName, runtimeConfigValues);
        //     string configJson = JsonUtility.ToJson(runtimeConfig, true);
        // 
        //     // Early out if the file file hasn't chenged
        //     if (File.Exists(RuntimeBuildConfig.FilePath) && File.ReadAllText(RuntimeBuildConfig.FilePath) == configJson)
        //     {
        //         return;
        //     }
        // 
        //     Directory.CreateDirectory(Path.GetDirectoryName(RuntimeBuildConfig.FilePath));
        //     File.WriteAllText(RuntimeBuildConfig.FilePath, configJson);
        //     AssetDatabase.ImportAsset(RuntimeBuildConfig.FilePath);
        //     AssetDatabase.Refresh();
        // }
        // 
        // private static IEnumerable<BuildConfigSettings> GetActiveConfigSettings()
        // {
        //     var activeConfig = Instance.ActiveBuildConfig;
        // 
        //     if (activeConfig == null)
        //     {
        //         yield break;
        //     }
        // 
        //     foreach (var type in TypeUtil.GetAllTypesOf<BuildConfigSettings>())
        //     {
        //         var settings = activeConfig.GetSettings(type);
        // 
        //         if (settings != null)
        //         {
        //             yield return settings;
        //         }
        //     }
        // }
    }
}
