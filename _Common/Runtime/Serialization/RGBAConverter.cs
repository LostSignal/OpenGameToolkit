//-----------------------------------------------------------------------
// <copyright file="RGBAConverter.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using Newtonsoft.Json;

    public class RGBAConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(RGBA);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ColorUtil.ParseColorHexString((string)reader.Value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var color = (RGBA)value;
            serializer.Serialize(writer, ColorUtil.ConvertToHexString(color));
        }
    }
}
