using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace USerialization
{
    public static class Shared
    {
        public static unsafe void WriteObject(SerializerOutput output, FieldData[] fields, byte* address)
        {
            var fieldsCount = fields.Length;

            var track = output.BeginSizeTrack();
            {
                if (fieldsCount > 255)
                    throw new Exception();

                output.WriteByte((byte)fieldsCount);

                for (var index = 0; index < fieldsCount; index++)
                {
                    var fieldData = fields[index];

                    output.WriteInt(fieldData.FieldNameHash);

                    output.WriteByte((byte)fieldData.SerializationMethods.DataType);

                    fieldData.SerializationMethods.Serialize(address + fieldData.Offset, output);
                }
            }
            output.WriteSizeTrack(track);
        }

    }

    public enum SizeTracker : long
    {

    }


    public class SerializerOutput
    {
        private byte[] _buffer;
        private long _position;
        private long _length;

        public long Position => _position;
        public long Length => _length;

        public SerializerOutput(int capacity)
        {
            _buffer = new byte[capacity];
            _position = 0;
            _length = 0;
        }

        public void EnsureSize(long size)//inline this
        {
            var capacity = _buffer.Length;

            if (size > capacity)
                ExpandCapacity(size);

            if (_length < size)
                _length = size;
        }

        private unsafe void ExpandCapacity(long size)
        {
            var newCapacity = size;
            var capacity = _buffer.Length;

            var doubledCapacity = capacity * 2;
            if (newCapacity < doubledCapacity)
            {
                newCapacity = doubledCapacity;
            }

            if ((uint) doubledCapacity > (int.MaxValue / 2))
            {
                newCapacity = size > (int.MaxValue / 2) ? size : (int.MaxValue / 2);
            }

            var newBuffer = new byte[newCapacity];

            fixed (byte* newBufferPtr = newBuffer)
            {
                fixed (byte* bufferPtr = _buffer)
                {
                    UnsafeUtility.MemCpy(newBufferPtr, bufferPtr, _length);
                }
            }

            _buffer = newBuffer;
        }


        public SizeTracker BeginSizeTrack()
        {
            EnsureSize(_position + 4);
            _position += 4;
            return (SizeTracker)_position;
        }

        public void WriteSizeTrack(SizeTracker tracker)
        {
            var length = _position - (long)tracker;

            if (length > Int32.MaxValue)
                throw new Exception();

            var oldPos = _position;

            _position = (long)tracker - 4;

            var len = (int)length;
            WriteInt(len);

            _position = oldPos;
        }

        public void WriteInt(int value)
        {
            EnsureSize(_position + 4);
            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 24);
        }

        public unsafe void WriteString(string value)
        {
            if (value == null)
            {
                WriteNull();
                return;
            }

            var length = value.Length * sizeof(char);

            EnsureSize(_position + 4 + length);

            _buffer[_position++] = (byte)length;
            _buffer[_position++] = (byte)(length >> 8);
            _buffer[_position++] = (byte)(length >> 16);
            _buffer[_position++] = (byte)(length >> 24);

            fixed (void* textPtr = value)
            {
                fixed (byte* bufferPtr = _buffer)
                {
                    UnsafeUtility.MemCpy(bufferPtr + _position, textPtr, length);
                }
            }
            _position += length;
        }

        public void WriteNull()
        {
            WriteInt(-1);
        }

        public byte[] GetData()
        {
            return _buffer;
        }

        public unsafe void WriteBytes(byte[] bytes)
        {
            EnsureSize(_position + bytes.Length);

            fixed (void* ptr = bytes)
            {
                fixed (byte* bufferPtr = _buffer)
                {
                    UnsafeUtility.MemCpy(bufferPtr + _position, ptr, bytes.Length);
                }
            }
        }

        public unsafe void WriteBytes(byte* bytes, int length)
        {
            fixed (byte* bufferPtr = _buffer)
                UnsafeUtility.MemCpy(bufferPtr + _position, bytes, length);
            _position += length;
        }

        public void WriteByte(byte data)
        {
            EnsureSize(_position + 1);
            _buffer[_position++] = data;
        }

        public void Clear()
        {
            _position = 0;
            _length = 0;
        }

        public unsafe void WriteFloat(float value)
        {
            EnsureSize(_position + 4);
            //write the value
            uint tmpValue = *(uint*)&value;
            _buffer[_position++] = (byte)tmpValue;
            _buffer[_position++] = (byte)(tmpValue >> 8);
            _buffer[_position++] = (byte)(tmpValue >> 16);
            _buffer[_position++] = (byte)(tmpValue >> 24);
        }

        public void WriteUInt(uint value)
        {
            EnsureSize(_position + 4);
            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 24);
        }

        public void WriteUInt64(ulong value)
        {
            EnsureSize(_position + 8);

            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 24);
            _buffer[_position++] = (byte)(value >> 32);
            _buffer[_position++] = (byte)(value >> 40);
            _buffer[_position++] = (byte)(value >> 48);
            _buffer[_position++] = (byte)(value >> 56);
        }

        public void WriteInt64(long value)
        {
            EnsureSize(_position + 8);

            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 24);
            _buffer[_position++] = (byte)(value >> 32);
            _buffer[_position++] = (byte)(value >> 40);
            _buffer[_position++] = (byte)(value >> 48);
            _buffer[_position++] = (byte)(value >> 56);
        }

        public void WriteInt16(short value)
        {
            EnsureSize(_position + 2);

            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
        }

        public void WriteUInt16(ushort value)
        {
            EnsureSize(_position + 2);

            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
        }
    }

}