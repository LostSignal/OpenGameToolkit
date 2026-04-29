//-----------------------------------------------------------------------
// <copyright file="FloatText.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections;
    using OGT;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;

    public class FloatText : MonoBehaviour
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

#pragma warning disable 0649
        [SerializeField] private TMP_Text text;
        [SerializeField] private string unsetText = "?";
        [SerializeField] private string prefixValue = string.Empty;
        [SerializeField] private string postfixValue = string.Empty;
        [SerializeField] private UnityEvent onStartAnimation;
        [SerializeField] private UnityEvent onEndAnimation;
        [SerializeField] private AnimationCurve animationCurve = new(new Keyframe { time = 0, value = 0 }, new Keyframe { time = 1, value = 1 });
        [SerializeField] private FloatFormat format;
        [SerializeField] private int decimalPlaces;
#pragma warning restore 0649

        private Coroutine animateToGoalCoroutine;
        private float currentValue = int.MinValue;
        private float goalValue = int.MinValue;

        public TMP_Text Text => this.text;

        public bool HasValueBeenSet
        {
            get { return this.currentValue != int.MinValue; }
        }

        public float CurrentValue
        {
            get { return this.currentValue; }
        }

        public float GoalValue
        {
            get { return this.goalValue; }
        }

        public void UpdateValue(float newValue, TextUpdateType updateType)
        {
            if (this.goalValue == newValue)
            {
                return;
            }

            this.goalValue = newValue;

            if (updateType == TextUpdateType.Instant)
            {
                if (this.currentValue != newValue)
                {
                    this.currentValue = newValue;
                    this.goalValue = newValue;

                    this.UpdateText();
                }
            }
            else if (updateType == TextUpdateType.Animate)
            {
                this.AnimateToGoal();
            }
            else
            {
                Logger.LogErrorFormat("FloatText.UpdateValue found unknown TextUpdateType {0}", updateType.ToString());
            }
        }

        public void AnimateToGoal()
        {
            if (this.currentValue != this.goalValue)
            {
                if (this.animateToGoalCoroutine != null)
                {
                    CoroutineRunner.Instance.StopCoroutine(this.animateToGoalCoroutine);
                    this.animateToGoalCoroutine = null;
                }

                this.animateToGoalCoroutine = CoroutineRunner.Instance.StartCoroutine(this.AnimateToGoalCoroutine());
            }
        }

        private IEnumerator AnimateToGoalCoroutine()
        {
            if (this.HasValueBeenSet == false)
            {
                this.currentValue = 0;
                this.text.text = "0";
            }

            this.onStartAnimation.SafeInvoke();

            float startValue = this.currentValue;
            float endValue = this.goalValue;
            float difference = endValue - startValue;
            float animationTime = this.animationCurve.TimeLength();
            float currentTime = 0;

            while (currentTime < animationTime)
            {
                float newValue = startValue + (difference * this.animationCurve.Evaluate(currentTime));

                this.currentValue = (int)newValue;
                this.UpdateText();

                yield return null;

                currentTime += Time.deltaTime;
            }

            this.currentValue = this.goalValue;
            this.UpdateText();

            this.onEndAnimation.SafeInvoke();
        }

        private void OnValidate()
        {
            this.EditorGetComponent(ref this.text);
        }

        private void Awake()
        {
            this.OnValidate();

            Localization.Localization.OnLanguagedChanged += this.UpdateText;
        }

        private void OnEnable()
        {
            this.UpdateText();
        }

        private void OnDestroy()
        {
            Localization.Localization.OnLanguagedChanged -= this.UpdateText;
        }

        private void UpdateText()
        {
            if (this.text != null)
            {
                if (this.currentValue == int.MinValue)
                {
                    this.text.text = this.unsetText;
                }
                else
                {
                    BetterStringBuilder.New()
                        .Append(this.prefixValue)
                        .Append(this.currentValue, this.decimalPlaces, this.format)
                        .Append(this.postfixValue)
                        .Set(this.text);
                }
            }
        }
    }
}
