//-----------------------------------------------------------------------
// <copyright file="Analyzer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class Analyzer
    {
        [SerializeField] private string name;
        [SerializeField] private TextAsset ruleset;
        [SerializeField] private TextAsset config;
        [SerializeField] private List<TextAsset> dlls;
        [SerializeField] private List<string> csProjects;

        public string Name
        {
            get => this.name;
            set => this.name = value;
        }

        public List<TextAsset> DLLs
        {
            get => this.dlls;
            set => this.dlls = value;
        }

        public TextAsset Ruleset
        {
            get => this.ruleset;
            set => this.ruleset = value;
        }

        public TextAsset Config
        {
            get => this.config;
            set => this.config = value;
        }

        public List<string> CSProjects
        {
            get => this.csProjects;
            set => this.csProjects = value;
        }
    }
}
