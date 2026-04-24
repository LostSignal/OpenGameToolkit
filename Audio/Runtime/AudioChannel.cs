//-----------------------------------------------------------------------
// <copyright file="AudioChannel.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Runtime.CompilerServices;
    using UnityEngine;

    [CreateAssetMenu(menuName = "OGT/Audio/Audio Channel")]
    public class AudioChannel : ScriptableObject
    {
        #pragma warning disable 0649
        [SerializeField] private string volumeKey;
        [SerializeField] private string isMutedKey;

        [Header("Defaults")]
        [SerializeField] private float defaultVolume = 1.0f;
        [SerializeField] private bool defaultIsMuted = false;
        #pragma warning restore 0649

        private float volume;
        private bool isMuted;

        public bool IsDirty { get; private set; }

        public float Volume
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return this.volume;
            }

            set
            {
                if (this.volume != value)
                {
                    this.IsDirty = true;
                    this.volume = value;
                }
            }
        }

        public bool IsMuted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return this.isMuted;
            }

            set
            {
                if (this.isMuted != value)
                {
                    this.IsDirty = true;
                    this.isMuted = value;
                }
            }
        }

        public void Save()
        {
            if (this.IsDirty)
            {
                this.IsDirty = false;
                LostPlayerPrefs.SetInt(this.volumeKey, (int)(this.volume * 100));
                LostPlayerPrefs.SetBool(this.isMutedKey, this.isMuted);
                LostPlayerPrefs.Save();
            }
        }

        public void Load()
        {
            this.volume = LostPlayerPrefs.GetInt(this.volumeKey, (int)(this.defaultVolume * 100)) / 100.0f;
            this.isMuted = LostPlayerPrefs.GetBool(this.isMutedKey, this.defaultIsMuted);
        }
    }
}
