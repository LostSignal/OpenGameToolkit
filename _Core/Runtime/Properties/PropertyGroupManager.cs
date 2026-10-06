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

        protected override Task InitializeManager(Bootloader bootloader)
        {
            return Task.CompletedTask;
        }
    }
}
