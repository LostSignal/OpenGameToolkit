//-----------------------------------------------------------------------
// <copyright file="PlantGeneratorBranch.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT.PlantGenerator
{
    using UnityEngine;
    using UnityEngine.Serialization;

    /// <summary>
    /// This class represents a branch of the plant generator.  You setup a mesh with an aim target and this
    /// class will make sure the mesh points towards that aim target.  In order for everything to work
    /// properly, the branch should be facing down the forward vector (0, 0, 1).
    /// </summary>
    public class PlantGeneratorBranch : GameBehavior, IValidate
    {
#pragma warning disable 0649
        [Tooltip("How fast the MeshRoot should lerp towards the mesh aim target.")]
        [FormerlySerializedAs("LerpSpeed")]
        [SerializeField] private float lerpSpeed = 5;

        [Tooltip("The transform that the MeshRoot should always point at.")]
        [FormerlySerializedAs("AimTarget")]
        [SerializeField] private Transform aimTarget;

        [Tooltip("The Mesh's parent that will be rotated to make sure this mesh always points to the MeshAimTarget.")]
        [FormerlySerializedAs("Mesh")]
        [SerializeField] private Transform mesh;

        [ReadOnly]
        [Tooltip("This is auto populated in Editor by grabbing the aimTarget's Rigidbody.")]
        [SerializeField] private Rigidbody aimTargetRigidbody;
#pragma warning restore 0649

        /// <summary>
        /// Updates the MeshRoot rotation point towards the mesh aim target.
        /// </summary>
        public void UpdateBranch(float deltaTime)
        {
            if (this.aimTargetRigidbody.IsSleeping() == false)
            {
                this.mesh.rotation = Quaternion.Lerp(this.mesh.rotation, this.aimTarget.rotation, this.lerpSpeed * deltaTime);
            }
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            if (this.aimTargetRigidbody == null && this.aimTarget != null)
            {
                this.aimTargetRigidbody = this.aimTarget.GetComponent<Rigidbody>();
                EditorUtil.SetDirty(this);
            }
        }
    }
}

#endif
