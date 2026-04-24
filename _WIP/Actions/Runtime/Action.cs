//-----------------------------------------------------------------------
// <copyright file="GameObjectStateAction.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.SSS
{
    using System;
    using System.Runtime.CompilerServices;
    using OGT;
    using UnityEngine;

    [Serializable]
    public abstract class Action
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

#pragma warning disable 0649
        [SerializeField] private string description;
        [SerializeField] private float delayBeforeStart;
        [SerializeField] private float duration;
#pragma warning restore 0649

        public string Description => this.description;

        public abstract string Category { get; }

        public abstract string DisplayName { get; }

        public float DelayBeforeStart
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.delayBeforeStart;
        }

        public virtual float Duration
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.duration;
        }

        public virtual float TotalTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.delayBeforeStart + this.duration;
        }

        public virtual bool OnValidate()
        {
            return false;
        }

        public virtual void Initialize()
        {
        }

        public abstract void StateStarted();

        public virtual bool Update(float currentTime)
        {
            // Checking if we're done
            if (currentTime >= this.TotalTime)
            {
                this.UpdateProgress(1.0f);
                return true;
            }

            // Checking if we haven't started yet
            if (currentTime < this.delayBeforeStart)
            {
                this.UpdateProgress(0.0f);
                return false;
            }

            // NOTE [bgish]: I'm pretty sure this will never by hit, but putting it here just in case
            if (this.duration == 0.0f)
            {
                Logger.LogWarning("Action has duration 0.0f!");
                this.UpdateProgress(1.0f);
                return true;
            }

            // We're in progress
            this.UpdateProgress((currentTime - this.delayBeforeStart) / this.duration);
            return false;
        }

        protected abstract void UpdateProgress(float progress);
    }
}
