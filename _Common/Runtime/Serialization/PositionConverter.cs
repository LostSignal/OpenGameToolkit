//-----------------------------------------------------------------------
// <copyright file="PositionConverter.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class PositionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Position2D) || objectType == typeof(Position3D);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var temp = JObject.Load(reader);

            if (objectType == typeof(Position2D))
            {
                return new Position2D((float)temp["x"], (float)temp["y"]);
            }
            else if (objectType == typeof(Position3D))
            {
                return new Position3D((float)temp["x"], (float)temp["y"], (float)temp["z"]);
            }

            throw new Exception("Tried to read Json of unkonwn Vector type!");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Position2D vector2)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("x");
                writer.WriteValue(vector2.X);
                writer.WritePropertyName("y");
                writer.WriteValue(vector2.Y);
                writer.WriteEndObject();
            }
            else if (value is Position3D vector3)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("x");
                writer.WriteValue(vector3.X);
                writer.WritePropertyName("y");
                writer.WriteValue(vector3.Y);
                writer.WritePropertyName("z");
                writer.WriteValue(vector3.Z);
                writer.WriteEndObject();
            }
            else
            {
                throw new Exception("Tried to write Json of unkonwn Vector type!");
            }
        }
    }
}
