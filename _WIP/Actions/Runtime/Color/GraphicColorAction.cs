//-----------------------------------------------------------------------
// <copyright file="GraphicColorAction.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.SSS
{
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.UI;

    public sealed class GraphicColorAction : ColorAction<Graphic>
    {
        public override string Category => "Graphic";

        public override string DisplayName => "Color Graphic";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override Color GetCurrentValue() => this.Target.color;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetValue(Color newValue) => this.Target.color = newValue;
    }
}
