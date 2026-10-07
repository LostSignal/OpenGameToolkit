//-----------------------------------------------------------------------
// <copyright file="PropertyGroupManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using OGT.Networking;
using UnityEngine;

namespace OGT.Properties
{
    public class PropertyGroupManager : Manager
    {
        private const string PlayerPrefsKey = "PropertyGroupDeviceData";

        private static readonly OGTLogger Logger = new OGTLogger("Properties");
        private static readonly NetworkReader reader = new();
        private static readonly NetworkWriter writer = new();

        [SerializeField] private List<PropertyGroup> propertyGroups = new();
        [SerializeField] private bool loadDeviceSettingsOnStartup = true;
        [SerializeField] private bool saveDeviceSettingsOnExit = true;

        protected override Task InitializeManager(Bootloader bootloader)
        {
            if (this.loadDeviceSettingsOnStartup)
            {
                var deviceSettingsString = PlayerPrefs.GetString(PlayerPrefsKey, string.Empty);

                if (string.IsNullOrWhiteSpace(deviceSettingsString) == false)
                {
                    Logger.Log("PropertyGroupManager: Loading device settings from PlayerPrefs: " + deviceSettingsString);   
                    this.DeserializeDeviceSettingsFromString(deviceSettingsString);
                }
            }

            return Task.CompletedTask;
        }

        public override void OnManagerDestroyed()
        {
            base.OnManagerDestroyed();

            if (this.saveDeviceSettingsOnExit)
            {
                var deviceSettingsString = this.GetDeviceSettingsAsString();
                Logger.Log("PropertyGroupManager: Saving device settings to PlayerPrefs: " + deviceSettingsString);
                PlayerPrefs.SetString(PlayerPrefsKey, deviceSettingsString);
            }
        }

        private string GetDeviceSettingsAsString()
        {
            writer.SeekZero();

            int deviceTypeCount = this.propertyGroups.Count(x => x.GroupType == PropertyGroupType.Device);
            writer.Write(deviceTypeCount);

            foreach (var propertyGroup in this.propertyGroups)
            {
                if (propertyGroup.GroupType == PropertyGroupType.Device)
                {
                    writer.Write(propertyGroup.GroupId);
                    propertyGroup.Serialize(writer);
                }
            }

            return writer.ToBase64String();
        }

        private void DeserializeDeviceSettingsFromString(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
            {
                Debug.LogError($"PropertyGroupManager: Base64 string is null or empty.");
                return;
            }

            reader.FromBase64String(base64String);
            reader.SeekZero();

            int deviceTypeCount = reader.ReadInt32();

            for (int i = 0; i < deviceTypeCount; i++)
            {
                int groupId = reader.ReadInt32();
                var propertyGroup = GetPropertyGroup(groupId);

                if (propertyGroup != null)
                {
                    propertyGroup.Deserialize(reader);
                }
                else
                {
                    Debug.LogError($"Property Group with id {groupId} not found.");

                    var nonDefaultValuesToRead = reader.ReadInt32();
                    var bytesToRead = reader.ReadInt32();

                    // Skip the bytes for this property group since we don't have it in our list.
                    for (int j = 0; j < bytesToRead; j++)
                    {
                        reader.ReadByte();
                    }
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
