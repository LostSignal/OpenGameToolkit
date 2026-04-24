//-----------------------------------------------------------------------
// <copyright file="SpriteRendererFadeAction.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.SSS
{
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public sealed class SpriteRendererFadeAction : FloatAction<SpriteRenderer>
    {
        public override string Category => "Sprite";

        public override string DisplayName => "Fade Sprite Renderer";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override float GetCurrentValue() => this.Target.color.a;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetValue(float newValue) => this.Target.color = this.Target.color.SetA(newValue);
    }
}
