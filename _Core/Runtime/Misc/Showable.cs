
namespace OGT
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    [RequireComponent(typeof(Animation))]
    public class Showable : GameBehavior, IHasHidableComponents, IStart, IValidate
    {
        private static List<AnimationClip> clipsToRemoveCache = new();

        [HideInInspector]
        [SerializeField] private Animation simpleAnimation;
        [SerializeField] private bool showOnStart;

        [SerializeField] private List<GameObject> enableOnShow;
        [SerializeField] private List<GameObject> disableOnHide;

        [Header("Animations")]
        [SerializeField] private AnimationClip show;
        [SerializeField] private AnimationClip idle;
        [SerializeField] private AnimationClip hide;

        [Header("Actions")]
        [SerializeField] private UnityEvent onShowStart;
        [SerializeField] private UnityEvent onShowEnd;
        [SerializeField] private UnityEvent onHideStart;
        [SerializeField] private UnityEvent onHideEnd;

        public List<GameObject> EnableOnShow => this.enableOnShow;

        public List<GameObject> DisableOnHide => this.disableOnHide;

        public AnimationClip ShowClip => this.show;

        public AnimationClip IdleClip => this.idle;

        public AnimationClip HideClip => this.hide;

        public UnityEvent OnShowStart => this.onShowStart;

        public UnityEvent OnShowEnd => this.onShowEnd;

        public UnityEvent OnHideStart => this.onHideStart;

        public UnityEvent OnHideEnd => this.onHideEnd;

        private Coroutine idleCoroutine;
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
            this.IsShown = true;
            this.CancelIdleCoroutine();
            this.CancelHideCoroutine();

            this.onShowStart?.Invoke();

            if (this.enableOnShow != null)
            {
                for (int i = 0; i < this.enableOnShow.Count; i++)
                {
                    this.enableOnShow[i].SetActive(true);
                }
            }

            this.simpleAnimation.Play(this.show.name);
            this.ExecuteDelayed(this.show.length, () =>
            {
                this.onShowEnd?.Invoke();
            });

            if (this.idle != null)
            {
                this.idleCoroutine = this.StartCoroutine(PlayIdleCoroutine());
            }

            IEnumerator PlayIdleCoroutine()
            {
                yield return WaitForUtil.Seconds(this.show.length);
                this.simpleAnimation.Play(this.idle.name);
                this.idleCoroutine = null;
            }
        }

        public void Hide()
        {
            this.IsShown = false;
            this.CancelIdleCoroutine();
            this.CancelHideCoroutine();
            this.onHideStart?.Invoke();

            this.simpleAnimation.Play(this.hide.name);

            this.hideCoroutine = this.StartCoroutine(PlayHideCoroutine());

            IEnumerator PlayHideCoroutine()
            {
                yield return WaitForUtil.Seconds(this.hide.length);

                if (this.disableOnHide == null)
                {
                    yield break;
                }

                for (int i = 0; i < this.disableOnHide.Count; i++)
                {
                    this.disableOnHide[i].SetActive(false);
                }

                this.hideCoroutine = null;
                this.onHideEnd?.Invoke();
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

        public void OnStart()
        {
            if (this.showOnStart)
            {
                this.Show();
            }
        }

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            // Making sure lists are created
            bool createLists = this.enableOnShow == null || this.disableOnHide == null;
            this.enableOnShow ??= new List<GameObject>();
            this.disableOnHide ??= new List<GameObject>();

            if (createLists)
            {
                EditorUtil.SetDirty(this);
            }

            // Getting Animation component
            this.EditorGetComponent(ref this.simpleAnimation);

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

        private void CancelIdleCoroutine()
        {
            if (this.idleCoroutine != null)
            {
                this.StopCoroutine(this.idleCoroutine);
                this.idleCoroutine = null;
            }
        }

        private void CancelHideCoroutine()
        {
            if (this.hideCoroutine != null)
            {
                this.StopCoroutine(this.hideCoroutine);
                this.hideCoroutine = null;
            }
        }
    }
}
