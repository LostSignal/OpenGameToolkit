//-----------------------------------------------------------------------
// <copyright file="AudioBlockInstance.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.Animations;

    [RequireComponent(typeof(Spawnable))]
    public class AudioBlockInstance : GameBehavior, IAwake, ISpawn, IValidate
    {
        private static readonly List<ConstraintSource> empty = new();
        private static readonly OGTLogger Logger = OGTLogger.Audio;

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private PositionConstraint positionConstraint;
        [SerializeField] private Spawnable spawnable;
        private AudioBlock audioBlock;

        private SpawnManager spawnManager;

        public AudioSource AudioSource
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.audioSource;
        }

        public PositionConstraint PositionConstraint
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.positionConstraint;
        }

        public Spawnable Spawnable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.spawnable;
        }

        public void OnAwake(Bootloader bootloader)
        {
            this.spawnManager = bootloader.FindManager<SpawnManager>();
        }

        public void SetAudioBlock(AudioBlock audioBlock)
        {
            this.audioBlock = audioBlock;
            this.audioBlock.AddAudioBlockInstance(this);
        }

        public void UpdatePitch(float newPitch)
        {
            this.audioSource.pitch = newPitch;
        }

        public void SetPosition(Vector3 position)
        {
            this.positionConstraint.enabled = false;
            this.positionConstraint.SetSources(null);
            this.transform.position = position;
        }

        public void SetTransform(Transform transform)
        {
            var newConstraint = new ConstraintSource { sourceTransform = transform, weight = 1.0f };

            if (this.positionConstraint.sourceCount == 0)
            {
                this.positionConstraint.AddSource(newConstraint);
            }
            else if (this.positionConstraint.sourceCount == 1)
            {
                this.positionConstraint.SetSource(0, newConstraint);
            }
            else
            {
                Logger.LogError($"AudioBlockInstance {this.name} has too many Position Constraints!");
            }

            this.positionConstraint.enabled = true;
        }

        public void Stop()
        {
            this.spawnManager.Despawn(this.spawnable);
        }

        public void OnSpawn()
        {
            this.audioSource.enabled = true;
        }

        public void OnDespawn()
        {
            this.audioSource.Stop();
            this.audioSource.clip = null;
            this.audioSource.enabled = false;
            this.positionConstraint.enabled = false;
            this.positionConstraint.SetSources(empty);

            this.audioBlock?.RemoveAudioBlockInstance(this);
            this.audioBlock = null;
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNull(this, this.audioSource, nameof(this.audioSource));
            report.AssertNotNull(this, this.positionConstraint, nameof(this.positionConstraint));
        }
    }
}
