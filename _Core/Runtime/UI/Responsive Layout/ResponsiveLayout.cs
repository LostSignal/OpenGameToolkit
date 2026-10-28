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

    [RequireComponent(typeof(Showable))]
    public partial class ResponsiveLayout : GameBehavior, IAwake
    {
#if UNITY_EDITOR
        [Unity.Scripting.LifecycleManagement.AutoStaticsCleanup]
        private static OrientationManager.Orientation currentOrientation;

        [Unity.Scripting.LifecycleManagement.AutoStaticsCleanup]
        private static UnityEditor.SceneManagement.PrefabStage currentIsPrefabStage;
#endif

        [ReadOnly]
        [SerializeField] private Showable showable;

        [Header("Recorded Layouts")]
        [SerializeField] private List<RectTransformSnapshot> portraitLayout = new();
        [SerializeField] private List<RectTransformSnapshot> landscapeLayout = new();

        [Header("XR")]
        [SerializeField] private OrientationManager.Orientation xrOrientation = OrientationManager.Orientation.Landscape;

        private OrientationManager orientationManager;
        private XRManager xrManager;

        public void ApplyLayout(OrientationManager.Orientation orientation)
        {
            // If we're in XR mode and the XR orientation is unspecified, we don't apply any layout changes. This is because the XR orientation is not known yet, and we don't want to apply a layout that may be incorrect.
            if (this.xrManager != null && this.xrManager.IsFlatMode == false && this.xrOrientation == OrientationManager.Orientation.Unspecified)
            {
                return;
            }

            // If we're in XR mode, we always use the configured XR orientation instead of the current device orientation
            orientation = (this.xrManager == null || this.xrManager.IsFlatMode) ? orientation : this.xrOrientation;

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

            this.xrManager = bootloader.FindManager<XRManager>();

            this.showable.OnShowStart.AddListener(() =>
            {
                this.ApplyLayout(this.orientationManager.CurrentOrientation);
            });

            // NOTE [bgish]: This is a hack to remove any snapshots that are no longer valid. We will eventually do this
            //               at build time (EditorEvents.OnProcessSceneBuild), but not sure how to do this for prefabs.
            this.RemoveRedundantSnapshots();

            if (this.showable.IsShown)
            {
                this.ApplyLayout(this.orientationManager.CurrentOrientation);
            }
        }

#if UNITY_EDITOR

        public void EditorRecordCurrentLayout()
        {
            RecordCurrentLayout(OrientationManager.GetCurrentOrientation());

            void RecordCurrentLayout(OrientationManager.Orientation orientation)
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
        }

#endif

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

        private void RemoveRedundantSnapshots()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            // Remove any snapshots that are null or whose target has been destroyed
            this.portraitLayout.RemoveAll(snapshot => snapshot == null || snapshot.target == null);
            this.landscapeLayout.RemoveAll(snapshot => snapshot == null || snapshot.target == null);

            // Remove snapshots that live under an object the same layout disables, since they'll never be seen in that layout.
            // This must happen before the equality pass, because that pass can remove the disabled parent's snapshot.
            RemoveSnapshotsUnderDisabledParents(this.portraitLayout);
            RemoveSnapshotsUnderDisabledParents(this.landscapeLayout);

            // Remove snapshots that are identical in both layouts, since applying them would never change anything
            var portraitByTarget = new Dictionary<RectTransform, RectTransformSnapshot>(this.portraitLayout.Count);

            foreach (RectTransformSnapshot snapshot in this.portraitLayout)
            {
                portraitByTarget[snapshot.target] = snapshot;
            }

            var redundantTargets = new HashSet<RectTransform>();

            foreach (RectTransformSnapshot landscapeSnapshot in this.landscapeLayout)
            {
                if (portraitByTarget.TryGetValue(landscapeSnapshot.target, out RectTransformSnapshot portraitSnapshot) &&
                    portraitSnapshot.Matches(landscapeSnapshot))
                {
                    redundantTargets.Add(landscapeSnapshot.target);
                }
            }

            if (redundantTargets.Count > 0)
            {
                this.portraitLayout.RemoveAll(snapshot => redundantTargets.Contains(snapshot.target));
                this.landscapeLayout.RemoveAll(snapshot => redundantTargets.Contains(snapshot.target));
            }

            void RemoveSnapshotsUnderDisabledParents(List<RectTransformSnapshot> layout)
            {
                var disabledTargets = new HashSet<Transform>();

                foreach (RectTransformSnapshot snapshot in layout)
                {
                    if (snapshot.isActive == false)
                    {
                        disabledTargets.Add(snapshot.target);
                    }
                }

                if (disabledTargets.Count == 0)
                {
                    return;
                }

                layout.RemoveAll(snapshot => HasDisabledAncestor(snapshot.target, disabledTargets));
            }

            bool HasDisabledAncestor(Transform target, HashSet<Transform> disabledTargets)
            {
                // Only walk up to this layout's root, since nothing above it is part of the recorded layout
                for (Transform parent = target.parent; parent != null; parent = parent.parent)
                {
                    if (disabledTargets.Contains(parent))
                    {
                        return true;
                    }

                    if (parent == this.transform)
                    {
                        break;
                    }
                }

                return false;
            }
        }

