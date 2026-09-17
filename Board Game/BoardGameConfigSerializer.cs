//-----------------------------------------------------------------------
// <copyright file="BoardGameConfigSerializer.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BoardGame
{
    /*
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Serializes <see cref="BoardGameConfig"/> so it can travel over the network and be replayed later. Uses type information
    /// so the concrete config type round trips, and replaces collections instead of appending to initializer defaults.
    /// </summary>
    public static class BoardGameConfigSerializer
    {
        private static readonly Random SeedRandom = new Random();

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
            Converters = new JsonConverter[]
            {
                new RGBAConverter(),
                new PositionConverter(),
            },
        };

        public static string Serialize(BoardGameConfig config)
        {
            return JsonConvert.SerializeObject(config, typeof(BoardGameConfig), Settings);
        }

        public static BoardGameConfig Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<BoardGameConfig>(json, Settings);
        }

        /// <summary>
        /// Assigns a random seed if the config has none (mutating the input) and returns a JSON round tripped copy. Every game
        /// is started from a normalized copy so local play, the host and every remote peer run exactly the same data.
        /// </summary>
        public static BoardGameConfig Normalize(BoardGameConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (config.Seed == 0)
            {
                config.Seed = SeedRandom.Next(1, int.MaxValue);
            }

            return Deserialize(Serialize(config));
        }
    }

    */
}
