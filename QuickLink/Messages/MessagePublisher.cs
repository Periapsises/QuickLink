using System;
using System.Collections.Generic;

namespace QuickLink
{
    /// <summary>
    /// Represents a message publisher that allows subscribing to and publishing messages.
    /// </summary>
    public class MessagePublisher
    {
        private readonly Dictionary<MessageType, List<Action<MessageReader>>> _messageHandlers = new Dictionary<MessageType, List<Action<MessageReader>>>();

        /// <summary>
        /// Subscribes to a specific message type with a handler.
        /// </summary>
        /// <param name="type">The message type to subscribe to.</param>
        /// <param name="handler">The handler to be invoked when a message of the specified type is published.</param>
        public void Subscribe(MessageType type, Action<MessageReader> handler)
        {
            if (!_messageHandlers.ContainsKey(type))
            {
                _messageHandlers[type] = new List<Action<MessageReader>>();
            }

            _messageHandlers[type].Add(handler);
        }

        /// <summary>
        /// Publishes a message to all subscribed handlers.
        /// </summary>
        /// <param name="reader">The message reader containing the message to be published.</param>
        public void Publish(MessageReader reader)
        {
            MessageType messageType = reader.Type;
            if (!_messageHandlers.ContainsKey(messageType))
                return;

            foreach (var handler in _messageHandlers[messageType])
            {
                handler(reader);
                reader.Seek(0);
            }
        }
    }
}