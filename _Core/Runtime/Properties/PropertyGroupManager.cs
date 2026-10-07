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
                    this.DeserializeDeviceSettingsFromString(deviceSettingsString);
                    Logger.Log("PropertyGroupManager: Finished loading device settings from PlayerPrefs");
                }
            }

            return Task.CompletedTask;
        }

        public override void OnManagerDestroyed()
        {
            base.OnManagerDestroyed();

            if (this.saveDeviceSettingsOnExit)
            {
                var deviceSettingsString = this.GetDeviceSettingsAsBase64String();
                Logger.Log("PropertyGroupManager: Saving device settings to PlayerPrefs: " + deviceSettingsString);
                PlayerPrefs.SetString(PlayerPrefsKey, deviceSettingsString);
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
