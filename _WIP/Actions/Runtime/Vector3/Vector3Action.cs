//-----------------------------------------------------------------------
// <copyright file="Vector3Action.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.SSS
{
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public abstract class Vector3Action<TTarget> : ActionT<TTarget, Vector3>
        where TTarget : class
    {
#pragma warning disable 0649
        [SerializeField] private Vector3 endValue;
        [SerializeField] private AnimationCurve curve;
#pragma warning restore 0649

        protected Vector3 EndValue
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.endValue;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.endValue = value;
        }

        public override void Initialize()
        {
            base.Initialize();
            this.curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override Vector3 GetDesiredValue(float progress)
        {
            return Vector3.Lerp(this.InitialValue, this.endValue, this.curve.Evaluate(progress));
        }
    }
}
