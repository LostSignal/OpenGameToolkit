namespace OGT
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using TMPro;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    [RequireComponent(typeof(Animation))]
    public class Showable : GameBehavior, IHasHidableComponents, IAwake, IStart, IValidate
    {
        private static readonly int MaskableGraphicsBatchSize = 30;
        private static List<AnimationClip> clipsToRemoveCache = new();

        [HideInInspector]
        [SerializeField] private Animation simpleAnimation;
        [SerializeField] private bool showOnStart;

        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Animations")]
        [SerializeField] private AnimationClip show;
        [SerializeField] private AnimationClip idle;
        [SerializeField] private AnimationClip hide;

        [Header("Actions")]
        [SerializeField] private UnityEvent onShowStart;
        [SerializeField] private UnityEvent onShowEnd;
        [SerializeField] private UnityEvent onHideStart;
        [SerializeField] private UnityEvent onHideEnd;

        private MaskableGraphic[] maskableGraphics;
        private LayoutGroup[] layoutGroups;
        private ScrollRect[] scrollRects;
        private TMP_Text[] textMeshProTexts;
        private Graphic[] raycastTargets;
        private RectMask2D[] rectMask2Ds;
        private GraphicRaycaster[] graphicRaycasters;
        private CanvasScaler canvasScaler;

        public AnimationClip ShowClip => this.show;

        public AnimationClip IdleClip => this.idle;

        public AnimationClip HideClip => this.hide;

        public UnityEvent OnShowStart => this.onShowStart;

        public UnityEvent OnShowEnd => this.onShowEnd;

        public UnityEvent OnHideStart => this.onHideStart;

        public UnityEvent OnHideEnd => this.onHideEnd;

        private Coroutine showCoroutine;
        private Coroutine hideCoroutine;

        [field: NonSerialized]
        public bool AreComponentsHidden { get; set; } = true;

        public bool IsShown { get; private set; }

        public IEnumerable<Type> GetHidableComponents()
        {
            yield return typeof(Animation);
        }

        public void ToggleShow()
        {
            if (this.IsShown)
            {
                this.Hide();
            }
            else
            {
                this.Show();
            }
        }

        public void Show()
        {
            if (this.IsShown)
            {
                return;
            }

            this.CancelHideCoroutine();
            this.IsShown = true;
            this.showCoroutine = CoroutineRunner.Instance.StartCoroutine(ShowCoroutine());

            IEnumerator ShowCoroutine()
            {
                yield return null;
                this.onShowStart?.Invoke();
                yield return null;

                if (this.canvasScaler)
                {
                    this.canvasScaler.enabled = true;
                }

                yield return null;
                this.SetLayoutGroups(true);
                yield return null;
                yield return this.SetMaskableGraphicsCoroutine(true);
                yield return null;
                this.SetTextMeshProTextsScaleStatic(false);
                yield return null;
                this.SetRectMasks2ds(true);
                yield return null;

                if (this.canvasGroup != null)
                {
                    this.canvasGroup.alpha = 1f;
                }

                this.simpleAnimation.Play(this.show.name);

                yield return WaitForUtil.Seconds(this.show.length);

                if (this.canvasGroup != null)
                {
                    yield return null;
                    this.canvasGroup.interactable = true;
                    yield return null;
                    this.canvasGroup.blocksRaycasts = true;
                    yield return null;
                }

                this.SetGraphicRaycasters(true);
                yield return null;

                this.SetRaycastTargets(true);
                yield return null;

                this.SetScrollRects(true);
                yield return null;

                this.onShowEnd?.Invoke();

                if (this.idle != null)
                {
                    this.simpleAnimation.Play(this.idle.name);
                }

                this.showCoroutine = null;
            }
        }

        public void Hide() => this.Hide(null);

        public void Hide(Action onHideComplete)
        {
            if (this.IsShown == false)
            {
                return;
            }

            this.CancelShowCoroutine();
            this.IsShown = false;
            this.hideCoroutine = this.StartCoroutine(HideCoroutine());

            IEnumerator HideCoroutine()
            {
                yield return null;
                this.onHideStart?.Invoke();
                yield return null;
                this.SetScrollRects(false);
                yield return null;

                this.simpleAnimation.Play(this.hide.name);

                yield return WaitForUtil.Seconds(this.hide.length);

                yield return this.SetMaskableGraphicsCoroutine(false);
                this.SetTextMeshProTextsScaleStatic(true);
                yield return null;

                if (this.canvasGroup != null)
                {
                    this.canvasGroup.alpha = 0f;
                    yield return null;
                    this.canvasGroup.interactable = false;
                    yield return null;
                    this.canvasGroup.blocksRaycasts = false;
                    yield return null;
                }

                this.SetLayoutGroups(false);
                yield return null;

                this.SetGraphicRaycasters(false);
                yield return null;

                this.SetRaycastTargets(false);
                yield return null;

                this.SetRectMasks2ds(false);
                yield return null;

                if (this.canvasScaler)
                {
                    this.canvasScaler.enabled = true;
                }

                yield return null;

                this.onHideEnd?.Invoke();
                yield return null;

                onHideComplete?.Invoke();
                this.hideCoroutine = null;
            }
        }

        public void HideIfShowing()
        {
            if (this.IsShown)
            {
                this.Hide();
            }
        }

        public void SetupAnimationComponent()
        {
            var animation = this.GetComponent<Animation>();

            if (this.simpleAnimation != animation)
            {
                this.simpleAnimation = animation;
                EditorUtil.SetDirty(this);
            }

            if (this.show != null && this.simpleAnimation.GetClip(this.show.name) == null)
            {
                this.simpleAnimation.AddClip(this.show, this.show.name);
                EditorUtil.SetDirty(this.simpleAnimation);
            }

            if (this.hide != null && this.simpleAnimation.GetClip(this.hide.name) == null)
            {
                this.simpleAnimation.AddClip(this.hide, this.hide.name);
                EditorUtil.SetDirty(this.simpleAnimation);
            }

            if (this.idle != null && this.simpleAnimation.GetClip(this.idle.name) == null)
            {
                this.simpleAnimation.AddClip(this.idle, this.idle.name);
                EditorUtil.SetDirty(this.simpleAnimation);
            }

            clipsToRemoveCache.Clear();

            foreach (var animationStateObject in this.simpleAnimation)
            {
                var animationState = (AnimationState)animationStateObject;

                if (animationState.clip == null ||
                    animationState.clip == this.show ||
                    animationState.clip == this.hide ||
                    animationState.clip == this.idle)
                {
                    continue;
                }

                clipsToRemoveCache.Add(animationState.clip);
                EditorUtil.SetDirty(this.simpleAnimation);
            }

            foreach (var clip in clipsToRemoveCache)
            {
                this.simpleAnimation.RemoveClip(clip);
            }

            clipsToRemoveCache.Clear();
        }

        public void OnAwake(Bootloader bootloader)
        {
            this.layoutGroups = this.GetComponentsInChildren<LayoutGroup>(true).Where(x => x.enabled == true).ToArray();
            this.scrollRects = this.GetComponentsInChildren<ScrollRect>(true).Where(x => x.enabled == true).ToArray();
            this.textMeshProTexts = this.GetComponentsInChildren<TMP_Text>(true).Where(x => x.enabled == true).ToArray();
            this.maskableGraphics = this.GetComponentsInChildren<MaskableGraphic>(true).Where(x => x.enabled == true).ToArray();
            this.raycastTargets = this.GetComponentsInChildren<Graphic>(true).Where(x => x.raycastTarget == true).ToArray();
            this.rectMask2Ds = this.GetComponentsInChildren<RectMask2D>(true).Where(x => x.enabled == true).ToArray();
            this.canvasScaler = this.GetComponent<CanvasScaler>();
            this.graphicRaycasters = this.GetComponentsInChildren<GraphicRaycaster>(true).Where(x =>
            {
                var canvas = x.GetComponent<Canvas>();
                return canvas != null && canvas.overrideSorting == true;
            }).ToArray();

            if (this.canvasGroup != null)
            {
                this.canvasGroup.alpha = 0f;
                this.canvasGroup.interactable = false;
                this.canvasGroup.blocksRaycasts = false;
            }

            this.SetLayoutGroups(false);
            this.SetScrollRects(false);
            this.SetTextMeshProTextsScaleStatic(true);
            this.SetMaskableGraphicsImmediate(false);
            this.SetRaycastTargets(false);
            this.SetRectMasks2ds(false);
            this.SetGraphicRaycasters(false);

            if (this.canvasScaler != null)
            {
                this.canvasScaler.enabled = false;
            }
        }

        public void OnStart()
        {
            if (this.showOnStart)
            {
                this.Show();
            }
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            this.EditorGetComponent(ref this.simpleAnimation);

            // If this Showable is on a Canvas, we need to make sure it has a CanvasGroup so we can fade it in and out
            if (this.canvasGroup == null && this.GetComponent<Canvas>() != null)
            {
                if (this.GetComponent<CanvasGroup>() == null)
                {
                    this.gameObject.AddComponent<CanvasGroup>();
                    EditorUtil.SetDirty(this);
                }

                this.EditorGetComponent(ref this.canvasGroup);
            }

            // We never want this to play automatically, our OnAwake function will figure that out
            if (this.simpleAnimation.playAutomatically)
            {
                this.simpleAnimation.playAutomatically = false;
                EditorUtil.SetDirty(this);
            }

            this.SetupAnimationComponent();

            report.AssertNotNull(this, this.simpleAnimation, nameof(this.simpleAnimation));
            report.AssertNull(this, this.simpleAnimation.clip, nameof(this.simpleAnimation.clip));
            report.AssertFalse(this, this.simpleAnimation.playAutomatically, nameof(this.simpleAnimation.playAutomatically));
        }

        private void CancelHideCoroutine()
        {
            if (this.hideCoroutine != null)
            {
                this.StopCoroutine(this.hideCoroutine);
                this.hideCoroutine = null;
            }
        }

        private void CancelShowCoroutine()
        {
            if (this.showCoroutine != null)
            {
                this.StopCoroutine(this.showCoroutine);
                this.showCoroutine = null;
            }
        }

        private void SetLayoutGroups(bool enabled)
        {
            if (this.layoutGroups == null)
            {
                return;
            }

            foreach (var layoutGroup in this.layoutGroups)
            {
                if (layoutGroup == null)
                {
                    continue;
                }

                layoutGroup.enabled = enabled;
            }
        }

        private void SetScrollRects(bool enabled)
        {
            if (this.scrollRects == null)
            {
                return;
            }

            foreach (var scrollRect in this.scrollRects)
            {
                scrollRect.enabled = enabled;
            }
        }

        private void SetTextMeshProTextsScaleStatic(bool isScaleStatic)
        {
            if (this.textMeshProTexts == null)
            {
                return;
            }

            foreach (var textMeshProText in this.textMeshProTexts)
            {
                if (textMeshProText == null)
                {
                    continue;
                }

                textMeshProText.isTextObjectScaleStatic = isScaleStatic;
            }
        }

        private IEnumerator SetMaskableGraphicsCoroutine(bool enabled)
        {
            if (this.maskableGraphics == null)
            {
                yield break;
            }

            int batchSize = Mathf.Max(1, MaskableGraphicsBatchSize);
            int processed = 0;

            foreach (var rectMask2D in this.maskableGraphics)
            {
                if (rectMask2D == null)
                {
                    continue;
                }

                rectMask2D.enabled = enabled;

                processed++;
                if (processed >= batchSize)
                {
                    processed = 0;
                    yield return null;
                }
            }
        }

        private void SetMaskableGraphicsImmediate(bool enabled)
        {
            if (this.maskableGraphics == null)
            {
                return;
            }

            foreach (var rectMask2D in this.maskableGraphics)
            {
                if (rectMask2D == null)
                {
                    continue;
                }

                rectMask2D.enabled = enabled;
            }
        }

        private void SetRaycastTargets(bool enabled)
        {
            if (this.raycastTargets == null)
            {
                return;
            }

            foreach (var raycastTarget in this.raycastTargets)
            {
                if (raycastTarget == null)
                {
                    continue;
                }

                raycastTarget.raycastTarget = enabled;
            }
        }

        private void SetRectMasks2ds(bool enabled)
        {
            if (this.rectMask2Ds == null)
            {
                return;
            }

            foreach (var rectMask2D in this.rectMask2Ds)
            {
                if (rectMask2D == null)
                {
                    continue;
                }

                rectMask2D.enabled = enabled;
            }
        }

        private void SetGraphicRaycasters(bool enabled)
        {
            if (this.graphicRaycasters == null)
            {
                return;
            }

            foreach (var graphicRaycaster in this.graphicRaycasters)
            {
                if (graphicRaycaster == null)
                {
                    continue;
                }
                graphicRaycaster.enabled = enabled;
            }
        }
    }
}
