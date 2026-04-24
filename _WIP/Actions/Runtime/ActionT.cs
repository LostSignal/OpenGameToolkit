//-----------------------------------------------------------------------
// <copyright file="ActionT.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.SSS
{
    using OGT;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public abstract class ActionT<TTarget, TValue> : Action
        where TTarget : class
    {
        private static readonly EqualityComparer<TValue> Comparer = EqualityComparer<TValue>.Default;
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        #pragma warning disable 0649
        [UnityEngine.SerializeField] private TTarget target;
        #pragma warning restore 0649

        private TValue initialValue;
        private TValue currentValue;
        private bool isTargetValid;

        protected TTarget Target
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.target;
        }

        protected TValue InitialValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.initialValue;
        }

        public override void StateStarted()
        {
            this.isTargetValid = this.target != null;

            if (this.isTargetValid == false)
            {
                Logger.LogWarning($"SSS Action \"{this.Description}\" has a null target!");
                return;
            }

            this.initialValue = this.currentValue = this.GetCurrentValue();
        }

        protected override void UpdateProgress(float currentTime)
        {
            if (this.isTargetValid == false)
            {
                return;
            }

            var desiredValue = currentTime > 0.0f ? this.GetDesiredValue(currentTime) : this.initialValue;

            if (Comparer.Equals(this.currentValue, desiredValue) == false)
            {
                this.currentValue = desiredValue;
                this.SetValue(this.currentValue);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract TValue GetCurrentValue();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract TValue GetDesiredValue(float progress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void SetValue(TValue newValue);
    }
}
