using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace USerialization
{
    public enum EndObject : int
    {

    }

    public class SerializerInput
    {
        private readonly byte[] _buffer;
        private long _position;

        //private long _position;
        //public long Positon => _position;

        public SerializerInput(byte[] stream)
        {
            _buffer = stream;
            _position = 0;
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

            endObject = (EndObject)(_position + length);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndObject(EndObject endObject)
        {
            _position = (long)endObject;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            return _buffer[_position++];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt()
        {
            uint value = (uint)(_buffer[_position++] | _buffer[_position++] << 8 | _buffer[_position++] << 16 | _buffer[_position++] << 24);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe float ReadFloat()
        {
            uint tmpBuffer = (uint)(_buffer[_position++] | _buffer[_position++] << 8 | _buffer[_position++] << 16 | _buffer[_position++] << 24);
            //_position += 4;
            return *((float*)&tmpBuffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt()
        {
            var value = _buffer[_position++] | _buffer[_position++] << 8 | _buffer[_position++] << 16 | _buffer[_position++] << 24;
            //_position += 4;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe string ReadString()
        {
            int length = ReadInt();

            if (length == -1)
            {
                return null;
            }

            if (length == 0)
            {
                return string.Empty;
            }


            fixed (byte* bufferPtr = _buffer)
            {
                var str = new string((char*)(bufferPtr + _position), 0, length / sizeof(char));

                _position += length;

                return str;
            }
        }

        public void SkipData(DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Byte:
                    _position += 1;
                    return;
                case DataType.Boolean:
                    _position += 1;
                    return;
                case DataType.Int32:
                    _position += 4;
                    break;
                case DataType.Int64:
                    _position += 8;
                    break;
                case DataType.Single:
                    _position += 4;
                    break;
                case DataType.Double:
                    _position += 8;
                    break;
                case DataType.Object:
                    {
                        var size = ReadInt();
                        Debug.Assert(size >= -1);
                        if (size > 0)
                            _position += size;
                        break;
                    }
                case DataType.String:
                    {
                        var size = ReadInt();
                        Debug.Assert(size >= -1);
                        if (size > 0)
                            _position += size;
                        break;
                    }
                case DataType.Array:
                    {
                        var size = ReadInt();
                        Debug.Assert(size >= -1);
                        if (size > 0)
                            _position += size;
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            uint lo = (uint)(_buffer[_position++] | _buffer[_position++] << 8 |
                             _buffer[_position++] << 16 | _buffer[_position++] << 24);
            uint hi = (uint)(_buffer[_position++] | _buffer[_position++] << 8 |
                             _buffer[_position++] << 16 | _buffer[_position++] << 24);
            return ((ulong)hi) << 32 | lo;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadInt64()
        {
            uint lo = (uint)(_buffer[_position++] | _buffer[_position++] << 8 |
                             _buffer[_position++] << 16 | _buffer[_position++] << 24);
            uint hi = (uint)(_buffer[_position++] | _buffer[_position++] << 8 |
                             _buffer[_position++] << 16 | _buffer[_position++] << 24);
            return (long)(((ulong)hi) << 32 | lo);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16()
        {
            return (short)(_buffer[_position++] | _buffer[_position++] << 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16()
        {
            return (ushort)(_buffer[_position++] | _buffer[_position++] << 8);
        }
    }
}