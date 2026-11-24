//-----------------------------------------------------------------------
// <copyright file="PropertyGroupManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using OGT.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static OGT.Properties.PropertyGroup;

namespace OGT.Properties
{
    public class PropertyGroupManager : Manager
    {
        private const string PlayerPrefsKey = "PropertyGroupDeviceData";

        private static readonly OGTLogger Logger = new OGTLogger("Properties");
        private readonly NetworkReader reader = new();
        private readonly NetworkWriter writer = new();

        [SerializeField] private List<PropertyGroup> propertyGroups = new();
        [SerializeField] private bool loadDeviceSettingsOnStartup = true;
        [SerializeField] private bool saveDeviceSettingsOnExit = true;

        private bool hasLoadedDeviceSettings;
        private string lastSavedDeviceSettings;

        protected override Task InitializeManager(Bootloader bootloader)
        {
            // Making sure the cache is initialized and up to date
            foreach (var propertyGroup in this.propertyGroups)
            {
                propertyGroup.ForceInitializeCache();
            }

            if (this.loadDeviceSettingsOnStartup)
            {
                var deviceSettingsString = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);
                if (string.IsNullOrWhiteSpace(deviceSettingsString) == false)
                {
                    Logger.Log("PropertyGroupManager: Loading device settings from PlayerPrefs: " + deviceSettingsString);

                    try
                    {
                        this.DeserializeDeviceSettingsFromString(deviceSettingsString);
                        this.lastSavedDeviceSettings = deviceSettingsString;
                        Logger.Log("PropertyGroupManager: Finished loading device settings from PlayerPrefs");
                    }
                    catch (Exception e)
                    {
                        // Bad or out of date data must never stop the manager (and so the whole app) from starting up.
                        // Whatever was read before the failure is kept, and everything else stays on its default.
                        Logger.LogError($"PropertyGroupManager: Failed to load device settings from PlayerPrefs: {e}");
                    }
                }
            }

            // Nothing is saved before this point, so a save can never write defaults over settings that haven't been read yet
            this.hasLoadedDeviceSettings = true;

            return Task.CompletedTask;
        }

        public override void OnManagerDestroyed()
        {
            base.OnManagerDestroyed();
            this.SaveDeviceSettings();
        }

        // Writes the device settings to disk now. Call this after changing settings that must survive the app being killed.
        public void SaveDeviceSettings()
        {
            if (this.saveDeviceSettingsOnExit == false || this.hasLoadedDeviceSettings == false)
            {
                return;
            }

            var deviceSettingsString = this.GetDeviceSettingsAsBase64String();

            if (deviceSettingsString == this.lastSavedDeviceSettings)
            {
                return;
            }

            Logger.Log("PropertyGroupManager: Saving device settings to PlayerPrefs: " + deviceSettingsString);
            PlayerPrefs.SetString(PlayerPrefsKey, deviceSettingsString);

            // On Android / Quest the app is normally killed by the OS rather than quit, so PlayerPrefs never gets its
            // automatic write on exit and has to be flushed by hand
            PlayerPrefs.Save();
            this.lastSavedDeviceSettings = deviceSettingsString;
        }

        // Taking the headset off, opening the system menu or going back to Home pauses / unfocuses the app, and on Quest
        // that is usually the last thing that runs before the process is killed, so it's the reliable place to save
        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                this.SaveDeviceSettings();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus == false)
            {
                this.SaveDeviceSettings();
            }
        }

        private string GetDeviceSettingsAsBase64String()
        {
            int propertiesWritten = 0;

            // Writing the number of properties written at the beginning of the stream, we will go back and write this value after we finish writing all the properties
            writer.SeekZero();
            writer.Write(propertiesWritten);

            foreach (var propertyGroup in this.propertyGroups)
            {
                if (propertyGroup.GroupType == PropertyGroupType.Device)
                {
                    propertiesWritten += propertyGroup.Serialize(writer);
                }
            }

            // Go back and write the number of properties written at the beginning of the stream
            uint position = writer.Position;
            writer.SeekZero();
            writer.Write(propertiesWritten);
            writer.Seek(position);

            return writer.ToBase64String();
        }

        private void DeserializeDeviceSettingsFromString(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
            {
                Logger.LogError($"PropertyGroupManager: Base64 string is null or empty.");
                return;
            }

            Logger.Log($"PropertyGroupManager: FromBase64String");
            reader.FromBase64String(base64String);

            int propertyCount = reader.ReadInt32();
            PropertyGroup propertyGroup = null;

            for (int i = 0; i < propertyCount; i++)
            {
                uint groupId = reader.ReadPackedUInt32();
                uint propertyId = reader.ReadPackedUInt32();
                var propertyType = (PropertyType)reader.ReadByte();

                if (propertyGroup == null || propertyGroup.GroupId != groupId)
                {
                    propertyGroup = this.GetPropertyGroup((int)groupId);
                }

                if (propertyType == PropertyType.Bool)
                {
                    bool value = reader.ReadBoolean();
                    propertyGroup?.SetBoolPropertyValue(propertyId, value);
                }
                else if (propertyType == PropertyType.Int)
                {
                    int value = reader.ReadInt32();
                    propertyGroup?.SetIntPropertyValue(propertyId, value);
                }
                else if (propertyType == PropertyType.Float)
                {
                    float value = reader.ReadSingle();
                    propertyGroup?.SetFloatPropertyValue(propertyId, value);
                }
                else if (propertyType == PropertyType.String)
                {
                    string value = reader.ReadString();
                    propertyGroup?.SetStringPropertyValue(propertyId, value);
                }
                else if (propertyType == PropertyType.Enum)
                {
                    int value = reader.ReadInt32();
                    propertyGroup?.SetEnumPropertyValueByIndex(propertyId, value);
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported property type: {propertyType}");
                }
            }
        }

        public PropertyGroup GetPropertyGroup(int groupId)
        {
            foreach (var propertyGroup in this.propertyGroups)
            {
                if (propertyGroup.GroupId == groupId)
                {
                    return propertyGroup;
                }
            }

            Debug.LogError($"Property Group with id {groupId} not found.");
            return null;
        }
    }
}
