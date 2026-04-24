//-----------------------------------------------------------------------
// <copyright file="MessageCollection.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OGT.Networking
{
    using System;
    using System.Collections.Generic;

    public class MessageCollection
    {
        private static readonly OGTLogger Logger = OGTLogger.OGT;

        private readonly Dictionary<short, Type> messageTypes = new Dictionary<short, Type>();
        private readonly Dictionary<short, List<Message>> messagePools = new Dictionary<short, List<Message>>();
        private readonly NetworkReader reader = new NetworkReader();

        public static short GetMessageId(byte[] data)
        {
            ushort value = 0;
            value |= data[0];
            value |= (ushort)(data[1] << 8);

            return (short)value;
        }

        public void RegisterMessage<T>()
            where T : Message, new ()
        {
            Message message = new T();
            short messageId = message.GetId();

            if (this.messageTypes.ContainsKey(messageId))
            {
                Logger.LogError($"Message Type {message.GetType().Name} Has Duplicate Id {messageId}");
                return;
            }

            this.messageTypes.Add(messageId, typeof(T));
            this.messagePools.Add(messageId, new List<Message> { message });
        }

        public void RecycleMessage(Message message)
        {
            short messageId = message.GetId();

            if (this.messagePools.ContainsKey(messageId) == false)
            {
                Logger.LogError($"Tried to recycle Message {message.GetType().Name} with Id {messageId} without registering it!");
                return;
            }

            this.messagePools[messageId].Add(message);
        }

        public Message GetMessage(byte[] data)
        {
            Message message = this.GetMessage(GetMessageId(data));

            this.reader.Replace(data);
            message.Deserialize(this.reader);

            return message;
        }

        public Message GetMessage(short messageId)
        {
            if (this.messagePools.TryGetValue(messageId, out List<Message> pool) == false)
            {
                Logger.LogError($"Tried to get Message Id {messageId} without registering it!");
                return null;
            }

            if (pool.Count > 0)
            {
                int lastIndex = pool.Count - 1;
                Message message = pool[lastIndex];
                pool.RemoveAt(lastIndex);
                return message;
            }
            else
            {
                if (this.messageTypes.TryGetValue(messageId, out Type roomMessageType))
                {
                    return (Message)Activator.CreateInstance(roomMessageType);
                }
                else
                {
                    Logger.LogError($"Tried to get Message Id {messageId} without registering it!");
                    return null;
                }
            }
        }

        /*
        private void Copy(RoomMessage to, RoomMessage from)
        {
            if (this.copyBytes == null)
            {
                this.copyBytes = new byte[4096];
                this.copyNetworkReader = new NetworkReader(this.copyBytes);
                this.copyNetworkWriter = new NetworkWriter(this.copyBytes);
            }

            this.copyNetworkWriter.SeekZero();
            this.copyNetworkReader.SeekZero();

            from.Serialize(this.copyNetworkWriter);

            if (copyNetworkWriter.Position > copyBytes.Length)
            {
                // NOTE [bgish]: Because the buffer grew, all 3 of these objects are now hosed (so recreate them)
                this.copyBytes = null;
                this.copyNetworkReader = null;
                this.copyNetworkWriter = null;

                // NOTE [bgish]: If the buffer grows, then we're hosed!  Tell the user that we shouldn't be sending more then 4k!
                OGT.Logger.LogError("UserInfo Message exceeded 4k!  Sending UserInfo will now leak memory.  Please limit size to under 4k");

                NetworkWriter newNetworkWriter = new NetworkWriter();
                from.Serialize(newNetworkWriter);

                NetworkReader newNetworkReader = new NetworkReader(newNetworkWriter);
                to.Deserialize(newNetworkReader);
            }
            else
            {
                to.Deserialize(copyNetworkReader);
            }
        }
        */
    }
}
