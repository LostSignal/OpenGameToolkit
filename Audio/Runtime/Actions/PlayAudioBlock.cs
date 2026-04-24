//-----------------------------------------------------------------------
// <copyright file="PlayAudioBlock.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.SSS
{
    using OGT;
    using UnityEngine;

    public class PlayAudioBlock : Action
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

#pragma warning disable 0649
        [SerializeField] private AudioBlock audioBlock;
        [SerializeField] private Transform audioBlockTransform;
#pragma warning restore 0649

        public override string Category => "Audio";

        public override string DisplayName => "Play Audio Block";

        private bool hasPlayed;

        public override void StateStarted()
        {
            this.hasPlayed = false;
        }

        protected override void UpdateProgress(float progress)
        {
            if (this.hasPlayed == false && progress > 0.0f)
            {
                this.hasPlayed = true;

                if (this.audioBlock == null)
                {
                    Logger.LogError("PlayAudioBlock Action has no AudioBlock!");
                    return;
                }

                if (this.audioBlockTransform != null)
                {
                    this.audioBlock.PlayOneShot(this.audioBlockTransform);
                }
                else
                {
                    this.audioBlock.PlayOneShot();
                }
            }
        }
    }
}
