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
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            DateFormatString = "yyyy-MM-ddTHH:mm:ssZ",
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new JsonConverter[]
            {
                new RGBAConverter(),
                new PositionConverter(),
            },
        };

        private static readonly JsonSerializerSettings jsonSerializerSettingsWithTypes = new JsonSerializerSettings(jsonSerializerSettings)
        {
            TypeNameHandling = TypeNameHandling.All,
        };

        public static string Serialize(object obj, bool includeTypeInformation = false)
        {
            if (obj == null)
            {
                return null;
            }

            return JsonConvert.SerializeObject(obj, includeTypeInformation ? jsonSerializerSettingsWithTypes : jsonSerializerSettings);
        }

        public static T Deserialize<T>(string json, bool includeTypeInformation = false)
        {
            return JsonConvert.DeserializeObject<T>(json, includeTypeInformation ? jsonSerializerSettingsWithTypes : jsonSerializerSettings);
        }

        public static object Deserialize(string json, System.Type type, bool includeTypeInformation = false)
        {
            return JsonConvert.DeserializeObject(json, type, includeTypeInformation ? jsonSerializerSettingsWithTypes : jsonSerializerSettings);
        }
    }
}
