﻿using System;
using System.Text;

namespace QuickLink.Messaging
{
    /// <summary>
    /// Represents a reader for reading messages from a byte buffer.
    /// </summary>
    public class MessageReader
    {
        /// <summary>
        /// Gets the type of the message being read.
        /// </summary>
        public MessageType Type { get; private set; }

        private readonly byte[] _buffer;
        private int _offset;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageReader"/> class with the specified buffer.
        /// </summary>
        /// <param name="buffer">The byte buffer to read from.</param>
        public MessageReader(byte[] buffer)
        {
            _buffer = new byte[buffer.Length - 4];
            Array.Copy(buffer, 4, _buffer, 0, _buffer.Length);
            _offset = 0;

            Type = MessageType.Get(BitConverter.ToUInt32(buffer, 0));
        }

        private void EnsureCanReadLength(int length)
        {
            if (_offset + length <= _buffer.Length) return;

            throw new InvalidOperationException("Cannot read beyond the end of the buffer");
        }

        /// <summary>
        /// Reads a boolean from the buffer.
        /// </summary>
        /// <returns>The boolean read from the buffer</returns>
        public bool ReadBool()
        {
            EnsureCanReadLength(1);
            return _buffer[_offset++] == 1;
        }

        /// <summary>
        /// Reads a byte from the buffer.
        /// </summary>
        /// <returns>The byte read from the buffer</returns>
        public byte ReadByte()
        {
            EnsureCanReadLength(1);
            return _buffer[_offset++];
        }

        /// <summary>
        /// Reads a 16-bit signed integer from the buffer.
        /// </summary>
        /// <returns>The 16-bit signed integer read from the buffer.</returns>
        public int ReadInt16()
        {
            EnsureCanReadLength(2);
            int value = BitConverter.ToInt16(_buffer, _offset);
            _offset += 2;
            return value;
        }

        /// <summary>
        /// Reads a 32-bit signed integer from the buffer.
        /// </summary>
        /// <returns>The 32-bit signed integer read from the buffer.</returns>
        public int ReadInt32()
        {
            EnsureCanReadLength(4);
            int value = BitConverter.ToInt32(_buffer, _offset);
            _offset += 4;
            return value;
        }

        /// <summary>
        /// Reads a 16-bit unsigned integer from the buffer.
        /// </summary>
        /// <returns>The 16-bit unsigned integer read from the buffer.</returns>
        public uint ReadUInt16()
        {
            EnsureCanReadLength(2);
            uint value = BitConverter.ToUInt16(_buffer, _offset);
            _offset += 2;
            return value;
        }

        /// <summary>
        /// Reads a 32-bit unsigned integer from the buffer.
        /// </summary>
        /// <returns>The 32-bit unsigned integer read from the buffer.</returns>
        public uint ReadUInt32()
        {
            EnsureCanReadLength(4);
            uint value = BitConverter.ToUInt32(_buffer, _offset);
            _offset += 4;
            return value;
        }

        /// <summary>
        /// Reads a 32-bit signed float from the buffer.
        /// </summary>
        /// <returns>The float read from the buffer.</returns>
        public float ReadFloat()
        {
            EnsureCanReadLength(4);
            float value = BitConverter.ToSingle(_buffer, _offset);
            _offset += 4;
            return value;
        }

        /// <summary>
        /// Reads a 32-bit signed double from the buffer.
        /// </summary>
        /// <returns>The double read from the buffer.</returns>
        public double ReadDouble()
        {
            EnsureCanReadLength(8);
            double value = BitConverter.ToDouble(_buffer, _offset);
            _offset += 8;
            return value;
        }

        /// <summary>
        /// Reads a string from the buffer.
        /// </summary>
        /// <returns>The string read from the buffer.</returns>
        public string ReadString()
        {
            int length = ReadInt32();
            EnsureCanReadLength(length);
            string value = Encoding.UTF8.GetString(_buffer, _offset, length);
            _offset += length;
            return value;
        }

        /// <summary>
        /// Writes the specified amount of bytes to the provided buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write the bytes to.</param>
        /// <param name="offset">The offset at which to write the bytes in the output buffer.</param>
        /// <param name="length">The amount of bytes to write to the buffer.</param>
        public void ReadBytes(byte[] buffer, int offset, int length)
        {
            EnsureCanReadLength(length);
            Array.Copy(_buffer, _offset, buffer, offset, length);
            _offset += length;
        }

        /// <summary>
        /// Sets the current position in the buffer to the specified offset.
        /// </summary>
        /// <param name="offset">The offset to seek to.</param>
        public void Seek(int offset)
        {
            _offset = offset;
        }

        /// <summary>
        /// Creates a new <see cref="MessageWriter"/> instance with the same message type as this reader.
        /// </summary>
        /// <returns>A new <see cref="MessageWriter"/> instance.</returns>
        public MessageWriter ToWriter()
        {
            MessageWriter writer = new MessageWriter(Type);
            writer.WriteBytes(_buffer, 2, _buffer.Length - 2);
            return writer;
        }
    }
}