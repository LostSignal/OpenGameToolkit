#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="Actor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if true //UNITY

namespace Lost
{
    using System.Runtime.CompilerServices;
    using OGT;
    using UnityEngine;

    public class Actor : MonoBehaviour
    {
        public static readonly ObjectTracker<Actor> AllActors = new ObjectTracker<Actor>(50);

#pragma warning disable 0649
        [Tooltip("Represents the affiliation (or team) of the actor. Actors of the same affiliation are friendly to eachother")]
        [SerializeField] private int affiliation;

        [Tooltip("Represents point where other actors will aim when they attack this actor")]
        [SerializeField] private Transform aimPoint;
#pragma warning restore 0649

        public int Affiliation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.affiliation;
        }

        public Transform AimPoint
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.aimPoint;
        }

        private void OnEnable()
        {
            AllActors.Add(this);
        }

        private void OnDisable()
        {
            AllActors.Remove(this);
        }
    }
}

#endif
