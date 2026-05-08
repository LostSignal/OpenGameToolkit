//-----------------------------------------------------------------------
// <copyright file="PanelManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;

    public class PanelManager : Manager
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        private readonly Dictionary<Type, PanelLogic> panelLogics = new();
        private readonly Dictionary<string, Panel> panels = new();

        [Header("Canvas Scaler Settings")]
        [SerializeField] private CanvasScaler.ScaleMode uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        [SerializeField] private Vector2 portraitReferenceResolution = new Vector2(1080.0f, 1920.0f);
        [SerializeField] private Vector2 landscapeReferenceResolution = new Vector2(1920.0f, 1080.0f);
        [SerializeField] private CanvasScaler.ScreenMatchMode screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        [SerializeField] private float matchWidthOrHeight = 1.0f;
        [SerializeField] private float referencePixelsPerUnit = 100.0f;

        private List<Panel> panelStack = new();
        private AppOrientation appOrientation;

        public void ConfigureCanvasScaler(CanvasScaler canvasScaler)
        {
            canvasScaler.uiScaleMode = this.uiScaleMode;
            canvasScaler.screenMatchMode = this.screenMatchMode;
            canvasScaler.matchWidthOrHeight = this.matchWidthOrHeight;
            canvasScaler.referencePixelsPerUnit = this.referencePixelsPerUnit;

            if (this.appOrientation == AppOrientation.Landscape)
            {
                canvasScaler.referenceResolution = this.landscapeReferenceResolution;
            }
            else if (this.appOrientation == AppOrientation.Portrait)
            {
                canvasScaler.referenceResolution = this.portraitReferenceResolution;
            }
            else
            {
                canvasScaler.referenceResolution = Screen.width > Screen.height ? this.landscapeReferenceResolution : this.portraitReferenceResolution;
            }
        }

        public void Push(Panel panel)
        {
            this.panelStack.AddIfNotNullAndUnique(panel);
        }

        public void Pop(Panel panel)
        {
            this.panelStack.Remove(panel);
        }

        public void RegisterPanel(Panel panel)
        {
            if (this.panels.ContainsKey(panel.name))
            {
                Logger.LogError($"Panel {panel.name} already exists!");
                return;
            }

            this.panels.Add(panel.name, panel);
        }

        public void UnregisterPanel(Panel panel)
        {
            this.panels.Remove(panel.name);
        }

        public void RegisterPanelLogic<T>(T panelLogic)
            where T : PanelLogic
        {
            Type type = panelLogic.GetType();

            if (this.panelLogics.ContainsKey(type))
            {
                Logger.LogError($"Panel Logic Type {type.Name} already exists!");
                return;
            }

            this.panelLogics.Add(type, panelLogic);
        }

        public void UnregisterPanelLogic<T>(T panelLogic)
            where T : PanelLogic
        {
            this.panelLogics.Remove(typeof(T));
        }

        public Panel GetPanelByName(string panelName)
        {
            return this.panels.TryGetValue(panelName, out Panel panel) ? panel : null;
        }

        public T GetPanel<T>()
            where T : PanelLogic
        {
            return this.panelLogics.TryGetValue(typeof(T), out PanelLogic panelLogic) ? (T)panelLogic : null;
        }

        protected override Task InitializeManager(Bootloader bootloader)
        {
            this.appOrientation = bootloader.SupportedOrientation;

            // Registering any PanelLogics contained under this manager
            foreach (var panelLogic in this.GetChildrenOfType<PanelLogic>())
            {
                panelLogic.Register(bootloader);
            }

            Platform.OnBackButtonPressed += this.OnBackButtonPressed;

            return Task.CompletedTask;
        }

        public override void OnManagerDestroyed()
        {
            Platform.OnBackButtonPressed -= this.OnBackButtonPressed;
        }

        // NOTE [bgish]: Doing static deregistering twice so ProjectAuditor wont complain
        private void OnDisable() => Platform.OnBackButtonPressed -= this.OnBackButtonPressed;

        private void OnBackButtonPressed(object sender, EventArgs e)
        {
            this.panelStack.LastOrDefault()?.RaiseOnBackButtonPressed();
        }
    }
}
