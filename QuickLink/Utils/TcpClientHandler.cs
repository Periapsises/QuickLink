using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace QuickLink.Utils
{
    internal class TcpClientHandler : IDisposable
    {
        internal Action<byte[]>? DataRecieved;
        internal Action<Exception>? ExceptionThrown;
        internal Action? ClientDisconnected;

        private readonly TcpClient _tcpClient;
        private readonly ConcurrentQueue<byte[]> _queue = new ConcurrentQueue<byte[]>();
        private readonly SemaphoreSlim _semaphore =  new SemaphoreSlim(0);
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
        private bool _disposed = false;

        internal TcpClientHandler(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
        }

        internal void Start()
        {
            Task.Run(HandleReadingFromClient);
            Task.Run(HandleWritingToClient);
        }

        internal void QueueData(byte[] data)
        {
            _queue.Enqueue(data);
            _semaphore.Release();
        }

        private async Task HandleWritingToClient()
        {
            try
            {
                using (NetworkStream stream = _tcpClient.GetStream())
                {
                    while (!_cancellation.Token.IsCancellationRequested)
                    {
                        await _semaphore.WaitAsync(_cancellation.Token);

                        if (_queue.TryDequeue(out byte[] data))
                        {
                            byte[] lengthBuffer = BitConverter.GetBytes((uint)data.Length);
                            await stream.WriteAsync(lengthBuffer, 0, lengthBuffer.Length, _cancellation.Token);
                            await stream.WriteAsync(data, 0, data.Length, _cancellation.Token);
                            await stream.FlushAsync(_cancellation.Token);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionThrown?.Invoke(exception);
            }
        }

        private async Task HandleReadingFromClient()
        {
            try
            {
                byte[] lengthBuffer = new byte[4];

                using (NetworkStream stream = _tcpClient.GetStream())
                {
                    while (!_cancellation.Token.IsCancellationRequested)
                    {
                        int headerLength = await stream.ReadAsync(lengthBuffer, 0, lengthBuffer.Length, _cancellation.Token);
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

                        while (offset < length && (bytesRead = await stream.ReadAsync(data, offset, (int)length - offset, _cancellation.Token)) > 0)
                        {
                            offset += bytesRead;
                        }

                        DataRecieved?.Invoke(data);
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionThrown?.Invoke(exception);
            }

            ClientDisconnected?.Invoke();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _semaphore.Dispose();
                    _cancellation.Dispose();
                }

                _disposed = true;
            }
        }

        ~TcpClientHandler()
        {
            Dispose(false);
        }
    }
}