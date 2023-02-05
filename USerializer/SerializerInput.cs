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

        private int _bufferSize;

        private int _position;
        public Stream Stream => _stream;
        
        private int _availBytes;

        public long StreamPosition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var unusedBytes = _availBytes - _position;
                var streamPos = _stream.Position - unusedBytes;
                return streamPos;
            }
        }

        public SerializerInput(int capacity)
        {
            _buffer = (byte*) Marshal.AllocHGlobal(capacity).ToPointer();
            _bufferSize = capacity;

            _position = -1;
            _availBytes = -1;
        }

        public SerializerInput(int capacity, Stream stream) : this(capacity)
        {
            _stream = stream;
        }

        public void SetStream(Stream stream)
        {
            _stream = stream;
            _position = -1;
            _availBytes = -1;
        }

        public void FinishRead()
        {
            var unusedBytes = _availBytes - _position;

            _stream.Position = _stream.Position - unusedBytes;

            _position = -1;
            _availBytes = -1;
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

            endObject = (EndObject) (StreamPosition + length);
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

            endObject = (EndObject) (StreamPosition + length);
            return true;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndObject(EndObject endObject)
        {
            SetPosition((long) endObject);
        }

        public void SetPosition(long initialPosition)
        {
            if (_availBytes >= _position) //if these are equal then we might have no valid data
            {
                var positionInBuffer = initialPosition - (_stream.Position - _availBytes);

                if (positionInBuffer >= 0 &&
                    positionInBuffer <= _availBytes)
                {
                    _position = (int) positionInBuffer;
                    return;
                }
            }

            _stream.Position = initialPosition;
            _position = -1;
            _availBytes = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            EnsureNext(1);
            return _buffer[_position++];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>() where T : unmanaged
        {
            EnsureNext(sizeof(T));
            var value = Unsafe.ReadUnaligned<T>(_buffer + _position);
            _position += sizeof(T);
            return value;
        }

        public int Read7BitEncodedInt()
        {
            int count = 0;
            int shift = 0;
            byte b;

            var unusedBytes = _availBytes - _position;

            if (unusedBytes >= 5)
            {
                do
                {
#if DEBUG
                     if (shift == 5 * 7) // 5 bytes max
                         throw new FormatException("WTF");
#endif

                    b = _buffer[_position++];

                    count |= (b & 0x7F) << shift;
                    shift += 7;
                } while ((b & 0x80) != 0);

                return count;
            }

            do
            {
#if DEBUG
                if (shift == 5 * 7)  // 5 bytes max
                    throw new FormatException("WTF");
#endif

                EnsureNext(1);
                b = _buffer[_position++];

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
            _position += toSkip;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> GetNext<T>(int count) where T : unmanaged
        {
            if (count < 0)
                throw new Exception("Skip needs to be positive!");

            var byteCount = count * sizeof(T);
            EnsureNext(byteCount);
            var span = new ReadOnlySpan<T>(_buffer + _position, count);
            _position += byteCount;
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

            Unsafe.CopyBlockUnaligned(readPtr, _buffer + _position, (uint) count);

            _position += count;
        }


        public void EnsureNext(int count)
        {
            var end = _position + count;

            if (end > _availBytes)
            {
                var bufferSize = _bufferSize;

                if (_availBytes != -1)
                {
                    if (_availBytes < bufferSize)
                    {
                        //Debug.Assert(Stream.Position == Stream.Length);
                        throw new Exception("Trying to read pass the stream!"); //read out of stream
                    }

                    var unusedBytes = _availBytes - _position;

#if DEBUG
                    if (unusedBytes < 0)
                        throw new Exception("Unused bytes is negative!");
#endif

                    if (count > bufferSize)
                    {
                        var expanded = count * 2;

                        var newBuffer = (byte*) Marshal.AllocHGlobal(expanded).ToPointer();

                        Unsafe.CopyBlockUnaligned(newBuffer, _buffer + _position, (uint) unusedBytes);

                        Marshal.FreeHGlobal(new IntPtr(_buffer));

                        _buffer = newBuffer;
                        bufferSize = expanded;
                    }
                    else
                    {
                        Unsafe.CopyBlockUnaligned(_buffer, _buffer + _position, (uint) unusedBytes);
                    }

                    _position = 0;

                    var toRead = new Span<byte>(_buffer + unusedBytes, bufferSize - unusedBytes);

                    _availBytes = unusedBytes + _stream.Read(toRead);
                }
                else
                {
                    _position = 0;

                    var toRead = new Span<byte>(_buffer, bufferSize);
                    _availBytes = _stream.Read(toRead);
                }
            }
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