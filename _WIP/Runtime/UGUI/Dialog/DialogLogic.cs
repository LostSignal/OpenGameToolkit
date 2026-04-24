//-----------------------------------------------------------------------
// <copyright file="DialogLogic.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Runtime.CompilerServices;
    using UnityEngine;

    //// NOTE [bgish]: Possible Future Events
    ////     OnShown (when showing is done)
    ////     OnHidden (when hidding is done)
    ////     OnCoverred (when a dialog has been covered up by another)
    ////     OnUncoverred (when a dialog has been uncovered from a dialog going away)

    [AddComponentMenu("")]
    [RequireComponent(typeof(Dialog))]
    public abstract class DialogLogic : GameBehavior, IValidate
    {
#pragma warning disable 0649
        [SerializeField]
        [ReadOnly]
        private Dialog dialog;
#pragma warning restore 0649

        private DialogManager dialogManager;

        public Dialog Dialog
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return this.dialog; }
        }

        public virtual void OnAwake(Bootloader bootloader)
        {
            this.dialog.OnShow.AddListener(this.OnShow);
            this.dialog.OnHide.AddListener(this.OnHide);
            this.dialog.OnBackButtonPressed.AddListener(this.OnBackButtonPressed);

            this.dialogManager = bootloader.FindManager<DialogManager>();
            dialogManager.RegisterDialog(this);
        }

        public virtual void Validate(ValidationReport report, bool isSceneObject)
        {
            this.EditorGetComponent(ref this.dialog);

            report.AssertNotNull(this, this.dialog, nameof(this.dialog));
        }

        protected virtual void OnDestroy()
        {
            if (this.dialogManager != null)
            {
                this.dialogManager.UnregisterDialog(this);
            }
        }

        protected virtual void OnShow()
        {
        }

        protected virtual void OnHide()
        {
        }

        protected virtual void OnBackButtonPressed()
        {
        }
    }
}
