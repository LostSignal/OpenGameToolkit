//-----------------------------------------------------------------------
// <copyright file="GuidFixerSettings.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using UnityEngine;

    [Serializable]
    public class GuidFixerSettings
    {
        #pragma warning disable 0649
        [Multiline]
        [SerializeField]
        [Tooltip("CSV of 'GUID, ASSET PATH' that will force those assets to have the supplied guid.")]
        private string badGuidsCsv;
        #pragma warning restore 0649

        public string BadGuidsCsv => this.badGuidsCsv;
    }
}
