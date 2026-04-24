//-----------------------------------------------------------------------
// <copyright file="ActionsPropertyDrawer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.SSS
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityEditorInternal;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(Actions), true)]
    public class ActionsPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return label != GUIContent.none && Screen.width < 333 ? (16f + 18f) : 16f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.serializedObject.Update();

            ReorderableListPropertyDrawer.Draw(property.FindPropertyRelative("actions"));

            property.serializedObject.ApplyModifiedProperties();

            //// EditorGUI.BeginProperty(position, label, property);
            //// int indent = EditorGUI.indentLevel;
            //// EditorGUI.indentLevel = 0;
            //// Rect pos = new Rect(position.x, position.y, 100, position.height);
            //// Rect color = new Rect(position.x + 250, position.y, 50, position.height);
            //// Rect contentPosition = EditorGUI.PrefixLabel(position, label);
            //// 
            //// if (position.height > 16f)
            //// {
            ////     position.height = 16f;
            ////     EditorGUI.indentLevel += 1;
            ////     contentPosition = EditorGUI.IndentedRect(position);
            ////     contentPosition.y += 18f;
            //// }
            //// 
            //// contentPosition.width *= 0.75f;
            //// EditorGUI.indentLevel = 0;
            //// //EditorGUI.PropertyField(position, property, GUIContent.none);
            //// //EditorGUI.PropertyField(position, property.FindPropertyRelative("points"), new GUIContent("Vertecies"), true);
            //// EditorGUI.LabelField(position, "Test");
            //// EditorGUI.indentLevel = indent;
            //// EditorGUI.EndProperty();
        }
   
        public static class ReorderableListPropertyDrawer
        {
            private static readonly Dictionary<SerializedProperty, ReorderableList> _lists = new Dictionary<SerializedProperty, ReorderableList>();

            public static void Draw(SerializedProperty serializedProperty)
            {
                Draw(serializedProperty, DefaultDrawElement);
            }

            public static void Draw(SerializedProperty serializedProperty, System.Action<SerializedProperty, Rect, int, bool, bool> elementDrawCallback)
            {
                var list = GetList(serializedProperty);

                list.drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, $"{serializedProperty.displayName} [{list.count}]");
                };

                list.drawElementCallback = (rect, index, active, focused) =>
                {
                    elementDrawCallback(serializedProperty, rect, index, active, focused);
                };

                list.elementHeightCallback = index => GetListHeight(serializedProperty, index);

                list.onAddDropdownCallback += OnClickToAddNew;

                list.DoLayoutList();
            }

            public static void Dispose(SerializedProperty serializedProperty)
            {
                _lists.Remove(serializedProperty);
            }

            private static ReorderableList GetList(SerializedProperty serializedProperty)
            {
                if (!_lists.TryGetValue(serializedProperty, out var list))
                {
                    list = new ReorderableList(serializedProperty.serializedObject, serializedProperty);
                    _lists.Add(serializedProperty, list);
                }

                return list;
            }

            private static float GetListHeight(SerializedProperty serializedProperty, int index)
            {
                if (serializedProperty.arraySize == 0)
                {
                    return 18.0f;
                }

                return EditorGUI.GetPropertyHeight(serializedProperty.GetArrayElementAtIndex(index));
            }

            private static void DefaultDrawElement(SerializedProperty property, Rect rect, int index, bool isActive, bool isFocused)
            {
                var guiContent = new GUIContent($"Element {index}");
                var element = property.GetArrayElementAtIndex(index);

                EditorGUI.PropertyField(rect, element, guiContent, true);
            }


            private static Dropdown actionsDropdownCache;

            private static void OnClickToAddNew(Rect buttonRect, ReorderableList list)
            {
                if (actionsDropdownCache == null)
                {
                    actionsDropdownCache = new Dropdown(new AdvancedDropdownState());
                }

                actionsDropdownCache.Show(buttonRect, list, OnNewVariableTypeSelected);
            }

            private static void OnNewVariableTypeSelected(DropdownItem dropdownItem, ReorderableList reorderableList)
            {
                var newAction = Activator.CreateInstance(dropdownItem.Type) as Action;
                reorderableList.serializedProperty.AddElementToArray(newAction);
                reorderableList.DoLayoutList();
            }
        }

        private sealed class DropdownItem : AdvancedDropdownItem
        {
            private readonly Type type;

            public DropdownItem(Type type, string displayName)
                : base(displayName) => this.type = type;

            public Type Type => this.type;
        }

        private sealed class Dropdown : AdvancedDropdown
        {
            private Action<DropdownItem, ReorderableList> callBack;
            private ReorderableList list;

            public Dropdown(AdvancedDropdownState state)
                : base(state) => this.minimumSize = new Vector2(200, 300);

            public void Show(Rect rect, ReorderableList list, Action<DropdownItem, ReorderableList> onItemSelectedCallback)
            {
                this.callBack = onItemSelectedCallback;
                this.list = list;

                this.Show(rect);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                Dictionary<string, AdvancedDropdownItem> folders = new Dictionary<string, AdvancedDropdownItem>();
                AdvancedDropdownItem root = new AdvancedDropdownItem("Actions");

                TypeCache.TypeCollection availableTypes = TypeCache.GetTypesDerivedFrom(typeof(Action));

                foreach (Type type in availableTypes.Where(x => x.IsAbstract == false))
                {
                    var action = Activator.CreateInstance(type) as Action;
                    var folder = this.GetFolder(action.Category, root);
                    folder.AddChild(new DropdownItem(type, action.DisplayName));
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                base.ItemSelected(item);
                this.callBack?.Invoke(item as DropdownItem, this.list);
            }

            private AdvancedDropdownItem GetFolder(string category, AdvancedDropdownItem root)
            {
                AdvancedDropdownItem current = root;
                string[] folders = category.Split("/");

                for (int i = 0; i < folders.Length; i++)
                {
                    var subFolder = current.children.FirstOrDefault(x => x.name == folders[i]);

                    if (subFolder == null)
                    {
                        subFolder = new DropdownItem(null, folders[i]);
                        current.AddChild(subFolder);
                    }
                    
                    current = subFolder;
                }

                return current;
            }
        }
    }

    
}
