//-----------------------------------------------------------------------
// <copyright file="PanelLogic.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEngine;

    //// NOTE [bgish]: Possible Future Events
    ////     OnShown (when showing is done)
    ////     OnHidden (when hidding is done)
    ////     OnCoverred (when a dialog has been covered up by another)
    ////     OnUncoverred (when a dialog has been uncovered from a dialog going away)

    [AddComponentMenu("")]
    [RequireComponent(typeof(Panel))]
    public abstract class PanelLogic : GameBehavior, IValidate, IAwake
    {
        [SerializeField][ReadOnly] private Panel panel;

        private PanelManager panelManager;

        public Panel Panel => this.panel;

        public Showable Showable => this.panel.Showable;

        public PanelManager PanelManager => this.panelManager;

        private bool isRegistered;

        public void Register(Bootloader bootloader)
        {
            if (this.isRegistered == false)
            {
                this.panelManager = bootloader.FindManager<PanelManager>();
                this.panelManager.RegisterPanelLogic(this);
                this.isRegistered = true;
            }
        }

        public virtual void OnAwake(Bootloader bootloader)
        {
            this.Register(bootloader);
        }

        public virtual void Validate(ValidationReport report, bool isSceneObject)
        {
            this.EditorGetComponent(ref this.panel);
            report.AssertNotNull(this, this.panel, nameof(this.panel));
        }

        protected virtual void OnDestroy()
        {
            if (this.panelManager != null)
            {
                this.panelManager.UnregisterPanelLogic(this);
            }
        }
    }
}
