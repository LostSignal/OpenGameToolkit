//-----------------------------------------------------------------------
// <copyright file="PackageMapping.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using UnityEngine;

    [Serializable]
    public class PackageMapping
    {
        #pragma warning disable 0649
        [SerializeField] private string packageIdentifier;
        [SerializeField] private string gitUrl;
        [SerializeField] private string localPath;
        #pragma warning restore 0649

        public string PackageIdentifier
        {
            get => this.packageIdentifier;
            set => this.packageIdentifier = value;
        }

        public string GitUrl
        {
            get => this.gitUrl;
            set => this.gitUrl = value;
        }

        public string LocalPath
        {
            get => this.localPath;

            set
            {
                if (value != null)
                {
                    this.localPath = value.Replace("\\", "/");
                }
                else
                {
                    this.localPath = value;
                }
            }
        }
    }
}
