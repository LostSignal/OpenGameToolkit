#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenClimbableSettingsObject.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Haven
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Lost/Haven/HXR Climbable Settings")]
    public class HavenClimbableSettingsObject : ScriptableObject
    {
#pragma warning disable 0649
        [ChildrenOnly]
        [Indent(1)]
        [SerializeField]
        private HavenClimbableSettings settings;
#pragma warning restore 0649

        public void Apply(HavenClimbable climb)
        {
            this.settings.Apply(climb);
        }
    }
}
