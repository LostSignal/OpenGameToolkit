//-----------------------------------------------------------------------
// <copyright file="OGTToolbar.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Toolbars;
    using UnityEngine;

    //// NOTE [bgish]: Mirrors all of the "Tools/OGT/..." and "Assets/OGT/..." menu items in a single
    ////               main toolbar dropdown.  The menu is rebuilt every time it's opened so all checkmarks
    ////               and enabled states are always up to date.
    public static class OGTToolbar
    {
        public const string ToolbarPath = "OGT/OGT";

        private const string BootloaderSettingName = "OGT.Bootloader";
        private const string BootloaderMenu = "Bootloader/";
        private const string LocalizationMenu = "Localization/";
        private const string LoggingMenu = "Logging/";
        private const string ValidationMenu = "Validation/";
        private const string EditorEventsMenu = "Editor Events/";
        private const string ReferencesMenu = "References/";
        private const string ToolsMenu = "Tools/";
        private const string UtilityMenu = "Utility/";
        private const string FoldersMenu = "Folders/";
        private const string FastPlatformSwitchMenu = "Fast Platform Switch (Beta)/";
        private const string AssetsMenu = "Assets/";

        private static readonly LoggingLevel[] LoggingLevels = (LoggingLevel[])Enum.GetValues(typeof(LoggingLevel));

        [MainToolbarElement(ToolbarPath, defaultDockPosition = MainToolbarDockPosition.Left)]
        public static MainToolbarElement CreateOGTDropdown()
        {
            var content = new MainToolbarContent("OGT", "Open Game Toolkit");
            return new MainToolbarDropdown(content, ShowMenu);
        }

        public static void ShowMenu(Rect dropdownRect)
        {
            var menu = new GenericMenu();

            // Misc
            menu.AddItem(new GUIContent("Is Development Build"), RuntimeSettings.IsOgtDevelopmentBuild, () => RuntimeSettings.IsOgtDevelopmentBuild = !RuntimeSettings.IsOgtDevelopmentBuild);

            menu.AddSeparator(string.Empty);

            AddBootloaderSection(menu);
            AddLocalizationSection(menu);
            AddLoggingSection(menu);

            menu.AddSeparator(string.Empty);

            AddToolsSection(menu);

            menu.DropDown(dropdownRect);
        }

        // ---------------------------------------- Bootloader ----------------------------------------

        public static string ActiveBootloaderGuid => RuntimeSettings.GetSetting<string>(BootloaderSettingName) ?? string.Empty;

        public static void SetActiveBootloader(string guid) => RuntimeSettings.SetSetting<string>(BootloaderSettingName, guid ?? string.Empty);

        public static Bootloader GetActiveBootloaderPrefab()
        {
            var guid = ActiveBootloaderGuid;

            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            return string.IsNullOrEmpty(assetPath) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(assetPath)?.GetComponent<Bootloader>();
        }

        private static void AddBootloaderSection(GenericMenu menu)
        {
            var activeGuid = ActiveBootloaderGuid;

            menu.AddItem(new GUIContent(BootloaderMenu + "Disable"), string.IsNullOrEmpty(activeGuid), () => SetActiveBootloader(string.Empty));

            var bootloaders = ProjectSettingsBootloader.Instance.Bootloaders.Where(x => x.GenerateMenuItem && x.Bootloader != null).ToList();

            if (bootloaders.Count > 0)
            {
                menu.AddSeparator(BootloaderMenu);
            }

            foreach (var bootloaderReference in bootloaders)
            {
                var guid = bootloaderReference.Bootloader.AssetGUID;
                var name = string.IsNullOrWhiteSpace(bootloaderReference.MenuItemName) ? guid : bootloaderReference.MenuItemName;
                menu.AddItem(new GUIContent(BootloaderMenu + name), activeGuid == guid, () => SetActiveBootloader(guid));
            }
        }

        // ---------------------------------------- Localization ----------------------------------------

        private static void AddLocalizationSection(GenericMenu menu)
        {
            var bootloader = GetActiveBootloaderPrefab();

            if (bootloader == null)
            {
                menu.AddDisabledItem(new GUIContent(LocalizationMenu + "No Active Bootloader"));
                return;
            }

            var localizationManager = bootloader.GetComponentInChildren<LocalizationManager>(true);

            if (localizationManager == null)
            {
                menu.AddDisabledItem(new GUIContent(LocalizationMenu + $"No LocalizationManager on {bootloader.name}"));
                return;
            }

            var currentLanguage = OGT.Localization.Localization.CurrentLanguage;

            foreach (var language in localizationManager.SupportedLanguages)
            {
                var label = language.NativeLanguageName == language.IsoLanguageName
                    ? language.IsoLanguageName
                    : $"{language.IsoLanguageName} ({language.NativeLanguageName})";

                menu.AddItem(new GUIContent(LocalizationMenu + label), currentLanguage == language, () => OGT.Localization.Localization.CurrentLanguage = language);
            }
        }

        // ---------------------------------------- Logging ----------------------------------------

        private static void AddLoggingSection(GenericMenu menu)
        {
            var channels = UnityLoggingProvider.GetSavedChannels();

            if (channels.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent(LoggingMenu + "No Logging Channels In Runtime Settings"));
                return;
            }

            foreach (var channel in channels.OrderBy(x => x.Name))
            {
                var channelName = channel.Name;

                foreach (var level in LoggingLevels)
                {
                    var path = $"{LoggingMenu}{channelName} ({channel.Level})/{level}";
                    menu.AddItem(new GUIContent(path), channel.Level == level, () => UnityLoggingProvider.SetSavedChannelLevel(channelName, level));
                }
            }
        }

        // ---------------------------------------- Tools/OGT ----------------------------------------

        private static void AddToolsSection(GenericMenu menu)
        {
            // Validation
            menu.AddItem(new GUIContent(ValidationMenu + "Validate Open Scenes"), false, global::OGT.Validation.Validation.ValidateOpenScenes);
            menu.AddItem(new GUIContent(ValidationMenu + "Validate Open Prefab"), false, global::OGT.Validation.Validation.ValidateOpenPrefab);
            menu.AddItem(new GUIContent(ValidationMenu + "Validate Prefabs and ScriptableObjects"), false, global::OGT.Validation.Validation.ValidatePrefabsAndScriptableObjects);

            // Editor Events
            menu.AddItem(
                new GUIContent(EditorEventsMenu + "Disable Logging"),
                EditorEvents.EditorEventsExecutor.DisableLogging,
                () => EditorEvents.EditorEventsExecutor.DisableLogging = !EditorEvents.EditorEventsExecutor.DisableLogging);

            // References
            menu.AddItem(new GUIContent(ReferencesMenu + "Find All Outside References (Selected Directory)"), false, ReferenceFinder.FindAllOutsideReferences);
            menu.AddItem(new GUIContent(ReferencesMenu + "Find All References Outside Selected Directories"), false, ReferenceFinder.FindAllReferencesInAssets);

            // Tools
            menu.AddItem(new GUIContent(ToolsMenu + "APK Install Tool"), false, ApkInstallTool.ShowWindow);
            menu.AddItem(new GUIContent(ToolsMenu + "Export Scene NavMesh"), false, ExportNavMeshToObj.ExportNavMeshForCurrentScene);

            // Utility
            menu.AddItem(new GUIContent(UtilityMenu + "Remove Empty Directories (Selected Directory)"), false, MenuItemTools.RemoveEmptyDirectoriesFromSelectedDirectory);
            menu.AddItem(new GUIContent(UtilityMenu + "Cleanup C# Files (Selected Directory)"), false, MenuItemTools.ConvertAllCSharpFiles);
            menu.AddItem(new GUIContent(UtilityMenu + "Disable Warnings In C# Files (Selected Directory)"), false, MenuItemTools.DisableWarnings);
            menu.AddItem(new GUIContent(UtilityMenu + "Force Recompile All Code"), false, MenuItemTools.ForceRecompile);
            menu.AddItem(new GUIContent(UtilityMenu + "Get All Components In Open Prefab"), false, MenuItemTools.GetAllComponentsInOpenPrefab);
            menu.AddItem(new GUIContent(UtilityMenu + "Get All Images In Open Prefab"), false, MenuItemTools.GetAllImagesInOpenPrefab);

            // Folders
            menu.AddItem(new GUIContent(FoldersMenu + "Show Editor Logs"), false, FolderFinder.OpenEditorLogs);
            menu.AddItem(new GUIContent(FoldersMenu + "Show Player Log"), false, FolderFinder.OpenPlayerLog);
            menu.AddItem(new GUIContent(FoldersMenu + "Show Persistent Data Path"), false, FolderFinder.OpenPersistentDataPath);

            // Fast Platform Switch
            AddPlatformSwitchItem(menu, "Win", BuildTarget.StandaloneWindows);
            AddPlatformSwitchItem(menu, "Win64", BuildTarget.StandaloneWindows64);
            AddPlatformSwitchItem(menu, "OSX", BuildTarget.StandaloneOSX);
            AddPlatformSwitchItem(menu, "iOS", BuildTarget.iOS);
            AddPlatformSwitchItem(menu, "Android", BuildTarget.Android);
            AddPlatformSwitchItem(menu, "PS5", BuildTarget.PS5);
            AddPlatformSwitchItem(menu, "Xbox", BuildTarget.GameCoreXboxOne);

            menu.AddSeparator(string.Empty);

            // Edit Secrets
            menu.AddItem(new GUIContent("Edit Secrets"), false, SecretsEditorWindow.Open);
        }

        private static void AddPlatformSwitchItem(GenericMenu menu, string name, BuildTarget buildTarget)
        {
            var content = new GUIContent(FastPlatformSwitchMenu + name);

            if (EditorUserBuildSettings.activeBuildTarget == buildTarget)
            {
                menu.AddDisabledItem(content, true);
            }
            else
            {
                menu.AddItem(content, false, () => FastPlatformSwitcher.SwitchTo(buildTarget));
            }
        }
    }
}
