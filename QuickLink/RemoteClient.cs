using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace QuickLink
{
    /// <summary>
    /// Represents a remotely connected client.
    /// </summary>
    public class RemoteClient : IDisposable
    {
        public readonly TcpClient? Client;
        public readonly Host? Host;

        public event EventHandler<MessageReader>? MessageReceived;
        public event EventHandler<Exception>? ExceptionOccured;

        private readonly ConcurrentQueue<byte[]> _queue = new ConcurrentQueue<byte[]>();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private bool _disposed = false;

        public RemoteClient(TcpClient client)
        {
            Client = client;
            Task.Run(HandleReadingFromClient);
            Task.Run(HandleWritingToClient);
        }

        public RemoteClient(Host host)
        {
            Host = host;
        }

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
            catch (Exception ex)
            {
                ExceptionOccured?.Invoke(this, ex);
            }
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
                ExceptionOccured?.Invoke(this, exception);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

        ~RemoteClient()
        {
            Dispose(false);
        }
    }
}