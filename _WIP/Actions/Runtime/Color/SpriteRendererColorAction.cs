//-----------------------------------------------------------------------
// <copyright file="SpriteRendererColorAction.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.SSS
{
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public sealed class SpriteRendererColorAction : ColorAction<SpriteRenderer>
    {
        public override string Category => "Sprite";

        public override string DisplayName => "Color Sprite Renderer";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override Color GetCurrentValue() => this.Target.color;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetValue(Color newValue) => this.Target.color = newValue;
    }
}
