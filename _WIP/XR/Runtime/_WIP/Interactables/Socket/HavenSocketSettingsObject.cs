//-----------------------------------------------------------------------
// <copyright file="HavenSocketSettingsObject.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Haven
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "Lost/Haven/HXR Socket Settings")]
    public class HavenSocketSettingsObject : ScriptableObject
    {
#pragma warning disable 0649
        [ChildrenOnly]
        [Indent(1)]
        [SerializeField]
        private HavenSocketSettings settings;
#pragma warning restore 0649

        public void Apply(HavenSocket socket)
        {
            this.settings.Apply(socket);
        }
    }
}
