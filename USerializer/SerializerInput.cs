using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{
    public enum EndObject : long
    {
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed unsafe class SerializerInput : IDisposable
    {
        private Stream _stream;

        private byte* _buffer;

        private int _bufferCapacity;

        private int _bufferPosition;

        private int _bufferCount;
        public Stream Stream => _stream;

        private long _streamPosition;

        public long StreamPosition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var unusedBytes = _bufferCount - _bufferPosition;
                var streamPos = _streamPosition - unusedBytes;
                return streamPos;
            }
        }

        public SerializerInput(int capacity)
        {
            _buffer = (byte*)Marshal.AllocHGlobal(capacity).ToPointer();
            _bufferCapacity = capacity;

            _bufferPosition = -1;
            _bufferCount = -1;
        }

        public SerializerInput(int capacity, Stream stream) : this(capacity)
        {
            _stream = stream;
            _streamPosition = stream.Position;
        }

        public void SetStream(Stream stream)
        {
            _stream = stream;
            _bufferPosition = 0;
            _bufferCount = 0;
            _streamPosition = _stream.Position;
        }

        public void FinishRead()
        {
            _stream.Position = StreamPosition;
            _bufferPosition = -1;
            _bufferCount = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool BeginReadSize(out EndObject endObject)
        {
            var length = Read<int>();

#if DEBUG
            if (length < -1)
                throw new Exception("Something went wrong!");
#endif

            if (length == -1)
            {
                endObject = default;
                return false;
            }

            endObject = (EndObject)(StreamPosition + length);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool BeginReadSize(out EndObject endObject, out int length)
        {
            length = Read<int>();

#if DEBUG
            if (length < -1)
                throw new Exception("Something went wrong!");
#endif

            if (length == -1)
            {
                endObject = default;
                return false;
            }

            endObject = (EndObject)(StreamPosition + length);
            return true;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndObject(EndObject endObject)
        {
            //SetPosition((long)endObject);
        }

        public void SetPosition(long initialPosition)
        {
            if (_bufferCount >= _bufferPosition) //if these are equal then we might have no valid data
            {
                var positionInBuffer = initialPosition - (_streamPosition - _bufferCount);

                if (positionInBuffer >= 0 &&
                    positionInBuffer <= _bufferCount)
                {
                    _bufferPosition = (int)positionInBuffer;
                    return;
                }
            }

            _streamPosition = initialPosition;
            _bufferPosition = -1;
            _bufferCount = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            EnsureNext(1);
            return _buffer[_bufferPosition++];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>() where T : unmanaged
        {
            EnsureNext(sizeof(T));
            var value = Unsafe.ReadUnaligned<T>(_buffer + _bufferPosition);
            _bufferPosition += sizeof(T);
            return value;
        }

        public int Read7BitEncodedInt()
        {
            int count = 0;
            int shift = 0;
            byte b;

            var unusedBytes = _bufferCount - _bufferPosition;

            if (unusedBytes >= 5)
            {
                do
                {
#if DEBUG
                    if (shift == 5 * 7) // 5 bytes max
                        throw new FormatException("WTF");
#endif

                    b = _buffer[_bufferPosition++];

                    count |= (b & 0x7F) << shift;
                    shift += 7;
                } while ((b & 0x80) != 0);

                return count;
            }

            do
            {
#if DEBUG
                if (shift == 5 * 7) // 5 bytes max
                    throw new FormatException("WTF");
#endif

                EnsureNext(1);
                b = _buffer[_bufferPosition++];

                count |= (b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);

            return count;
        }

        public void Skip(int toSkip)
        {
            if (toSkip < 0)
                throw new Exception("Skip needs to be positive!");

            EnsureNext(toSkip);
            _bufferPosition += toSkip;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> GetNext<T>(int count) where T : unmanaged
        {
            if (count < 0)
                throw new Exception("Skip needs to be positive!");

            var byteCount = count * sizeof(T);
            EnsureNext(byteCount);
            var span = new ReadOnlySpan<T>(_buffer + _bufferPosition, count);
            _bufferPosition += byteCount;
            return span;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadBytes(void* readPtr, int count)
        {
            EnsureNext(count);
#if DEBUG
            if (count < 0)
                throw new Exception("Count is negative!");
#endif

            Unsafe.CopyBlockUnaligned(readPtr, _buffer + _bufferPosition, (uint)count);

            _bufferPosition += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureNext(int count)
        {
            var end = _bufferPosition + count;
            if (end > _bufferCount)
                ReadMore(count);
        }

        private int ReadInSpan(Span<byte> span)
        {
            var cRead = 0;

            while (cRead < span.Length)
            {
                var read = _stream.Read(span.Slice(cRead));
                if (read == 0)
                    break;

                cRead += read;
            }

            return cRead;
        }

        private void ReadMore(int count)
        {
            var unusedBytes = _bufferCount - _bufferPosition;

#if DEBUG
            if (unusedBytes < 0)
                throw new Exception("Unused bytes is negative!");
#endif

            if (count > _bufferCapacity)
            {
                var expanded = count * 2;

                var newBuffer = (byte*)Marshal.AllocHGlobal(expanded).ToPointer();

                if (unusedBytes > 0)
                    Unsafe.CopyBlockUnaligned(newBuffer, _buffer + _bufferPosition, (uint)unusedBytes);

                Marshal.FreeHGlobal(new IntPtr(_buffer));

                _buffer = newBuffer;
                _bufferCapacity = expanded;
            }
            else
            {
                if (unusedBytes > 0)
                    Unsafe.CopyBlockUnaligned(_buffer, _buffer + _bufferPosition, (uint)unusedBytes);
            }

            _bufferPosition = 0;

            var toRead = new Span<byte>(_buffer + unusedBytes, _bufferCapacity - unusedBytes);

            var read = ReadInSpan(toRead);
            _streamPosition += read;
            _bufferCount = unusedBytes + read;

            if (_bufferCount < count)
                throw new Exception("Trying to read pass the stream!"); //read out of stream
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

        ~SerializerInput()
        {
            InternalDispose();
        }
    }
}