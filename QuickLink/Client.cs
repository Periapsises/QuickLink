using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace QuickLink
{
    /// <summary>
    /// Represents the state of the connection.
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        /// The connection is disconnected.
        /// </summary>
        Disconnected,

        /// <summary>
        /// The connection is in the process of connecting.
        /// </summary>
        Connecting,

        /// <summary>
        /// The connection is successfully established.
        /// </summary>
        Connected,

        /// <summary>
        /// An error occurred during the connection.
        /// </summary>
        Error
    }

    /// <summary>
    /// Represents a client that connects to a server using TCP/IP.
    /// </summary>
    public class Client : IClient, IDisposable
    {
        /// <summary>
        /// Current state of the client's TCP connection.
        /// </summary>
        public ConnectionState ConnectionState = ConnectionState.Disconnected;

        /// <summary>
        /// Event that is raised when the client successfully connects to the server.
        /// </summary>
        public EventPublisher Connected = new EventPublisher();

        /// <summary>
        /// Event that is raised when the client disconnects from the server.
        /// </summary>
        public EventPublisher Disconnected = new EventPublisher();

        /// <summary>
        /// Event that is raised when a message is received from the server.
        /// </summary>
        public MessagePublisher MessageReceived => _messageReceived;

        private readonly ConcurrentQueue<byte[]> _queue = new ConcurrentQueue<byte[]>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private readonly MessagePublisher _messageReceived = new MessagePublisher();
        private readonly TcpClient _client = new TcpClient();
        private bool _disposed = false;

        /// <summary>
        /// Connects the client to the specified host and port.
        /// </summary>
        /// <param name="host">The host name or IP address of the server.</param>
        /// <param name="port">The port number to connect to.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="SocketException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public async Task Connect(string host, int port)
        {
#if DEBUG
            Console.WriteLine("[Client] Connecting to the server");
#endif

            ConnectionState = ConnectionState.Connecting;

            try
            {
                await _client.ConnectAsync(host, port);
            }
            catch
            {
                ConnectionState = ConnectionState.Error;
                throw;
            }

#if DEBUG
            Console.WriteLine("[Client] Connected to the server");
#endif

            ConnectionState = ConnectionState.Connected;

            _ = Task.Run(HandleReceiveFromServer);
            _ = Task.Run(HandleSendToServer);

            Connected.Publish();
        }

        private async Task HandleReceiveFromServer()
        {
            byte[] lengthBuffer = new byte[4];

            using (NetworkStream stream = _client.GetStream())
            {
                while (stream.CanRead && !_cancellationToken.Token.IsCancellationRequested)
                {
                    await stream.ReadAsync(lengthBuffer, 0, lengthBuffer.Length, _cancellationToken.Token);
                    uint length = BitConverter.ToUInt32(lengthBuffer, 0);
#if DEBUG
                    Console.WriteLine($"[Client] Received header for a {length} byte message from the server");
#endif

                    int offset = 0;
                    int bytesRead;

                    byte[] data = new byte[length];

                    while (offset < length && (bytesRead = await stream.ReadAsync(data, offset, (int)length - offset, _cancellationToken.Token)) > 0)
                    {
                        offset += bytesRead;
                    }

                    _messageReceived.Publish(new MessageReader(data));
                }
            }

            ConnectionState = ConnectionState.Disconnected;
            Disconnected.Publish();
        }

        private async Task HandleSendToServer()
        {
            using (NetworkStream stream = _client.GetStream())
            {
                while (stream.CanWrite && !_cancellationToken.Token.IsCancellationRequested)
                {
                    await _semaphore.WaitAsync();
#if DEBUG
                    Console.WriteLine("[Client] Semaphore notified data is available");
#endif

                    if (_queue.TryDequeue(out byte[] buffer))
                    {
#if DEBUG
                        Console.WriteLine($"[Client] Got {buffer.Length} bytes from the queue");
#endif
                        byte[] lengthBuffer = BitConverter.GetBytes((uint)buffer.Length);
                        await stream.WriteAsync(lengthBuffer, 0, lengthBuffer.Length, _cancellationToken.Token);
                        await stream.WriteAsync(buffer, 0, buffer.Length, _cancellationToken.Token);
                        await stream.FlushAsync();
#if DEBUG
                        Console.WriteLine("[Client] Sent data to the server");
#endif
                    }
                }
            }

            ConnectionState = ConnectionState.Disconnected;
        }

        /// <summary>
        /// Sends a message to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="SemaphoreFullException"></exception>
        public void SendToServer(MessageWriter message)
        {
#if DEBUG
            Console.WriteLine($"[Client] Enqueueing {message.ToArray().Length} bytes to the queue");
#endif
            _queue.Enqueue(message.ToArray());
            _semaphore.Release();
        }

        /// <summary>
        /// Releases the resources used by the <see cref="Client"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="Client"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _cancellationToken.Cancel();
                _semaphore.Release();
                _client.Dispose();
            }

            _disposed = true;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Client"/> class.
        /// </summary>
        ~Client()
        {
            Dispose(false);
        }
    }
}