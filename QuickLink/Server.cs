using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using QuickLink.Messaging;

namespace QuickLink
{
    /// <summary>
    /// Represents a server that listens for incoming TCP connections and handles client communication.
    /// </summary>
    public class Server : IDisposable
    {
        /// <summary>
        /// Event that is raised when a message is received from a client.
        /// </summary>
        public readonly MessagePublisher MessageReceived = new MessagePublisher();

        /// <summary>
        /// Event that is raised when a client connects to the server.
        /// </summary>
        public EventPublisher<NetworkEntity> ClientConnected = new EventPublisher<NetworkEntity>();

        /// <summary>
        /// Event that is raised when a client disconnects from the server.
        /// </summary>
        public EventPublisher<NetworkEntity> ClientDisconnected = new EventPublisher<NetworkEntity>();

        private readonly TcpListener _listener;
        private readonly ConcurrentBag<NetworkEntity> _clients = new ConcurrentBag<NetworkEntity>();
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
        private uint _currentUserID = 2; // UID 0 is reserved for the server and UID 1 for the host.
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class with the specified port and host.
        /// </summary>
        /// <param name="port">The port number on which the server listens for incoming connections.</param>
        /// <param name="host">The host object that handles received messages.</param>
        public Server(int port, Host host)
        {
            _listener = new TcpListener(IPAddress.Any, port);

            NetworkEntity hostEntity = new NetworkHost(1, host);
            _clients.Add(hostEntity);
        }

        /// <summary>
        /// Starts the server, allowing it to accept incoming client connections.
        /// </summary>
        public void Start()
        {
            _listener.Start();
            _ = AcceptClientsAsync();
        }

        private async Task AcceptClientsAsync()
        {
            while (!_cancellation.Token.IsCancellationRequested)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
#if DEBUG
                Console.WriteLine($"[Server] Client connected: {client.Client.RemoteEndPoint}  User ID: {_currentUserID}");
#endif
                NetworkEntity clientEntity = new NetworkClient(_currentUserID++, client);
                clientEntity.SetMessageReceivedCallback(ReceiveMessage);
                clientEntity.SetClientDisconnectedCallback(() => { ClientDisconnected.Publish(clientEntity); });
                _clients.Add(clientEntity);
                ClientConnected.Publish(clientEntity);
            }
        }

        /// <summary>
        /// Receives a message from a client and publishes it to the <see cref="MessageReceived"/> event.
        /// </summary>
        /// <param name="message">The message received from the client.</param>
        public void ReceiveMessage(MessageReader message)
        {
            MessageReceived.Publish(message);
        }

        /// <summary>
        /// Broadcasts a message to all connected clients.
        /// </summary>
        /// <param name="message">The message to broadcast.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public void BroadcastMessage(MessageWriter message)
        {
#if DEBUG
            Console.WriteLine($"[Server] Broadcasting message to {_clients.Count} clients");
#endif
            foreach (NetworkEntity client in _clients)
                client.SendMessage(message);
        }

        /// <summary>
        /// Releases all resources used by the server.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="Server"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _cancellation.Cancel();
                _listener.Stop();
            }

            _disposed = true;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Server"/> class.
        /// </summary>
        ~Server()
        {
            Dispose(false);
        }
    }
}