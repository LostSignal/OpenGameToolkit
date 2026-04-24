//-----------------------------------------------------------------------
// <copyright file="LocalPositionAction.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.SSS
{
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public sealed class LocalPositionAction : Vector3Action<Transform>
    {
        public override string Category => "Transform";

        public override string DisplayName => "Local Position";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override Vector3 GetCurrentValue() => this.Target.localPosition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetValue(Vector3 newValue) => this.Target.localPosition = newValue;
    }
}
