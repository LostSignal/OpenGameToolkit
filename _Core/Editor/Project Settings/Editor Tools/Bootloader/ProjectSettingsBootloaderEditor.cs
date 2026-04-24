//-----------------------------------------------------------------------
// <copyright file="ProjectSettingsBootloaderEditor.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(ProjectSettingsBootloader))]
    public class ProjectSettingsBootloaderEditor : OGT.Editor
    {
        protected override void OnEnable()
        {
            base.OnEnable();

            try
            {
                this.GetProperty("bootloaders").isExpanded = true;
            }
            catch
            {
            }

            // Making sure all Names are updated to the correct Path
            var settings = this.target as ProjectSettingsBootloader;

            if (settings == null || settings.Bootloaders.IsNullOrEmpty())
            {
                return;
            }

            foreach (var bootloaderRef in settings.Bootloaders)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(bootloaderRef.Bootloader.AssetGUID);

                if (bootloaderRef.AssetPath != assetPath)
                {
                    bootloaderRef.AssetPath = assetPath;
                }
            }
        }

        protected override void NewOnInspectorGUI()
        {
            var settings = this.target as ProjectSettingsBootloader;

            this.DrawMember("bootloaders");

            if (GUILayout.Button("Find Bootloaders"))
            {
                foreach (var bootloader in AssetDatabaseUtil.GetAllPrefabsOfType<Bootloader>("Searching for Bootloaders..."))
                {
                    var guid = bootloader.EditorGetGuid();

                    if (settings.Bootloaders.Any(x => x.Bootloader.AssetGUID == guid))
                    {
                        continue;
                    }

                    string name = bootloader.gameObject.name
                        .Replace("Bootloader", string.Empty)
                        .Replace("bootloader", string.Empty)
                        .Trim();

                    settings.Bootloaders.Add(new ProjectSettingsBootloader.BootloaderReference
                    {
                        AssetPath = bootloader.EditorGetAssetPath(),
                        MenuItemName = string.IsNullOrEmpty(name) ? bootloader.name : name,
                        Bootloader = new UnityEngine.AddressableAssets.AssetReferenceT<Bootloader>(guid),
                    });
                }
            }

            if (GUILayout.Button("Generate Menu Items"))
            {
                OGT.GenerateMenuItems.Generate();
            }
        }

        protected override void OnGUIChanged()
        {
            base.OnGUIChanged();

            ProjectSettingsBootloader.Instance.Save();
        }

        [SettingsProvider]
        private static SettingsProvider CreateLostLibrarySettingsProvider() =>
            SettingsProviderUtil.GetSettingsProvider(ProjectSettingsBootloader.Instance, "Project/Open Game Toolkit/Bootloader");

        [GenerateMenuItems]
        public static void GenerateMenuItems(List<MenuItemData> menuItems)
        {
            var settings = ProjectSettingsBootloader.Instance;
            var menuItemPathBase = "Tools/OGT/Bootloader/";

            AddBootloader(menuItems, "Disable", menuItemPathBase + "Disable", -100, string.Empty);

            int priority = 0;
            foreach (var bootloaderRefence in settings.Bootloaders.Where(x => x.GenerateMenuItem))
            {
                AddBootloader(
                    menuItems,
                    bootloaderRefence.MenuItemName,
                    menuItemPathBase + bootloaderRefence.MenuItemName,
                    priority++,
                    bootloaderRefence.Bootloader.AssetGUID);
            }

            static void AddBootloader(List<MenuItemData> menuItems, string menuItemName, string path, int priority, string guid)
            {
                menuItems.Add(new MenuItemData
                {
                    MethodName = $"{menuItemName}Bootloader".Replace(" ", string.Empty),
                    MenuPath = path,
                    Priority = priority,
                    MethodBody = $"RuntimeSettings.SetSetting<string>(\"OGT.Bootloader\", \"{guid}\");",
                    ValidateBody = $"UnityEditor.Menu.SetChecked(\"{path}\", RuntimeSettings.GetSetting<string>(\"OGT.Bootloader\") == \"{guid}\");\nreturn true;",
                });
            }
        }
    }
}
