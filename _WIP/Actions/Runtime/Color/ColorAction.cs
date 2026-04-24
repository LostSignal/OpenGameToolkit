//-----------------------------------------------------------------------
// <copyright file="ColorAction.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.SSS
{
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public abstract class ColorAction<TTarget> : ActionT<TTarget, Color>
        where TTarget : class
    {
#pragma warning disable 0649
        [SerializeField] private Color endValue;
        [SerializeField] private AnimationCurve curve;
#pragma warning restore 0649

        public override void Initialize()
        {
            base.Initialize();
            this.curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override Color GetDesiredValue(float progress)
        {
            return Color.Lerp(this.InitialValue, this.endValue, this.curve.Evaluate(progress));
        }
    }
}
