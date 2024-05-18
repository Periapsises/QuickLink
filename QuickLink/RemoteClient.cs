using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using QuickLink.Messaging;

namespace QuickLink
{
    /// <summary>
    /// Represents a remotely connected client.
    /// </summary>
    public class RemoteClient : IDisposable
    {
        /// <summary>
        /// Gets the TCP client associated with the remote client.
        /// </summary>
        public readonly TcpClient? Client;

        /// <summary>
        /// Gets the host associated with the remote client.
        /// </summary>
        public readonly Host? Host;

        /// <summary>
        /// Occurs when a message is received from the remote client.
        /// </summary>
        public event EventHandler<MessageReader>? MessageReceived;

        /// <summary>
        /// Occurs when an exception occurs while communicating with the remote client.
        /// </summary>
        public EventPublisher<Exception> ExceptionOccured = new EventPublisher<Exception>();

        /// <summary>
        /// Occurs when the client disconnects from the server.
        /// </summary>
        public EventPublisher ClientDisconnected = new EventPublisher();

        private readonly ConcurrentQueue<byte[]> _queue = new ConcurrentQueue<byte[]>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private bool _hasNotifiedDisconnect = false;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteClient"/> class with the specified TCP client.
        /// </summary>
        /// <param name="client">The TCP client associated with the remote client.</param>
        public RemoteClient(TcpClient client)
        {
            Client = client;
            Task.Run(HandleReadingFromClient);
            Task.Run(HandleWritingToClient);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteClient"/> class with the specified host.
        /// </summary>
        /// <param name="host">The host associated with the remote client.</param>
        public RemoteClient(Host host)
        {
            Host = host;
        }

        /// <summary>
        /// Sends a message to the remote client.
        /// </summary>
        /// <param name="writer">The message writer containing the message to send.</param>
        public void Send(MessageWriter writer)
        {
            if (Client != null)
            {
                _queue.Enqueue(writer.ToArray());
                _semaphore.Release();
            }
            else if (Host != null)
            {
                Host.RecieveFromServer(writer.ToReader());
            }
        }

        private async Task HandleReadingFromClient()
        {
            try
            {
                byte[] lengthBuffer = new byte[4];

                using (NetworkStream stream = Client!.GetStream())
                {
                    while (stream.CanRead && !_cancellationToken.Token.IsCancellationRequested)
                    {
                        int headerLength = await stream.ReadAsync(lengthBuffer, 0, lengthBuffer.Length, _cancellationToken.Token);
                        if (headerLength == 0)
                        {
#if DEBUG
                            Console.WriteLine("[Server] Client disconnected, stopping reading from client");
#endif
                            break;
                        }

                        uint length = BitConverter.ToUInt32(lengthBuffer, 0);
#if DEBUG
                        Console.WriteLine($"[Server] Received header for a {length} bytes message from a client");
#endif

                        int offset = 0;
                        int bytesRead;
                        
                        byte[] data = new byte[length];

                        while (offset < length && (bytesRead = await stream.ReadAsync(data, offset, (int)length - offset, _cancellationToken.Token)) > 0)
                        {
                            offset += bytesRead;
                        }

                        MessageReader reader = new MessageReader(data);
                        MessageReceived?.Invoke(this, reader);
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionOccured.Publish(exception);
            }

            NotifyDisconnect();
        }

        private async Task HandleWritingToClient()
        {
            try
            {
                using (NetworkStream stream = Client!.GetStream())
                {
                    while (stream.CanWrite && !_cancellationToken.Token.IsCancellationRequested)
                    {
                        await _semaphore.WaitAsync(_cancellationToken.Token);
#if DEBUG
                        Console.WriteLine("[Server] Semaphore notified data is available");
#endif

                        if (_queue.TryDequeue(out byte[] data))
                        {
#if DEBUG
                            Console.WriteLine($"[Server] Got {data.Length} bytes from the queue");
#endif
                            byte[] lengthBytes = BitConverter.GetBytes((uint)data.Length);
                            await stream.WriteAsync(lengthBytes, 0, lengthBytes.Length, _cancellationToken.Token);
                            await stream.WriteAsync(data, 0, data.Length, _cancellationToken.Token);
                            await stream.FlushAsync();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionOccured.Publish(exception);
            }
        }

        private void NotifyDisconnect()
        {
            if (_hasNotifiedDisconnect) return;
            _hasNotifiedDisconnect = true;

            ClientDisconnected.Publish();
        }

        /// <summary>
        /// Releases all resources used by the <see cref="RemoteClient"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="RemoteClient"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _cancellationToken.Cancel();
                _semaphore.Release();
            }

            _disposed = true;
        }

        /// <summary>
        /// Finalizes the remote client instance.
        /// </summary>
        ~RemoteClient()
        {
            Dispose(false);
        }
    }
}