#if UNITY_EDITOR

        [EditorEvents.OnEditorUpdate]
        private static void EditorUpdate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            var currentState = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            bool didPrefabStageBecomeInvisible = false;

            if (currentIsPrefabStage == null && currentState != null)
            {
                Debug.Log("ResponsiveLayout: Prefab stage became visible, updating layouts.");
                didPrefabStageBecomeInvisible = true;
            }

            currentIsPrefabStage = currentState;

            if (didPrefabStageBecomeInvisible == false && currentOrientation == OrientationManager.GetCurrentOrientation())
            {
                return;
            }

            currentOrientation = OrientationManager.GetCurrentOrientation();

            var didUpdate = false;

            foreach (ResponsiveLayout responsiveLayout in GameObject.FindObjectsByType<ResponsiveLayout>(FindObjectsInactive.Include))
            {
                responsiveLayout.ApplyLayout(currentOrientation);
                didUpdate = true;
            }

            // FindObjectsByType skips preview scenes, so the prefab editing view needs its own pass
            var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();

            if (prefabStage != null && prefabStage.prefabContentsRoot != null)
            {
                foreach (ResponsiveLayout responsiveLayout in prefabStage.prefabContentsRoot.GetComponentsInChildren<ResponsiveLayout>(true))
                {
                    responsiveLayout.ApplyLayout(currentOrientation);
                    didUpdate = true;
                }
            }

            if (didUpdate)
            {
                UnityEditor.SceneView.RepaintAll();
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            }
        }

        private void OnValidate()
        {
            if (this.showable == null)
            {
                this.showable = this.GetComponent<Showable>();
                EditorUtil.SetDirty(this);
            }
        }
#endif

        [Serializable]
        private class RectTransformSnapshot
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
            public bool dontApplyIsActive;

            public void Record(RectTransform rectTransform)
            {
                target = rectTransform;
                isActive = rectTransform.gameObject.activeSelf;
                dontApplyIsActive = rectTransform.GetComponent<IgnoreResponsiveLayoutIsActive>() != null;

                anchorMin = rectTransform.anchorMin;
                anchorMax = rectTransform.anchorMax;
                anchoredPosition = rectTransform.anchoredPosition;
                sizeDelta = rectTransform.sizeDelta;
                pivot = rectTransform.pivot;

                localEulerAngles = rectTransform.localEulerAngles;
                localScale = rectTransform.localScale;
            }

            // Tolerances are per unit type: anchors/pivot/scale are normalized, positions/sizes are UI units, angles are degrees
            private const float NormalizedEpsilon = 0.001f;
            private const float UnitsEpsilon = 0.01f;
            private const float DegreesEpsilon = 0.01f;

            public bool Matches(RectTransformSnapshot other)
            {
                return target == other.target &&
                    isActive == other.isActive &&
                    IsClose(anchorMin, other.anchorMin, NormalizedEpsilon) &&
                    IsClose(anchorMax, other.anchorMax, NormalizedEpsilon) &&
                    IsClose(pivot, other.pivot, NormalizedEpsilon) &&
                    IsClose(anchoredPosition, other.anchoredPosition, UnitsEpsilon) &&
                    IsClose(sizeDelta, other.sizeDelta, UnitsEpsilon) &&
                    IsClose(localScale, other.localScale, NormalizedEpsilon) &&
                    IsAngleClose(localEulerAngles, other.localEulerAngles, DegreesEpsilon);
            }

            private static bool IsClose(Vector2 a, Vector2 b, float epsilon)
            {
                return Mathf.Abs(a.x - b.x) <= epsilon &&
                    Mathf.Abs(a.y - b.y) <= epsilon;
            }

            private static bool IsClose(Vector3 a, Vector3 b, float epsilon)
            {
                return Mathf.Abs(a.x - b.x) <= epsilon &&
                    Mathf.Abs(a.y - b.y) <= epsilon &&
                    Mathf.Abs(a.z - b.z) <= epsilon;
            }

            private static bool IsAngleClose(Vector3 a, Vector3 b, float epsilon)
            {
                // DeltaAngle handles wrap-around, so 359.999 and 0 count as a match
                return Mathf.Abs(Mathf.DeltaAngle(a.x, b.x)) <= epsilon &&
                    Mathf.Abs(Mathf.DeltaAngle(a.y, b.y)) <= epsilon &&
                    Mathf.Abs(Mathf.DeltaAngle(a.z, b.z)) <= epsilon;
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
                if (dontApplyIsActive == false)
                {
                    target.gameObject.SetActive(isActive);
                }                
            }
        }
    }
}
