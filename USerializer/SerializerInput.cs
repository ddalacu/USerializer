using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace USerialization
{
    public enum EndObject : int
    {
    }

    [StructLayout(LayoutKind.Auto)]
    public ref struct SerializerInput
    {
        private ReadOnlySpan<byte> _buffer;

        private int _bufferPosition;
        
        public object Context;
        
        public SerializerInput(ReadOnlySpan<byte> buffer)
        {
            _buffer = buffer;
            _bufferPosition = 0;
            Context = null;
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

            endObject = (EndObject)(_bufferPosition + length);
            return true;
        }

        public bool NotNull() => Read<int>() != -1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndObject(EndObject endObject)
        {
            if (_bufferPosition == (long)endObject)
                return;

            SetPosition((int)endObject);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetPosition(int initialPosition)
        {
            _bufferPosition = initialPosition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            var value = _buffer[_bufferPosition];
            _bufferPosition++;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>() where T : unmanaged
        {
            var count = Unsafe.SizeOf<T>();
            var value = MemoryMarshal.Read<T>(_buffer.Slice(_bufferPosition, count));
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
            _bufferPosition += toSkip;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> GetSpan<T>(int count) where T : unmanaged
        {
            var byteCount = count * Unsafe.SizeOf<T>();
            var span = MemoryMarshal.Cast<byte, T>(_buffer.Slice(_bufferPosition, byteCount));
            _bufferPosition += byteCount;
            return span;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> GetSpan(int count)
        {
            var span = _buffer.Slice(_bufferPosition, count);
            _bufferPosition += count;
            return span;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillSpan(Span<byte> readPtr)
        {
            var length = readPtr.Length;
            _buffer.Slice(_bufferPosition, length).CopyTo(readPtr);
            _bufferPosition += length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillSpan<T>(Span<T> span) where T : unmanaged
        {
            FillSpan(MemoryMarshal.AsBytes(span));
        }
    }
}