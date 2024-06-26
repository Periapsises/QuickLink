﻿using System.Collections.Generic;
using QuickLink.Utils;

namespace QuickLink.Messaging
{
    /// <summary>
    /// Represents a message type.
    /// </summary>
    public class MessageType
    {
        private static readonly Dictionary<uint, MessageType> _messageTypes = new Dictionary<uint, MessageType>();
        private static readonly object _lock = new object();

        /// <summary>
        /// Gets the unique identifier of the message type.
        /// </summary>
        public uint Id { get; private set; }

        /// <summary>
        /// Gets the name of the message type.
        /// </summary>
        public string Name { get; private set; }

        private MessageType(uint id)
        {
            Id = id;
            Name = "UNKNOWN";
        }

        /// <summary>
        /// Gets the message type with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the message type.</param>
        /// <returns>The message type with the specified identifier.</returns>
        public static MessageType Get(uint id)
        {
            lock (_lock)
            {
                if (!_messageTypes.ContainsKey(id))
                {
                    _messageTypes.Add(id, new MessageType(id));
                }

                return _messageTypes[id];
            }
        }

        /// <summary>
        /// Gets the message type with the specified name.
        /// If the message type does not exist, a new one is created and added to the list of message types.
        /// </summary>
        /// <param name="name">The name of the message type.</param>
        /// <returns>The message type with the specified name.</returns>
        public static MessageType Get(string name)
        {
            lock (_lock)
            {
                uint id = CRC32.GenerateHash(name);
                MessageType messageType = Get(id);

                if (messageType.Name == "UNKNOWN")
                {
                    messageType.Name = name;
                }

                return messageType;
            }
        }
    }
}