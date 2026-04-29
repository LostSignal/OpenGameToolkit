// Whenever a property is set, mark the properties as dirty so they get saved to disk
// Whenever a property is set, fire off an event so that the UI can update
// Use uint instead of int for property IDs
// SaveToDisk(PropertyBagTyepe type) to save only a specific type of property bag
// Make sure not to fire off events if the data didn't actually change
// Durring initialization, make sure actions exists for all properties
// Nake a property manager class that holds a list of Property objects and makes sure they are all initialized on startup

// Property Manager
//   - Needs to run validation on startup to make sure all property IDs are unique across all property SOs

// Should i removed the idea of a property bag and just have every scriptable object be the bag?
// - That would mean the PropertyManager will hold a bunch of these scriptable objects
// - PropertyManager.SaveDeviceProperties() would go through all the scriptable objects of type device and save them
// - OGT Device Settings
//    - App.OpenCount
//    - App.LastOpenedDateTime
//    - App.LastOpenedVersion
//    - App.ShowWelcomeScreen
//    - Audio.Music.IsMuted
//    - Audio.Music.Volume
//    - Audio.SFX.IsMuted
//    - Audio.SFX.Volume
// - OGT Profile Settings
//   -

namespace OGT.Properties
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Properties", menuName = "ScriptableObjects/Properties", order = 1)]
    public class Properties : ScriptableObject
    {
        private enum PropertiesType
        {
            Device,
            Profile,
            Game,
        }

        [SerializeField] private PropertiesType type;
        [SerializeReference] private List<Property> properties;

        private Dictionary<int, Property> propertyCache = new();

        public string[] GetPropertyNames(Type type)
        {
            type = type == typeof(bool) ? typeof(BoolProperty) :
                type == typeof(int) ? typeof(IntProperty) :
                type == typeof(float) ? typeof(FloatProperty) :
                type == typeof(string) ? typeof(StringProperty) : null;

            if (type == null)
            {
                throw new ArgumentException($"Unsupported property type: {type}");
            }

            return this.properties
                .Where(p => p.GetType() == type)
                .Select(p => p.Name)
                .OrderBy(name => name)
                .ToArray();
        }

        public void ResetProperties()
        {
            this.propertyCache.Clear();

            foreach (var prop in this.properties)
            {
                prop.Reset();
            }
        }

        // Populate the PropertyCache for quick lookup
        public void Initialize()
        {
            if (this.propertyCache.Count > 0)
            {
                return; // Already populated
            }

            foreach (var prop in this.properties)
            {
                prop.Reset();

                if (propertyCache.ContainsKey(prop.Id) == false)
                {
                    propertyCache[prop.Id] = prop;
                }
                else
                {
                    throw new Exception($"Duplicate property ID {prop.Id} found in Properties ScriptableObject {this.name}. Each property ID must be unique.");
                }
            }
        }

        public string GetPropertyNameById(int propertyId)
        {
            this.Initialize();

            if (propertyCache.TryGetValue(propertyId, out var prop))
            {
                return prop.Name;
            }

            return null;
        }

        public int GetPropertyIdByName(string propertyName)
        {
            foreach (var prop in this.properties)
            {
                if (prop.Name == propertyName)
                {
                    return prop.Id;
                }
            }

            throw new KeyNotFoundException($"Property with name {propertyName} not found.");
        }

        //// ---------------------- Getters and Setters for properties ----------------------

        public bool GetBoolPropertyValue(int propertyId)
        {
            this.Initialize();

            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is BoolProperty boolProp)
            {
                return boolProp.CurrentValue;
            }

            throw new KeyNotFoundException($"Bool property with ID {propertyId} not found.");
        }

        public void SetBoolPropertyValue(int propertyId, bool value)
        {
            this.Initialize();

            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is BoolProperty boolProp)
            {
                var oldValue = boolProp.CurrentValue;
                boolProp.CurrentValue = value;
                boolProp.OnChange?.Invoke(oldValue, boolProp.CurrentValue);
            }
            else
            {
                throw new KeyNotFoundException($"Bool property with ID {propertyId} not found.");
            }
        }

        public int GetIntPropertyValue(int propertyId)
        {
            this.Initialize();

            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is IntProperty intProp)
            {
                return intProp.CurrentValue;
            }

            throw new KeyNotFoundException($"Int property with ID {propertyId} not found.");
        }

        public void SetIntPropertyValue(int propertyId, int value)
        {
            this.Initialize();

            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is IntProperty intProp)
            {
                if (value < intProp.Min || value > intProp.Max)
                {
                    // LOGGER Print Warning about clamping
                    throw new ArgumentOutOfRangeException($"Value {value} is out of range for property ID {propertyId}.");
                }

                var oldValue = intProp.CurrentValue;
                intProp.CurrentValue = Math.Clamp(value, intProp.Min, intProp.Max);
                intProp.OnChange?.Invoke(oldValue, intProp.CurrentValue);
            }
            else
            {
                throw new KeyNotFoundException($"Int property with ID {propertyId} not found.");
            }
        }

        public float GetFloatPropertyValue(int propertyId)
        {
            this.Initialize();

            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is FloatProperty floatProp)
            {
                return floatProp.CurrentValue;
            }

            throw new KeyNotFoundException($"Float property with ID {propertyId} not found.");
        }

        public float GetFloatPropertyMin(int propertyId)
        {
            this.Initialize();

            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is FloatProperty floatProp)
            {
                return floatProp.Min;
            }

            throw new KeyNotFoundException($"Float property with ID {propertyId} not found.");
        }

        public float GetFloatPropertyMax(int propertyId)
        {
            this.Initialize();

            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is FloatProperty floatProp)
            {
                return floatProp.Max;
            }

            throw new KeyNotFoundException($"Float property with ID {propertyId} not found.");
        }

        public void SetFloatPropertyValue(int propertyId, float value)
        {
            this.Initialize();

            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is FloatProperty floatProp)
            {
                if (value < floatProp.Min || value > floatProp.Max)
                {
                    throw new ArgumentOutOfRangeException($"Value {value} is out of range for property ID {propertyId}.");
                }

                var oldValue = floatProp.CurrentValue;
                floatProp.CurrentValue = value;
                floatProp.OnChange?.Invoke(oldValue, floatProp.CurrentValue);
            }
            else
            {
                throw new KeyNotFoundException($"Float property with ID {propertyId} not found.");
            }
        }

        public EnumValue GetEnumPropertyValue(int propertyId)
        {
            this.Initialize();

            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is EnumProperty enumProperty)
            {
                return enumProperty.EnumType.EnumValues[enumProperty.CurrentIndex];
            }

            throw new KeyNotFoundException($"Enum property with ID {propertyId} not found.");
        }

        public int GetEnumPropertyIndex(int propertyId)
        {
            this.Initialize();

            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is EnumProperty enumProperty)
            {
                return enumProperty.CurrentIndex;
            }

            throw new KeyNotFoundException($"Enum property with ID {propertyId} not found.");
        }

        public void SetEnumPropertyValue(int propertyId, EnumValue value)
        {
            this.Initialize();

            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is EnumProperty enumProp)
            {
                int newIndex = enumProp.EnumType.IndexOf(value);

                if (newIndex < 0)
                {
                    throw new ArgumentOutOfRangeException($"Value '{value.Name}' does not belong to Enum {enumProp.EnumType.name}.");
                }

                var oldIndex = enumProp.CurrentIndex;

                if (newIndex != oldIndex)
                {
                    enumProp.CurrentIndex = newIndex;
                    enumProp.OnChange?.Invoke(enumProp.EnumType.EnumValues[oldIndex], enumProp.EnumType.EnumValues[newIndex]);
                }
            }
            else
            {
                throw new KeyNotFoundException($"Enum property with ID {propertyId} not found.");
            }
        }

        public string GetStringPropertyValue(int propertyId)
        {
            this.Initialize();

            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is StringProperty stringProp)
            {
                return stringProp.CurrentValue;
            }

            throw new KeyNotFoundException($"String property with ID {propertyId} not found.");
        }

        public void SetStringPropertyValue(int propertyId, string value)
        {
            this.Initialize();

            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is StringProperty stringProp)
            {
                var oldValue = stringProp.CurrentValue;
                stringProp.CurrentValue = value;
                stringProp.OnChange?.Invoke(oldValue, stringProp.CurrentValue);
            }
            else
            {
                throw new KeyNotFoundException($"String property with ID {propertyId} not found.");
            }
        }

        //// ---------------------- Event Handlers for property changes ----------------------

        public void AddBoolHandler(int propertyId, Action<bool, bool> action)
        {
            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is BoolProperty boolProp)
            {
                boolProp.OnChange += action;
            }
            else
            {
                throw new Exception($"Bool property with ID {propertyId} not found.");
            }
        }

        public void RemoveBoolHandler(int propertyId, Action<bool, bool> action)
        {
            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is BoolProperty boolProp)
            {
                boolProp.OnChange -= action;
            }
            else
            {
                throw new Exception($"Bool property with ID {propertyId} not found.");
            }
        }

        public void AddIntHandler(int propertyId, Action<int, int> action)
        {
            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is IntProperty intProp)
            {
                intProp.OnChange += action;
            }
            else
            {
                throw new Exception($"Int property with ID {propertyId} not found.");
            }
        }

        public void RemoveIntHandler(int propertyId, Action<int, int> action)
        {
            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is IntProperty intProp)
            {
                intProp.OnChange -= action;
            }
            else
            {
                throw new Exception($"Int property with ID {propertyId} not found.");
            }
        }

        public void AddFloatHandler(int propertyId, Action<float, float> action)
        {
            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is FloatProperty floatProp)
            {
                floatProp.OnChange += action;
            }
            else
            {
                throw new Exception($"Float property with ID {propertyId} not found.");
            }
        }

        public void RemoveFloatHandler(int propertyId, Action<float, float> action)
        {
            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is FloatProperty floatProp)
            {
                floatProp.OnChange -= action;
            }
            else
            {
                throw new Exception($"Float property with ID {propertyId} not found.");
            }
        }

        public void AddStringHandler(int propertyId, Action<string, string> action)
        {
            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is StringProperty stringProp)
            {
                stringProp.OnChange += action;
            }
            else
            {
                throw new Exception($"String property with ID {propertyId} not found.");
            }
        }

        public void RemoveStringHandler(int propertyId, Action<string, string> action)
        {
            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is StringProperty stringProp)
            {
                stringProp.OnChange -= action;
            }
            else
            {
                throw new Exception($"String property with ID {propertyId} not found.");
            }
        }

        public void AddEnumHandler(int propertyId, Action<EnumValue, EnumValue> action)
        {
            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is EnumProperty enumProp)
            {
                enumProp.OnChange += action;
            }
            else
            {
                throw new Exception($"Enum property with ID {propertyId} not found.");
            }
        }

        public void RemoveEnumHandler(int propertyId, Action<EnumValue, EnumValue> action)
        {
            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is EnumProperty enumProp)
            {
                enumProp.OnChange -= action;
            }
            else
            {
                throw new Exception($"Enum property with ID {propertyId} not found.");
            }
        }

        //// ---------------------- Utilities ----------------------

        internal T AddProperty<T>()
            where T : Property, new()
        {
            this.properties.Add(new T()
            {
                Id = this.properties.Count == 0 ? 1 : this.properties.Max(x => x.Id) + 1
            });

            return this.properties.Last() as T;
        }

        //// ---------------------- Types ----------------------

        [Serializable]
        internal abstract class Property
        {
            [SerializeField] private int id;
            [SerializeField] private string name;

            public int Id { get => this.id; set => this.id = value; }

            public string Name { get => this.name; set => this.name = value; }

            public abstract void Reset();
        }

        [Serializable]
        internal abstract class Property<T> : Property
        {
            [SerializeField] private T defaultValue;
            [SerializeField] private T currentValue;

            public T DefaultValue { get => defaultValue; set => defaultValue = value; }
            public T CurrentValue { get => currentValue; set => this.currentValue = value; }
            public Action<T, T> OnChange;

            public override void Reset()
            {
                this.currentValue = this.defaultValue;
                this.OnChange = null;
            }
        }

        [Serializable]
        internal abstract class NumberProperty<T> : Property<T>
        {
            [SerializeField] private T min;
            [SerializeField] private T max;

            public T Min { get => min; set => min = value; }
            public T Max { get => max; set => max = value; }
        }

        [Serializable]
        internal class BoolProperty : Property<bool>
        {
        }

        [Serializable]
        internal class IntProperty : NumberProperty<int>
        {
        }

        [Serializable]
        internal class StringProperty : Property<string>
        {
        }

        [Serializable]
        internal class FloatProperty : NumberProperty<float>
        {
        }

        [Serializable]
        internal class EnumProperty : Property
        {
            [SerializeField] private int defaultIndex;
            [SerializeField] private int currentIndex;
            [SerializeField] private Enum enumType;

            public Enum EnumType => enumType;

            public int DefaultIndex { get => defaultIndex; set => defaultIndex = value; }
            public int CurrentIndex { get => currentIndex; set => this.currentIndex = value; }

            public Action<EnumValue, EnumValue> OnChange;

            public override void Reset()
            {
                this.currentIndex = this.defaultIndex;
                this.OnChange = null;
            }
        }

        //// ---------------------- Editor ----------------------

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(Properties))]
        internal class PropertiesEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                using (new UnityEditor.EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Bool"))
                    {
                        (this.target as Properties).AddProperty<BoolProperty>();
                        UnityEditor.EditorUtility.SetDirty(this);
                    }

                    if (GUILayout.Button("Int"))
                    {
                        var property = (this.target as Properties).AddProperty<IntProperty>();
                        property.Min = int.MinValue;
                        property.Max = int.MaxValue;
                        UnityEditor.EditorUtility.SetDirty(this);
                    }

                    if (GUILayout.Button("Float"))
                    {
                        var property = (this.target as Properties).AddProperty<FloatProperty>();
                        property.Min = float.MinValue;
                        property.Max = float.MaxValue;
                        UnityEditor.EditorUtility.SetDirty(this);
                    }

                    if (GUILayout.Button("String"))
                    {
                        (this.target as Properties).AddProperty<StringProperty>();
                        UnityEditor.EditorUtility.SetDirty(this);
                    }

                    if (GUILayout.Button("Enum"))
                    {
                        (this.target as Properties).AddProperty<EnumProperty>();
                        UnityEditor.EditorUtility.SetDirty(this);
                    }
                }
            }
        }

        [UnityEditor.CustomPropertyDrawer(typeof(BoolProperty))]
        [UnityEditor.CustomPropertyDrawer(typeof(IntProperty))]
        [UnityEditor.CustomPropertyDrawer(typeof(FloatProperty))]
        [UnityEditor.CustomPropertyDrawer(typeof(StringProperty))]
        [UnityEditor.CustomPropertyDrawer(typeof(EnumProperty))]
        private class PropertyPropertyDrawer : UnityEditor.PropertyDrawer
        {
            private static readonly Dictionary<string, string> typeNameCache = new();
            private static readonly Dictionary<ulong, string[]> enumNamesCache = new();

            public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
            {
                bool isIntOrFloatProperty = GetTypeName(property.type) == nameof(IntProperty) || GetTypeName(property.type) == nameof(FloatProperty);
                bool isEnumProperty = GetTypeName(property.type) == nameof(EnumProperty);

                // Don't make child fields be indented
                var indent = UnityEditor.EditorGUI.indentLevel;
                UnityEditor.EditorGUI.indentLevel = 0;

                UnityEditor.EditorGUI.BeginProperty(position, label, property);

                float rowHeight = base.GetPropertyHeight(property, label);
                position.height = rowHeight;

                // Draw foldout for int, float, and enum properties
                if (isIntOrFloatProperty || isEnumProperty)
                {
                    position.x += 5;
                    property.isExpanded = UnityEditor.EditorGUI.Foldout(position, property.isExpanded, GUIContent.none);
                    position.x += 5;
                }
                else
                {
                    position.x += 10;
                }

                // Draw label
                position = UnityEditor.EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), GUIContent.none);
                float totalWidth = position.width;
                float leftSideWidth = totalWidth * 0.55f;
                float rightSideWidth = totalWidth - leftSideWidth;

                // Calculate Rects
                var column0Rect = new Rect(position.x + 0, position.y, 40, rowHeight);
                var column1Rect = new Rect(column0Rect.x + 40, position.y, leftSideWidth - 45, rowHeight);
                var column2Rect = new Rect(column1Rect.x + (leftSideWidth - 40), position.y, 45, rowHeight);
                var column3Rect = new Rect(column2Rect.x + 50, position.y, rightSideWidth - 60, rowHeight);

                DrawNameAndDefault();

                // Move down to next row
                column0Rect.y += rowHeight + 2;
                column1Rect.y += rowHeight + 2;
                column2Rect.y += rowHeight + 2;
                column3Rect.y += rowHeight + 2;

                if (isIntOrFloatProperty)
                {
                    DrawMinMax();
                }
                else if (isEnumProperty)
                {
                    DrawEnum();
                }

                // Set indent back to what it was
                UnityEditor.EditorGUI.indentLevel = indent;
                UnityEditor.EditorGUI.EndProperty();

                void DrawNameAndDefault()
                {
                    UnityEditor.EditorGUI.LabelField(column0Rect, GetTypeName(property.type).Replace("Property", string.Empty));
                    UnityEditor.EditorGUI.PropertyField(column1Rect, property.FindPropertyRelative("name"), GUIContent.none);
                    UnityEditor.EditorGUI.LabelField(column2Rect, "Default");

                    if (isEnumProperty)
                    {
                        var enumType = property.FindPropertyRelative("enumType").objectReferenceValue as Enum;

                        if (enumType == null)
                        {
                            UnityEditor.EditorGUI.HelpBox(column3Rect, "Assign Enum Type", UnityEditor.MessageType.Warning);
                        }
                        else
                        {
                            int index = property.FindPropertyRelative("defaultIndex").intValue;
                            int newIndex = UnityEditor.EditorGUI.Popup(column3Rect, index, GetEnumNames(enumType));

                            if (index != newIndex && newIndex >= 0)
                            {
                                property.FindPropertyRelative("defaultIndex").intValue = newIndex;
                            }
                        }
                    }
                    else
                    {
                        UnityEditor.EditorGUI.PropertyField(column3Rect, property.FindPropertyRelative("defaultValue"), GUIContent.none);
                    }
                }

                void DrawMinMax()
                {
                    if (property.isExpanded == false)
                    {
                        return;
                    }

                    UnityEditor.EditorGUI.LabelField(column0Rect, "Min");
                    UnityEditor.EditorGUI.PropertyField(column1Rect, property.FindPropertyRelative("min"), GUIContent.none);
                    UnityEditor.EditorGUI.LabelField(column2Rect, "Max");
                    UnityEditor.EditorGUI.PropertyField(column3Rect, property.FindPropertyRelative("max"), GUIContent.none);
                }

                void DrawEnum()
                {
                    if (property.isExpanded == false)
                    {
                        return;
                    }

                    UnityEditor.EditorGUI.LabelField(column0Rect, "Enum");
                    UnityEditor.EditorGUI.PropertyField(column1Rect, property.FindPropertyRelative("enumType"), GUIContent.none);
                }
            }

            public override float GetPropertyHeight(UnityEditor.SerializedProperty property, GUIContent label)
            {
                return base.GetPropertyHeight(property, label) * (property.isExpanded ? 2 : 1) + 2;
            }

            private static string GetTypeName(string fullTypeName)
            {
                if (typeNameCache.TryGetValue(fullTypeName, out var typeName) == false)
                {
                    typeName = fullTypeName.Replace("managedReference<", string.Empty).Replace(">", string.Empty);
                    typeNameCache.Add(fullTypeName, typeName);
                }

                return typeName;
            }

            private static string[] GetEnumNames(Enum enumType)
            {
                if (enumType == null)
                {
                    return Array.Empty<string>();
                }

                if (enumNamesCache.TryGetValue(EntityId.ToULong(enumType.GetEntityId()), out var enumNames) == false)
                {
                    enumNames = enumType.EnumValues.Select(x => x.Name).ToArray();
                    enumNamesCache.Add(EntityId.ToULong(enumType.GetEntityId()), enumNames);
                }

                return enumNames;
            }
        }
