//-----------------------------------------------------------------------
// <copyright file="Editor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    public class ArrayDisplayOptions
    {
        public string ArrayMemberName { get; set; }
        public List<ArrayDisplayColumn> Columns { get; set; }
        public bool ShowAddDeleteButtons { get; set; }
        public bool ShowUpDownButtons { get; set; }
    }

    public class ArrayDisplayColumn
    {
        public string ColumnName { get; set; }
        public string MemberName { get; set; }
        public float Width { get; set; }
    }

    public abstract class Editor : UnityEditor.Editor
    {
        private static readonly Dictionary<ulong, SerializedObject> SerializedObjectCache = new();
        private static readonly OGTLogger Logger = OGTLogger.OGTEditor;
        private static readonly string DownString = '\u2193'.ToString();
        private static readonly string UpString = '\u2191'.ToString();
        private static GUIStyle ArrayHeaderStyle;

        private readonly Dictionary<string, SerializedProperty> serializedProperties = new();

        public override void OnInspectorGUI()
        {
            this.NewOnInspectorGUI();

            DefaultMonoBehaviourEditor.DrawDefaultContent(this.target);

            if (GUI.changed)
            {
                this.serializedObject.ApplyModifiedProperties();
                this.OnGUIChanged();
            }
        }

        protected virtual void OnEnable()
        {
            this.serializedProperties.Clear();

            DefaultMonoBehaviourEditor.UpdateHidableComponents(this.target);
        }

        protected virtual void OnDisable() => this.serializedProperties.Clear();

        protected void DrawMember(string propertyName, params GUILayoutOption[] options)
        {
            EditorGUILayout.PropertyField(this.GetProperty(propertyName), options);
        }

        protected void DrawMember(string propertyName, string labelName, params GUILayoutOption[] options)
        {
            EditorGUILayout.PropertyField(this.GetProperty(propertyName), new GUIContent(labelName), options);
        }

        protected void DrawMember(string propertyName, GUIContent label, params GUILayoutOption[] options)
        {
            EditorGUILayout.PropertyField(this.GetProperty(propertyName), label, options);
        }

        protected void DrawArray(ArrayDisplayOptions options)
        {
            var arrayProperty = this.GetProperty(options.ArrayMemberName);
            var columnPadding = 3.0f;
            var totalWidth = GetTotalWidth();

            // Drawing the Column Header
            using (new GUILayout.HorizontalScope("box", GUILayout.Width(totalWidth)))
            {
                if (ArrayHeaderStyle == null)
                {
                    ArrayHeaderStyle = new GUIStyle(GUI.skin.label);
                    ArrayHeaderStyle.fontStyle = FontStyle.Bold;
                }

                options.Columns.ForEach(x => GUILayout.Label(x.ColumnName, ArrayHeaderStyle, GUILayout.Width(x.Width + columnPadding)));
            }

            // Drawing each row
            for (int arrayIndex = 0; arrayIndex < arrayProperty.arraySize; arrayIndex++)
            {
                using (new GUILayout.HorizontalScope("box"))
                {
                    foreach (var column in options.Columns)
                    {
                        ArrayDrawMemberNoLabel(options.ArrayMemberName, column.MemberName, arrayIndex, column.Width);
                        GUILayout.Space(columnPadding);
                    }

                    if (options.ShowUpDownButtons)
                    {
                        GUILayout.Space(10);

                        // Down Button
                        if (arrayIndex == arrayProperty.arraySize - 1)
                        {
                            GUILayout.Space(23);
                        }
                        else
                        {
                            if (GUILayout.Button(DownString, GUILayout.Width(20)))
                            {
                                // TODO [bgish]: Implement
                                Logger.LogError("Move Down Not Implemented Yet!");
                            }
                        }

                        // Up Button
                        if (arrayIndex == 0)
                        {
                            GUILayout.Space(23);
                        }
                        else
                        {
                            if (GUILayout.Button(UpString, GUILayout.Width(20)))
                            {
                                // TODO [bgish]: Implement
                                Logger.LogError("Move Up Not Implemented Yet!");
                            }
                        }
                    }

                    // Delete Button
                    if (options.ShowAddDeleteButtons)
                    {
                        GUILayout.Space(10);

                        if (GUILayout.Button("Delete", GUILayout.Width(50)))
                        {
                            arrayProperty.DeleteArrayElementAtIndex(arrayIndex);
                            break;
                        }
                    }
                }
            }

            if (options.ShowAddDeleteButtons && GUILayout.Button("Add", GUILayout.Width(totalWidth)))
            {
                arrayProperty.arraySize++;
            }

            void ArrayDrawMemberNoLabel(string arrayMember, string member, int index, float width)
            {
                this.DrawMember($"{arrayMember}.Array.data[{index}].{member}", GUIContent.none, GUILayout.Width(width));
            }

            float GetTotalWidth()
            {
                float result = options.Columns.Sum(x => x.Width + columnPadding);

                result += (options.ShowUpDownButtons && options.ShowAddDeleteButtons == false) ? 73.0f :
                          (options.ShowAddDeleteButtons && options.ShowUpDownButtons == false) ? 81.0f :
                          (options.ShowAddDeleteButtons && options.ShowUpDownButtons) ? 137.0f : 0.0f;

                return result;
            }
        }

        protected void DrawMember(UnityEngine.Object objectValue, string propertyName, params GUILayoutOption[] options)
        {
            if (SerializedObjectCache.TryGetValue(EntityId.ToULong(objectValue.GetEntityId()), out SerializedObject serializedObject) == false)
            {
                serializedObject = new SerializedObject(objectValue);
                SerializedObjectCache.Add(EntityId.ToULong(objectValue.GetEntityId()), serializedObject);
            }

            var property = serializedObject.FindProperty(propertyName);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(property, options);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtil.SetDirty(objectValue);
            }
        }

        protected void DrawProperty<T>(T objectValue, string propertyName, params GUILayoutOption[] options)
        {
            PropertyEditorDrawerUtil.DrawProperty(objectValue, propertyName, () =>
            {
                if (objectValue is UnityEngine.Object unityObject)
                {
                    EditorUtil.SetDirty(unityObject);
                }
            });
        }

        protected void DrawProperty<T>(T objectValue, string propertyName, string labelName, params GUILayoutOption[] options)
        {
            PropertyEditorDrawerUtil.DrawProperty(objectValue, propertyName, labelName, () =>
            {
                if (objectValue is UnityEngine.Object unityObject)
                {
                    EditorUtil.SetDirty(unityObject);
                }
            });
        }

        protected void DrawProperty<T>(T objectValue, string propertyName, GUIContent label, params GUILayoutOption[] options)
        {
            PropertyEditorDrawerUtil.DrawProperty(objectValue, propertyName, label, () =>
            {
                if (objectValue is UnityEngine.Object unityObject)
                {
                    EditorUtil.SetDirty(unityObject);
                }
            });
        }

        protected void DrawPropertyAsSortingLayer<T>(T objectValue, string propertyName, params GUILayoutOption[] options)
        {
            DrawPropertyAsSortingLayer(objectValue, propertyName, null, options);
        }

        protected void DrawPropertyAsSortingLayer<T>(T objectValue, string propertyName, string labelName, params GUILayoutOption[] options)
        {
            var label = string.IsNullOrEmpty(labelName) ? ObjectNames.NicifyVariableName(propertyName) : labelName;
            var propertyInfo = objectValue.GetType().GetProperty(propertyName);

            if (propertyInfo == null)
            {
                Debug.LogError($"Couldn't find property {propertyName}");
                return;
            }

            int currentLayerId = (int)propertyInfo.GetValue(objectValue);

            var layers = SortingLayer.layers.Select(x => x.name).ToArray();
            var ids = SortingLayer.layers.Select(x => x.id).ToList();
            var currentIndex = ids.IndexOf(currentLayerId);
            var newIndex = EditorGUILayout.Popup(label, currentIndex, layers, options);

            if (currentIndex != newIndex)
            {
                propertyInfo.SetValue(objectValue, newIndex, null);

                // Making sure it's marked as diry if it needs to be
                if (objectValue is UnityEngine.Object unityObject)
                {
                    EditorUtil.SetDirty(unityObject);
                }
            }
        }

        protected SerializedProperty GetProperty(string propertyName)
        {
            if (this.serializedProperties.TryGetValue(propertyName, out SerializedProperty prop) == false)
            {
                prop = this.serializedObject.FindProperty(propertyName);
                this.serializedProperties.Add(propertyName, prop);
            }

            return prop;
        }

        protected abstract void NewOnInspectorGUI();

        protected virtual void OnGUIChanged()
        {
        }

        protected void Foldout(string name, Action action, bool defaultVisible = false)
        {
            long id = HashCode.Combine(this.target.GetEntityId(), name);

            using (new FoldoutScope(id, name, out bool visible, defaultVisible))
            {
                if (visible)
                {
                    using (new IndentLevelScope(1))
                    {
                        action.Invoke();
                    }
                }
            }
        }

        protected void BoxArea(Action action, params GUILayoutOption[] options)
        {
            this.BoxArea(string.Empty, action, options);
        }

        protected void BoxArea(string name, Action action, params GUILayoutOption[] options)
        {
            using (new BoxAreaScope(name, options))
            {
                using (new IndentLevelScope(1))
                {
                    action.Invoke();
                }
            }
        }

        protected void Space(float distance)
        {
            GUILayout.Space(distance);
        }

        protected T GetComponent<T>()
        {
            if (this.target is GameObject gameObject)
            {
                return gameObject.GetComponent<T>();
            }
            else if (this.target is Component component)
            {
                return component.GetComponent<T>();
            }

            return default;
        }
    }

    //// 
    //// Needs to Support Enums (almost there, just need to get indexes and cache values)
    //// Needs to Support Lists
    ////
    public static class PropertyEditorDrawerUtil
    {
        public static void DrawProperty(object objectValue, string propertyName, Action onDirty)
        {
            DrawProperty(objectValue, propertyName, (GUIContent)null, onDirty);
        }

        public static void DrawProperty(object objectValue, string propertyName, string labelName, Action onDirty)
        {
            DrawProperty(objectValue, propertyName, new GUIContent(labelName), onDirty);
        }

        public static void DrawProperty(object objectValue, string propertyName, GUIContent label, Action onDirty)
        {
            PropertyInfo propertyInfo = objectValue.GetType().GetProperty(propertyName);

            if (propertyInfo == null)
            {
                Debug.LogError($"No property called {propertyName} found!", objectValue as UnityEngine.Object);
                return;
            }

            bool isDirty = false;

            DrawPropertyRecursive(objectValue, propertyInfo, propertyInfo.GetValue(objectValue), label, ref isDirty);

            if (isDirty)
            {
                onDirty?.Invoke();
            }
        }

        public static void DrawObject(object objectValue, Action onDirty)
        {
            bool isDirty = false;

            foreach (var propertyInfo in objectValue.GetType().GetProperties())
            {
                DrawPropertyRecursive(objectValue, propertyInfo, propertyInfo.GetValue(objectValue), null, ref isDirty);
            }

            if (isDirty)
            {
                onDirty?.Invoke();
            }
        }

        private static void DrawPropertyRecursive(object propertyParentObject, PropertyInfo propertyInfo, object propertyValue, GUIContent label, ref bool didDataChange)
        {
            // Making sure this property has a value
            if (propertyValue == null && propertyInfo.PropertyType.IsClass && propertyInfo.PropertyType != typeof(string))
            {
                propertyValue = Activator.CreateInstance(propertyInfo.PropertyType);
                propertyInfo.SetValue(propertyParentObject, propertyValue);
                didDataChange = true;
            }

            if (label == null)
            {
                label = new GUIContent(ObjectNames.NicifyVariableName(propertyInfo.Name));
            }

            bool wasHandled = DrawSingleProperty(propertyParentObject, propertyInfo, propertyValue, label, ref didDataChange);

            if (wasHandled)
            {
                return;
            }

            // Go through all children
            try
            {
                EditorGUI.indentLevel++;
                bool show = EditorGUILayout.Foldout(true, ObjectNames.NicifyVariableName(propertyInfo.Name));

                foreach (var childPropertyInfo in propertyInfo.PropertyType.GetProperties())
                {
                    var childPropertyValue = propertyValue != null ? childPropertyInfo.GetValue(propertyValue, null) : null;

                    try
                    {
                        EditorGUI.indentLevel++;
                        DrawPropertyRecursive(propertyValue, childPropertyInfo, childPropertyValue, null, ref didDataChange);
                    }
                    finally
                    {
                        EditorGUI.indentLevel--;
                    }
                }
            }
            finally
            {
                EditorGUI.indentLevel--;
            }
        }

        private static bool DrawSingleProperty(object propertyParentObject, PropertyInfo propertyInfo, object propertyValue, GUIContent label, ref bool didDataChange)
        {
            var propertyType = propertyInfo.PropertyType;

            if (propertyType.Name == "List`1")
            {
                EditorGUI.indentLevel++;
                bool show = EditorGUILayout.Foldout(false, ObjectNames.NicifyVariableName(propertyInfo.Name));
                EditorGUI.indentLevel--;

                // TODO [bgish]: If it's a list, then do something special
                return true;
            }
            else if (propertyType.Name == "Dictionary`2")
            {
                // TODO [bgish]: If it's a dictionary, then do something special
                return true;
            }
            else if (propertyType.IsEnum)
            {
                GetEnumNamesAndValues(propertyType, out string[] enumNames, out List<int> enumValues);

                int currentEnumValue = (int)propertyInfo.GetValue(propertyParentObject);
                int currentIndex = enumValues.IndexOf(currentEnumValue);
                int newIndex = EditorGUILayout.Popup(label, currentIndex, enumNames);

                if (currentIndex != newIndex)
                {
                    propertyInfo.SetValue(propertyParentObject, newIndex, null);
                    didDataChange = true;
                }

                return true;
            }
            else if (DrawPrimitiveAndUnityTypes(propertyParentObject, propertyInfo, propertyType, propertyValue, label, ref didDataChange))
            {
                return true;
            }
            else if (propertyType == typeof(char) || propertyType == typeof(ulong))
            {
                EditorGUILayout.LabelField($"Property {propertyInfo.Name} Not Supported!");
            }

            return false;
        }

        private static bool DrawPrimitiveAndUnityTypes(object propertyParentObject, PropertyInfo propertyInfo, Type propertyType, object propertyValue, GUIContent label, ref bool didDataChange)
        {
            using (new GUILayout.HorizontalScope())
            {
                if (propertyType == typeof(byte))
                {
                    DrawInteger(propertyParentObject, propertyInfo, label, (byte)propertyValue, byte.MinValue, byte.MaxValue, ref didDataChange);
                    return true;
                }
                else if (propertyType == typeof(sbyte))
                {
                    DrawInteger(propertyParentObject, propertyInfo, label, (sbyte)propertyValue, sbyte.MinValue, sbyte.MaxValue, ref didDataChange);
                    return true;
                }
                else if (propertyType == typeof(short))
                {
                    DrawInteger(propertyParentObject, propertyInfo, label, (short)propertyValue, short.MinValue, short.MaxValue, ref didDataChange);
                    return true;
                }
                else if (propertyType == typeof(ushort))
                {
                    DrawInteger(propertyParentObject, propertyInfo, label, (ushort)propertyValue, ushort.MinValue, ushort.MaxValue, ref didDataChange);
                    return true;
                }
                else if (propertyType == typeof(int))
                {
                    DrawInteger(propertyParentObject, propertyInfo, label, (int)propertyValue, int.MinValue, int.MaxValue, ref didDataChange);
                    return true;
                }
                else if (propertyType == typeof(uint))
                {
                    DrawInteger(propertyParentObject, propertyInfo, label, (uint)propertyValue, uint.MinValue, uint.MaxValue, ref didDataChange);
                    return true;
                }
                else if (propertyType == typeof(long))
                {
                    DrawInteger(propertyParentObject, propertyInfo, label, (long)propertyValue, long.MinValue, long.MaxValue, ref didDataChange);
                    return true;
                }
                else if (propertyType == typeof(bool))
                {
                    var oldValue = (bool)propertyValue;
                    var newValue = EditorGUILayout.Toggle(label, oldValue);

                    if (newValue != oldValue)
                    {
                        didDataChange = true;
                        propertyInfo.SetValue(propertyParentObject, newValue);
                    }

                    return true;
                }
                else if (propertyType == typeof(float))
                {
                    var oldValue = (float)propertyValue;
                    var newValue = EditorGUILayout.FloatField(label, oldValue);

                    if (newValue != oldValue)
                    {
                        didDataChange = true;
                        propertyInfo.SetValue(propertyParentObject, newValue);
                    }

                    return true;
                }
                else if (propertyType == typeof(double))
                {
                    var oldValue = (double)propertyValue;
                    var newValue = EditorGUILayout.DoubleField(label, oldValue);

                    if (newValue != oldValue)
                    {
                        didDataChange = true;
                        propertyInfo.SetValue(propertyParentObject, newValue);
                    }

                    return true;
                }
                else if (propertyType == typeof(string))
                {
                    var oldValue = (string)propertyValue;
                    var newValue = EditorGUILayout.TextField(label, oldValue);

                    if (newValue != oldValue)
                    {
                        didDataChange = true;
                        propertyInfo.SetValue(propertyParentObject, newValue);
                    }

                    return true;
                }
                else if (propertyType == typeof(Vector2))
                {
                    var oldValue = (Vector2)propertyValue;
                    var newValue = EditorGUILayout.Vector2Field(label, oldValue);

                    if (newValue != oldValue)
                    {
                        didDataChange = true;
                        propertyInfo.SetValue(propertyParentObject, newValue);
                    }

                    return true;
                }
                else if (propertyType == typeof(Vector3))
                {
                    var oldValue = (Vector3)propertyValue;
                    var newValue = EditorGUILayout.Vector3Field(label, oldValue);

                    if (newValue != oldValue)
                    {
                        didDataChange = true;
                        propertyInfo.SetValue(propertyParentObject, newValue);
                    }

                    return true;
                }
                else if (IsUnityType(propertyType))
                {
                    var oldValue = (UnityEngine.Object)propertyValue;
                    var newValue = EditorGUILayout.ObjectField(label, oldValue, propertyType, allowSceneObjects: false);

                    if (newValue != oldValue)
                    {
                        didDataChange = true;
                        propertyInfo.SetValue(propertyParentObject, newValue);
                    }

                    return true;
                }
            }

            return false;

            static void DrawInteger<T>(object propertyParentObject, PropertyInfo propertyInfo, GUIContent label, T value, T minValue, T maxValue, ref bool didDataChange)
                where T : struct, IConvertible
            {
                var oldValue = Convert.ToInt64(value);
                var newValue = EditorGUILayout.LongField(label, oldValue);
                newValue = Math.Clamp(newValue, Convert.ToInt64(minValue), Convert.ToInt64(maxValue));

                if (newValue != oldValue)
                {
                    didDataChange = true;

                    var type = typeof(T);

                    if (type == typeof(byte)) propertyInfo.SetValue(propertyParentObject, Convert.ToByte(newValue));
                    else if (type == typeof(sbyte)) propertyInfo.SetValue(propertyParentObject, Convert.ToSByte(newValue));
                    else if (type == typeof(short)) propertyInfo.SetValue(propertyParentObject, Convert.ToInt16(newValue));
                    else if (type == typeof(ushort)) propertyInfo.SetValue(propertyParentObject, Convert.ToUInt16(newValue));
                    else if (type == typeof(int)) propertyInfo.SetValue(propertyParentObject, Convert.ToInt32(newValue));
                    else if (type == typeof(uint)) propertyInfo.SetValue(propertyParentObject, Convert.ToUInt32(newValue));
                    else if (type == typeof(long)) propertyInfo.SetValue(propertyParentObject, newValue);
                    else throw new Exception($"Unknown Integer Type {type.Name} found!");
                }
            }
        }

        private static bool IsUnityType(Type type) => typeof(UnityEngine.Object).IsAssignableFrom(type);

        //// TODO [bgish]: Cache These!!!
        private static void GetEnumNamesAndValues(Type propertyType, out string[] enumNames, out List<int> enumValues)
        {
            enumNames = Enum.GetNames(propertyType).Select(x => ObjectNames.NicifyVariableName(x)).ToArray();
            enumValues = new List<int>(enumNames.Length);

            foreach (var enumValue in Enum.GetValues(propertyType))
            {
                enumValues.Add((int)enumValue);
            }
        }
    }
}
