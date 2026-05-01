
```csharp
public class OGTWizard
{
    // Project Line Engdings
    // Serialization Mode
    // Parrellel Import
    // OGT Generated Output Directory
        
    public class OGTSettings
    {
        public bool UseUnrealNamingCollisionImporter;
        public bool UseApplyFolderPresetsImporter;
        public bool AutomaticallyFixLineEndings;
        public bool GenerateEditorConfigForSolution;
        public bool UseWarpedImaginationNextLevelHierarchy;
        public bool OverrideTemplateFiles;
    }
}
```

---

- Wizard System For Installing OpenUPM packages
    - ZLogger?
    - Better Attributes systems (like naughty attributes)
    - [InGameDebugConsole](https://openupm.com/packages/com.yasirkula.ingamedebugconsole/)?
    - [Remote Actions](https://github.com/sabresaurus/Remote-Actions/tree/main/Runtime)
    * [Directory Duplicator](https://openupm.com/packages/com.bg.directoryduplicator/)
    * [Build Report Inspector](https://docs.unity3d.com/Packages/com.unity.build-report-inspector@0.2/manual/index.html)
    - Proxima Free? with option to upgrade

---

# Add the Fast Script Reload package????
openupm add com.handzlikchris.fastscriptreload

# Add ZLogger?
openupm add com.cysharp.zlogger

# Naughty Attributes
openupm add com.dbrizov.naughtyattributes

---

Move the OGT Installer to a github gist.  Once a day, check the gist, if the version online does not match the version locally, prompt the user and to update, and it will download and replace the file.

Project Settings
- Set Project Line Endings
- Set Company Name
- Set Root Namespace
- Set Serialization Mode
- Force Parallel Import
- Disable Domain Reload on Play
- Disable Default on Default Collision

Editor Tools
- Automatically Fix Line Endings mismatch
- User Warped Imagination Next Level Hierachy
- Override Template Files

Asset Importers (ogt-importers.json is just a HashSet of values)
- Use Unreal Collision Naming Importer (Adds "UnrealCollisionNaming" to HashSet)
- Use Apply Folder Presets Importer (Writes "ApplyPresetsPerFolder" to HashSet)
- Generate Editor Config (Writes "GenerateEditorConfig" to HashSet)

Source Control
- Plastic
    - Auto Set File Casing Error (Are these even needed anymore?)
    - Auto Set Yaml Merge Tool Path (Are these even needed anymore?)

# Optional Packages

General
- Easy Text Effect:  com.qiaozhilei.easy-text-effects
- In-game Debug Console: com.yasirkula.ingamedebugconsole
- DirectoryDuplicator: com.bg.directoryduplicator
- Build Report Inspector: com.unity.build-report-inspector

 Unity
- com.unity.ai.navigation
- com.unity.behavior
* com.unity.animation.rigging
* com.unity.cinemachine
* com.unity.localization
* com.unity.splines
* com.unity.visualscripting

Narrative
- Yarn Spinner:  dev.yarnspinner.unity
- ink-Unity integration: com.inkle.ink-unity-integration

XR
- com.unity.xr.interaction.toolkit
* com.unity.xr.management
* com.unity.xr.oculus
* com.unity.xr.openxr
* com.unity.xr.arfoundation
* com.unity.xr.arcore
* com.unity.xr.arkit

Mobile
* com.unity.ads
* com.unity.ads.ios-support
* com.unity.device-simulator.devices
* com.unity.purchasing
* com.unity.purchasing.udp
* com.unity.mobile.android-logcat
* com.unity.mobile.notifications

---

### Optional
* Memory Analyzer / Profiler?
* GLTF
    - com.atteneder.gltfast
    - com.siccity.gltfutility
    - org.khronos.unitygltf
   * Unity
     * Adaptive Performance
     * Ads
     * Analytics Library
     * Asset Bundle Browser
     * Game Foundation
     * UI Builder
     * Unity User Reporting
     * Vector Graphics
