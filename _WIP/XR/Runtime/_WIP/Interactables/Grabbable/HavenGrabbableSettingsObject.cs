#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenGrabbableSettingsObject.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Haven
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Lost/Haven/HXR Grabbable Settings")]
    public class HavenGrabbableSettingsObject : ScriptableObject
    {
#pragma warning disable 0649
        [ChildrenOnly]
        [Indent(1)]
        [SerializeField]
        private HavenGrabbableSettings settings;
#pragma warning restore 0649

        public void Apply(HavenGrabbable grabable)
        {
            this.settings.Apply(grabable);
        }
    }
}
