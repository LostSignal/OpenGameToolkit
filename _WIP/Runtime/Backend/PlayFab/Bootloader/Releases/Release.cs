//-----------------------------------------------------------------------
// <copyright file="Release.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using System;
    using UnityEngine;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2235:Mark all non-serializable fields", Justification = "Using Unity Serialization")]
    [Serializable]
    public class Release
    {
#pragma warning disable 0649
        [SerializeField] private string appVersion = "0.1.0";
        [SerializeField] private string dataVersion = "0";
        [SerializeField] private bool forceAppUpdate;
#pragma warning restore 0649

        public string AppVersion
        {
            get => this.appVersion;
            set => this.appVersion = value;
        }

        public string DataVersion
        {
            get => this.dataVersion;
            set => this.dataVersion = value;
        }

        public bool ForceAppUpdate
        {
            get => this.forceAppUpdate;
            set => this.forceAppUpdate = value;
        }
    }
}

#endif
