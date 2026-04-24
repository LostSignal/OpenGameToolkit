//-----------------------------------------------------------------------
// <copyright file="DelayExecuteManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;

    public class DelayExecuteManager : Manager, IAwake, IUpdate
    {
#pragma warning disable 0649
        [SerializeField] private int initialCapacity;
#pragma warning restore 0649

        private DelayedActionList delayedActionList;
        private List<int> idsToDelete;
        private int currentId = 0;

        //// private UpdateChannelReceipt updateReceipt;
        //// private DelayedAction[] delayedActions;
        //// private int count;

        public int UpdateOrder => 1;

        public override void ResetToDefaults()
        {
            this.initialCapacity = 30;
        }

        protected override Task InitializeManager(Bootloader bootloader)
        {
            this.delayedActionList = new DelayedActionList("Delay Action List", initialCapacity);
            this.idsToDelete = new List<int>(initialCapacity);

            //// UpdateManager.OnInitialized += SetupUpdateChannel;
            ////
            //// this.delayedActions = new DelayedAction[this.initialCapacity];
            //// this.count = 0;
            //// this.SetInstance(this);
            ////
            //// void SetupUpdateChannel()
            //// {
            ////     var updateChannel = UpdateManager.Instance.GetChannel(ChannelName);
            ////
            ////     if (updateChannel == null)
            ////     {
            ////         Lost.Logger.LogError($"{nameof(DelayExecuteManager)} couldn't find Update Channel \"{ChannelName}\".  This manager will not work!", this);
            ////     }
            ////     else
            ////     {
            ////         this.updateReceipt = updateChannel.RegisterCallback(this, this);
            ////     }
            //// }

            return Task.CompletedTask;
        }

        public void OnAwake(Bootloader bootloader)
        {
            this.delayedActionList.OnDeleteId += (id) =>
            {
                this.idsToDelete.Add(id);
            };
        }

        public void Add(Action action, float seconds)
        {
            int id = this.currentId++;

            this.delayedActionList.Add(
                id,
                new DelayedActionListItem
                {
                    Id = id,
                    Action = action,
                    ExecuteTime = seconds,
                },
                null);

            //// if (this.count >= this.delayedActions.Length)
            //// {
            ////     Lost.Logger.LogWarning("DelayExecuteManager had to grow in size at runtime.  Please update initialCapacity to stop this from happening.", this);
            ////     Array.Resize(ref this.delayedActions, this.delayedActions.Length * 2);
            //// }
            ////
            //// this.delayedActions[this.count++] = new DelayedAction { ExecuteTime = Time.realtimeSinceStartup + seconds, Action = action };
        }

        void IUpdate.OnUpdate(float deltaTime)
        {
            this.delayedActionList.RunAll();

            if (this.idsToDelete.Count > 0)
            {
                for (int i = 0; i < this.idsToDelete.Count; i++)
                {
                    this.delayedActionList.Remove(this.idsToDelete[i]);
                }

                this.idsToDelete.Clear();
            }

            //// float currentTime = Time.realtimeSinceStartup;
            //// int i = 0;
            ////
            //// while (i < this.count)
            //// {
            ////     if (this.delayedActions[i].ExecuteTime <= currentTime)
            ////     {
            ////         this.delayedActions[i].Action?.Invoke();
            ////
            ////         int lastIndex = this.count - 1;
            ////
            ////         if (i != lastIndex)
            ////         {
            ////             this.delayedActions[i] = this.delayedActions[lastIndex];
            ////         }
            ////
            ////         this.delayedActions[lastIndex] = default;
            ////
            ////         currentTime = Time.realtimeSinceStartup;
            ////         this.count--;
            ////     }
            ////     else
            ////     {
            ////         i++;
            ////     }
            //// }
        }

        private void OnDestroy()
        {
            //// this.updateReceipt.Cancel();
        }

        private struct DelayedActionListItem
        {
            public int Id;
            public float ExecuteTime;
            public Action Action;
        }

        private class DelayedActionList : ProcessList<DelayedActionListItem>
        {
            public Action<int> OnDeleteId;

            public DelayedActionList(string name, int capacity)
                : base(name, capacity)
            {
            }

            protected override void Process(ref DelayedActionListItem item)
            {
                this.OnDeleteId?.Invoke(item.Id);
            }
        }
    }
}

#endif
