//-----------------------------------------------------------------------
// <copyright file="ProjectSettingsEditorTools.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using UnityEngine;

    public class ProjectSettingsEditorTools : ProjectSettingsBase<ProjectSettingsEditorTools>
    {
        private static readonly OGTLogger Logger = OGTLogger.OGTEditor;

        public enum SourceControlType
        {
            Plastic,
            Perforce,
            Git,
        }

#pragma warning disable 0649

        // Custom Asset Importers
        [SerializeField] private bool useUnrealNamingCollisionImporter;
        [SerializeField] private bool useApplyFolderPresetsImporter;
        [SerializeField] private bool automaticallyFixLineEndingsMismatch;

        // Warped Imagination Hierarchy Icons
        [SerializeField] private bool useWarpedImaginationNextLevelHierarchy;

        // Override Template Files
        [SerializeField] private bool overrideTemplateFiles;
        [SerializeField] private TextAsset templateMonoBehaviour;
        [SerializeField] private TextAsset templatePlayableAsset;
        [SerializeField] private TextAsset templatePlayableBehaviour;
        [SerializeField] private TextAsset templateStateMachineBehaviour;
        [SerializeField] private TextAsset templateSubStateMachineBehaviour;
        [SerializeField] private TextAsset templateEditorTestScript;

        // Generate Source Control Files
        [SerializeField] private SourceControlType sourceControlType;
        [SerializeField] private TextAsset ignoreTemplateGit;
        [SerializeField] private TextAsset ignoreTemplatePlastic;
        [SerializeField] private TextAsset ignoreTemplateP4;

        // Perforce Source Control
        [SerializeField] private string p4IgnoreFileName;
        [SerializeField] private bool autosetP4IgnoreEnvironmentVariable;

        // PlasticSCM Source Control
        [SerializeField] private bool plasticAutoSetFileCasingError;
        [SerializeField] private bool plasticAutoSetYamlMergeToolPath;

        // Generate .editorconfig
        [SerializeField] private bool useEditorConfig;
        [SerializeField] private string editorConfigFileName;
        [SerializeField] private TextAsset editorConfigTemplate;
#pragma warning restore 0649

        public override string AssetName => nameof(ProjectSettingsEditorTools);

        // Custom Importers
        public bool AutomaticallyFixLineEndingMismatches => this.automaticallyFixLineEndingsMismatch;
        public bool UseUnrealNamingCollisionImporter => this.useUnrealNamingCollisionImporter;
        public bool UseApplyFolderPresetsImporter => this.useApplyFolderPresetsImporter;

        // Warped Imagination Hierarchy Icons
        public bool UseWarpedImaginationNextLevelHierarchy => this.useWarpedImaginationNextLevelHierarchy;

        // Overriding Template Files
        public bool OverrideTemplateFiles => this.overrideTemplateFiles;
        public TextAsset TemplateMonoBehaviour => this.templateMonoBehaviour;
        public TextAsset TemplatePlayableAsset => this.templatePlayableAsset;
        public TextAsset TemplatePlayableBehaviour => this.templatePlayableBehaviour;
        public TextAsset TemplateStateMachineBehaviour => this.templateStateMachineBehaviour;
        public TextAsset TemplateSubStateMachineBehaviour => this.templateSubStateMachineBehaviour;
        public TextAsset TemplateEditorTestScript => this.templateEditorTestScript;

        // Source Control
        public SourceControlType SourceControl => this.sourceControlType;
        public TextAsset IgnoreTemplateGit => ignoreTemplateGit;
        public TextAsset IgnoreTemplatePlastic => ignoreTemplatePlastic;
        public TextAsset IgnoreTemplateP4 => ignoreTemplateP4;

        // Perforce Source Control
        public string P4IgnoreFileName => p4IgnoreFileName;
        public bool AutosetP4IgnoreEnvironmentVariable => autosetP4IgnoreEnvironmentVariable;

        // PlasticSCM Source Control
        public bool PlasticAutoSetFileCasingError => plasticAutoSetFileCasingError;
        public bool PlasticAutoSetYamlMergeToolPath => plasticAutoSetYamlMergeToolPath;

        // .editorCongif
        public bool UseEditorConfig => this.useEditorConfig;
        public string EditorConfigFileName => this.editorConfigFileName;
        public TextAsset EditorConfigTempate => this.editorConfigTemplate;

        public override void LoadDefaults()
        {
            // ---- Custom Asset Importers ----
            this.useUnrealNamingCollisionImporter = true;
            this.useApplyFolderPresetsImporter = true;
            this.automaticallyFixLineEndingsMismatch = true;

            // ---- Warped Imagination Hierarchy Icons ----
            this.useWarpedImaginationNextLevelHierarchy = true;

            // ---- Override Template Files ----
            this.overrideTemplateFiles = true;
            this.templateMonoBehaviour = EditorUtil.GetAssetByGuid<TextAsset>("5ec2f7fdcef1e6f45b2c1a7510be3eaa");
            this.templatePlayableAsset = EditorUtil.GetAssetByGuid<TextAsset>("e4d5fd6d65c83d24da92fbd00d7f5499");
            this.templatePlayableBehaviour = EditorUtil.GetAssetByGuid<TextAsset>("6ccc7dcc8373b7f4197de5cd7d7e7a16");
            this.templateStateMachineBehaviour = EditorUtil.GetAssetByGuid<TextAsset>("fed9948eb87d1be48ae323bd48cf729f");
            this.templateSubStateMachineBehaviour = EditorUtil.GetAssetByGuid<TextAsset>("09afd0c31b0565e4a8a74ecb68ceef24");
            this.templateEditorTestScript = EditorUtil.GetAssetByGuid<TextAsset>("c31e8a34fb6708144809d22dffdc73f6");

            // ---- Source Control ----

            // Git Settings
            this.ignoreTemplateGit = EditorUtil.GetAssetByGuid<TextAsset>("fae63426d3cf11c4cb39244488e2ec17");

            // Perforce Settings
            this.ignoreTemplateP4 = EditorUtil.GetAssetByGuid<TextAsset>("6d6c8d3e6aeaff34d89c7f2be0a80a0d");
            this.p4IgnoreFileName = ".p4ignore";
            this.autosetP4IgnoreEnvironmentVariable = true;

            // PlasticSCM Settings
            this.ignoreTemplatePlastic = EditorUtil.GetAssetByGuid<TextAsset>("aafcbe005eaa6754b921e846efb9043d");
            this.plasticAutoSetFileCasingError = true;
            this.plasticAutoSetYamlMergeToolPath = true;

            // ---- Editorconfig ----
            this.useEditorConfig = true;
            this.editorConfigFileName = ".editorconfig";
            this.editorConfigTemplate = EditorUtil.GetAssetByGuid<TextAsset>("f6c774b1ff43524428c88bc6afaca2d7");
        }
    }
}
