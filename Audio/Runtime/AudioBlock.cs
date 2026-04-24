//-----------------------------------------------------------------------
// <copyright file="AudioBlock.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// TODO [bgish]: Add some editor button for playing audio blocks at runtime to help test this all out
// TOOD [bgish]: Update AudioManger to have a simple "SaveAudioSettigns" and "ApplyAudioSettings" and stop using LostPlayerPrefs (OR MAKE A NEW DeviceSettingsManager?)

namespace OGT
{
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(menuName = "OGT/Audio/Audio Block")]
    public class AudioBlock : ScriptableObject, IValidate
    {
        private static readonly OGTLogger Logger = OGTLogger.Audio;
        private static AudioManager audioManagerInstance = null;

#pragma warning disable 0649
        [SerializeField] private LocalizedString closeCaptioning;
        [SerializeField] private AudioChannel audioChannel;
        [SerializeField] private AudioClip[] audioClips;
        [SerializeField] private float minPitch = 1.0f;
        [SerializeField] private float maxPitch = 1.0f;
        [SerializeField] private float minVolume = 1.0f;
        [SerializeField] private float maxVolume = 1.0f;
        [SerializeField] private float cooldownTime = 0.0f;
        [SerializeField] private PlayType playType;
#pragma warning restore 0649

        private List<AudioBlockInstance> audioBlockInstances = new();
        private float lastPlayedTime = -1.0f;
        private int roundRobinIndex;

        // TODO [bgish]: Add RandomWithMemory which is random, but wont repeat till all have played
        public enum PlayType
        {
            Random,
            RoundRobin,
        }

        [InspectorButton]
        public void PlayOneShot() => this.InternalPlay(default, default, false, false);

        public void PlayOneShot(Vector3 position) => this.InternalPlay(default, position, true, false);

        public void PlayOneShot(Transform transform) => this.InternalPlay(transform, default, true, false);

        public void PlayOneShot(Vector3 position, float pitchPercentageOverride, float volumePercentageOverride) =>
            this.InternalPlay(default, position, true, false, pitchPercentageOverride, volumePercentageOverride);

        public void PlayOneShot(Transform transform, float pitchPercentageOverride, float volumePercentageOverride) =>
            this.InternalPlay(transform, default, true, false, pitchPercentageOverride, volumePercentageOverride);

        [InspectorButton]
        public AudioBlockInstance PlayLooping() => this.InternalPlay(default, default, false, true);

        [InspectorButton]
        public void StopAllInstances()
        {
            for (int i = this.audioBlockInstances.Count - 1; i >= 0; i--)
            {
                if (this.audioBlockInstances[i])
                {
                    this.audioBlockInstances[i].Stop();
                }
            }

            this.audioBlockInstances.Clear();
        }

        private AudioClip GetAudioClip()
        {
            if (this.audioClips == null || this.audioClips.Length == 0)
            {
                Logger.LogError($"AudioBlock {this.name} has no AudioClip assigned.", this);
                return null;
            }
            else if (this.audioClips.Length == 1)
            {
                return this.audioClips[0];
            }
            else
            {
                if (this.playType == PlayType.Random)
                {
                    int randomIndex = Random.Range(0, this.audioClips.Length);
                    return this.audioClips[randomIndex];
                }
                else if (this.playType == PlayType.RoundRobin)
                {
                    AudioClip audioClip = this.audioClips[this.roundRobinIndex++];
                    this.roundRobinIndex %= this.audioClips.Length;
                    return audioClip;
                }
                else
                {
                    Logger.LogError($"AudioBlock {this.name} enountered unkonwn PlayType {this.playType}", this);
                    return null;
                }
            }
        }

        private float GetPitch(float pitchPercentageOverride)
        {
            return pitchPercentageOverride < 0.0f ?
                Random.Range(this.minPitch, this.maxPitch) :
                Mathf.Lerp(this.minPitch, this.maxPitch, Mathf.Clamp01(pitchPercentageOverride));
        }

        private float GetVolume(float volumePercentageOverride)
        {
            return volumePercentageOverride < 0.0f ?
                Random.Range(this.minVolume, this.maxVolume) :
                Mathf.Lerp(this.minVolume, this.maxVolume, Mathf.Clamp01(volumePercentageOverride));
        }

        private AudioBlockInstance InternalPlay(Transform parent, Vector3 position, bool isPositionalAudio, bool isLooping, float pitchPercentageOverride = -1, float volumePercentageOverride = -1)
        {
            var audioManager = GetAudioManager();

            if (audioManager == null)
            {
                Logger.LogError($"Tried to play AudioBlock {this.name} before AudioManager was initialized.", this);
                return null;
            }

            if (this.audioChannel == null)
            {
                Logger.LogError($"AudioBlock {this.name} failed to play.  It does not have a valid AudioChannel.", this);
                return null;
            }

            if (audioManager.ContainsAudioChannel(this.audioChannel) == false)
            {
                Logger.LogError($"AudioBlock {this.name} failed to play.  Audio Channel {this.audioChannel.name} is not registered with the Audio Manager.", this);
                return null;
            }

            // Early out if we can't play or we're muted
            if (this.CanPlay() == false || this.audioChannel.IsMuted || this.audioChannel.Volume == 0.0f)
            {
                return null;
            }

            AudioBlockInstance audioBlockInstance = audioManager.GetAudioBlockInstance(parent, position, isPositionalAudio);
            audioBlockInstance.SetAudioBlock(this);

            AudioSource audioSource = audioBlockInstance.AudioSource;
            audioSource.spatialBlend = isPositionalAudio ? 1.0f : 0.0f;
            audioSource.clip = this.GetAudioClip();
            audioSource.pitch = this.GetPitch(pitchPercentageOverride);
            audioSource.volume = this.GetVolume(volumePercentageOverride) * this.audioChannel.Volume;
            audioSource.loop = isLooping;
            audioSource.Play();

            if (isLooping == false)
            {
                CoroutineRunner.Instance.ExecuteDelayed(audioSource.clip.length, () =>
                {
                    audioBlockInstance.Stop();
                });
            }

            return audioBlockInstance;
        }

        public void AddAudioBlockInstance(AudioBlockInstance instance)
        {
            this.audioBlockInstances.Add(instance);
        }

        public void RemoveAudioBlockInstance(AudioBlockInstance instance)
        {
            this.audioBlockInstances.Remove(instance);
        }

        private bool CanPlay()
        {
            float currentTime = Time.time;
            if (currentTime - this.lastPlayedTime >= this.cooldownTime)
            {
                this.lastPlayedTime = currentTime;
                return true;
            }

            return false;
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            if (this.closeCaptioning?.IsValid == true)
            {
                this.closeCaptioning.Validate(report, isSceneObject);
            }
        }

        // NOTE [bgish]: This is a very hacky way to get it. There is a world where their
        //               could be multiple bootloaders and this will fail, but for now
        //               we'll continue on using this.
        private AudioManager GetAudioManager()
        {
            audioManagerInstance ??= GameObject.FindFirstObjectByType<Bootloader>()?.FindManager<AudioManager>();
            return audioManagerInstance;
        }

#if UNITY_EDITOR
        private readonly static List<AudioBlock> activeAudioBlocks = new();

        private void OnEnable()
        {
            activeAudioBlocks.Add(this);
        }

        private void OnDisable()
        {
            activeAudioBlocks.Remove(this);
        }

        [EditorEvents.OnExitPlayMode]
        private static void ResetAudioBlocks()
        {
            audioManagerInstance = null;

            foreach (AudioBlock audioBlock in activeAudioBlocks)
            {
                if (audioBlock)
                {
                    audioBlock.audioBlockInstances.Clear();
                    audioBlock.lastPlayedTime = -1.0f;
                    audioBlock.roundRobinIndex = 0;
                }
            }
        }
#endif
    }
}
