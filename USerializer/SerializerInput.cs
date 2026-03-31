using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace USerialization
{
    public enum EndObject : long
    {
    }

    public sealed unsafe class SerializerInput : IDisposable
    {
        private Stream _stream;

        private byte[] _buffer;

        private int _bufferCapacity;

        private int _bufferPosition;

        private int _bufferCount;

        private readonly ArrayPool<byte> _pool;

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

        public SerializerInput(int capacity, ArrayPool<byte> pool)
        {
            _pool = pool;
            _buffer = _pool.Rent(capacity);
            _bufferCapacity = _buffer.Length;

            _bufferPosition = -1;
            _bufferCount = -1;
        }

        public SerializerInput(int capacity, Stream stream, ArrayPool<byte> pool) : this(capacity, pool)
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
            _bufferPosition = 0;
            _bufferCount = 0;
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
            SetPosition((long)endObject);
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
            _bufferPosition = 0;
            _bufferCount = 0;

            _stream.Position = _streamPosition;
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
            var value = Unsafe.ReadUnaligned<T>(ref _buffer[_bufferPosition]);
            _bufferPosition += sizeof(T);
            return value;
        }

        public int Read7BitEncodedInt()
        {
            int count = 0;
            int shift = 0;
            byte b;

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
            var span = MemoryMarshal.Cast<byte, T>(_buffer.AsSpan(_bufferPosition, byteCount));
            _bufferPosition += byteCount;
            return span;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadSpan(Span<byte> readPtr)
        {
            EnsureNext(readPtr.Length);
            Unsafe.CopyBlockUnaligned(ref readPtr[0], ref Unsafe.Add(ref _buffer[0], _bufferPosition),
                (uint)readPtr.Length);
            _bufferPosition += readPtr.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadSpan<T>(Span<T> span) where T : unmanaged
        {
            ReadSpan(MemoryMarshal.AsBytes(span));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureNext(int count)
        {
            var end = _bufferPosition + count;
            if (end > _bufferCount)
                ReadMore(count);
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
                var expanded = Math.Max(_bufferCapacity * 2, count + unusedBytes);

                var newBuffer = _pool.Rent(expanded);

                if (unusedBytes > 0)
                    _buffer.AsSpan(_bufferPosition, unusedBytes).CopyTo(newBuffer);

                _pool.Return(_buffer);
                _buffer = newBuffer;
                _bufferCapacity = newBuffer.Length;
            }
            else
            {
                if (unusedBytes > 0)
                    _buffer.AsSpan(_bufferPosition, unusedBytes).CopyTo(_buffer);
            }

            _bufferPosition = 0;

            var toRead = _buffer.AsSpan(unusedBytes, _bufferCapacity - unusedBytes);

            var read = ReadInSpan(toRead);
            _streamPosition += read;
            _bufferCount = unusedBytes + read;

            if (_bufferCount < count)
                throw new Exception("Trying to read pass the stream!"); //read out of stream
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

        private void InternalDispose()
        {
            if (_buffer != null)
            {
                _pool.Return(_buffer);
                _buffer = null;
            }
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