//-----------------------------------------------------------------------
// <copyright file="FloatAction.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.SSS
{
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public abstract class FloatAction<TTarget> : ActionT<TTarget, float>
        where TTarget : class
    {
#pragma warning disable 0649
        [SerializeField] private float endValue;
        [SerializeField] private AnimationCurve curve;
#pragma warning restore 0649

        public override void Initialize()
        {
            base.Initialize();
            this.curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override float GetDesiredValue(float progress)
        {
            return Mathf.Lerp(this.InitialValue, this.endValue, this.curve.Evaluate(progress));
        }
    }
}
