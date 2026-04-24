
using UnityEditor;
using UnityEngine;

namespace OGT.Properties
{
    [CustomPropertyDrawer(typeof(BoolProperty))]
    [CustomPropertyDrawer(typeof(IntProperty))]
    [CustomPropertyDrawer(typeof(FloatProperty))]
    [CustomPropertyDrawer(typeof(StringProperty))]
    [CustomPropertyDrawer(typeof(EnumProperty))]
    public class PropertyPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 8;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), GUIContent.none);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate Rects
            var labelX = position.x;
            var labelWidth = 200;
            var labelRect = new Rect(position.x, position.y, labelWidth, position.height);

            var nameX = labelX + labelWidth + 10;
            var nameWidth = 200;
            var nameRect = new Rect(nameX, position.y, nameWidth, position.height);

            // var valueX = nameX + nameWidth + 10;
            // var valueRect = new Rect(valueX, position.y, position.width - labelWidth - nameWidth - 20, position.height);

            // Draw fields - pass PropertyField.none to each so they are drawn without labels
            EditorGUI.PropertyField(labelRect, property.FindPropertyRelative("properties"));
            EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("propertyId"));
            //UnityEditor.EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("defaultValue"), GUIContent.none);

            // GUI.enabled = false;
            // UnityEditor.EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("currentValue"), GUIContent.none);
            // GUI.enabled = true;

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        public static void Draw(Property property, Rect position)
        {
            var nameRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var boolRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);

            property.Properties = EditorGUI.ObjectField(nameRect, "Properties", property.Properties, typeof(Properties), false) as Properties;

            if (property.Properties == null)
            {
                EditorGUI.HelpBox(boolRect, "Please assign a Properties asset.", MessageType.Warning);
                return;
            }

            string propertyName = property.PropertyId == 0 ? "None" : property.Name;

            if (EditorGUI.DropdownButton(boolRect, new GUIContent(property.Name), FocusType.Keyboard))
            {
                var genericMenu = new GenericMenu();

                foreach (string option in property.Properties.GetPropertyNames(property.Type))
                {
                    string[] parts = option.Split('.');
                    string menuPath = string.Join("/", parts);

                    genericMenu.AddItem(
                        new GUIContent(menuPath),
                        false,
                        (selected) =>
                        {
                            int newId = property.Properties.GetPropertyIdByName((string)selected);

                            if (property.PropertyId == newId)
                            {
                                return;
                            }

                            property.PropertyId = newId;
                        },
                        option);
                }

                genericMenu.ShowAsContext();
            }
        }
    }
}
