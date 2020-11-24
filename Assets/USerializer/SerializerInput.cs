using System;
using System.IO;
using UnityEngine;

namespace USerialization
{
    public enum EndObject : int
    {

    }

    public class SerializerInput
    {
        private readonly Stream _stream;

        //private long _position;
        //public long Positon => _position;

        public SerializerInput(Stream stream)
        {
            _stream = stream;
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

            endObject = (EndObject)(_stream.Position + length);
            return true;
        }

        public void EndObject(EndObject endObject)
        {
            _stream.Position = (long)endObject;
        }

        public byte ReadByte()
        {
            return (byte)_stream.ReadByte();
        }

        private byte[] _buffer = new byte[32];

        public uint ReadUInt()
        {
            _stream.Read(_buffer, 0, 4);
            uint value = (uint)(_buffer[0] | _buffer[1] << 8 | _buffer[2] << 16 | _buffer[3] << 24);
            return value;
        }

        public unsafe float ReadFloat()
        {
            _stream.Read(_buffer, 0, 4);

            uint tmpBuffer = (uint)(_buffer[0] | _buffer[1] << 8 | _buffer[2] << 16 | _buffer[3] << 24);
            //_position += 4;
            return *((float*)&tmpBuffer);
        }

        public int ReadInt()
        {
            _stream.Read(_buffer, 0, 4);

            var value = _buffer[0] | _buffer[1] << 8 | _buffer[2] << 16 | _buffer[3] << 24;
            //_position += 4;
            return value;
        }

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

            var ptr = new byte[length];
            _stream.Read(ptr, 0, length);

            fixed (byte* bufferPtr = ptr)
            {
                var str = new string((char*)(bufferPtr), 0, length / sizeof(char));
                //_position += length;
                return str;
            }
        }

        public void SkipData(DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Byte:
                    _stream.Position += 1;
                    return;
                case DataType.Boolean:
                    _stream.Position += 1;
                    return;
                case DataType.Int32:
                    _stream.Position += 4;
                    break;
                case DataType.Int64:
                    _stream.Position += 8;
                    break;
                case DataType.Single:
                    _stream.Position += 4;
                    break;
                case DataType.Double:
                    _stream.Position += 8;
                    break;
                case DataType.Object:
                {
                    var size = ReadInt();
                    Debug.Assert(size >= -1);
                    if (size > 0)
                        _stream.Position += size;
                    break;
                }
                case DataType.String:
                {
                    var size = ReadInt();
                    Debug.Assert(size >= -1);
                    if (size > 0)
                        _stream.Position += size;
                    break;
                }
                case DataType.Array:
                {
                    var size = ReadInt();
                    Debug.Assert(size >= -1);
                    if (size > 0)
                        _stream.Position += size;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null);
            }
        }

        public ulong ReadUInt64()
        {
            _stream.Read(_buffer, 0, 8);
            var position = 0;
            uint lo = (uint)(_buffer[position++] | _buffer[position++] << 8 |
                             _buffer[position++] << 16 | _buffer[position++] << 24);
            uint hi = (uint)(_buffer[position++] | _buffer[position++] << 8 |
                             _buffer[position++] << 16 | _buffer[position++] << 24);
            return ((ulong)hi) << 32 | lo;
        }

        public long ReadInt64()
        {
            _stream.Read(_buffer, 0, 8);
            var position = 0;
            uint lo = (uint)(_buffer[position++] | _buffer[position++] << 8 |
                             _buffer[position++] << 16 | _buffer[position++] << 24);
            uint hi = (uint)(_buffer[position++] | _buffer[position++] << 8 |
                             _buffer[position++] << 16 | _buffer[position++] << 24);
            return (long)(((ulong)hi) << 32 | lo);
        }

        public short ReadInt16()
        {
            _stream.Read(_buffer, 0, 2);
            var position = 0;
            return (short)(_buffer[position++] | _buffer[position++] << 8);
        }

        public ushort ReadUInt16()
        {
            _stream.Read(_buffer, 0, 2);
            var position = 0;
            return (ushort)(_buffer[position++] | _buffer[position++] << 8);
        }
    }
}