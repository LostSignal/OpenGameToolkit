//-----------------------------------------------------------------------
// <copyright file="ChangeDisplayNameButton.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY && USING_PLAYFAB

namespace OGT.PlayFab
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    [RequireComponent(typeof(Button))]
    public class ChangeDisplayNameButton : GameBehavior, IAwake, IValidate
    {
#pragma warning disable 0649
        [HideInInspector][SerializeField] private Button button;
        [SerializeField] private UnityEvent onNameChangedSuccess;
        [SerializeField] private UnityEvent onNameChangedFailed;
#pragma warning restore 0649

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            this.EditorGetComponent(ref this.button);
            report.AssertNotNull(this, this.button, nameof(this.button));
        }

        public void OnAwake(Bootloader bootloader)
        {
            this.button.onClick.AddListener(this.ButtonClicked);
        }

        private void ButtonClicked()
        {
            this.button.interactable = false;

            CoroutineRunner.Instance.StartCoroutine(Coroutine());

            IEnumerator Coroutine()
            {
                var changeName = PlayFab.PlayFabManager.Instance.User.ChangeDisplayNameWithPopup();

                yield return changeName;

                if (changeName.HasError == false)
                {
                    this.onNameChangedSuccess?.Invoke();
                }
                else
                {
                    this.onNameChangedFailed?.Invoke();
                }

                this.button.interactable = true;
            }
        }
    }
}

#endif
