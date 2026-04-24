//-----------------------------------------------------------------------
// <copyright file="ProjectSettingsBootloader.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.AddressableAssets;

    public class ProjectSettingsBootloader : ProjectSettingsBase<ProjectSettingsBootloader>
    {
        public override string AssetName => nameof(ProjectSettingsBootloader);

        [SerializeField]
        private List<BootloaderReference> bootloaders = new();

        public List<BootloaderReference> Bootloaders => this.bootloaders;

        public override void LoadDefaults()
        {
        }

        [Serializable]
        public class BootloaderReference
        {
            [ReadOnly]
            [SerializeField] private string assetPath;
            [SerializeField] private string menuItemName;
            [SerializeField] private AssetReferenceT<Bootloader> bootloader;
            [SerializeField] private bool generateMenuItem = true;

            public string AssetPath
            {
                get => this.assetPath;
                set => this.assetPath = value;
            }

            public string MenuItemName
            {
                get => this.menuItemName;
                set => this.menuItemName = value;
            }

            public AssetReferenceT<Bootloader> Bootloader
            {
                get => this.bootloader;
                set => this.bootloader = value;
            }

            public bool GenerateMenuItem
            {
                get => this.generateMenuItem;
                set => this.generateMenuItem = value;
            }
        }
    }
}
