//-----------------------------------------------------------------------
// <copyright file="MessageRoomStats.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BoardGame.Networking
{
    using System.Collections.Generic;
    using System.Text;

    public sealed class MessageTypeStats
    {
        public short Id { get; set; }

        public string Name { get; set; }

        public int Count { get; set; }

        public int MinBytes { get; set; }

        public int MaxBytes { get; set; }

        public long TotalBytes { get; set; }

        public double AverageBytes => this.Count == 0 ? 0 : (double)this.TotalBytes / this.Count;

        public void Record(int bytes)
        {
            if (this.Count == 0)
            {
                this.MinBytes = bytes;
                this.MaxBytes = bytes;
            }
            else
            {
                if (bytes < this.MinBytes)
                {
                    this.MinBytes = bytes;
                }

                if (bytes > this.MaxBytes)
                {
                    this.MaxBytes = bytes;
                }
            }

            this.Count++;
            this.TotalBytes += bytes;
        }

        public MessageTypeStats Copy()
        {
            return new MessageTypeStats
            {
                Id = this.Id,
                Name = this.Name,
                Count = this.Count,
                MinBytes = this.MinBytes,
                MaxBytes = this.MaxBytes,
                TotalBytes = this.TotalBytes,
            };
        }

        public override string ToString()
        {
            return $"{this.Name} ({this.Id}): count = {this.Count}, min = {this.MinBytes}, avg = {this.AverageBytes:0.#}, max = {this.MaxBytes}, total = {this.TotalBytes}";
        }
    }

    public sealed class RoomStats
    {
        public List<MessageTypeStats> Sent { get; } = new List<MessageTypeStats>();

        public List<MessageTypeStats> Received { get; } = new List<MessageTypeStats>();

        public int TotalMessagesSent { get; set; }

        public int TotalMessagesReceived { get; set; }

        public long TotalBytesSent { get; set; }

        public long TotalBytesReceived { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Sent: {this.TotalMessagesSent} messages, {this.TotalBytesSent} bytes");

            foreach (var stats in this.Sent)
            {
                builder.Append("  ").AppendLine(stats.ToString());
            }

            builder.AppendLine($"Received: {this.TotalMessagesReceived} messages, {this.TotalBytesReceived} bytes");

            foreach (var stats in this.Received)
            {
                builder.Append("  ").AppendLine(stats.ToString());
            }

            return builder.ToString();
        }
    }
}
