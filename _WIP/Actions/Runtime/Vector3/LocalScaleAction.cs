//-----------------------------------------------------------------------
// <copyright file="LocalScaleAction.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.SSS
{
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public sealed class LocalScaleAction : Vector3Action<Transform>
    {
        public override string Category => "Transform";

        public override string DisplayName => "Local Scale";

        public override void Initialize()
        {
            base.Initialize();
            this.EndValue = Vector3.one;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override Vector3 GetCurrentValue() => this.Target.localScale;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetValue(Vector3 newValue) => this.Target.localScale = newValue;
    }
}
