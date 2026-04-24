//-----------------------------------------------------------------------
// <copyright file="GraphicFadeAction.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.SSS
{
    using System.Runtime.CompilerServices;
    using UnityEngine.UI;

    public sealed class GraphicFadeAction : FloatAction<Graphic>
    {
        public override string Category => "Graphic";

        public override string DisplayName => "Fade Graphic";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override float GetCurrentValue() => this.Target.color.a;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetValue(float newValue) => this.Target.color = this.Target.color.SetA(newValue);
    }
}
