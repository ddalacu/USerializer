﻿using System;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;


namespace USerialization
{
    public enum EndObject : long
    {

    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class SerializerInput
    {
        private Stream _stream;
        private byte[] _buffer;
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
            _buffer = new byte[capacity];
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
            var length = ReadInt();

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
            length = ReadInt();

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
            if (_availBytes >= _position)//if these are equal then we might have no valid data
            {
                var positionInBuffer = initialPosition - (_stream.Position - _availBytes);

                if (positionInBuffer >= 0 &&
                    positionInBuffer <= _availBytes)
                {
                    _position = (int)positionInBuffer;
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

        public byte ReadByteUnchecked()
        {
            return _buffer[_position++];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt()
        {
            EnsureNext(4);
            uint value = (uint)(_buffer[_position++] | _buffer[_position++] << 8 | _buffer[_position++] << 16 | _buffer[_position++] << 24);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe float ReadFloat()
        {
            EnsureNext(4);
            uint tmpBuffer = (uint)(_buffer[_position++] | _buffer[_position++] << 8 | _buffer[_position++] << 16 | _buffer[_position++] << 24);
            //_position += 4;
            return *((float*)&tmpBuffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe double ReadDouble()
        {
            EnsureNext(8);

            uint lo = (uint)(_buffer[_position++] | _buffer[_position++] << 8 |
                             _buffer[_position++] << 16 | _buffer[_position++] << 24);
            uint hi = (uint)(_buffer[_position++] | _buffer[_position++] << 8 |
                             _buffer[_position++] << 16 | _buffer[_position++] << 24);

            ulong tmpBuffer = ((ulong)hi) << 32 | lo;
            return *((double*)&tmpBuffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe float ReadFloatUnchecked()
        {
            uint tmpBuffer = (uint)(_buffer[_position++] | _buffer[_position++] << 8 | _buffer[_position++] << 16 | _buffer[_position++] << 24);
            //_position += 4;
            return *((float*)&tmpBuffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt()
        {
            EnsureNext(4);
            var value = _buffer[_position++] | _buffer[_position++] << 8 | _buffer[_position++] << 16 | _buffer[_position++] << 24;
            //_position += 4;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadIntUnchecked()
        {
            var value = _buffer[_position++] | _buffer[_position++] << 8 | _buffer[_position++] << 16 | _buffer[_position++] << 24;
            //_position += 4;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe string ReadString()
        {
            var length = Read7BitEncodedInt();

            length -= 1;

            if (length == -1)
                return null;

            if (length == 0)
                return string.Empty;

            var byteLength = length * sizeof(char);

#if DEBUG
            if (byteLength < 0)
                throw new Exception("byteLength is negative!");
#endif

            EnsureNext(byteLength);

            fixed (byte* bufferPtr = _buffer)
            {
                var str = new string((char*)(bufferPtr + _position), 0, length);

                _position += byteLength;

                return str;
            }
        }

        public void Skip(int toSkip)
        {
            if (toSkip < 0)
                throw new Exception("Skip needs to be positive!");

            EnsureNext(toSkip);
            _position += toSkip;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> GetNext(int count)
        {
            if (count < 0)
                throw new Exception("Skip needs to be positive!");

            EnsureNext(count);

            var span = new ReadOnlySpan<byte>(_buffer, _position, count);
            _position += count;
            return span;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            EnsureNext(8);

            uint lo = (uint)(_buffer[_position++] | _buffer[_position++] << 8 |
                             _buffer[_position++] << 16 | _buffer[_position++] << 24);
            uint hi = (uint)(_buffer[_position++] | _buffer[_position++] << 8 |
                             _buffer[_position++] << 16 | _buffer[_position++] << 24);
            return ((ulong)hi) << 32 | lo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64()
        {
            EnsureNext(8);

            uint lo = (uint)(_buffer[_position++] | _buffer[_position++] << 8 |
                             _buffer[_position++] << 16 | _buffer[_position++] << 24);
            uint hi = (uint)(_buffer[_position++] | _buffer[_position++] << 8 |
                             _buffer[_position++] << 16 | _buffer[_position++] << 24);
            return (long)(((ulong)hi) << 32 | lo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16()
        {
            EnsureNext(2);
            return (short)(_buffer[_position++] | _buffer[_position++] << 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16()
        {
            EnsureNext(2);
            return (ushort)(_buffer[_position++] | _buffer[_position++] << 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe byte[] ReadBytes(int count)
        {
            EnsureNext(count);

            var read = new byte[count];

            fixed (byte* readPtr = read)
            {
                fixed (byte* bufferPtr = _buffer)
                {
#if DEBUG
                    if (count < 0)
                        throw new Exception("Count is negative!");
#endif

                    Unsafe.CopyBlock(readPtr, bufferPtr + _position, (uint)count);
                }
            }

            _position += (int)count;
            return read;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void ReadBytes(byte[] read,int count)
        {
            EnsureNext(count);

            fixed (byte* readPtr = read)
            {
                fixed (byte* bufferPtr = _buffer)
                {
#if DEBUG
                    if (count < 0)
                        throw new Exception("Count is negative!");
#endif

                    Unsafe.CopyBlock(readPtr, bufferPtr + _position, (uint)count);
                }
            }

            _position += (int)count;
        }
        
        public unsafe bool[] ReadBools(int count)
        {
            EnsureNext(count);

            var read = new bool[count];

            fixed (bool* readPtr = read)
            {
                fixed (byte* bufferPtr = _buffer)
                {
#if DEBUG
                    if (count < 0)
                        throw new Exception("Count is negative!");
#endif

                    Unsafe.CopyBlock(readPtr, bufferPtr + _position, (uint)count);
                }
            }

            _position += (int)count;
            return read;
        }

        public unsafe void ReadBools(bool[] read,int count)
        {
            EnsureNext(count);
            
            fixed (bool* readPtr = read)
            {
                fixed (byte* bufferPtr = _buffer)
                {
#if DEBUG
                    if (count < 0)
                        throw new Exception("Count is negative!");
#endif

                    Unsafe.CopyBlock(readPtr, bufferPtr + _position, (uint)count);
                }
            }

            _position += (int)count;
        }
        
        public unsafe void EnsureNext(int count)
        {
            var end = _position + count;

            if (end > _availBytes)
            {
                var bufferSize = _buffer.Length;

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
                        var newBuffer = new byte[count * 2];

                        fixed (byte* bufferPtr = _buffer)
                        fixed (byte* newBufferPtr = newBuffer)
                        {
                            Unsafe.CopyBlock(newBufferPtr, bufferPtr + _position, (uint)unusedBytes);
                        }

                        _buffer = newBuffer;
                        bufferSize = _buffer.Length;
                    }
                    else
                    {
                        fixed (byte* bufferPtr = _buffer)
                        {
                            Unsafe.CopyBlock(bufferPtr, bufferPtr + _position, (uint)unusedBytes);
                        }
                    }

                    _position = 0;
                    _availBytes = unusedBytes + _stream.Read(_buffer, unusedBytes, bufferSize - unusedBytes);
                }
                else
                {
                    _position = 0;
                    _availBytes = _stream.Read(_buffer, 0, bufferSize);
                }
            }
        }


    }
}