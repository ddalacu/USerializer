using System.IO;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace USerialization
{

    public enum SizeTracker : long
    {

    }

    public enum LateWrite : long
    {

    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public class SerializerOutput
    {
        private readonly Stream _stream;
        private byte[] _buffer;
        private int _position;

        public SerializerOutput(int capacity, Stream stream)
        {
            _stream = stream;
            _buffer = new byte[capacity];
            _position = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureNext(int count)//inline this
        {
            var size = _position + count;

            if (size > _buffer.Length)
            {
                Flush();

                if (count > _buffer.Length)
                    _buffer = new byte[count * 2];
            }
        }

        /// <summary>
        /// Makes sure all date in buffer is written to the stream
        /// </summary>
        public void Flush()
        {
            _stream.Write(_buffer, 0, _position);
            _position = 0;
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

            if ((uint)doubledCapacity > (int.MaxValue / 2))
            {
                newCapacity = size > (int.MaxValue / 2) ? size : (int.MaxValue / 2);
            }

            var newBuffer = new byte[newCapacity];

            fixed (byte* newBufferPtr = newBuffer)
            {
                fixed (byte* bufferPtr = _buffer)
                {
                    UnsafeUtility.MemCpy(newBufferPtr, bufferPtr, _position);
                }
            }

            _buffer = newBuffer;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SizeTracker BeginSizeTrack()
        {
            EnsureNext(4);
            _position += 4;
            return (SizeTracker)(_stream.Position + _position);
        }


        private readonly byte[] _trackBytes = new byte[4];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSizeTrack(SizeTracker tracker)
        {
            var streamPos = _stream.Position;
            int length = (int)((streamPos + _position) - tracker);

            if ((int)tracker <= _stream.Position)
            {
                _trackBytes[0] = (byte)length;
                _trackBytes[1] = (byte)(length >> 8);
                _trackBytes[2] = (byte)(length >> 16);
                _trackBytes[3] = (byte)(length >> 24);

                _stream.Position = (long)tracker - 4;
                _stream.Write(_trackBytes, 0, 4);
                _stream.Position = streamPos;
            }
            else
            {
                var offset = (int)(tracker - _stream.Position);

                _buffer[offset - 4] = (byte)length;
                _buffer[offset - 3] = (byte)(length >> 8);
                _buffer[offset - 2] = (byte)(length >> 16);
                _buffer[offset - 1] = (byte)(length >> 24);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LateWrite BeginLateWriteInt()
        {
            EnsureNext(4);
            _position += 4;
            return (LateWrite)(_stream.Position + _position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndLateWriteInt(LateWrite lateWrite, int value)
        {
            var streamPos = _stream.Position;

            if ((int)lateWrite <= _stream.Position)
            {
                _trackBytes[0] = (byte)value;
                _trackBytes[1] = (byte)(value >> 8);
                _trackBytes[2] = (byte)(value >> 16);
                _trackBytes[3] = (byte)(value >> 24);

                _stream.Position = (long)lateWrite - 4;
                _stream.Write(_trackBytes, 0, 4);
                _stream.Position = streamPos;
            }
            else
            {
                var offset = (int)(lateWrite - _stream.Position);

                _buffer[offset - 4] = (byte)value;
                _buffer[offset - 3] = (byte)(value >> 8);
                _buffer[offset - 2] = (byte)(value >> 16);
                _buffer[offset - 1] = (byte)(value >> 24);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt(int value)
        {
            EnsureNext(4);
            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 24);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteIntUnchecked(int value)
        {
            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 24);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteString(string value)
        {
            if (value == null)
            {
                WriteNull();
                return;
            }

            var length = value.Length * sizeof(char);

            EnsureNext(4 + length);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNull()
        {
            EnsureNext(4);
            _buffer[_position++] = 255;
            _buffer[_position++] = 255;
            _buffer[_position++] = 255;
            _buffer[_position++] = 255;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteBytes(byte[] bytes, int length)//not so safe :|
        {
            EnsureNext(length);
            fixed (void* ptr = bytes)
            {
                fixed (byte* bufferPtr = _buffer)
                {
                    UnsafeUtility.MemCpy(bufferPtr + _position, ptr, length);
                }
            }
            _position += length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteBytesUnchecked(byte[] bytes, int length)//not so safe :|
        {
            fixed (void* ptr = bytes)
            {
                fixed (byte* bufferPtr = _buffer)
                {
                    UnsafeUtility.MemCpy(bufferPtr + _position, ptr, length);
                }
            }
            _position += length;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte data)
        {
            EnsureNext(1);
            _buffer[_position++] = data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByteUnchecked(byte data)
        {
            _buffer[_position++] = data;
        }

        public void Clear()
        {
            _position = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteFloat(float value)
        {
            EnsureNext(4);
            //write the value
            uint tmpValue = *(uint*)&value;
            _buffer[_position++] = (byte)tmpValue;
            _buffer[_position++] = (byte)(tmpValue >> 8);
            _buffer[_position++] = (byte)(tmpValue >> 16);
            _buffer[_position++] = (byte)(tmpValue >> 24);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt(uint value)
        {
            EnsureNext(4);
            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 24);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt64(ulong value)
        {
            EnsureNext(8);

            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 24);
            _buffer[_position++] = (byte)(value >> 32);
            _buffer[_position++] = (byte)(value >> 40);
            _buffer[_position++] = (byte)(value >> 48);
            _buffer[_position++] = (byte)(value >> 56);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt64(long value)
        {
            EnsureNext(8);

            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
            _buffer[_position++] = (byte)(value >> 16);
            _buffer[_position++] = (byte)(value >> 24);
            _buffer[_position++] = (byte)(value >> 32);
            _buffer[_position++] = (byte)(value >> 40);
            _buffer[_position++] = (byte)(value >> 48);
            _buffer[_position++] = (byte)(value >> 56);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt16(short value)
        {
            EnsureNext(2);

            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt16(ushort value)
        {
            EnsureNext(2);

            _buffer[_position++] = (byte)value;
            _buffer[_position++] = (byte)(value >> 8);
        }
    }

}