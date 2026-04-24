//-----------------------------------------------------------------------
// <copyright file="JsonUtil.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using Newtonsoft.Json;

    public static class JsonUtil
    {
        static JsonUtil()
        {
            JsonSerializerSettings.Converters.Add(new RGBAConverter());
            JsonSerializerSettings.Converters.Add(new PositionConverter());
        }

        public static JsonSerializerSettings JsonSerializerSettings { get; private set; } = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            DateFormatString = "yyyy-MM-ddTHH:mm:ssZ",
            NullValueHandling = NullValueHandling.Ignore,
        };

        public static JsonSerializerSettings JsonSerializerSettingsWithTypes { get; private set; } = new JsonSerializerSettings(JsonSerializerSettings)
        {
            TypeNameHandling = TypeNameHandling.All,
        };

        public static string Serialize(object obj, bool includeTypeInformation = false)
        {
            if (obj == null)
            {
                return null;
            }

            return JsonConvert.SerializeObject(obj, includeTypeInformation ? JsonSerializerSettingsWithTypes : JsonSerializerSettings);
        }

        public static T Deserialize<T>(string json, bool includeTypeInformation = false)
        {
            return JsonConvert.DeserializeObject<T>(json, includeTypeInformation ? JsonSerializerSettingsWithTypes : JsonSerializerSettings);
        }

        public static object Deserialize(string json, System.Type type)
        {
            return JsonConvert.DeserializeObject(json, type, JsonSerializerSettings);
        }
    }
}
