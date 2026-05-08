//-----------------------------------------------------------------------
// <copyright file="AnimatorHelper.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace OGT
{
    using UnityEngine;

    [RequireComponent(typeof(Animator))]
    public class AnimatorHelper : GameBehavior, IValidate
    {
#pragma warning disable 0649
        [SerializeField][HideInInspector] private Animator animator;
#pragma warning restore 0649

        public void SetBoolTrue(string paramName)
        {
            this.animator.SetBool(paramName, true);
        }

        public void SetBoolFalse(string paramName)
        {
            this.animator.SetBool(paramName, false);
        }

        public void ToggleBool(string paramName)
        {
            this.animator.SetBool(paramName, !this.animator.GetBool(paramName));
        }

        public void TimedDisable(float seconds)
        {
            this.ExecuteDelayed(seconds, () => this.animator.enabled = false);
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            this.EditorGetComponent<Animator>(ref this.animator);

            report.AssertNotNull(this, this.animator, nameof(this.animator));
        }
    }
}

#endif
