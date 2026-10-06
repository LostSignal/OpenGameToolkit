//-----------------------------------------------------------------------
// <copyright file="BoolPropertySetter.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Properties
{
    using UnityEngine;

    public class BoolPropertySetter : GameBehavior
    {
        [SerializeField] private BoolProperty flag;

        public void SetTrue() => this.flag.Value = true;

        public void SetFalse() => this.flag.Value = false;

        public void Toggle() => this.flag.Value = !this.flag.Value;

        public void SetValue(bool value) => this.flag.Value = value;
    }
}
