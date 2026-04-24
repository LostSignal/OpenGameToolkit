
namespace OGT
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(GraphicRaycaster))]
    [RequireComponent(typeof(Showable))]
    public class Panel : GameBehavior, IHasHidableComponents, IValidate, IAwake
    {
        public enum BackButtonAction
        {
            DoNothing,
            Hide,
            ExitApplication,
        }

        [SerializeField] private Showable showable;
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasScaler canvasScaler;
        [SerializeField] private GameObject content;
        [SerializeField] private bool registerForBackButtonPressed;
        [SerializeField] private BackButtonAction backButtonAction;
        [SerializeField] private UnityEvent onBackButtonPressed;

        private PanelManager panelManager;

        [field: NonSerialized]
        public bool AreComponentsHidden { get; set; } = true;

        public Showable Showable => this.showable;

        public UnityEvent OnBackButtonPressed => this.onBackButtonPressed;

        public bool IsShown => this.showable.IsShown;

        public void OnAwake(Bootloader bootloader)
        {
            this.panelManager = bootloader.FindManager<PanelManager>();
            this.panelManager.ConfigureCanvasScaler(this.canvasScaler);
            this.panelManager.RegisterPanel(this);

            this.showable.OnShowStart?.AddListener(this.WakeUp);
            this.showable.OnHideEnd?.AddListener(this.Hibernate);

            this.showable.OnShowStart?.AddListener(this.PushPanelOnStack);
            this.showable.OnHideEnd?.AddListener(this.PopPanelFromStack);

            if (this.canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                this.canvas.worldCamera = bootloader.FindManager<CameraManager>().CameraState.Camera;
            }
        }

        public IEnumerable<Type> GetHidableComponents()
        {
            yield return typeof(Canvas);
            yield return typeof(CanvasScaler);
            yield return typeof(GraphicRaycaster);
            yield return typeof(Showable);
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            this.EditorGetComponent(ref this.showable);
            this.EditorGetComponent(ref this.canvas);
            this.EditorGetComponent(ref this.canvasScaler);

            if (this.content == null && this.transform.Find("Content") != null)
            {
                this.content = this.transform.Find("Content").gameObject;
            }

            // Making sure Showable hides/shows the content
            var showable = this.GetComponent<Showable>();
            if (showable.EnableOnShow.Contains(this.content) == false)
            {
                showable.EnableOnShow.Add(this.content);
                EditorUtil.SetDirty(showable);
            }

            if (showable.DisableOnHide.Contains(this.content) == false)
            {
                showable.DisableOnHide.Add(this.content);
                EditorUtil.SetDirty(showable);
            }

            report.AssertNotNull(this, this.showable, nameof(this.showable));
            report.AssertNotNull(this, this.canvas, nameof(this.canvas));
            report.AssertNotNull(this, this.canvasScaler, nameof(this.canvasScaler));
            report.AssertNotNull(this, this.content, nameof(this.content));
        }

        public Coroutine HideThenShow(PanelLogic panelLogic)
        {
            return this.HideThenShow(panelLogic.Panel);
        }

        public Coroutine HideThenShow(Panel panel)
        {
            return CoroutineRunner.Instance.StartCoroutine(Coroutine());

            IEnumerator Coroutine()
            {
                this.showable.Hide();
                yield return WaitForUtil.Seconds(this.showable.HideClip.length);
                panel?.Show();
            }
        }

        public void Show()
        {
            this.showable.Show();
        }

        public void Hide()
        {
            this.HideThenShow((Panel)null);
        }

        public void RaiseOnBackButtonPressed()
        {
            if (this.registerForBackButtonPressed == false)
            {
                return;
            }

            if (this.backButtonAction == BackButtonAction.DoNothing)
            {
                // Do Nothing
            }
            else if (this.backButtonAction == BackButtonAction.Hide)
            {
                this.Hide();
            }
            else if (this.backButtonAction == BackButtonAction.ExitApplication)
            {
                this.PromptToExitApplication();
            }

            this.onBackButtonPressed?.Invoke();
        }

        public void PromptToExitApplication()
        {
            // TODO [bgish]: Add localization
            this.panelManager.GetPanel<NewMessageBox>().ShowYesNo(
                "Quit?",
                "Are you sure you want to quit?",
                () => Platform.QuitApplication(),
                null);
        }

        private void OnDestroy()
        {
            if (this.panelManager != null)
            {
                this.panelManager.UnregisterPanel(this);
            }

            if (this.showable)
            {
                this.showable.OnShowStart?.RemoveListener(this.WakeUp);
                this.showable.OnHideEnd?.RemoveListener(this.Hibernate);

                this.showable.OnShowStart?.RemoveListener(this.PushPanelOnStack);
                this.showable.OnHideEnd?.RemoveListener(this.PopPanelFromStack);
            }
        }

        private void Hibernate()
        {
            this.content.SetActive(false);
            this.canvas.enabled = false;
        }

        private void WakeUp()
        {
            this.canvas.enabled = true;
            this.content.SetActive(true);
        }

        private void PushPanelOnStack()
        {
            if (this.registerForBackButtonPressed)
            {
                this.panelManager.Push(this);
            }
        }

        private void PopPanelFromStack()
        {
            if (this.registerForBackButtonPressed)
            {
                this.panelManager.Pop(this);
            }
        }
    }
}
