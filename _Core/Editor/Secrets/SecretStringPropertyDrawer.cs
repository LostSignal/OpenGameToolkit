//-----------------------------------------------------------------------
// <copyright file="SecretStringPropertyDrawer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(SecretString))]
    public class SecretStringPropertyDrawer : PropertyDrawer
    {
        private const string NoneLabel = "(None)";
        private const float ButtonWidth = 22f;
        private const float Spacing = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var keyProp = property.FindPropertyRelative("secretKey");
            var currentKey = keyProp.stringValue;

            var fieldRect = EditorGUI.PrefixLabel(position, label);
            var dropdownRect = new Rect(fieldRect.x, fieldRect.y, fieldRect.width - ButtonWidth - Spacing, fieldRect.height);
            var editButtonRect = new Rect(dropdownRect.xMax + Spacing, fieldRect.y, ButtonWidth, fieldRect.height);

            string displayName;
            if (string.IsNullOrEmpty(currentKey))
            {
                displayName = NoneLabel;
            }
            else if (SecretsStore.Contains(currentKey))
            {
                displayName = currentKey;
            }
            else
            {
                displayName = $"[Missing: {currentKey}]";
            }

            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(displayName), FocusType.Keyboard))
            {
                ShowKeySelectionMenu(dropdownRect, keyProp);
            }

            if (GUI.Button(editButtonRect, EditorGUIUtility.IconContent("d_Settings", "Edit Secrets")))
            {
                SecretsEditorWindow.Open();
            }

            EditorGUI.EndProperty();
        }

        private static void ShowKeySelectionMenu(Rect dropdownRect, SerializedProperty keyProp)
        {
            // The SerializedProperty may be disposed by the time a menu item is clicked, so
            // re-resolve it through the serialized object and property path in the callback.
            var serializedObject = keyProp.serializedObject;
            var propertyPath = keyProp.propertyPath;
            var currentKey = keyProp.stringValue;

            var menu = new GenericMenu();

            menu.AddItem(new GUIContent(NoneLabel), string.IsNullOrEmpty(currentKey), () => SetKey(serializedObject, propertyPath, string.Empty));

            var keys = SecretsStore.GetKeys().ToList();

            if (keys.Count > 0)
            {
                menu.AddSeparator(string.Empty);

                foreach (var key in keys)
                {
                    menu.AddItem(new GUIContent(key), key == currentKey, () => SetKey(serializedObject, propertyPath, key));
                }
            }
            else
            {
                menu.AddSeparator(string.Empty);
                menu.AddDisabledItem(new GUIContent("No secrets found, use Tools/OGT/Edit Secrets to add some"));
            }

            menu.DropDown(dropdownRect);
        }

        private static void SetKey(SerializedObject serializedObject, string propertyPath, string key)
        {
            serializedObject.Update();
            serializedObject.FindProperty(propertyPath).stringValue = key;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
