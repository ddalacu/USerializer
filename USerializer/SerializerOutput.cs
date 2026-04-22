#if DEBUG
using System;
#endif
using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace USerialization
{
    public enum SizeTracker
    {
    }

    public ref struct SerializerOutput
    {
        private byte[] _buffer;

        private int _bufferSize;

        private int _position;

        private readonly ArrayPool<byte> _pool;

        public object Context;

        public SerializerOutput(int capacity, ArrayPool<byte> pool)
        {
            _pool = pool;
            _buffer = _pool.Rent(capacity);
            _bufferSize = _buffer.Length;
            _position = 0;
            Context = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureNext(int count)
        {
            var size = _position + count;
            if (size > _bufferSize)
                Expand(size);
        }

        private void Expand(int minCapacity)
        {
            var capacity = Math.Max(_bufferSize * 2, minCapacity);
            var newBuffer = _pool.Rent(capacity);
            Array.Copy(_buffer, 0, newBuffer, 0, _position);
            _pool.Return(_buffer);
            _buffer = newBuffer;
            _bufferSize = _buffer.Length;
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
        public void WriteSizeTrack(SizeTracker tracker)
        {
            int length = (int)((_position) - tracker);

            var point = (int)(tracker - 4);
            Unsafe.WriteUnaligned(ref _buffer[point], length);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> GetWriteableSpan(int size)
        {
            EnsureNext(size);
            return _buffer.AsSpan(_position, size);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AdvancePosition(int size)
        {
            _position += size;
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
            var byteSpanLength = span.Length * Unsafe.SizeOf<T>();
            EnsureNext(byteSpanLength);

            ref byte src = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span));

            Unsafe.CopyBlockUnaligned(ref _buffer[_position], ref src, (uint)byteSpanLength);
            _position += byteSpanLength;
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

        public void Dispose()
        {
            if (_buffer != null)
            {
                _pool.Return(_buffer);
                _buffer = null;
            }
        }
    }
}