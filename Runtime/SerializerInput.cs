﻿using System;
using System.IO;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace USerialization
{
    public enum EndObject : long
    {

    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public class SerializerInput
    {
        private readonly Stream _stream;
        private byte[] _buffer;
        private int _position;

        public Stream Stream => _stream;

        private int _availBytes;

        public long StreamPosition
        {
            get
            {
                var unusedBytes = _availBytes - _position;
                var streamPos = _stream.Position - unusedBytes;
                return streamPos;
            }
        }

        public SerializerInput(int capacity, Stream stream)
        {
            _stream = stream;
            _buffer = new byte[capacity];

            _position = capacity;
            _availBytes = capacity;
        }

        public void FinishRead()
        {
            var unusedBytes = _availBytes - _position;

            _stream.Position = _stream.Position - unusedBytes;

            _position = _buffer.Length;
            _availBytes = _buffer.Length;
        }

        public bool BeginReadSize(out EndObject endObject)
        {
            var length = ReadInt();

            Debug.Assert(length >= -1);

            if (length == -1)
            {
                endObject = default;
                return false;
            }

            var unusedBytes = _availBytes - _position;
            var streamPos = _stream.Position - unusedBytes;
            endObject = (EndObject)(streamPos + length);
            return true;
        }

        public bool BeginReadSize(out EndObject endObject, out int length)
        {
            length = ReadInt();

            Debug.Assert(length >= -1);

            if (length == -1)
            {
                endObject = default;
                return false;
            }

            var unusedBytes = _availBytes - _position;
            var streamPos = _stream.Position - unusedBytes;

            endObject = (EndObject)(streamPos + length);
            return true;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndObject(EndObject endObject)
        {
            var unusedBytes = _availBytes - _position;
            var streamPos = _stream.Position - unusedBytes;

            var delta = (int)((long)endObject - streamPos);

            if (delta > 0)
            {
                EnsureNext(delta);
                _position += delta;
            }
            else
            {
                if (delta < 0)
                    throw new NotImplementedException("Something went wrong!" + delta);
                //throw new NotImplementedException("You can't go back atm, if you need this feature ask!");
            }
        }

        public void SetPosition(long initialPosition)
        {
            //var positionInBuffer = _stream.Position - initialPosition;

            //if (positionInBuffer < _availBytes)
            //{
            //    _position = (int)positionInBuffer;
            //}
            //else
            {
                _stream.Position = initialPosition;
                _availBytes = _buffer.Length;
                _position = _availBytes;
            }
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

        public int Read7BitEncodedInt()
        {
            int count = 0;
            int shift = 0;
            byte b;
            do
            {
                if (shift == 5 * 7)  // 5 bytes max
                    throw new FormatException("WTF");

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
            var length = ReadInt();

            if (length == -1)
                return null;

            if (length == 0)
                return string.Empty;

            EnsureNext(length);

            fixed (byte* bufferPtr = _buffer)
            {
                var str = new string((char*)(bufferPtr + _position), 0, length / sizeof(char));

                _position += length;

                return str;
            }
        }

        public void SkipData(DataType dataType)
        {
            int toSkip;
            switch (dataType)
            {
                case DataType.Byte:
                    toSkip = sizeof(byte);
                    break;
                case DataType.Boolean:
                    toSkip = sizeof(bool);
                    break;
                case DataType.Int32:
                    toSkip = sizeof(int);
                    break;
                case DataType.Int64:
                    toSkip = sizeof(long);
                    break;
                case DataType.Single:
                    toSkip = sizeof(float);
                    break;
                case DataType.Double:
                    toSkip = sizeof(double);
                    break;
                case DataType.Object:
                    toSkip = ReadInt();
                    if (toSkip == -1)//null
                        return;
                    break;
                case DataType.String:
                    toSkip = ReadInt();//null
                    if (toSkip == -1)
                        return;
                    break;
                case DataType.Array:
                    toSkip = ReadInt();//null
                    if (toSkip == -1)
                        return;
                    break;
                case DataType.SByte:
                    toSkip = sizeof(sbyte);
                    break;
                case DataType.Char:
                    toSkip = sizeof(char);
                    break;
                case DataType.Int16:
                    toSkip = sizeof(short);
                    break;
                case DataType.UInt16:
                    toSkip = sizeof(ushort);
                    break;
                case DataType.UInt32:
                    toSkip = sizeof(uint);
                    break;
                case DataType.UInt64:
                    toSkip = sizeof(ulong);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
            }

            Debug.Assert(toSkip >= 0);
            EnsureNext(toSkip);
            _position += toSkip;
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

        public unsafe byte[] ReadBytes(int count)
        {
            EnsureNext(count);

            var read = new byte[count];

            fixed (byte* readPtr = read)
            {
                fixed (byte* bufferPtr = _buffer)
                {
                    UnsafeUtility.MemCpy(readPtr, bufferPtr + _position, count);
                }
            }

            _position += count;
            return read;
        }


        public unsafe void EnsureNext(int count)
        {
            var end = _position + count;

            if (end > _availBytes)
            {
                var bufferSize = _buffer.Length;

                if (_availBytes < bufferSize)
                {
                    Debug.Assert(Stream.Position == Stream.Length);
                    throw new Exception("Trying to read pass the stream!"); //read out of stream
                }

                var unusedBytes = _availBytes - _position;

                if (count > bufferSize)
                {
                    var newBuffer = new byte[count * 2];

                    fixed (byte* bufferPtr = _buffer)
                    fixed (byte* newBufferPtr = newBuffer)
                        UnsafeUtility.MemCpy(newBufferPtr, bufferPtr + _position, unusedBytes);

                    _buffer = newBuffer;
                    bufferSize = _buffer.Length;
                }
                else
                {
                    fixed (byte* bufferPtr = _buffer)
                        UnsafeUtility.MemCpy(bufferPtr, bufferPtr + _position, unusedBytes);
                }

                _position = 0;
                _availBytes = unusedBytes + _stream.Read(_buffer, unusedBytes, bufferSize - unusedBytes);
            }
        }


    }
}