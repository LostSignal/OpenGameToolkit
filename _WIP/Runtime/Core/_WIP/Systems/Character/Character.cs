//-----------------------------------------------------------------------
// <copyright file="Character.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public class Character : MonoBehaviour
    {
        private static readonly List<Character> Characters = new List<Character>(50);
        private static readonly ReadOnlyCollection<Character> CharactersReadOnly = new ReadOnlyCollection<Character>(Characters);

#pragma warning disable 0649
        [SerializeField] private bool isMainCharacter;
        [SerializeField] private int teamId;
#pragma warning restore 0649

        private Transform characterTransform;

        static Character() => UnityPlatformProvider.OnReset += Characters.Clear;

        public static ReadOnlyCollection<Character> AllCharacters
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CharactersReadOnly;
        }

        public Transform Transform
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.characterTransform;
        }

        public bool IsMainCharacter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.isMainCharacter;
        }

        public int TeamId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.teamId;
        }

        private void Awake()
        {
            this.characterTransform = this.transform;
        }

        private void OnEnable()
        {
            Characters.Add(this);
        }

        private void OnDisable()
        {
            Characters.Remove(this);
        }
    }
}

#endif
