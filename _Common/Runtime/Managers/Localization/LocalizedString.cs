//-----------------------------------------------------------------------
// <copyright file="LocalizedString.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2235:Mark all non-serializable fields", Justification = "Using Unity Serialization")]
    [Serializable]
    public class LocalizedString : IValidate
    {
#pragma warning disable 0649
        [SerializeField] private string localizedStringId;
#pragma warning restore 0649

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => string.IsNullOrEmpty(this.localizedStringId) == false;
        }

        public string Id
        {
            get { return this.localizedStringId; }
            set { this.localizedStringId = value; }
        }

        public string Value
        {
            // TODO [bgish]: Actually query localizatin system to get this info
            get { return this.Id; } // throw new NotImplementedException(); }
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            // TODO [bgish]: Actually verify that localizedStringId is not null and exists in a localization table
        }
    }
}
