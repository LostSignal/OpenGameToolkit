//-----------------------------------------------------------------------
// <copyright file="LostSettingsShaderStripper.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;

    public class ProjectSettingsShaderStripper : ProjectSettingsBase<ProjectSettingsShaderStripper>
    {
#pragma warning disable 0649
        [SerializeField] private bool enableShaderStripping;
        [SerializeField] private bool isForwardRednderer = true;
        [SerializeField] private List<DefaultAsset> approvedFolders = new();
        [SerializeField] private List<string> essentialShadersStartWith = new();
        [SerializeField] private List<string> essentialShadersEqual = new();
        [SerializeField] private List<string> varientsToSkip = new();
#pragma warning restore 0649

        public bool EnableShaderStripping => this.enableShaderStripping;

        public bool IsForwardRenderer => this.isForwardRednderer;

        public IEnumerable<string> ApprovedFolders
        {
            get => this.approvedFolders
            .Select(x => AssetDatabase.GetAssetPath(x))
            .Where(x => x != null && Directory.Exists(x));
        }

        public List<string> EssentialShadersStartWith => this.essentialShadersStartWith;

        public List<string> EssentialShadersEqual => this.essentialShadersEqual;

        public IEnumerable<ShaderKeyword> VarientsToSkip
        {
            get => this.varientsToSkip.Select(x => new ShaderKeyword(x));
        }

        public override string AssetName => nameof(ProjectSettingsShaderStripper);

        public override void LoadDefaults()
        {
            this.enableShaderStripping = false;
            this.isForwardRednderer = true;
            this.approvedFolders = new List<DefaultAsset>();

            this.essentialShadersStartWith = new List<string>()
            {
                "Hidden/",
                "Lost/",
                "Particles/",
                "TextMeshPro/",
                "Unlit/",
            };

            this.essentialShadersEqual = new List<string>()
            {
                "Legacy Shaders/Diffuse",        // Non URP
                "Legacy Shaders/VertexLit",      // Non URP
                "Skybox/Procedural",
                "Sprites/Default",
                "Sprites/Mask",
                "UI/Default",
                "Universal Render Pipeline/Lit",
            };

            this.varientsToSkip = new List<string>()
            {
                // "DIRECTIONAL_COOKIE",
                // "POINT_COOKIE",
                // "LIGHTPROBE_SH",
            };
        }
    }
}
