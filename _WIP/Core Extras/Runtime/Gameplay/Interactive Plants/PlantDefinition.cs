//-----------------------------------------------------------------------
// <copyright file="PlantDefinition.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT.PlantGenerator
{
    using System;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.Serialization;

    /// <summary>
    /// A game object that takes a bunch of parameters like materials and branch prefabs and generates
    /// a nice looking random plants.
    /// </summary>
    [CreateAssetMenu(menuName = "Lost/Plant Definition")]
    public class PlantDefinition : ScriptableObject
    {
        /// <summary>
        /// The name of the Layer the plants use for their physics.
        /// </summary>
        public static readonly string LayerName = "InteractivePlants";

#pragma warning disable 0649
        [Tooltip("All the branch parameters associated with this plant generator.")]
        [FormerlySerializedAs("GroupParameters")]
        [SerializeField]
        private BranchGroupParameters[] groupParameters = new BranchGroupParameters[1] { new BranchGroupParameters() };
#pragma warning restore 0649

        /// <summary>
        /// Gets all the branch parameters associated with this plant generator.
        /// </summary>
        /// <value>The branch group parameters.</value>
        public BranchGroupParameters[] GroupParameters
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.groupParameters;
        }

        /// <summary>
        /// Returns the min and max branch count the generated plant can have.
        /// </summary>
        /// <param name="minBranchCount">Min branches possible.</param>
        /// <param name="maxBranchCount">Max branches possible.</param>
        public void GetMinMaxBranchCount(out int minBranchCount, out int maxBranchCount)
        {
            minBranchCount = 0;
            maxBranchCount = 0;

            if (this.GroupParameters != null)
            {
                for (int i = 0; i < this.GroupParameters.Length; i++)
                {
                    minBranchCount += this.GroupParameters[i].MinCount;
                    maxBranchCount += this.GroupParameters[i].MaxCount;
                }
            }
        }

        /// <summary>
        /// Parameters associated with creating a group of branches.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2235:Mark all non-serializable fields", Justification = "Using Unity Serialization")]
        [Serializable]
        public class BranchGroupParameters
        {
#pragma warning disable 0649
            [Tooltip("Name of the group.")]
            [FormerlySerializedAs("Name")]
            [SerializeField]
            private string name;

            [Tooltip("Collection of all the different branch prefabs to spawn from.")]
            [FormerlySerializedAs("Variations")]
            [SerializeField]
            private GameObject[] variations = Array.Empty<GameObject>();

            [Tooltip("The minimum number of branches to spawn in this group.")]
            [FormerlySerializedAs("MinCount")]
            [SerializeField]
            private int minCount = 1;

            [Tooltip("The maximum number of branches to spawn in this group.")]
            [FormerlySerializedAs("MaxCount")]
            [SerializeField]
            private int maxCount = 5;

            [Tooltip("Branches are evenly rotated, but this adds randomness +/- this value to the rotation.")]
            [FormerlySerializedAs("RandomRotationOffset")]
            [SerializeField]
            private float randomRotationOffset = 10;

            [Tooltip("The desired minimum space between each of the branches in this group.")]
            [FormerlySerializedAs("RotationalWidth")]
            [SerializeField]
            private int rotationalWidth = 10;

            [Tooltip("The materials that a branch can be randomly assigned.  All renderers of the branch prefab will be set to the random material.")]
            [FormerlySerializedAs("Materials")]
            [SerializeField]
            private Material[] materials;

            [Tooltip("Varies the vertical height of the branch by +/- this offset.")]
            [FormerlySerializedAs("VerticalOffset")]
            [SerializeField]
            private float verticalOffset = 0f;
#pragma warning restore 0649

            /// <summary>
            /// Gets or sets the name of the group.
            /// </summary>
            /// <value>The name of the group.</value>
            public string Name
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.name;
                set => this.name = value;
            }

            /// <summary>
            /// Gets the collection of all the different branch prefabs to spawn from.
            /// </summary>
            /// <value>The branch prefab variations.</value>
            public GameObject[] Variations
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.variations;
            }

            /// <summary>
            /// Gets or sets the minimum number of branches to spawn in this group.
            /// </summary>
            /// <value>The minimum number of branches to spawn.</value>
            public int MinCount
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.minCount;
                set => this.minCount = value;
            }

            /// <summary>
            /// Gets or sets the maximum number of branches to spawn in this group.
            /// </summary>
            /// <value>The maximum number of branches to spawn.</value>
            public int MaxCount
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.maxCount;
                set => this.maxCount = value;
            }

            /// <summary>
            /// Gets the random rotation variance to add/subtract to the roation.
            /// </summary>
            /// <value>The random rotation variance.</value>
            public float RandomRotationOffset
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.randomRotationOffset;
            }

            /// <summary>
            /// Gets the desired minimum space between each of the branches in this group.
            /// </summary>
            /// <value>The desired minimum space between branches.</value>
            public int RotationalWidth
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.rotationalWidth;
            }

            /// <summary>
            /// Gets the materials that a branch can be randomly assigned.  All renderers of the
            /// branch prefab will be set to the random material.
            /// </summary>
            /// <value>The materials to assign to branches.</value>
            public Material[] Materials
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.materials;
            }

            /// <summary>
            /// Gets the vertical height of the branch by +/- this offset.
            /// </summary>
            /// <value>The vertical height offset.</value>
            public float VerticalOffset
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.verticalOffset;
            }
        }
    }
}

#endif
