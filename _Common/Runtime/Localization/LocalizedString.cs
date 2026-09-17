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
        [SerializeField] private string tableId;
        [SerializeField] private string localizedStringId;
#pragma warning restore 0649

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => string.IsNullOrEmpty(this.tableId) == false && string.IsNullOrEmpty(this.localizedStringId) == false;
        }

        public string TableId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.tableId;

#if UNITY_EDITOR
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.tableId = value;
#endif
        }

        public string Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.localizedStringId;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.localizedStringId = value;
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            //// TODO [bgish]: Actually verify that tableId and localizedStringId are not null and exist in a localization table
            ////
            //// if (string.IsNullOrEmpty(this.tableId))
            //// {
            ////     report.AddError("LocalizedString", "Table ID is not set", null);
            ////     return;
            //// }
            ////
            //// if (string.IsNullOrEmpty(this.localizedStringId))
            //// {
            ////     report.AddError("LocalizedString", "String ID is not set", null);
            ////     return;
            //// }
            ////
            //// var table = LocalizationTable.GetTableById(this.tableId);
            //// if (table == null)
            //// {
            ////     report.AddWarning("LocalizedString", $"Table '{this.tableId}' not found in registry", null);
            //// }
        }
    }
}
