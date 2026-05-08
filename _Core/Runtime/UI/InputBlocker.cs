//-----------------------------------------------------------------------
// <copyright file="InputBlocker.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using OGT;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasRenderer))]
    public class InputBlocker : MonoBehaviour, IPointerClickHandler
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        #pragma warning disable 0649
        [SerializeField][HideInInspector] private GraphicRaycaster graphicRaycaster;
        [SerializeField][HideInInspector] private CanvasRenderer canvasRenderer;
        [SerializeField][HideInInspector] private RectTransform rectTransform;
        [SerializeField][HideInInspector] private UnityEvent onClick;
        [SerializeField][HideInInspector] private Image image;
        #pragma warning restore 0649

        public UnityEvent OnClick
        {
            get { return this.onClick; }
        }

        public Color Color
        {
            get { return this.image.color; }
            set { this.image.color = value; }
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            this.onClick.Invoke();
        }

        private void Awake()
        {
            this.Setup();
        }

        private void Start()
        {
            Logger.Assert(this.graphicRaycaster != null, "InputBlocker doesn't live under a GraphicRaycaster and will not work!");
        }

        private void Reset()
        {
            // TODO [bgish]: Someday maybe have some app settings that will set the color of the image based on a project wide setting
            this.Setup();
        }

        private void OnValidate()
        {
            this.Setup();
        }

        private void Setup()
        {
            this.EditorGetComponent(ref this.canvasRenderer);
            this.EditorGetComponent(ref this.rectTransform);
            this.EditorGetComponent(ref this.image);

            // Making sure we have a graphic raycaster
            if (this.graphicRaycaster == null)
            {
                this.graphicRaycaster = this.GetComponentInParent<GraphicRaycaster>();
            }

            // Making sure onClick isn't null
            this.onClick ??= new UnityEvent();

            // Setting up initial values
            this.canvasRenderer.cullTransparentMesh = true;
            this.image.raycastTarget = true;

            if (this.rectTransform.anchorMin != Vector2.zero)
            {
                this.rectTransform.anchorMin = Vector2.zero;
            }

            if (this.rectTransform.anchorMax != Vector2.one)
            {
                this.rectTransform.anchorMax = Vector2.one;
            }

            if (this.rectTransform.sizeDelta != Vector2.zero)
            {
                this.rectTransform.sizeDelta = Vector2.zero;
            }
        }
    }
}
