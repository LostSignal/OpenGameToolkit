//-----------------------------------------------------------------------
// <copyright file="AudioManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using OGT.Properties;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;

    public sealed class AudioManager : Manager
    {
        private static readonly OGTLogger Logger = OGTLogger.Audio;

#pragma warning disable 0649
        [SerializeField] private BoolProperty isMutedProperty;
        [SerializeField] private AudioBlockInstance audioBlockInstancePrefab;
        [SerializeField] private List<AudioChannel> audioChannels;
#pragma warning restore 0649

        private CameraManager cameraManager;
        private SpawnManager spawnManager;

        public bool IsMuted => this.isMutedProperty.Value;

        public bool ContainsAudioChannel(AudioChannel channel)
        {
            // TODO [bgish]: Optimize this...
            return this.audioChannels.Contains(channel);
        }

        [InspectorButton]
        public override void ResetToDefaults()
        {
            if (this.audioBlockInstancePrefab == null)
            {
                this.audioBlockInstancePrefab = EditorUtil.GetAssetByGuid<AudioBlockInstance>("a648d1de492b94940a36f9fb7a3753b2");
            }

            if (this.audioChannels.IsNullOrEmpty())
            {
                this.audioChannels = new List<AudioChannel>
                {
                    EditorUtil.GetAssetByGuid<AudioChannel>("1eb86a6df8498cb4294ca9c61b78613c"),  // SFX
                    EditorUtil.GetAssetByGuid<AudioChannel>("e53b60e6332ac2f46b19ed87d34e5ece"),  // Music
                    EditorUtil.GetAssetByGuid<AudioChannel>("a3bdd2b082429ef4a9d05c11ee4a2b45"),  // Voice Over
                };
            }
        }

        protected override async Task InitializeManager(Bootloader bootloader)
        {
            this.cameraManager = bootloader.FindManager<CameraManager>();
            this.spawnManager = bootloader.FindManager<SpawnManager>();

            var propertyGroupManager = bootloader.FindManager<PropertyGroupManager>();

            await propertyGroupManager.WaitForInitialization();

            this.isMutedProperty.OnChange += (oldValue, newValue) =>
            {
                //// TODO [bgish]: Go through the whole pool of items and mute them individually. Make sure that we save off their volume so that if we unmute it we can restore it. 
            };
        }

        public AudioBlockInstance GetAudioBlockInstance(Transform parent, Vector3 position, bool isPositionalAudio)
        {
            var audioBlockInstance = this.spawnManager.Spawn<AudioBlockInstance>(this.audioBlockInstancePrefab.Spawnable);

            if (isPositionalAudio)
            {
                if (parent != null)
                {
                    audioBlockInstance.SetTransform(parent);
                }
                else
                {
                    audioBlockInstance.SetPosition(position);
                }
            }
            else
            {
                audioBlockInstance.SetTransform(this.cameraManager.CameraState.Transform);
            }

            return audioBlockInstance;
        }
    }
}
