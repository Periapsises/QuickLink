using System;
using QuickLink.Messaging;

namespace QuickLink
{
    /// <summary>
    /// Represents a host that communicates with a server and sends/receives messages.
    /// </summary>
    public class Host : IDisposable
    {
        /// <summary>
        /// Event that is raised when a message is received.
        /// </summary>
        public MessagePublisher MessageReceived => _messageReceived;

        /// <summary>
        /// The server associated with the host.
        /// </summary>
        public readonly Server Server;

        private readonly MessagePublisher _messageReceived = new MessagePublisher();
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Host"/> class with the specified port.
        /// </summary>
        /// <param name="port">The port number to use for communication.</param>
        public Host(int port)
        {
            Server = new Server(port, this);
            Server.Start();
        }

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public void SendToServer(MessageWriter message)
        {
            Server.ReceiveMessage(message.ToReader());
        }

        /// <summary>
        /// Receives a message from the server.
        /// </summary>
        /// <param name="message">The received message.</param>
        public void RecieveFromServer(MessageReader message)
        {
            MessageReceived.Publish(message);
        }

        /// <summary>
        /// Disposes the host and releases any resources used.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the host and releases any resources used.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                Server.Dispose();
            }

            _disposed = true;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Host"/> class.
        /// </summary>
        ~Host()
        {
            Dispose(false);
        }
    }
}