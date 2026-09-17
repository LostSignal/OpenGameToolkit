//-----------------------------------------------------------------------
// <copyright file="UserInfo.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    using System;
    using System.Collections.Generic;

    public class UserInfo
    {
        private static readonly Random Rand = new Random();

        public Action OnInfoChanged;

        public UserInfo()
        {
            this.CustomData = new Dictionary<string, string>();
        }

        public UserInfo(UserInfo copy)
        {
            this.CustomData = new Dictionary<string, string>();
            this.CopyFrom(copy);
        }

        public static UserInfo GenerateRandomUserInfo()
        {
            string userId = Rand.Next(1000, 9999).ToString();
            string displayName = $"Player{userId}";
            return new UserInfo { UserId = userId, DisplayName = displayName };
        }

        /// <summary>
        /// Gets or sets the connection id.  This is only used/set by the server.
        /// </summary>
        /// <value>The connection id.</value>
        public long ConnectionId { get; set; }

        public string UserId { get; set; }

        public string DisplayName { get; set; }

        public Dictionary<string, string> CustomData { get; set; }

        public void Deserialize(NetworkReader reader)
        {
            this.ConnectionId = reader.ReadInt64();
            this.UserId = reader.ReadString();
            this.DisplayName = reader.ReadString();

            // CustomData
            this.CustomData.Clear();

            byte count = reader.ReadByte();

            for (int i = 0; i < count; i++)
            {
                string key = reader.ReadString();
                string value = reader.ReadString();

                this.CustomData.Add(key, value);
            }
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(this.ConnectionId);
            writer.Write(this.UserId);
            writer.Write(this.DisplayName);

            // CustomData
            writer.Write((byte)this.CustomData.Count);

            foreach (var pair in this.CustomData)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value);
            }
        }

        public void CopyFrom(UserInfo source)
        {
            this.ConnectionId = source.ConnectionId;
            this.UserId = source.UserId;
            this.DisplayName = source.DisplayName;

            // CustomData
            this.CustomData.Clear();

            foreach (var pair in source.CustomData)
            {
                this.CustomData.Add(pair.Key, pair.Value);
            }
        }

        public UserInfo Copy()
        {
            return new UserInfo(this);
        }

        public void NotifyInfoChanged()
        {
            this.OnInfoChanged?.Invoke();
        }

        public override string ToString()
        {
            var builder = BetterStringBuilder.New()
                .Append("ConnectionId = ")
                .Append(this.ConnectionId)
                .Append(", UserId = ")
                .Append(this.UserId)
                .Append(", DisplayName = ")
                .Append(this.DisplayName)
                .Append(", Custom Data Count = ")
                .Append(this.CustomData.Count);

            foreach (var pair in this.CustomData)
            {
                builder = builder
                    .Append(", ")
                    .Append(pair.Key)
                    .Append(" => ")
                    .Append(pair.Value);
            }

            return builder.ToString();
        }
    }
}
