//-----------------------------------------------------------------------
// <copyright file="TriggerList.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public sealed class TriggerList : ProcessList<TriggerItem>
    {
        private ActorManager actorManager;
        private Vector3 playerPosition;

        public TriggerList(string name, int capacity, ActorManager actorManager) : base(name, capacity)
        {
            this.actorManager = actorManager;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void OnBeforeProcess()
        {
            base.OnBeforeProcess();

            this.playerPosition = this.actorManager.MainPlayerPosition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Process(ref TriggerItem item)
        {
            if (item.IsDynamic)
            {
                item.WorldToLocal = item.Transform.worldToLocalMatrix;
            }

            bool isInside = item.Area.IsInside(item.WorldToLocal, this.playerPosition);

            if (item.IsInitialized == false)
            {
                item.IsInitialized = true;
                item.HasEntered = isInside;
                item.Trigger.UpdateState(isInside);
            }
            else if (item.HasEntered != isInside)
            {
                item.HasEntered = isInside;
                item.Trigger.UpdateState(isInside);
            }
        }
    }
}

#endif
