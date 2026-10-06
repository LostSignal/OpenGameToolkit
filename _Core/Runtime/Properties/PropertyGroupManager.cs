//-----------------------------------------------------------------------
// <copyright file="PropertyGroupManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace OGT.Properties
{
    public class PropertyGroupManager : Manager
    {
        [SerializeField] private List<PropertyGroup> propertyGroups = new();
        [SerializeField] private bool loadDeviceSettingsOnStartup = true;
        [SerializeField] private bool saveDeviceSettingsOnExit = true;

        protected override Task InitializeManager(Bootloader bootloader)
        {
            if (this.loadDeviceSettingsOnStartup)
            {
                // foreach (var propertyGroup in this.propertyGroups)
                // {
                //     propertyGroup.LoadDeviceSettings();
                // }
            }

            return Task.CompletedTask;
        }

        public override void OnManagerDestroyed()
        {
            base.OnManagerDestroyed();

            if (this.saveDeviceSettingsOnExit)
            {
                // foreach (var propertyGroup in this.propertyGroups)
                // {
                //     propertyGroup.SaveDeviceSettings();
                // }
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
