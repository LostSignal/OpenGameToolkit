//-----------------------------------------------------------------------
// <copyright file="EnableGameObject.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.SSS
{
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public sealed class EnableGameObject : ActionT<GameObject, bool>
    {
        public override string Category => "GameObject";

        public override string DisplayName => "Enable GameObject";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool GetCurrentValue() => this.Target.activeSelf;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool GetDesiredValue(float progress) => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetValue(bool newValue) => this.Target.SetActive(newValue);
    }
}
