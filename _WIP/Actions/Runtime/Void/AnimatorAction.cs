//-----------------------------------------------------------------------
// <copyright file="AnimatorAction.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.SSS
{
    using UnityEngine;

    public class AnimatorAction : Action
    {
        public enum ParamType
        {
            None,
            Trigger,
            Bool,
            Float,
            Int
        }

#pragma warning disable 0649
        [SerializeField] private Animator animator;
        [SerializeField] private bool enableAnimatorIfDisabled;
        [SerializeField] private bool disableAnimatorWhenFinished;
        [SerializeField] private ParamType animationParameterType;

        [ShowIfNot("animationParameterType", ParamType.None)]
        [SerializeField] private string parameterName;

        [ShowIf("animationParameterType", ParamType.Bool)]
        [SerializeField] private bool boolValue;

        [ShowIf("animationParameterType", ParamType.Float)]
        [SerializeField] private float floatValue;

        [ShowIf("animationParameterType", ParamType.Int)]
        [SerializeField] private int intValue;
#pragma warning restore 0649

        public override string Category => "Animator";

        public override string DisplayName => "Animator";

        private bool didSetParameter;
        private int parameterId;

        public override void StateStarted()
        {
            this.didSetParameter = false;
            this.parameterId = Animator.StringToHash(this.parameterName);
        }

        protected override void UpdateProgress(float progress)
        {
            if (this.didSetParameter == false && progress > 0.0f)
            {
                this.didSetParameter = true;

                if (this.enableAnimatorIfDisabled && this.animator.enabled == false)
                {
                    this.animator.enabled = true;
                }

                if (this.animationParameterType == ParamType.Bool)
                {
                    this.animator.SetBool(this.parameterId, this.boolValue);
                }
                else if (this.animationParameterType == ParamType.Float)
                {
                    this.animator.SetFloat(this.parameterId, this.floatValue);
                }
                else if (this.animationParameterType == ParamType.Int)
                {
                    this.animator.SetInteger(this.parameterId, this.intValue);
                }
                else if (this.animationParameterType == ParamType.Trigger)
                {
                    this.animator.SetTrigger(this.parameterId);
                }
            }

            if (this.disableAnimatorWhenFinished && progress == 1.0f && this.animator.enabled)
            {
                this.animator.enabled = false;
            }
        }
    }
}
