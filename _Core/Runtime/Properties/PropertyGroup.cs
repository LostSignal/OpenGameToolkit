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
    using UnityEngine.Serialization;

    [CreateAssetMenu(fileName = "Property Group", menuName = "OGT/Properties/Property Group", order = 1)]
    public class PropertyGroup : Resource
    {
        private enum PropertyType
        {
            Device,
            Profile,
            Game,
        }

        [ReadOnly]
        [SerializeField] private int groupId;
        [SerializeField] private PropertyType type;
        [SerializeReference] private List<Property> properties;

        private Dictionary<int, Property> propertyCache = new();

        public int GroupId => this.groupId;

        public string[] GetPropertyNames(Type type)
        {
            return this.GetProperties(type)
                .Select(p => p.Name)
                .OrderBy(name => name)
                .ToArray();
        }

        // Returns every property in this group whose value type matches (bool, int, float, string or Enum)
        internal IEnumerable<Property> GetProperties(Type valueType)
        {
            var propertyType = valueType == typeof(bool) ? typeof(BoolProperty) :
                valueType == typeof(int) ? typeof(IntProperty) :
                valueType == typeof(float) ? typeof(FloatProperty) :
                valueType == typeof(string) ? typeof(StringProperty) :
                valueType == typeof(Enum) ? typeof(EnumProperty) : null;

            if (propertyType == null)
            {
                throw new ArgumentException($"Unsupported property type: {valueType}");
            }

            return (this.properties ?? Enumerable.Empty<Property>())
                .Where(p => p != null && p.GetType() == propertyType);
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

        public int GetIntPropertyMinValue(int propertyId)
        {
            this.Initialize();

            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is IntProperty intProp)
            {
                return intProp.Min;
            }

            throw new KeyNotFoundException($"Int property with ID {propertyId} not found.");
        }

        public int GetIntPropertyMaxValue(int propertyId)
        {
            this.Initialize();

            if (propertyCache.TryGetValue(propertyId, out var prop) && prop is IntProperty intProp)
            {
                return intProp.Max;
            }

            throw new KeyNotFoundException($"Int property with ID {propertyId} not found.");
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
            this.Initialize();

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
            this.Initialize();

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
            this.Initialize();

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
            this.Initialize();

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
            this.Initialize();

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
            this.Initialize();

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
            this.Initialize();

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
            this.Initialize();

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
            this.Initialize();

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
            this.Initialize();

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

    }

    [Serializable]
    public abstract class Property
    {
        [FormerlySerializedAs("properties")]
        [SerializeField][JsonProperty] private PropertyGroup propertyGroup;
        [SerializeField][JsonProperty] private int propertyId;

        [JsonIgnore]
        public string Name => propertyGroup?.GetPropertyNameById(propertyId);

        [JsonIgnore]
        public abstract Type Type { get; }

        [JsonIgnore]
        public PropertyGroup PropertyGroup
        {
            get => propertyGroup;
            set => propertyGroup = value;
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
            get => this.PropertyGroup.GetBoolPropertyValue(this.PropertyId);
            set => this.PropertyGroup.SetBoolPropertyValue(this.PropertyId, value);
        }

        public event Action<bool, bool> OnChange
        {
            add => this.PropertyGroup.AddBoolHandler(this.PropertyId, value);
            remove => this.PropertyGroup.RemoveBoolHandler(this.PropertyId, value);
        }
    }

    [Serializable]
    public class IntProperty : Property
    {
        public override Type Type => typeof(int);

        public int Value
        {
            get => this.PropertyGroup.GetIntPropertyValue(this.PropertyId);
            set => this.PropertyGroup.SetIntPropertyValue(this.PropertyId, value);
        }

        public int Min => this.PropertyGroup.GetIntPropertyMinValue(this.PropertyId);

        public int Max => this.PropertyGroup.GetIntPropertyMaxValue(this.PropertyId);

        public event Action<int, int> OnChange
        {
            add => this.PropertyGroup.AddIntHandler(this.PropertyId, value);
            remove => this.PropertyGroup.RemoveIntHandler(this.PropertyId, value);
        }
    }

    [Serializable]
    public class StringProperty : Property
    {
        public override Type Type => typeof(string);

        public string Value
        {
            get => this.PropertyGroup.GetStringPropertyValue(this.PropertyId);
            set => this.PropertyGroup.SetStringPropertyValue(this.PropertyId, value);
        }

        public event Action<string, string> OnChange
        {
            add => this.PropertyGroup.AddStringHandler(this.PropertyId, value);
            remove => this.PropertyGroup.RemoveStringHandler(this.PropertyId, value);
        }
    }

    [Serializable]
    public class FloatProperty : Property
    {
        public override Type Type => typeof(float);

        public float Value
        {
            get => this.PropertyGroup.GetFloatPropertyValue(this.PropertyId);
            set => this.PropertyGroup.SetFloatPropertyValue(this.PropertyId, value);
        }

        public float Min => this.PropertyGroup.GetFloatPropertyMin(this.PropertyId);

        public float Max => this.PropertyGroup.GetFloatPropertyMax(this.PropertyId);

        public event Action<float, float> OnChange
        {
            add => this.PropertyGroup.AddFloatHandler(this.PropertyId, value);
            remove => this.PropertyGroup.RemoveFloatHandler(this.PropertyId, value);
        }
    }

    [Serializable]
    public class EnumProperty : Property
    {
        public override Type Type => typeof(Enum);

        public EnumValue Value
        {
            get => this.PropertyGroup.GetEnumPropertyValue(this.PropertyId);
            set => this.PropertyGroup.SetEnumPropertyValue(this.PropertyId, value);
        }

        public int CurrentValueIndex
        {
            get => this.PropertyGroup.GetEnumPropertyIndex(this.PropertyId);
        }

        public event Action<EnumValue, EnumValue> OnChange
        {
            add => this.PropertyGroup.AddEnumHandler(this.PropertyId, value);
            remove => this.PropertyGroup.RemoveEnumHandler(this.PropertyId, value);
        }
    }
}
