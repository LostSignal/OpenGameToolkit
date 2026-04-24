//-----------------------------------------------------------------------
// <copyright file="Blackboard.cs" company="DefaultCompany">
//     Copyright (c) DefaultCompany. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class Blackboard : GameBehavior, IHasHidableComponents
    {
#if UNITY_EDITOR
        [SerializeField] private List<Component> componentsToHide;
        [SerializeField] private List<Member> membersToExpose;

        public IEnumerable<Type> GetHidableComponents()
        {
            if (this.componentsToHide == null)
            {
                yield break;
            }

            foreach (var component in this.componentsToHide)
            {
                yield return component.GetType();
            }
        }

        public List<Component> ComponentsToHide => this.componentsToHide;

        public List<Member> MembersToExpose => this.membersToExpose;

        [field: NonSerialized]
        public bool AreComponentsHidden { get; set; }

        [Serializable]
        public class Member
        {
            public Component component;
            public string fieldName;
            public MemberType type;
        }

        public enum MemberType
        {
            Member,
            Property,
            SortingLayer,
        }
#endif
    }
}
