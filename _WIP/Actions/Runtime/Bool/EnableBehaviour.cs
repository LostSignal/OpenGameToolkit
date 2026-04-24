//-----------------------------------------------------------------------
// <copyright file="EnableBehaviour.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.SSS
{
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public sealed class EnableBehaviour : ActionT<Behaviour, bool>
    {
        public override string Category => "Behaviour";

        public override string DisplayName => "Enable Behaviour";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool GetCurrentValue() => this.Target.enabled;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool GetDesiredValue(float progress) => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetValue(bool newValue) => this.Target.enabled = newValue;
    }
}
