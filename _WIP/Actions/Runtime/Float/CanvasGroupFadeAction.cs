//-----------------------------------------------------------------------
// <copyright file="CanvasGroupFadeAction.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.SSS
{
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public sealed class CanvasGroupFadeAction : FloatAction<CanvasGroup>
    {
        public override string Category => "Canvas";

        public override string DisplayName => "Fade Canvas Group";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override float GetCurrentValue() => this.Target.alpha;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetValue(float newValue) => this.Target.alpha = newValue;
    }
}
