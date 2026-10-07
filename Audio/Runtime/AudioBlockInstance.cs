//-----------------------------------------------------------------------
// <copyright file="AudioBlockInstance.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections;
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
        
        public System.Action OnStoppeed;

        private AudioManager audioManager;
        private AudioChannel audioChannel;
        private AudioBlock audioBlock;
        private SpawnManager spawnManager;
        private float instanceVolume;

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

        public void SetAudioBlockData(AudioManager audioManager, AudioChannel audioChannel, AudioBlock audioBlock, float instanceVolume)
        {
            this.audioBlock = audioBlock;
            this.audioBlock.AddAudioBlockInstance(this);

            this.audioManager = audioManager;
            this.audioChannel = audioChannel;
            this.SetInstanceVolume(instanceVolume);
        }

        public float GetInstanceVolume() => this.instanceVolume;

        public void SetInstanceVolume(float instanceVolume)
        {
            this.instanceVolume = instanceVolume;
            AudioSource.volume = this.instanceVolume * this.audioChannel.Volume * (this.audioManager.IsMuted ? 0 : 1);
        }

        public void UpdateVolume() => this.SetInstanceVolume(this.instanceVolume);

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

        public Coroutine FadeInVolume(float fadeInTime)
        {
            return CoroutineRunner.Instance.StartCoroutine(Coroutine());

            IEnumerator Coroutine()
            { 
                float elapsedTime = 0.0f;
                float volume = this.GetInstanceVolume();

                while (elapsedTime < fadeInTime)
                {
                    this.SetInstanceVolume(Mathf.Lerp(0.0f, volume, elapsedTime / fadeInTime));
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                this.SetInstanceVolume(volume);
            }
        }

        public Coroutine FadeOutAndStop(float fadeOutTime)
        {
            return CoroutineRunner.Instance.StartCoroutine(Coroutine());

            IEnumerator Coroutine()
            {
                float elapsedTime = 0.0f;
                float volume = this.GetInstanceVolume();

                while (elapsedTime < fadeOutTime)
                {
                    this.SetInstanceVolume(Mathf.Lerp(volume, 0.0f, elapsedTime / fadeOutTime));
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                this.SetInstanceVolume(0.0f);
                this.Stop();
            }
        }

        public void Stop()
        {
            this.spawnManager.Despawn(this.spawnable);
            this.OnStoppeed?.Invoke();
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
