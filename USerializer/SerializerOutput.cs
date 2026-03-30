#if DEBUG
using System;
#endif
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace USerialization
{
    public enum SizeTracker : long
    {
    }

    public enum LateWrite : long
    {
    }

    public sealed class SerializerOutput
    {
        private byte[] _buffer;

        private int _bufferSize;

        private int _position;

        public SerializerOutput(int capacity)
        {
            _buffer = new byte[capacity];
            _bufferSize = capacity;
            _position = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureNext(int count)
        {
            var size = _position + count;
            if (size > _bufferSize)
                Expand();
        }

        private void Expand()
        {
            var capacity = _bufferSize * 2;
            Array.Resize(ref _buffer, capacity);
            _bufferSize = capacity;
        }

        /// <summary>
        /// Makes sure all date in buffer is written to the stream
        /// </summary>
        public void Flush(Stream stream)
        {
            var span = new ReadOnlySpan<byte>(_buffer, 0, _position);
            stream.Write(span);
            _position = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SizeTracker BeginSizeTrack()
        {
            EnsureNext(4);
            _position += 4;
            return (SizeTracker)(_position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SizeTracker BeginSizeTrackUnchecked()
        {
            _position += 4;
            return (SizeTracker)(_position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSizeTrack(SizeTracker tracker)
        {
            int length = (int)((_position) - tracker);

            var point = (int)(tracker - 4);
            Unsafe.WriteUnaligned(ref _buffer[point], length);
        }

        public void Write7BitEncodedIntUnchecked(int value)
        {
            uint v = (uint)value;
            while (v >= 0x80)
            {
                _buffer[_position++] = (byte)(v | 0x80);
                v >>= 7;
            }

            _buffer[_position++] = (byte)v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNull()
        {
            EnsureNext(4);
            Unsafe.WriteUnaligned(ref _buffer[_position], (int)-1);
            _position += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSpan<T>(ReadOnlySpan<T> span) where T : unmanaged
        {
            EnsureNext(span.Length);
            var byteSpan = MemoryMarshal.AsBytes(span);
            byteSpan.CopyTo(_buffer.AsSpan(_position));
            _position += byteSpan.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte data)
        {
            EnsureNext(1);
            _buffer[_position++] = data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByteUnchecked(byte data)
        {
            _buffer[_position++] = data;
        }

        public void Clear()
        {
            _position = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(T value) where T : unmanaged
        {
            EnsureNext(Unsafe.SizeOf<T>());
            Unsafe.WriteUnaligned(ref _buffer[_position], value);
            _position += Unsafe.SizeOf<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUnchecked<T>(T value) where T : unmanaged
        {
            Unsafe.WriteUnaligned(ref _buffer[_position], value);
            _position += Unsafe.SizeOf<T>();
        }
    }
}