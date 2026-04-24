//-----------------------------------------------------------------------
// <copyright file="SerializedPropertyExtensions.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using OGT;
    using UnityEditor;

    ////
    //// Special thanks for github gist https://gist.github.com/aholkner/214628a05b15f0bb169660945ac7923b
    //// for GetValue, SetValue, SetValueNoRecord and all their private methods/struct.
    ////
    public static class SerializedPropertyExtensions
    {
        private static readonly Regex ArrayElementRegex = new Regex(@"\GArray\.data\[(\d+)\]", RegexOptions.Compiled);
        private static readonly OGTLogger Logger = OGTLogger.OGTEditor;

        public static void AddElementToArray(this SerializedProperty property, object newArrayItem)
        {
            Undo.RecordObject(property.serializedObject.targetObject, $"Add Element {property.name}");

            int newIndex = property.arraySize;
            property.arraySize++;
            property.serializedObject.ApplyModifiedProperties();
            property.GetArrayElementAtIndex(newIndex).SetValueNoRecord(newArrayItem);

            EditorUtil.SetDirty(property.serializedObject.targetObject);
        }

        public static Type GetSerializedPropertyType(this SerializedProperty serializedProperty)
        {
            return GetTypeRecursive(
                serializedProperty.propertyPath.Split('.'),
                0,
                serializedProperty.serializedObject.targetObject.GetType());

            static Type GetTypeRecursive(string[] propertyPath, int currentIndex, Type currentType)
            {
                if (currentIndex < propertyPath.Length)
                {
                    string currentFieldName = propertyPath[currentIndex];

                    FieldInfo currentFieldInfo;

                    do
                    {
                        currentFieldInfo = currentType.GetField(currentFieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                        currentType = currentType.BaseType;
                    }
                    while (currentFieldInfo == null && currentType != null);

                    bool isArray = propertyPath.Length > currentIndex + 1 && propertyPath[currentIndex + 1] == "Array";

                    if (isArray)
                    {
                        Type fieldType = currentFieldInfo.FieldType;
                        Type elementType;

                        if (fieldType.IsArray)
                        {
                            elementType = fieldType.GetElementType();
                        }
                        else
                        {
                            elementType = fieldType.GenericTypeArguments[0];
                        }

                        return GetTypeRecursive(propertyPath, currentIndex + 3, elementType);
                    }
                    else
                    {
                        return GetTypeRecursive(propertyPath, currentIndex + 1, currentFieldInfo.FieldType);
                    }
                }
                else
                {
                    return currentType;
                }
            }
        }

        public static object GetValue(this SerializedProperty property)
        {
            string propertyPath = property.propertyPath;
            object value = property.serializedObject.targetObject;
            int i = 0;

            while (NextPathComponent(propertyPath, ref i, out var token))
            {
                value = GetPathComponentValue(value, token);
            }

            return value;
        }

        public static void SetValue(this SerializedProperty property, object value)
        {
            Undo.RecordObject(property.serializedObject.targetObject, $"Set {property.name}");

            SetValueNoRecord(property, value);

            EditorUtility.SetDirty(property.serializedObject.targetObject);
            property.serializedObject.ApplyModifiedProperties();
        }

        public static void SetValueNoRecord(this SerializedProperty property, object value)
        {
            string propertyPath = property.propertyPath;
            object container = property.serializedObject.targetObject;
            int i = 0;

            NextPathComponent(propertyPath, ref i, out var deferredToken);

            while (NextPathComponent(propertyPath, ref i, out var token))
            {
                container = GetPathComponentValue(container, deferredToken);
                deferredToken = token;
            }

            Logger.Assert(!container.GetType().IsValueType, $"Cannot use SerializedObject.SetValue on a struct object, as the result will be set on a temporary.  Either change {container.GetType().Name} to a class, or use SetValue with a parent member.");
            SetPathComponentValue(container, deferredToken, value);
        }

        private static bool NextPathComponent(string propertyPath, ref int index, out PropertyPathComponent component)
        {
            component = new PropertyPathComponent();

            if (index >= propertyPath.Length)
            {
                return false;
            }

            var arrayElementMatch = ArrayElementRegex.Match(propertyPath, index);
            if (arrayElementMatch.Success)
            {
                index += arrayElementMatch.Length + 1; // Skip past next '.'
                component.ElementIndex = int.Parse(arrayElementMatch.Groups[1].Value);
                return true;
            }

            int dot = propertyPath.IndexOf('.', index);
            if (dot == -1)
            {
                component.PropertyName = propertyPath.Substring(index);
                index = propertyPath.Length;
            }
            else
            {
                component.PropertyName = propertyPath.Substring(index, dot - index);
                index = dot + 1; // Skip past next '.'
            }

            return true;
        }

        private static object GetPathComponentValue(object container, PropertyPathComponent component)
        {
            if (component.PropertyName == null)
            {
                return ((IList)container)[component.ElementIndex];
            }
            else
            {
                return GetMemberValue(container, component.PropertyName);
            }
        }

        private static void SetPathComponentValue(object container, PropertyPathComponent component, object value)
        {
            if (component.PropertyName == null)
            {
                ((IList)container)[component.ElementIndex] = value;
            }
            else
            {
                SetMemberValue(container, component.PropertyName, value);
            }
        }

        private static object GetMemberValue(object container, string name)
        {
            if (container == null)
            {
                return null;
            }

            var type = container.GetType();
            var members = type.GetMember(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < members.Length; ++i)
            {
                if (members[i] is FieldInfo field)
                {
                    return field.GetValue(container);
                }
                else if (members[i] is PropertyInfo property)
                {
                    return property.GetValue(container);
                }
            }

            return null;
        }

        private static void SetMemberValue(object container, string name, object value)
        {
            var type = container.GetType();
            var members = type.GetMember(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < members.Length; ++i)
            {
                if (members[i] is FieldInfo field)
                {
                    field.SetValue(container, value);
                    return;
                }
                else if (members[i] is PropertyInfo property)
                {
                    property.SetValue(container, value);
                    return;
                }
            }

            Logger.Assert(false, $"Failed to set member {container}.{name} via reflection");
        }

        private struct PropertyPathComponent
        {
            public string PropertyName;
            public int ElementIndex;
        }
    }
}
