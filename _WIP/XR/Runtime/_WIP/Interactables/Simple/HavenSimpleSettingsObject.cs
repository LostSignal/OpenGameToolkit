#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenSimpleSettingsObject.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Haven
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Lost/Haven/HXR Simple Settings")]
    public class HavenSimpleSettingsObject : ScriptableObject
    {
#pragma warning disable 0649
        [ChildrenOnly]
        [Indent(1)]
        [SerializeField]
        private HavenSimpleSettings settings;
#pragma warning restore 0649

        public void Apply(HavenSimple simple)
        {
            this.settings.Apply(simple);
        }
    }
}
