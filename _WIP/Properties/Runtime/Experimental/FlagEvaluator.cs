//-----------------------------------------------------------------------
// <copyright file="FlagEvaluator.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    public class FlagEvaluator : GameBehavior, IAwake, IStart
    {
#pragma warning disable 0649
        [SerializeField] private List<Properties.BoolProperty> flagsPresent;
        [SerializeField] private List<Properties.BoolProperty> flagsNotPresent;
        [SerializeField] private UnityEvent expressionTrue;
        [SerializeField] private UnityEvent expressionFalse;
#pragma warning restore 0649

        public void OnAwake(Bootloader bootloader)
        {
            foreach (var flag in this.flagsPresent)
            {
                flag.OnChange += this.OnFlagsChanged;
            }

            foreach (var flag in this.flagsNotPresent)
            {
                flag.OnChange += OnFlagsChanged;
            }
        }

        public void OnStart()
        {
            this.EvaluateFlags();
        }

        private void OnDestroy()
        {
            foreach (var flag in this.flagsPresent)
            {
                flag.OnChange -= this.OnFlagsChanged;
            }

            foreach (var flag in this.flagsNotPresent)
            {
                flag.OnChange -= OnFlagsChanged;
            }
        }

        private void OnFlagsChanged(bool oldValue, bool newValue)
        {
            this.EvaluateFlags();
        }

        private void EvaluateFlags()
        {
            bool expression = true;

            if (this.flagsPresent?.Count > 0)
            {
                foreach (var flag in this.flagsPresent)
                {
                    if (flag.Value == false)
                    {
                        expression = false;
                    }
                }
            }

            if (this.flagsNotPresent?.Count > 0)
            {
                foreach (var flag in this.flagsNotPresent)
                {
                    if (flag.Value)
                    {
                        expression = false;
                    }
                }
            }

            if (expression)
            {
                this.expressionTrue?.Invoke();
            }
            else
            {
                this.expressionFalse?.Invoke();
            }
        }
    }
}
