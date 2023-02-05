#if DEBUG
using System;
#endif
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
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
        private Stream _stream;

        private byte* _buffer;

        private int _bufferSize;

        private int _position;

        public Stream Stream => _stream;
        
        public SerializerOutput(int capacity)
        {
            _buffer = (byte*) Marshal.AllocHGlobal(capacity).ToPointer();
            _bufferSize = capacity;
            _position = 0;
        }

        public SerializerOutput(int capacity, Stream stream) : this(capacity)
        {
            _stream = stream;
        }

        public void SetStream(Stream stream)
        {
            _stream = stream;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureNext(int count)
        {
            var size = _position + count;

            if (size <= _bufferSize)
                return;

            Flush();

            if (count <= _bufferSize)
                return;

            Marshal.FreeHGlobal(new IntPtr(_buffer));

            var capacity = count * 2;
            _buffer = (byte*) Marshal.AllocHGlobal(capacity).ToPointer();
            _bufferSize = capacity;
        }

        /// <summary>
        /// Makes sure all date in buffer is written to the stream
        /// </summary>
        public void Flush()
        {
            var span = new ReadOnlySpan<byte>(_buffer, _position);
            _stream.Write(span);
            _position = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SizeTracker BeginSizeTrack()
        {
            EnsureNext(4);
            _position += 4;
            return (SizeTracker) (_stream.Position + _position);
        }


        private readonly byte[] _trackBytes = new byte[4];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSizeTrack(SizeTracker tracker)
        {
            var streamPos = _stream.Position;
            int length = (int) ((streamPos + _position) - tracker);

            if ((int) tracker <= streamPos)
            {
                Unsafe.WriteUnaligned(ref _trackBytes[0], length);

                _stream.Position = (long) tracker - 4;
                _stream.Write(_trackBytes, 0, 4);
                _stream.Position = streamPos;
            }
            else
            {
                var offset = (int) (tracker - streamPos);

                Unsafe.WriteUnaligned(ref _buffer[offset - 4], length);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LateWrite BeginLateWriteInt()
        {
            EnsureNext(4);
            _position += 4;
            return (LateWrite) (_stream.Position + _position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndLateWriteInt(LateWrite lateWrite, int value)
        {
            var streamPos = _stream.Position;

            if ((int) lateWrite <= streamPos)
            {
                Unsafe.WriteUnaligned(ref _trackBytes[0], value);

                _stream.Position = (long) lateWrite - 4;
                _stream.Write(_trackBytes, 0, 4);
                _stream.Position = streamPos;
            }
            else
            {
                var offset = (int) (lateWrite - streamPos);
                Unsafe.WriteUnaligned(ref _buffer[offset - 4], value);
            }
        }
        
        public void Write7BitEncodedInt(int value)
        {
            // Write out an int 7 bits at a time.  The high bit of the byte,
            // when on, tells reader to continue reading more bytes.
            uint v = (uint) value; // support negative numbers
            while (v >= 0x80)
            {
                EnsureNext(1);

                _buffer[_position++] = (byte) (v | 0x80);
                v >>= 7;
            }
            
            EnsureNext(1);

            _buffer[_position++] = (byte) v;
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