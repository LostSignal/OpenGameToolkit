//-----------------------------------------------------------------------
// <copyright file="ProjectSettingsBase.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    [Serializable]
    public abstract class ProjectSettingsBase<T> : ScriptableObject, IProjectSettings
        where T : ScriptableObject, IProjectSettings
    {
        private static readonly OGTLogger Logger = OGTLogger.OGTEditor;
        private static T instance;

        static ProjectSettingsBase()
        {
            EditorApplication.delayCall += () =>
            {
                Instance.Initialize();
            };
        }

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = ScriptableObject.CreateInstance<T>();

                    var settingsFilePath = GetProjectSettingsPath(instance.AssetName);
                    var errorLoading = false;

                    if (File.Exists(settingsFilePath))
                    {
                        try
                        {
                            var fileData = File.ReadAllText(settingsFilePath);
                            EditorJsonUtility.FromJsonOverwrite(fileData, instance);
                        }
                        catch (Exception exception)
                        {
                            // Quash the exception and take the default settings.
                            Logger.LogException(exception);
                            errorLoading = true;
                        }
                    }

                    if (File.Exists(settingsFilePath) == false || instance == null || errorLoading)
                    {
                        if (instance == null)
                        {
                            instance = ScriptableObject.CreateInstance<T>();
                        }

                        instance.LoadDefaults();
                        instance.Save();
                    }
                }

                return instance;
            }
        }

        public abstract string AssetName { get; }

        public abstract void LoadDefaults();

        public virtual void Initialize()
        {
        }

        public void Save()
        {
            if (Instance == null)
            {
                Logger.Log($"Can't save {this.AssetName}, no instance can be found!");
                return;
            }

            var settingsFilePath = GetProjectSettingsPath(this.AssetName);

            if (string.IsNullOrEmpty(settingsFilePath) == false)
            {
                string directoryName = Path.GetDirectoryName(settingsFilePath);

                if (Directory.Exists(directoryName) == false)
                {
                    Directory.CreateDirectory(directoryName);
                }

                File.WriteAllText(settingsFilePath, EditorJsonUtility.ToJson(instance, true));
            }
        }

        private static string GetProjectSettingsPath(string assetName)
        {
            assetName = assetName.Replace(".asset", string.Empty);
            return $"ProjectSettings/{assetName}.asset";
        }
    }
}
