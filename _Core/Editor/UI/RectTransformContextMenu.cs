//-----------------------------------------------------------------------
// <copyright file="RectTransformContextMenu.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEditor;
    using UnityEngine;

    public static class RectTransformContextMenu
    {
        [MenuItem("CONTEXT/RectTransform/Anchors To Corners")]
        private static void AnchorsToCorners(MenuCommand command)
        {
            var rectTransform = command.context as RectTransform;
            if (rectTransform == null)
                return;

            var parent = rectTransform.parent as RectTransform;
            if (parent == null)
            {
                Debug.LogWarning("RectTransform must have a RectTransform parent.");
                return;
            }

            Undo.RecordObject(rectTransform, "Anchors To Corners");

            // Get the four corners in world space.
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            // Convert bottom-left and top-right into parent-local space.
            Vector2 bottomLeft = parent.InverseTransformPoint(corners[0]);
            Vector2 topRight = parent.InverseTransformPoint(corners[2]);

            Rect parentRect = parent.rect;

            // Convert the local positions into normalized anchor coordinates.
            Vector2 anchorMin = new Vector2(
                Mathf.InverseLerp(parentRect.xMin, parentRect.xMax, bottomLeft.x),
                Mathf.InverseLerp(parentRect.yMin, parentRect.yMax, bottomLeft.y)
            );

            Vector2 anchorMax = new Vector2(
                Mathf.InverseLerp(parentRect.xMin, parentRect.xMax, topRight.x),
                Mathf.InverseLerp(parentRect.yMin, parentRect.yMax, topRight.y)
            );

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;

            // Since the anchors now define the current rectangle,
            // clear the offsets.
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            EditorUtility.SetDirty(rectTransform);
        }

        [MenuItem("CONTEXT/RectTransform/Anchors To Corners", true)]
        private static bool AnchorsToCornersValidate(MenuCommand command)
        {
            var rectTransform = command.context as RectTransform;
            return rectTransform != null &&
                rectTransform.parent is RectTransform;
        }
    }
}
