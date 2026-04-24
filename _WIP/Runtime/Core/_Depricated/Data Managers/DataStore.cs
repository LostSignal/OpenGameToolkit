//-----------------------------------------------------------------------
// <copyright file="Data.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    [Serializable]
    public class DataStore
    {
        private const uint CurrentVersion = 1;

        private static readonly Networking.NetworkWriter Writer = new ((byte[])null);
        private static readonly Networking.NetworkReader Reader = new ((byte[])null);
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        private Dictionary<string, int> intData;
        private Dictionary<string, int> enumData;
        private Dictionary<string, bool> boolData;
        private Dictionary<string, long> longData;
        private Dictionary<string, float> floatData;
        private Dictionary<string, double> doubleData;
        private Dictionary<string, string> stringData;
        private Dictionary<string, byte[]> byteArrayData;
        private Dictionary<string, DateTime> dateTimeData;

        private bool isDirty;

        private Action<string> onDataStoreKeyChanged;
        private Action onDataStoreChanged;

        public event Action<string> OnDataStoreKeyChanged
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            add => this.onDataStoreKeyChanged += value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            remove => this.onDataStoreKeyChanged -= value;
        }

        public event Action OnDataStoreChanged
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            add => this.onDataStoreChanged += value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            remove => this.onDataStoreChanged -= value;
        }

        public bool IsDirty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.isDirty;
        }

        public int Serialize(byte[] saveDataBuffer)
        {
            this.isDirty = false;

            Writer.ResetBuffer(saveDataBuffer);
            Writer.WritePackedUInt32(CurrentVersion);
            Writer.WritePackedUInt32(this.intData == null ? 0 : (uint)this.intData.Count);
            Writer.WritePackedUInt32(this.enumData == null ? 0 : (uint)this.enumData.Count);
            Writer.WritePackedUInt32(this.boolData == null ? 0 : (uint)this.boolData.Count);
            Writer.WritePackedUInt32(this.longData == null ? 0 : (uint)this.longData.Count);
            Writer.WritePackedUInt32(this.floatData == null ? 0 : (uint)this.floatData.Count);
            Writer.WritePackedUInt32(this.doubleData == null ? 0 : (uint)this.doubleData.Count);
            Writer.WritePackedUInt32(this.stringData == null ? 0 : (uint)this.stringData.Count);
            Writer.WritePackedUInt32(this.byteArrayData == null ? 0 : (uint)this.byteArrayData.Count);
            Writer.WritePackedUInt32(this.dateTimeData == null ? 0 : (uint)this.dateTimeData.Count);

            if (this.intData?.Count > 0)
            {
                foreach (var keyValuePair in this.intData)
                {
                    Writer.Write(keyValuePair.Key);
                    Writer.Write(keyValuePair.Value);
                }
            }

            if (this.enumData?.Count > 0)
            {
                foreach (var keyValuePair in this.enumData)
                {
                    Writer.Write(keyValuePair.Key);
                    Writer.Write(keyValuePair.Value);
                }
            }

            if (this.boolData?.Count > 0)
            {
                foreach (var keyValuePair in this.boolData)
                {
                    Writer.Write(keyValuePair.Key);
                    Writer.Write(keyValuePair.Value);
                }
            }

            if (this.longData?.Count > 0)
            {
                foreach (var keyValuePair in this.longData)
                {
                    Writer.Write(keyValuePair.Key);
                    Writer.Write(keyValuePair.Value);
                }
            }

            if (this.floatData?.Count > 0)
            {
                foreach (var keyValuePair in this.floatData)
                {
                    Writer.Write(keyValuePair.Key);
                    Writer.Write(keyValuePair.Value);
                }
            }

            if (this.doubleData?.Count > 0)
            {
                foreach (var keyValuePair in this.doubleData)
                {
                    Writer.Write(keyValuePair.Key);
                    Writer.Write(keyValuePair.Value);
                }
            }

            if (this.stringData?.Count > 0)
            {
                foreach (var keyValuePair in this.stringData)
                {
                    Writer.Write(keyValuePair.Key);
                    Writer.Write(keyValuePair.Value);
                }
            }

            if (this.byteArrayData?.Count > 0)
            {
                foreach (var keyValuePair in this.byteArrayData)
                {
                    Writer.Write(keyValuePair.Key);
                    Writer.WriteBytesFull(keyValuePair.Value);
                }
            }

            if (this.dateTimeData?.Count > 0)
            {
                foreach (var keyValuePair in this.dateTimeData)
                {
                    Writer.Write(keyValuePair.Key);
                    Writer.Write(keyValuePair.Value.ToFileTimeUtc());
                }
            }

            int byteCount = Writer.Position;
            Writer.ResetBuffer(null);
            return byteCount;
        }

        public void Deserialize(byte[] serializedData)
        {
            this.isDirty = false;

            this.intData ??= new Dictionary<string, int>();
            this.enumData ??= new Dictionary<string, int>();
            this.boolData ??= new Dictionary<string, bool>();
            this.longData ??= new Dictionary<string, long>();
            this.floatData ??= new Dictionary<string, float>();
            this.doubleData ??= new Dictionary<string, double>();
            this.stringData ??= new Dictionary<string, string>();
            this.byteArrayData ??= new Dictionary<string, byte[]>();
            this.dateTimeData ??= new Dictionary<string, DateTime>();

            // Make sure everything is cleared
            this.intData.Clear();
            this.enumData.Clear();
            this.boolData.Clear();
            this.longData.Clear();
            this.floatData.Clear();
            this.doubleData.Clear();
            this.stringData.Clear();
            this.byteArrayData.Clear();
            this.dateTimeData.Clear();

            // Making our network reader point to the given byte data
            Reader.ResetBuffer(serializedData);

            uint version = Reader.ReadPackedUInt32();

            if (version == 1)
            {
                this.DeserializeVersion1();
            }
            else
            {
                Logger.LogError($"Serialization Failed! {nameof(DataStore)} found unknown version number \"{version}\"");
            }

            Reader.ResetBuffer(null);
        }

        // Delete
        public void DeleteInt(string key) => this.DeleteKey(this.intData, key);

        public void DeleteEnum(string key) => this.DeleteKey(this.enumData, key);

        public void DeleteBool(string key) => this.DeleteKey(this.boolData, key);

        public void DeleteLong(string key) => this.DeleteKey(this.longData, key);

        public void DeleteFloat(string key) => this.DeleteKey(this.floatData, key);

        public void DeleteDouble(string key) => this.DeleteKey(this.doubleData, key);

        public void DeleteString(string key) => this.DeleteKey(this.stringData, key);

        public void DeleteByteArray(string key) => this.DeleteKey(this.byteArrayData, key);

        public void DeleteDateTime(string key) => this.DeleteKey(this.dateTimeData, key);

        // Gets
        public int GetInt(string key, int defaultValue = 0) => this.GetKey(this.intData, key, defaultValue);

        public T GetEnum<T>(string key, T defaultValue = default)
            where T : Enum
        {
            return (T)Enum.ToObject(typeof(T), this.GetKey(this.enumData, key, Convert.ToInt32(defaultValue)));
        }

        public int GetEnumAsInt(string key, int defaultValue = 0) => this.GetKey(this.enumData, key, defaultValue);

        public bool GetBool(string key, bool defaultValue = false) => this.GetKey(this.boolData, key, defaultValue);

        public long GetLong(string key, long defaultValue = 0) => this.GetKey(this.longData, key, defaultValue);

        public float GetFloat(string key, float defaultValue = 0.0f) => this.GetKey(this.floatData, key, defaultValue);

        public double GetDouble(string key, double defaultValue = 0.0) => this.GetKey(this.doubleData, key, defaultValue);

        public string GetString(string key, string defaultValue = null) => this.GetKey(this.stringData, key, defaultValue);

        public byte[] GetByteArray(string key, byte[] defaultValue = null) => this.GetKey(this.byteArrayData, key, defaultValue);

        public DateTime GetDateTime(string key, DateTime defaultValue) => this.GetKey(this.dateTimeData, key, defaultValue);

        // Has
        public bool HasInt(string key) => this.HasKey(this.intData, key);

        public bool HasEnum(string key) => this.HasKey(this.enumData, key);

        public bool HasBool(string key) => this.HasKey(this.boolData, key);

        public bool HasLong(string key) => this.HasKey(this.longData, key);

        public bool HasFloat(string key) => this.HasKey(this.floatData, key);

        public bool HasDouble(string key) => this.HasKey(this.doubleData, key);

        public bool HasString(string key) => this.HasKey(this.stringData, key);

        public bool HasByteArray(string key) => this.HasKey(this.byteArrayData, key);

        public bool HasDateTime(string key) => this.HasKey(this.dateTimeData, key);

        // Set
        public void SetInt(string key, int value) => this.SetValueTypeKey(ref this.intData, key, value);

        public void SetEnum<T>(string key, T value)
            where T : Enum
        {
            this.SetValueTypeKey(ref this.enumData, key, Convert.ToInt32(value));
        }

        public void SetEnumAsInt(string key, int value) => this.SetValueTypeKey(ref this.enumData, key, value);

        public void SetBool(string key, bool value) => this.SetValueTypeKey(ref this.boolData, key, value);

        public void SetLong(string key, long value) => this.SetValueTypeKey(ref this.longData, key, value);

        public void SetFloat(string key, float value) => this.SetValueTypeKey(ref this.floatData, key, value);

        public void SetDouble(string key, double value) => this.SetValueTypeKey(ref this.doubleData, key, value);

        public void SetString(string key, string value) => this.SetClassTypeKey(ref this.stringData, key, value);

        public void SetByteArray(string key, byte[] value) => this.SetClassTypeKey(ref this.byteArrayData, key, value);

        public void SetDateTime(string key, DateTime value) => this.SetValueTypeKey(ref this.dateTimeData, key, value);

        private void DeleteKey<T>(Dictionary<string, T> dictionary, string key)
        {
            if (dictionary?.Remove(key) == true)
            {
                this.isDirty = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T GetKey<T>(Dictionary<string, T> dictionary, string key, T defaultValue)
        {
            return dictionary == null ? defaultValue : dictionary.TryGetValue(key, out T value) ? value : defaultValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasKey<T>(Dictionary<string, T> dictionary, string key)
        {
            return dictionary?.ContainsKey(key) ?? false;
        }

        private void SetValueTypeKey<T>(ref Dictionary<string, T> dictionary, string key, T value)
            where T : struct
        {
            if (dictionary == null)
            {
                dictionary = new Dictionary<string, T>();
                this.isDirty = true;
            }

            bool valueChanged = false;

            if (dictionary.TryGetValue(key, out T currentDictionaryValue))
            {
                if (value.Equals(currentDictionaryValue))
                {
                    // Do nothing
                }
                else
                {
                    dictionary[key] = value;
                    this.isDirty = true;
                    valueChanged = true;
                }
            }
            else
            {
                dictionary.Add(key, value);
                this.isDirty = true;
                valueChanged = true;
            }

            if (valueChanged)
            {
                this.FireOnDataStoreKeyChanged(key);
            }
        }

        private void SetClassTypeKey<T>(ref Dictionary<string, T> dictionary, string key, T value)
            where T : class
        {
            if (dictionary == null)
            {
                dictionary = new Dictionary<string, T>();
                this.isDirty = true;
            }

            bool valueChanged = false;

            if (dictionary.TryGetValue(key, out T currentDictionaryValue))
            {
                if (value == null && currentDictionaryValue == null)
                {
                    // Do Nothing
                }
                else if (value?.Equals(currentDictionaryValue) == true)
                {
                    // Do Nothing
                }
                else
                {
                    dictionary[key] = value;
                    this.isDirty = true;
                    valueChanged = true;
                }
            }
            else
            {
                dictionary.Add(key, value);
                this.isDirty = true;
                valueChanged = true;
            }

            if (valueChanged)
            {
                this.FireOnDataStoreKeyChanged(key);
            }
        }

        private void FireOnDataStoreKeyChanged(string key)
        {
            try
            {
                this.onDataStoreKeyChanged?.Invoke(key);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void DeserializeVersion1()
        {
            uint intCount = Reader.ReadPackedUInt32();
            uint enumCount = Reader.ReadPackedUInt32();
            uint boolCount = Reader.ReadPackedUInt32();
            uint longCount = Reader.ReadPackedUInt32();
            uint floatCount = Reader.ReadPackedUInt32();
            uint doubleCount = Reader.ReadPackedUInt32();
            uint stringCount = Reader.ReadPackedUInt32();
            uint byteArrayCount = Reader.ReadPackedUInt32();
            uint dateTimeCount = Reader.ReadPackedUInt32();

            for (int i = 0; i < intCount; i++)
            {
                this.intData.Add(Reader.ReadString(), Reader.ReadInt32());
            }

            for (int i = 0; i < enumCount; i++)
            {
                this.enumData.Add(Reader.ReadString(), Reader.ReadInt32());
            }

            for (int i = 0; i < boolCount; i++)
            {
                this.boolData.Add(Reader.ReadString(), Reader.ReadBoolean());
            }

            for (int i = 0; i < longCount; i++)
            {
                this.longData.Add(Reader.ReadString(), Reader.ReadInt64());
            }

            for (int i = 0; i < floatCount; i++)
            {
                this.floatData.Add(Reader.ReadString(), Reader.ReadSingle());
            }

            for (int i = 0; i < doubleCount; i++)
            {
                this.doubleData.Add(Reader.ReadString(), Reader.ReadDouble());
            }

            for (int i = 0; i < stringCount; i++)
            {
                this.stringData.Add(Reader.ReadString(), Reader.ReadString());
            }

            for (int i = 0; i < byteArrayCount; i++)
            {
                this.byteArrayData.Add(Reader.ReadString(), Reader.ReadBytesAndSize());
            }

            for (int i = 0; i < dateTimeCount; i++)
            {
                this.dateTimeData.Add(Reader.ReadString(), DateTime.FromFileTimeUtc(Reader.ReadInt64()));
            }
        }
    }
}
