//-----------------------------------------------------------------------
// <copyright file="TriggerManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

using System.Threading.Tasks;
using UnityEngine;

namespace OGT
{
    public sealed class TriggerManager : Manager, IUpdate
    {
#pragma warning disable 0649
        [SerializeField] private int initialCapacity;
#pragma warning restore 0649

        private TriggerList triggers;

        public int UpdateOrder => 2;

        protected override Task InitializeManager(Bootloader bootloader)
        {
            if (this.triggers == null)
            {
                this.triggers = new TriggerList("Triggers", this.initialCapacity, bootloader.FindManager<ActorManager>());
            }

            return Task.CompletedTask;
        }

        public override void ResetToDefaults()
        {
            this.initialCapacity = 100;
        }

        public void OnUpdate(float deltaTime)
        {
            this.triggers.RunAll();
        }

        public void AddTrigger(Trigger trigger)
        {
            var triggerTransform = trigger.TriggerTransform;

            this.triggers.Add(
                trigger.GetEntityId(),
                new TriggerItem
                {
                    Area = trigger.Area,
                    HasEntered = false,
                    IsInitialized = false,
                    IsDynamic = trigger.IsDynamic,
                    Transform = triggerTransform,
                    Trigger = trigger,
                    WorldToLocal = triggerTransform.worldToLocalMatrix,
                },
                trigger);
        }

        public void RemoveTrigger(Trigger trigger)
        {
            this.triggers.Remove(trigger.GetEntityId());
        }
    }
}

#endif
