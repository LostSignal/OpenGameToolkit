//-----------------------------------------------------------------------
// <copyright file="HieraryIconDisplay.cs" company="Lost Signal">
//     Copyright (c) Lost Signal. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    // This tool was taking by the fantastic Warped Imagination video https://www.youtube.com/watch?v=EFh7tniBqkk&t=458s.
    // If you like OGT stuff, you must subscribe to Warped Imagination.
    public static class HierarchyIconDisplay
    {
        private static System.Type HierarchyWindowType;
        private static EditorWindow HierarchyEditorWindow;
        private static bool DoesHierarchyHaveFocus;
        private static bool IsLeftMouseDown;

        [EditorEvents.InitializeOnLoad]
        public static void UpdateHierarchyWindowItemOnGuiCallback()
        {
            if (ProjectSettingsEditorTools.Instance.UseWarpedImaginationNextLevelHierarchy)
            {
                // TODO [bgish]: In Unity 6.4 this changes to hierarchyWindowItemByEntityIdOnGUI
                EditorApplication.hierarchyWindowItemByEntityIdOnGUI += OnHierarchyWindowItemOnGUI;
                EditorApplication.update += OnEditorUpdate;
            }
            else
            {
                // TODO [bgish]: In Unity 6.4 this changes to hierarchyWindowItemByEntityIdOnGUI
                EditorApplication.hierarchyWindowItemByEntityIdOnGUI -= OnHierarchyWindowItemOnGUI;
                EditorApplication.update -= OnEditorUpdate;
            }
        }

        private static void OnEditorUpdate()
        {
            if (HierarchyWindowType == null)
            {
                HierarchyWindowType = System.Type.GetType("UnityEditor.SceneHierarchyWindow, UnityEditor");
            }

            if (HierarchyEditorWindow == null)
            {
                HierarchyEditorWindow = EditorWindow.GetWindow(HierarchyWindowType);
            }

            DoesHierarchyHaveFocus = EditorWindow.focusedWindow != null && EditorWindow.focusedWindow == HierarchyEditorWindow;
        }

        private static void OnHierarchyWindowItemOnGUI(EntityId entityId, Rect selectionRect)
        {
            var obj = EditorUtility.EntityIdToObject(entityId) as GameObject;

            // Making sure we have a object
            if (obj == null)
            {
                return;
            }

            // Don't overwrite prefab icons
            if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj) != null)
            {
                return;
            }

            Component[] components = obj.GetComponents<Component>();

            // Making sure we actually have components
            if (components == null || components.Length == 0)
            {
                return;
            }

            Component component = components.Length > 1 ? components[1] : components[0];

            if (component == null)
            {
                return;
            }

            System.Type type = component.GetType();

            GUIContent content = EditorGUIUtility.ObjectContent(component, type);
            content.text = null;
            content.tooltip = type.Name;

            if (content.image == null)
            {
                return;
            }

            bool isHovered = selectionRect.Contains(Event.current.mousePosition);
            bool isSelected = Selection.entityIds.Contains(entityId);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                IsLeftMouseDown = true;
            }
            else if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                IsLeftMouseDown = false;
            }

            // Special Case - There is a slight flicker when the left mouse button is down on another item,
            //                this item is still technically selected but it doesn't look that way anymore.
            bool isCtrlOrShiftDown = Event.current.modifiers.HasFlag(EventModifiers.Control) || Event.current.modifiers.HasFlag(EventModifiers.Shift);
            bool isBeingDeselected = isSelected && isHovered == false && IsLeftMouseDown && isCtrlOrShiftDown == false;

            // Special Case - There is a slight flicker when the left mouse button is down on this item,
            //                this item looks selected, but it technically isn't yet.
            bool isBeingSelected = isSelected == false && isHovered == true && IsLeftMouseDown;

            if (isBeingDeselected)
            {
                isSelected = false;
                isHovered = false;
            }
            else if (isBeingSelected)
            {
                isSelected = true;
                isHovered = false;
            }

            // Draw the background (covering existing icon)
            Color color = UnityEditorBackgroundColor.Get(isSelected, isHovered, DoesHierarchyHaveFocus);
            Rect backgroundRect = selectionRect;
            backgroundRect.width = 18.5f;
            EditorGUI.DrawRect(backgroundRect, color);

            // Drawing the new icon
            EditorGUI.LabelField(selectionRect, content);
        }
    }
}
