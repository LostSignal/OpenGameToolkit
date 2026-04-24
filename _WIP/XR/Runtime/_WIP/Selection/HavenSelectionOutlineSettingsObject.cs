#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenSelectionOutlineSettingsObject.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Runtime.CompilerServices;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Lost/Haven/HXR Selection Outline Settings")]
    public class HavenSelectionOutlineSettingsObject : ScriptableObject
    {
#pragma warning disable 0649
        [ChildrenOnly]
        [Indent(1)]
        [SerializeField]
        private HavenSelectionOutlineSettings settings;
#pragma warning restore 0649

        public HavenSelectionOutlineSettings Settings
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.settings;
        }
    }
}
