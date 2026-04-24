#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="HavenSelectionOutline.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;

    [AddComponentMenu("Haven XR/Selection/HXR Selection Outline")]
    public class HavenSelectionOutline : GameBehavior, IAwake, IValidate
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

#pragma warning disable 0649
        [SerializeField] private HavenSelectionOutlineSettingsObject settings;
        [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;
        [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor interactor;
        [SerializeField] private Renderer targetRenderer;
#pragma warning restore 0649

        private MaterialPropertyBlock materialPropertyBlock;
        private int highlightedPropertyId;
        private float highlightedValue;

        public void Validate(ValidationReport report, bool isSceneObject)
        {
            report.AssertNotNull(this, this.settings, nameof(this.settings));
            report.AssertNotNull(this, this.targetRenderer, nameof(this.targetRenderer));

            if (isSceneObject && this.interactable == null && this.interactor == null)
            {
                string description = $"{nameof(HavenSelectionOutline)} has null {nameof(this.interactable)} AND {nameof(this.interactor)}.  This component will not work.";
                report.ReportError(this, "Null Interactable and Interactor", description);
            }
        }

        public void OnAwake(Bootloader bootloader)
        {
#if USING_UNITY_XR_INTERACTION_TOOLKIT
            this.highlightedPropertyId = this.settings.Settings.GetMaterialPropertyId();
            this.materialPropertyBlock = new MaterialPropertyBlock();
            this.materialPropertyBlock.SetFloat(this.highlightedPropertyId, this.highlightedValue);
            this.targetRenderer.SetPropertyBlock(this.materialPropertyBlock);

            if (this.interactor != null)
            {
                this.interactor.hoverEntered.AddListener(this.Highlight);
                this.interactor.hoverExited.AddListener(this.RemoveHighlight);
            }

            if (this.interactable != null)
            {
                this.interactable.hoverEntered.AddListener(this.Highlight);
                this.interactable.hoverExited.AddListener(this.RemoveHighlight);
            }
#endif
        }

#if USING_UNITY_XR_INTERACTION_TOOLKIT

        private void OnDestroy()
        {
            if (this.interactor != null)
            {
                this.interactor.hoverEntered.RemoveListener(this.Highlight);
                this.interactor.hoverExited.RemoveListener(this.RemoveHighlight);
            }

            if (this.interactable != null)
            {
                this.interactable.hoverEntered.RemoveListener(this.Highlight);
                this.interactable.hoverExited.RemoveListener(this.RemoveHighlight);
            }
        }

        private void OnValidate()
        {
            EditorUtil.SetIfNull(this, ref this.interactable);
            EditorUtil.SetIfNull(this, ref this.interactor);
            EditorUtil.SetIfNull(this, ref this.targetRenderer);
            EditorUtil.SetIfNull(this, ref this.settings, "2f105c9520f0cc84fa47f1f66566e48d");
        }

        private void Highlight(HoverEnterEventArgs args)
        {
            this.highlightedValue = 1.0f;
            this.SetFloat(this.highlightedValue);
        }

        private void RemoveHighlight(HoverExitEventArgs args)
        {
            this.highlightedValue = 0.0f;
            this.SetFloat(this.highlightedValue);
        }

        private void SetFloat(float value)
        {
            this.targetRenderer.GetPropertyBlock(this.materialPropertyBlock);
            this.materialPropertyBlock.SetFloat(this.highlightedPropertyId, value);
            this.targetRenderer.SetPropertyBlock(this.materialPropertyBlock);
        }
#endif
    }
}
