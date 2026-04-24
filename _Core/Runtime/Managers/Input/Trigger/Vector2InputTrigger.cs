//-----------------------------------------------------------------------
// <copyright file="Vector2InputTrigger.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEngine;

    public class Vector2InputTrigger
    {
        public bool IsFired { get; private set; }

        public Vector2 Vector { get; private set; }

        public void Fire(Vector2 vector)
        {
            this.IsFired = true;
            this.Vector = vector;
        }

        public void Reset()
        {
            this.IsFired = false;
            this.Vector = Vector2.zero;
        }
    }
}
