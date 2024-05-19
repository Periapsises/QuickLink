using System;
using System.Net.Sockets;

using QuickLink.Messaging;
using QuickLink.Utils;

namespace QuickLink
{
    /// <summary>
    /// The representation of a client or host on the server.
    /// </summary>
    public abstract class NetworkEntity
    {
        /// <summary>
        /// A unique ID associated to the entity assigned by the server.
        /// </summary>
        public uint UserID { get; private set; }

        /// <summary>
        /// Fired when a message is received from this entity.
        /// </summary>
        protected Action<MessageReader>? OnMessageReceived;

        /// <summary>
        /// Fired when the client disconnects from the server.
        /// </summary>
        // TODO: Add a reason parameter
        protected Action? OnClientDisconnected;

        internal NetworkEntity(uint id)
        {
            UserID = id;
        }

        /// <summary>
        /// Sends a message to the entity from the server.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        public abstract void SendMessage(MessageWriter message);

        internal void SetMessageReceivedCallback(Action<MessageReader> callback)
        {
            OnMessageReceived = callback;
        }

        internal void SetClientDisconnectedCallback(Action callback)
        {
            OnClientDisconnected = callback;
        }
    }

    /// <summary>
    /// The representation of a remote client on the server.
    /// </summary>
    public class NetworkClient : NetworkEntity, IDisposable
    {
        private readonly TcpClientHandler _tcpClientHandler;
        private bool _disposed = false;

        internal NetworkClient(uint id, TcpClient client) : base(id)
        {
            _tcpClientHandler = new TcpClientHandler(client)
            {
                DataRecieved = (data) => { OnMessageReceived?.Invoke(new MessageReader(data)); },
                ClientDisconnected = () => { OnClientDisconnected?.Invoke(); }
            };
            _tcpClientHandler.Start();
        }

        /// <summary>
        /// Sends a message to the client.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        public override void SendMessage(MessageWriter message)
        {
            _tcpClientHandler.QueueData(message.ToArray());
        }

        /// <summary>
        /// Releases all resources used by the <see cref="NetworkClient"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="NetworkClient"/> object and optionally releases the managed resources. 
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _tcpClientHandler.Dispose();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizes the client network entity.
        /// </summary>
        ~NetworkClient()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// The representation of the host on the server.
    /// </summary>
    public class NetworkHost : NetworkEntity
    {
        private readonly Host _host;

        internal NetworkHost(uint id, Host host) : base(id)
        {
            _host = host;
        }

        /// <summary>
        /// Sends a message to the host.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        public override void SendMessage(MessageWriter message)
        {
            _host.MessageReceived.Publish(message.ToReader());
        }
    }
}