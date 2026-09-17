//-----------------------------------------------------------------------
// <copyright file="WebPubSubRoomProtocol.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.BoardGame.Networking
{
    using System;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Azure Web PubSub "json.webpubsub.azure.v1" client sub protocol. Rooms map to Web PubSub groups, so the client
    /// access token must carry the "webpubsub.joinLeaveGroup.{room}" and "webpubsub.sendToGroup.{room}" roles.
    /// See https://learn.microsoft.com/azure/azure-web-pubsub/reference-json-webpubsub-subprotocol.
    /// </summary>
    public sealed class WebPubSubRoomProtocol : IWebSocketRoomProtocol
    {
        public string SubProtocol => "json.webpubsub.azure.v1";

        public string BuildJoin(string room, int ackId)
        {
            return new JObject
            {
                ["type"] = "joinGroup",
                ["group"] = room,
                ["ackId"] = ackId,
            }.ToString(Formatting.None);
        }

        public string BuildLeave(string room, int ackId)
        {
            return new JObject
            {
                ["type"] = "leaveGroup",
                ["group"] = room,
                ["ackId"] = ackId,
            }.ToString(Formatting.None);
        }

        public string BuildSend(string room, byte[] data, int offset, int count, int ackId)
        {
            return new JObject
            {
                ["type"] = "sendToGroup",
                ["group"] = room,
                ["ackId"] = ackId,
                ["noEcho"] = true,
                ["dataType"] = "binary",
                ["data"] = Convert.ToBase64String(data, offset, count),
            }.ToString(Formatting.None);
        }

        public bool TryParseInbound(string text, out InboundFrame frame)
        {
            frame = default;

            JObject obj;

            try
            {
                obj = JObject.Parse(text);
            }
            catch (JsonException)
            {
                return false;
            }

            string type = (string)obj["type"];

            switch (type)
            {
                case "system":
                {
                    string systemEvent = (string)obj["event"];

                    if (systemEvent == "connected")
                    {
                        frame.Type = InboundFrameType.Connected;
                        frame.ConnectionId = (string)obj["connectionId"];
                        frame.UserId = (string)obj["userId"];
                        return true;
                    }

                    if (systemEvent == "disconnected")
                    {
                        frame.Type = InboundFrameType.Disconnected;
                        frame.Reason = (string)obj["message"];
                        return true;
                    }

                    return false;
                }

                case "ack":
                {
                    frame.Type = InboundFrameType.Ack;
                    frame.AckId = (int?)obj["ackId"] ?? 0;
                    frame.Success = (bool?)obj["success"] ?? false;

                    JToken error = obj["error"];

                    if (error != null && error.Type == JTokenType.Object)
                    {
                        frame.Error = $"{(string)error["name"]}: {(string)error["message"]}";
                    }

                    return true;
                }

                case "message":
                {
                    if ((string)obj["from"] != "group")
                    {
                        return false;
                    }

                    frame.Type = InboundFrameType.GroupMessage;
                    frame.Group = (string)obj["group"];
                    frame.UserId = (string)obj["fromUserId"];

                    string dataType = (string)obj["dataType"];
                    JToken data = obj["data"];

                    if (dataType == "binary")
                    {
                        frame.Data = Convert.FromBase64String((string)data);
                    }
                    else if (dataType == "text")
                    {
                        frame.Data = Encoding.UTF8.GetBytes((string)data ?? string.Empty);
                    }
                    else
                    {
                        frame.Data = Encoding.UTF8.GetBytes(data?.ToString(Formatting.None) ?? string.Empty);
                    }

                    return true;
                }

                default:
                    return false;
            }
        }
    }
}
