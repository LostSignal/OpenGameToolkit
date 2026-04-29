//-----------------------------------------------------------------------
// <copyright file="CountDownTimerText.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using UnityEngine;
    using Text = TMPro.TMP_Text;

    [RequireComponent(typeof(Text))]
    public class CountDownTimerText : MonoBehaviour
    {
#pragma warning disable 0649, 0044
        [SerializeField] private string finishedText = "00:00:00";
        [SerializeField] private Text text;
#pragma warning restore 0649, 0044

        private DateTime target;
        private float updateTextTimer;

        public Action OnTimerComplete;

        public DateTime Target
        {
            get
            {
                return this.target;
            }

            set
            {
                this.target = value.ToUniversalTime();
                this.UpdateText();
            }
        }

        private void OnValidate() => this.EditorGetComponent<Text>(ref this.text);

        private void Awake() => this.OnValidate();

        private void OnEnable() => this.UpdateText();

        private void Update()
        {
            // Making sure we don't update the text every frame, since that is a little expensive, but we also want to make sure it updates at least every second
            this.updateTextTimer += Time.unscaledDeltaTime;

            if (this.updateTextTimer >= 1.0f)
            {
                this.updateTextTimer = 0.0f;
                this.UpdateText();
            }
        }

        private void UpdateText()
        {
            // Update text can be called before Awake is called, so this is very necessary, but this will get called again OnEnable
            if (this.text == null)
            {
                Debug.LogError("CountDownTimerText is missing a reference to the Text component", this);
                return;
            }

            var utcNow = DateTime.UtcNow;

            // Seeing if we're finished
            if (utcNow > this.target)
            {
                this.text.text = this.finishedText;
                this.enabled = false;
                this.OnTimerComplete?.Invoke();
            }
            else
            {
                if (this.enabled == false)
                {
                    this.enabled = true;
                }

                TimeSpan timeLeft = this.target.Subtract(utcNow);

                BetterStringBuilder.New().
                    AppendTwoDigitNumber(timeLeft.Hours).
                    Append(':').
                    AppendTwoDigitNumber(timeLeft.Minutes).
                    Append(':').
                    AppendTwoDigitNumber(timeLeft.Seconds).
                    Set(this.text);
            }
        }
    }
}
