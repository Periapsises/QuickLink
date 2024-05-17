using System;
using System.IO;

namespace QuickLink
{
    /// <summary>
    /// Represents a writer for creating binary messages.
    /// </summary>
    /// <remarks>
    /// This class provides methods for writing various data types to a memory stream.
    /// </remarks>
    public class MessageWriter : IDisposable
    {
        private readonly MemoryStream _memoryStream = new MemoryStream();
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageWriter"/> class with the specified message type.
        /// </summary>
        /// <param name="type">The message type.</param>
        public MessageWriter(MessageType type)
        {
            WriteInt16(type.Id);
        }

        /// <summary>
        /// Writes a byte to the underlying memory stream
        /// </summary>
        /// <param name="value">The value to write</param>
        public void WriteByte(byte value)
        {
            _memoryStream.WriteByte(value);
        }

        /// <summary>
        /// Writes a 16-bit signed integer to the underlying memory stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteInt16(int value)
        {
            byte[] buffer = BitConverter.GetBytes((short)value);
            _memoryStream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes a 32-bit signed integer to the underlying memory stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public void WriteInt32(int value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            _memoryStream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes a string to the underlying memory stream.
        /// </summary>
        /// <param name="value">The string to write.</param>
        public void WriteString(string value)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(value);
            WriteInt32(buffer.Length);
            _memoryStream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes a byte array to the underlying memory stream.
        /// </summary>
        /// <param name="data">The byte array to write.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="data"/> at which to begin writing.</param>
        /// <param name="length">The number of bytes to write.</param>
        public void WriteBytes(byte[] data, int offset, int length)
        {
            _memoryStream.Write(data, offset, length);
        }

        /// <summary>
        /// Creates a <see cref="MessageReader"/> instance from the current state of the underlying memory stream.
        /// </summary>
        /// <returns>A new <see cref="MessageReader"/> instance.</returns>
        public MessageReader ToReader()
        {
            return new MessageReader(_memoryStream.ToArray());
        }

        /// <summary>
        /// Returns the contents of the underlying memory stream as a byte array.
        /// </summary>
        /// <returns>A byte array containing the contents of the memory stream.</returns>
        public byte[] ToArray()
        {
            return _memoryStream.ToArray();
        }

        /// <summary>
        /// Releases all resources used by the <see cref="MessageWriter"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="MessageWriter"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _memoryStream.Dispose();
            }

            _disposed = true;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="MessageWriter"/> class.
        /// </summary>
        ~MessageWriter()
        {
            Dispose(false);
        }
    }
}