//-----------------------------------------------------------------------
// <copyright file="LostPlayerPrefs.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;

    public static class LostPlayerPrefs
    {
        private const string TrueString = "True";
        private const string FalseString = "False";

        private static bool isDirty;

#if UNITY_6000_0_OR_NEWER
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => isDirty = false;
#endif

        public static bool HasKey(string key)
        {
#if UNITY
            return UnityEngine.PlayerPrefs.HasKey(key);
#else
            return false;
#endif
        }

        public static void DeleteKey(string key)
        {
            isDirty = true;
#if UNITY
            UnityEngine.PlayerPrefs.DeleteKey(key);
#endif
        }

        public static int GetInt(string key, int defaultValue)
        {
#if UNITY
            return UnityEngine.PlayerPrefs.GetInt(key, defaultValue);
#else
            return defaultValue;
#endif
        }

        public static string GetString(string key, string defaultValue)
        {
#if UNITY
            return UnityEngine.PlayerPrefs.GetString(key, defaultValue);
#else
            return defaultValue;
#endif
        }

        public static T GetEnum<T>(string key, T defaultValue)
            where T : System.Enum
        {
            int defaultInt = Convert.ToInt32(defaultValue);
#if UNITY
            int enumInt = UnityEngine.PlayerPrefs.GetInt(key, defaultInt);
            return (T)Enum.ToObject(typeof(T), enumInt);
#else
            return defaultValue;
#endif
        }

        public static long GetLong(string key, long defaultValue)
        {
            string longAsString = GetString(key, null);
            return long.TryParse(longAsString, out long value) ? value : defaultValue;
        }

        public static DateTime GetDateTimeUTC(string key, DateTime defaultValue)
        {
            long fileTime = GetLong(key, defaultValue.ToFileTimeUtc());
            return DateTime.FromFileTimeUtc(fileTime);
        }

        public static bool GetBool(string key, bool defaultValue)
        {
            return HasKey(key) ? GetString(key, FalseString) == TrueString : defaultValue;
        }

        public static void SetInt(string key, int value, bool save = false)
        {
            isDirty = true;

#if UNITY
            UnityEngine.PlayerPrefs.SetInt(key, value);
#endif

            if (save)
            {
                Save();
            }
        }

        public static void SetString(string key, string value, bool save = false)
        {
            isDirty = true;

#if UNITY
            UnityEngine.PlayerPrefs.SetString(key, value);
#endif

            if (save)
            {
                Save();
            }
        }

        public static void SetEnum<T>(string key, T value, bool save = false)
            where T : Enum
        {
            SetInt(key, Convert.ToInt32(value));

            if (save)
            {
                Save();
            }
        }

        public static void SetLong(string key, long value, bool save = false)
        {
            SetString(key, BetterStringBuilder.New().Append(value).ToString());

            if (save)
            {
                Save();
            }
        }

        public static void SetDateTimeUTC(string key, DateTime value, bool save = false)
        {
            SetLong(key, value.ToFileTimeUtc());

            if (save)
            {
                Save();
            }
        }

        public static void SetBool(string key, bool value, bool save = false)
        {
            SetString(key, value ? TrueString : FalseString);

            if (save)
            {
                Save();
            }
        }

        public static void Save()
        {
            if (isDirty)
            {
                isDirty = false;
#if UNITY
                UnityEngine.PlayerPrefs.Save();
#endif
            }
        }

        private static int EnumToInteger(System.Enum e)
        {
            return int.Parse(e.ToString("d"));
        }
    }
}
