//-----------------------------------------------------------------------
// <copyright file="AppVersions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2235:Mark all non-serializable fields", Justification = "Using Unity Serialization")]
    [Serializable]
    public class AppVersionsData
    {
        [SerializeField] private List<AppVersion> appVersions = new ();

        public List<AppVersion> AppVersions
        {
            get { return this.appVersions; }
            set { this.appVersions = value; }
        }
    }
}
