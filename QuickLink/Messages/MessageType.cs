using System.Collections.Generic;

namespace QuickLink
{
    /// <summary>
    /// Represents a message type.
    /// </summary>
    public class MessageType
    {
        private static readonly List<MessageType> _messageTypes = new List<MessageType>();

        /// <summary>
        /// Gets the unique identifier of the message type.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets the name of the message type.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the message type with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier of the message type.</param>
        /// <returns>The message type with the specified identifier.</returns>
        public static MessageType Get(int id)
        {
            return _messageTypes[id];
        }

        private MessageType(int id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Gets the message type with the specified name.
        /// If the message type does not exist, a new one is created and added to the list of message types.
        /// </summary>
        /// <param name="name">The name of the message type.</param>
        /// <returns>The message type with the specified name.</returns>
        public static MessageType Get(string name)
        {
            foreach (MessageType mType in _messageTypes)
            {
                if (mType.Name == name) return mType;
            }

            MessageType messageType = new MessageType(_messageTypes.Count, name);
            _messageTypes.Add(messageType);
            return messageType;
        }
    }
}