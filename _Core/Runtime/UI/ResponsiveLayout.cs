//-----------------------------------------------------------------------
// <copyright file="ResponsiveLayout.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [ExecuteAlways]
    [RequireComponent(typeof(Showable))]
    public class ResponsiveLayout : GameBehavior, IAwake
    {
        [Serializable]
        public class RectTransformSnapshot
        {
            public RectTransform target;

            public Vector2 anchorMin;
            public Vector2 anchorMax;
            public Vector2 anchoredPosition;
            public Vector2 sizeDelta;
            public Vector2 pivot;

            public Vector3 localEulerAngles;
            public Vector3 localScale;

            // Defaults to true so layouts saved before this was tracked don't read as "disabled"
            public bool isActive = true;

            public void Record(RectTransform rectTransform)
            {
                target = rectTransform;
                isActive = rectTransform.gameObject.activeSelf;

                anchorMin = rectTransform.anchorMin;
                anchorMax = rectTransform.anchorMax;
                anchoredPosition = rectTransform.anchoredPosition;
                sizeDelta = rectTransform.sizeDelta;
                pivot = rectTransform.pivot;

                localEulerAngles = rectTransform.localEulerAngles;
                localScale = rectTransform.localScale;
            }

            public void Apply()
            {
                if (target == null)
                    return;

                target.anchorMin = anchorMin;
                target.anchorMax = anchorMax;
                target.pivot = pivot;

                // Apply these after anchors/pivot because they depend on them.
                target.sizeDelta = sizeDelta;
                target.anchoredPosition = anchoredPosition;

                target.localEulerAngles = localEulerAngles;
                target.localScale = localScale;

                // Applying active state
                target.gameObject.SetActive(isActive);
            }
        }

        [ReadOnly]
        [SerializeField]
        private Showable showable;

        [Header("Recorded Layouts")]
        [SerializeField]
        private List<RectTransformSnapshot> portraitLayout = new();

        [SerializeField]
        private List<RectTransformSnapshot> landscapeLayout = new();

        private OrientationManager orientationManager;

#if UNITY_EDITOR

        private OrientationManager.Orientation currentEditorOrientation;

        // In Edit mode Unity only calls Update when the scene changes, and resizing the Game View doesn't count, so the
        // orientation is polled from the editor loop instead
        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                return;
            }

            UnityEditor.EditorApplication.update -= this.EditorUpdate;
            UnityEditor.EditorApplication.update += this.EditorUpdate;
            this.EditorUpdate();
        }

        private void OnDisable()
        {
            UnityEditor.EditorApplication.update -= this.EditorUpdate;
        }

        private void EditorUpdate()
        {
            if (this == null || Application.isPlaying)
            {
                UnityEditor.EditorApplication.update -= this.EditorUpdate;
                return;
            }

            var orientation = OrientationManager.GetCurrentOrientation();

            if (this.currentEditorOrientation != orientation)
            {
                this.currentEditorOrientation = orientation;
                this.ApplyLayout(orientation);
                UnityEditor.SceneView.RepaintAll();
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            }
        }

        public void RecordCurrentLayout() => this.RecordCurrentLayout(OrientationManager.GetCurrentOrientation());

        public void RecordCurrentLayout(OrientationManager.Orientation orientation)
        {
            List<RectTransformSnapshot> layout = orientation == OrientationManager.Orientation.Portrait
                ? this.portraitLayout
                : this.landscapeLayout;

            layout.Clear();

            foreach (RectTransform target in this.transform.GetComponentsInChildren<RectTransform>(true))
            {
                RectTransformSnapshot snapshot = new();
                snapshot.Record(target);
                layout.Add(snapshot);
            }

            // The other orientation may still hold objects that have since been deleted
            this.portraitLayout.RemoveAll(snapshot => snapshot == null || snapshot.target == null);
            this.landscapeLayout.RemoveAll(snapshot => snapshot == null || snapshot.target == null);

            EditorUtil.SetDirty(this);

            Debug.Log($"Recorded {orientation} layout with {layout.Count} objects.", this);
        }

#endif

        public void ApplyLayout(OrientationManager.Orientation orientation)
        {
            List<RectTransformSnapshot> layout = orientation == OrientationManager.Orientation.Portrait
                ? this.portraitLayout
                : this.landscapeLayout;

            foreach (RectTransformSnapshot snapshot in layout)
            {
                snapshot.Apply();
            }
        }

        public void OnAwake(Bootloader bootloader)
        {
            if (this.showable == null)
            {
                Debug.LogError($"ResponsiveLayout is missing a Showable component. Please add one to the GameObject '{this.gameObject.name}'.", this);
                return;
            }

            this.orientationManager = bootloader.FindManager<OrientationManager>();
            this.orientationManager.OrientationChanged += this.OnOrientationChanged;

            this.showable.OnShowStart.AddListener(() =>
            {
                this.ApplyLayout(this.orientationManager.CurrentOrientation);
            });

            if (this.showable.IsShown)
            {
                this.ApplyLayout(this.orientationManager.CurrentOrientation);
            }
        }

        private void OnDestroy()
        {
            if (this.orientationManager != null)
            {
                this.orientationManager.OrientationChanged -= this.OnOrientationChanged;
            }
        }

        private void OnOrientationChanged(OrientationManager.Orientation newOrientation)
        {
            if (this.showable.IsShown == false)
            {
                return;
            }

            this.ApplyLayout(this.orientationManager.CurrentOrientation);
        }

        private void OnValidate()
        {
            if (this.showable == null)
            {
                this.showable = this.GetComponent<Showable>();
                EditorUtil.SetDirty(this);
            }
        }
    }
}
