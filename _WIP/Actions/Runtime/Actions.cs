//-----------------------------------------------------------------------
// <copyright file="GameObjectStateAction.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.SSS
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    [Serializable]
    public class Actions
    {
#pragma warning disable 0649
        [SerializeField] private bool repeat;
        [SerializeReference] private List<Action> actions;
#pragma warning restore 0649

        private float currentTime;
        private float stateDuration;
        private bool areActionsFinished;

        public bool Repeat
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.repeat;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.repeat = value;
        }

        public List<Action> ActionList
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.actions;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.actions = value;
        }

        public bool AreActionsFinished
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.areActionsFinished;
        }

        public void Play()
        {
            // TODO [bgish]: Need to register this with the UpdateManager so it gets updates

            this.currentTime = 0.0f;
            this.areActionsFinished = false;

            for (int i = 0; i < this.actions.Count; i++)
            {
                this.actions[i].StateStarted();
                this.stateDuration = this.actions[i].TotalTime;
            }
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void UpdateState(float deltaTime)
        {
            this.currentTime += deltaTime;

            if (this.repeat)
            {
                while (this.currentTime > this.stateDuration)
                {
                    this.currentTime -= this.stateDuration;
                }
            }

            bool haveAllActionsFinshed = true;

            for (int i = 0; i < this.actions.Count; i++)
            {
                if (this.actions[i].Update(this.currentTime) == false)
                {
                    haveAllActionsFinshed = false;
                }
            }

            if (this.repeat == false && haveAllActionsFinshed)
            {
                this.areActionsFinished = true;
            }
        }
    }
}
