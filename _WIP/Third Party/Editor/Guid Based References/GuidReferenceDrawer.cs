//-----------------------------------------------------------------------
// <copyright file="GuidReferenceDrawer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using UnityEditor;
    using UnityEngine;

    // Using a property drawer to allow any class to have a field of type GuidRefernce and still get good UX
    // If you are writing your own inspector for a class that uses a GuidReference, drawing it with
    // EditorLayout.PropertyField(prop) or similar will get this to show up automatically
    [CustomPropertyDrawer(typeof(GuidReference))]
    public class GuidReferenceDrawer : PropertyDrawer
    {
        // Cache off GUI content to avoid creating garbage every frame in editor
        private readonly GUIContent sceneLabel = new ("Scene", "The target object is expected in this scene asset.");
        private readonly GUIContent clearButtonGUI = new ("Clear", "Remove Cross Scene Reference");

        private SerializedProperty guidProp;
        private SerializedProperty sceneProp;
        private SerializedProperty nameProp;
        private SerializedProperty guidStringProp;

        // Add an extra line to display source scene for targets
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + (EditorGUIUtility.singleLineHeight * 3);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            this.guidProp = property.FindPropertyRelative("serializedGuid");
            this.nameProp = property.FindPropertyRelative("cachedName");
            this.guidStringProp = property.FindPropertyRelative("cachedGuidString");
            this.sceneProp = property.FindPropertyRelative("cachedScene");

            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            position.height = EditorGUIUtility.singleLineHeight;

            // Draw prefix label, returning the new rect we can draw in
            var guidCompPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Working with array properties is a bit unwieldy, you have to get the property at each index manually
            byte[] byteArray = new byte[16];
            int arraySize = this.guidProp.arraySize;

            for (int i = 0; i < arraySize; ++i)
            {
                var byteProp = this.guidProp.GetArrayElementAtIndex(i);
                byteArray[i] = (byte)byteProp.intValue;
            }

            System.Guid currentGuid = new (byteArray);
            GameObject currentGO = GuidManager.ResolveGuid(currentGuid);
            GuidComponent currentGuidComponent = currentGO != null ? currentGO.GetComponent<GuidComponent>() : null;
            GuidComponent component = null;

            if (currentGuid != System.Guid.Empty && currentGuidComponent == null)
            {
                // if our reference is set, but the target isn't loaded, we display the target and the scene it is in, and provide a way to clear the reference
                float buttonWidth = 55.0f;

                guidCompPosition.xMax -= buttonWidth;

                bool guiEnabled = GUI.enabled;
                GUI.enabled = false;
                EditorGUI.LabelField(guidCompPosition, new GUIContent(this.nameProp.stringValue, "Target GameObject is not currently loaded."), EditorStyles.objectField);
                GUI.enabled = guiEnabled;

                Rect clearButtonRect = new (guidCompPosition)
                {
                    xMin = guidCompPosition.xMax,
                };

                clearButtonRect.xMax += buttonWidth;

                if (GUI.Button(clearButtonRect, this.clearButtonGUI, EditorStyles.miniButton))
                {
                    this.ClearPreviousGuid();
                }
            }
            else
            {
                // if our object is loaded, we can simply use an object field directly
                component = EditorGUI.ObjectField(guidCompPosition, currentGuidComponent, typeof(GuidComponent), true) as GuidComponent;
            }

            if (currentGuidComponent != null && component == null)
            {
                this.ClearPreviousGuid();
            }

            // if we have a valid reference, draw the scene name of the scene it lives in so users can find it
            if (component != null)
            {
                string scenePath = component.gameObject.scene.path;

                this.nameProp.stringValue = component.gameObject.GetFullName();
                this.guidStringProp.stringValue = component.GetGuid().ToString();
                this.sceneProp.objectReferenceValue = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

                // only update the GUID Prop if something changed. This fixes multi-edit on GUID References
                if (component != currentGuidComponent)
                {
                    byteArray = component.GetGuid().ToByteArray();
                    arraySize = this.guidProp.arraySize;
                    for (int i = 0; i < arraySize; ++i)
                    {
                        var byteProp = this.guidProp.GetArrayElementAtIndex(i);
                        byteProp.intValue = byteArray[i];
                    }
                }
            }

            EditorGUI.indentLevel++;
            bool cachedGUIState = GUI.enabled;
            float cachedLabelWidth = EditorGUIUtility.labelWidth;

            GUI.enabled = false;
            EditorGUIUtility.labelWidth = 60.0f;

            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.TextField(position, "Guid", this.guidStringProp.stringValue);

            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.ObjectField(position, this.sceneLabel, this.sceneProp.objectReferenceValue, typeof(SceneAsset), false);

            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.TextField(position, "Name", this.nameProp.stringValue);

            GUI.enabled = cachedGUIState;
            EditorGUIUtility.labelWidth = cachedLabelWidth;
            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }

        private void ClearPreviousGuid()
        {
            this.nameProp.stringValue = string.Empty;
            this.sceneProp.objectReferenceValue = null;

            int arraySize = this.guidProp.arraySize;
            for (int i = 0; i < arraySize; ++i)
            {
                var byteProp = this.guidProp.GetArrayElementAtIndex(i);
                byteProp.intValue = 0;
            }
        }
    }
}
