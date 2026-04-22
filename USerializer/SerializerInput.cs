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

    [StructLayout(LayoutKind.Auto)]
    public ref struct SerializerInput // : IDisposable
    {
        private Stream _stream;

        private byte[] _buffer;

        private int _bufferPosition;

        private int _bufferCount;

        private readonly ArrayPool<byte> _pool;

        public object Context;

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

        public SerializerInput(int capacity, Stream stream, ArrayPool<byte> pool)
        {
            _pool = pool;
            _buffer = _pool.Rent(capacity);
            _bufferPosition = -1;
            _bufferCount = -1;
            _stream = null;
            _stream = stream;
            _streamPosition = stream.Position;
            Context = null;
        }

        public void FinishRead()
        {
            if (_stream == null)
                return;

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

        public bool NotNull() => Read<int>() != -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndObject(EndObject endObject)
        {
            if (StreamPosition == (long)endObject)
                return;

            SetPosition((long)endObject);
        }

        private void SetPosition(long initialPosition)
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
            if (_bufferPosition + 1 > _bufferCount)
                ReadMore(1);

            ref byte bufferRef = ref _buffer.AsSpan().GetPinnableReference();
            var value = Unsafe.Add(ref bufferRef, _bufferPosition);
            _bufferPosition++;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>() where T : unmanaged
        {
            var count = Unsafe.SizeOf<T>();
            if (_bufferPosition + count > _bufferCount)
                ReadMore(count);

            ref byte bufferRef = ref _buffer.AsSpan().GetPinnableReference();
            var value = Unsafe.ReadUnaligned<T>(ref Unsafe.Add(ref bufferRef, _bufferPosition));
            _bufferPosition += count;
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

                b = ReadByte();

                count |= (b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);

            return count;
        }

        public void Skip(int toSkip)
        {
            if (toSkip < 0)
                throw new Exception("Skip needs to be positive!");

            if (_bufferPosition + toSkip > _bufferCount)
                ReadMore(toSkip);
            _bufferPosition += toSkip;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> GetSpan<T>(int count) where T : unmanaged
        {
            var byteCount = count * Unsafe.SizeOf<T>();
            if (_bufferPosition + byteCount > _bufferCount)
                ReadMore(byteCount);
            var span = MemoryMarshal.Cast<byte, T>(_buffer.AsSpan(_bufferPosition, byteCount));
            _bufferPosition += byteCount;
            return span;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> GetSpan(int count)
        {
            if (_bufferPosition + count > _bufferCount)
                ReadMore(count);

            var span = _buffer.AsSpan(_bufferPosition, count);
            _bufferPosition += count;
            return span;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillSpan(Span<byte> readPtr)
        {
            var length = readPtr.Length;

            if (_bufferPosition + length > _bufferCount)
                ReadMore(length);

            ref byte bufferRef = ref _buffer.AsSpan().GetPinnableReference();
            Unsafe.CopyBlockUnaligned(ref readPtr[0], ref Unsafe.Add(ref bufferRef, _bufferPosition),
                (uint)length);
            _bufferPosition += length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillSpan<T>(Span<T> span) where T : unmanaged
        {
            FillSpan(MemoryMarshal.AsBytes(span));
        }

        private void ReadMore(int count)
        {
            var unusedBytes = _bufferCount - _bufferPosition;

#if DEBUG
            if (unusedBytes < 0)
                throw new Exception("Unused bytes is negative!");
#endif

            if (count > _buffer.Length)
            {
                var expanded = Math.Max(_buffer.Length * 2, count + unusedBytes);

                var newBuffer = _pool.Rent(expanded);

                if (unusedBytes > 0)
                {
                    Unsafe.CopyBlockUnaligned(ref newBuffer[0], ref _buffer[_bufferPosition], (uint)unusedBytes);
                    //_buffer.AsSpan(_bufferPosition, unusedBytes).CopyTo(newBuffer);
                }

                _pool.Return(_buffer);
                _buffer = newBuffer;
            }
            else
            {
                if (unusedBytes > 0)
                {
                    _buffer.AsSpan(_bufferPosition, unusedBytes).CopyTo(_buffer);
                }
            }

            _bufferPosition = 0;

            var toRead = _buffer.AsSpan(unusedBytes, _buffer.Length - unusedBytes);

            var read = ReadInSpan(toRead);
            _streamPosition += read;
            _bufferCount = unusedBytes + read;

            if (_bufferCount < count)
                throw new Exception("Trying to read pass the stream!"); //read out of stream
        }

        private int ReadInSpan(Span<byte> span)
        {
            var cRead = 0;
            var length = span.Length;

            while (cRead < length)
            {
                var read = _stream.Read(span.Slice(cRead));
                if (read == 0)
                    break;

                cRead += read;
            }

            return cRead;
        }

        public void Dispose()
        {
            if (_pool != null)
            {
                _pool.Return(_buffer);
                _buffer = null;
            }
        }
    }
}