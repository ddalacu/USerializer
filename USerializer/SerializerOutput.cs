#if DEBUG
using System;
#endif
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{
    public enum SizeTracker : long
    {
    }

    public enum LateWrite : long
    {
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed unsafe class SerializerOutput : IDisposable
    {
        private byte* _buffer;

        private int _bufferSize;

        private int _position;

        public SerializerOutput(int capacity)
        {
            _buffer = (byte*) Marshal.AllocHGlobal(capacity).ToPointer();
            _bufferSize = capacity;
            _position = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureNext(int count)
        {
            var size = _position + count;

            if (size <= _bufferSize)
                return;
            
            var capacity = _bufferSize * 2;
            var expanded = (byte*) Marshal.AllocHGlobal(capacity).ToPointer();

            Unsafe.CopyBlock(expanded, _buffer, (uint) _position);
            Marshal.FreeHGlobal(new IntPtr(_buffer));

            _buffer = expanded;
            _bufferSize = capacity;
        }

        /// <summary>
        /// Makes sure all date in buffer is written to the stream
        /// </summary>
        public void Flush(Stream stream)
        {
            var span = new ReadOnlySpan<byte>(_buffer, _position);
            stream.Write(span);
            _position = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SizeTracker BeginSizeTrack()
        {
            EnsureNext(4);
            _position += 4;
            return (SizeTracker) (_position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SizeTracker BeginSizeTrackUnchecked()
        {
            _position += 4;
            return (SizeTracker) (_position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSizeTrack(SizeTracker tracker)
        {
            int length = (int) ((_position) - tracker);

            var point = (int) (tracker - 4);
            Unsafe.WriteUnaligned(ref _buffer[point], length);
        }
        
        public void Write7BitEncodedIntUnchecked(int value)
        {
            uint v = (uint) value;
            while (v >= 0x80)
            {
                _buffer[_position++] = (byte) (v | 0x80);
                v >>= 7;
            }

            _buffer[_position++] = (byte) v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNull()
        {
            EnsureNext(4);
            Unsafe.WriteUnaligned(_buffer + _position, (int) -1);
            _position += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBytes(void* ptr, int length) //not so safe :|
        {
            EnsureNext(length);

#if DEBUG
            if (length < 0)
                throw new Exception("byteLength is negative!");
#endif

            Unsafe.CopyBlockUnaligned(_buffer + _position, ptr, (uint) length);

            _position += length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBytesUnchecked(void* ptr, int length) //not so safe :|
        {
#if DEBUG
            if (length < 0)
                throw new Exception("byteLength is negative!");
#endif

            Unsafe.CopyBlockUnaligned(_buffer + _position, ptr, (uint) length);
            _position += length;
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
            EnsureNext(sizeof(T));
            Unsafe.WriteUnaligned(_buffer + _position, value);
            _position += sizeof(T);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUnchecked<T>(T value) where T : unmanaged
        {
            Unsafe.WriteUnaligned(_buffer + _position, value);
            _position += sizeof(T);
        }

        private void InternalDispose()
        {
            if (_buffer == null)
                return;

            Marshal.FreeHGlobal(new IntPtr(_buffer));
            _buffer = null;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            InternalDispose();
        }

        ~SerializerOutput()
        {
            InternalDispose();
        }
    }
}