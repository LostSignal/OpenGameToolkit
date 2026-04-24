//-----------------------------------------------------------------------
// <copyright file="HavenSelectionOutlineSettings.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    [Serializable]
    public class HavenSelectionOutlineSettings
    {
#pragma warning disable 0649
        [SerializeField] private string materialPropertyName;
#pragma warning restore 0649

        private int materialPropertyId;
        private bool isInitialized;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetMaterialPropertyId() => this.isInitialized ? this.materialPropertyId : this.Initialize();

        private int Initialize()
        {
            this.isInitialized = true;
            this.materialPropertyId = Shader.PropertyToID(this.materialPropertyName);
            return this.materialPropertyId;
        }
    }
}