#endif
    }

    [Serializable]
    public abstract class Property
    {
        [SerializeField][JsonProperty] private Properties properties;
        [SerializeField][JsonProperty] private int propertyId;

        [JsonIgnore]
        public string Name => properties?.GetPropertyNameById(propertyId);

        [JsonIgnore]
        public abstract Type Type { get; }

        [JsonIgnore]
        public Properties Properties
        {
            get => properties;
            set => properties = value;
        }

        [JsonIgnore]
        public int PropertyId
        {
            get => propertyId;
            set => propertyId = value;
        }
    }

    [Serializable]
    public class BoolProperty : Property
    {
        [JsonIgnore]
        public override Type Type => typeof(bool);

        [JsonIgnore]
        public bool Value
        {
            get => this.Properties.GetBoolPropertyValue(this.PropertyId);
            set => this.Properties.SetBoolPropertyValue(this.PropertyId, value);
        }

        public event Action<bool, bool> OnChange
        {
            add => this.Properties.AddBoolHandler(this.PropertyId, value);
            remove => this.Properties.RemoveBoolHandler(this.PropertyId, value);
        }
    }

    [Serializable]
    public class IntProperty : Property
    {
        public override Type Type => typeof(int);

        public int Value
        {
            get => this.Properties.GetIntPropertyValue(this.PropertyId);
            set => this.Properties.SetIntPropertyValue(this.PropertyId, value);
        }

        public event Action<int, int> OnChange
        {
            add => this.Properties.AddIntHandler(this.PropertyId, value);
            remove => this.Properties.RemoveIntHandler(this.PropertyId, value);
        }
    }

    [Serializable]
    public class StringProperty : Property
    {
        public override Type Type => typeof(string);

        public string Value
        {
            get => this.Properties.GetStringPropertyValue(this.PropertyId);
            set => this.Properties.SetStringPropertyValue(this.PropertyId, value);
        }

        public event Action<string, string> OnChange
        {
            add => this.Properties.AddStringHandler(this.PropertyId, value);
            remove => this.Properties.RemoveStringHandler(this.PropertyId, value);
        }
    }

    [Serializable]
    public class FloatProperty : Property
    {
        public override Type Type => typeof(float);

        public float Value
        {
            get => this.Properties.GetFloatPropertyValue(this.PropertyId);
            set => this.Properties.SetFloatPropertyValue(this.PropertyId, value);
        }

        public float Min => this.Properties.GetFloatPropertyMin(this.PropertyId);

        public float Max => this.Properties.GetFloatPropertyMax(this.PropertyId);

        public event Action<float, float> OnChange
        {
            add => this.Properties.AddFloatHandler(this.PropertyId, value);
            remove => this.Properties.RemoveFloatHandler(this.PropertyId, value);
        }
    }

    [Serializable]
    public class EnumProperty : Property
    {
        public override Type Type => typeof(Enum);

        public EnumValue Value
        {
            get => this.Properties.GetEnumPropertyValue(this.PropertyId);
            set => this.Properties.SetEnumPropertyValue(this.PropertyId, value);
        }

        public int CurrentValueIndex
        {
            get => this.Properties.GetEnumPropertyIndex(this.PropertyId);
        }

        public event Action<EnumValue, EnumValue> OnChange
        {
            add => this.Properties.AddEnumHandler(this.PropertyId, value);
            remove => this.Properties.RemoveEnumHandler(this.PropertyId, value);
        }
    }
}
