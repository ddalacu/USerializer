#if DEBUG
using System;
#endif
using System.IO;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

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
    public sealed class SerializerOutput
    {
        private Stream _stream;

        private byte[] _buffer;

        private int _position;

        public Stream Stream => _stream;

        public byte[] Buffer => _buffer;

        public long StreamPosition => _cachedStreamPosition + _position;

        private long _cachedStreamPosition;

        public SerializerOutput(int capacity)
        {
            _buffer = new byte[capacity];
            _position = 0;
        }

        public SerializerOutput(int capacity, Stream stream) : this(capacity)
        {
            _stream = stream;
        }

        public void SetStream(Stream stream)
        {
            _stream = stream;
            if (stream != null)
                _cachedStreamPosition = stream.Position;
            else
                _cachedStreamPosition = 0;
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
            _cachedStreamPosition = _stream.Position;
            _position = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SizeTracker BeginSizeTrack()
        {
            EnsureNext(4);
            _position += 4;
            return (SizeTracker)(_cachedStreamPosition + _position);
        }


        private readonly byte[] _trackBytes = new byte[4];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSizeTrack(SizeTracker tracker)
        {
            var streamPos = _cachedStreamPosition;
            int length = (int)((streamPos + _position) - tracker);

            if ((int)tracker <= streamPos)
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
                var offset = (int)(tracker - streamPos);

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
            return (LateWrite)(_cachedStreamPosition + _position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndLateWriteInt(LateWrite lateWrite, int value)
        {
            var streamPos = _cachedStreamPosition;

            if ((int)lateWrite <= streamPos)
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
                var offset = (int)(lateWrite - streamPos);

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
            _buffer[_position] = (byte)value;
            _buffer[_position + 1] = (byte)(value >> 8);
            _buffer[_position + 2] = (byte)(value >> 16);
            _buffer[_position + 3] = (byte)(value >> 24);

            _position += 4;
        }

        public void Write7BitEncodedInt(int value)
        {
            // Write out an int 7 bits at a time.  The high bit of the byte,
            // when on, tells reader to continue reading more bytes.
            uint v = (uint)value;   // support negative numbers
            while (v >= 0x80)
            {
                EnsureNext(1);

                _buffer[_position++] = (byte)(v | 0x80);
                v >>= 7;
            }


            EnsureNext(1);

            _buffer[_position++] = (byte)v;
        }

        public void Write7BitEncodedIntUnchecked(int value)
        {
            uint v = (uint)value;
            while (v >= 0x80)
            {
                _buffer[_position++] = (byte)(v | 0x80);
                v >>= 7;
            }

            _buffer[_position++] = (byte)v;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteIntUnchecked(int value)
        {
            _buffer[_position] = (byte)value;
            _buffer[_position + 1] = (byte)(value >> 8);
            _buffer[_position + 2] = (byte)(value >> 16);
            _buffer[_position + 3] = (byte)(value >> 24);
            _position += 4;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteString(string value)
        {
            if (value == null)
            {
                //Write7BitEncodedInt(0);
                EnsureNext(1);
                _buffer[_position++] = 0;//instead of calling encode method we write 0 directly
                return;
            }

            var valueLength = value.Length;

            var byteLength = valueLength * sizeof(char);
            EnsureNext(byteLength + 5); //5 if from the max size of Write7BitEncodedIntUnchecked

            Write7BitEncodedIntUnchecked(valueLength + 1);

            fixed (void* textPtr = value)
            {
                fixed (byte* bufferPtr = _buffer)
                {
#if DEBUG
                    if (byteLength < 0)
                        throw new Exception("byteLength is negative!");
#endif

                    Unsafe.CopyBlock(bufferPtr + _position, textPtr, (uint)byteLength);
                }
            }
            _position += byteLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNull()
        {
            EnsureNext(4);
            _buffer[_position] = 255;
            _buffer[_position + 1] = 255;
            _buffer[_position + 2] = 255;
            _buffer[_position + 3] = 255;
            _position += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteBytes(byte[] bytes, int length)//not so safe :|
        {
            EnsureNext(length);
            fixed (void* ptr = bytes)
            {
                fixed (byte* bufferPtr = _buffer)
                {
#if DEBUG
                    if (length < 0)
                        throw new Exception("byteLength is negative!");
#endif

                    Unsafe.CopyBlock(bufferPtr + _position, ptr, (uint)length);
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
#if DEBUG
                    if (length < 0)
                        throw new Exception("byteLength is negative!");
#endif

                    Unsafe.CopyBlock(bufferPtr + _position, ptr, (uint)length);
                }
            }
            _position += length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteBoolsUnchecked(bool[] bytes, int length)//not so safe :|
        {
            fixed (void* ptr = bytes)
            {
                fixed (byte* bufferPtr = _buffer)
                {
#if DEBUG
                    if (length < 0)
                        throw new Exception("byteLength is negative!");
#endif

                    Unsafe.CopyBlock(bufferPtr + _position, ptr, (uint)length);
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
            _buffer[_position] = (byte)tmpValue;
            _buffer[_position + 1] = (byte)(tmpValue >> 8);
            _buffer[_position + 2] = (byte)(tmpValue >> 16);
            _buffer[_position + 3] = (byte)(tmpValue >> 24);
            _position += 4;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteFloatUnchecked(float value)
        {
            //write the value
            uint tmpValue = *(uint*)&value;
            _buffer[_position] = (byte)tmpValue;
            _buffer[_position + 1] = (byte)(tmpValue >> 8);
            _buffer[_position + 2] = (byte)(tmpValue >> 16);
            _buffer[_position + 3] = (byte)(tmpValue >> 24);
            _position += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteDouble(double value)
        {
            EnsureNext(8);

            ulong TmpValue = *(ulong*)&value;
            _buffer[_position] = (byte)TmpValue;
            _buffer[_position + 1] = (byte)(TmpValue >> 8);
            _buffer[_position + 2] = (byte)(TmpValue >> 16);
            _buffer[_position + 3] = (byte)(TmpValue >> 24);
            _buffer[_position + 4] = (byte)(TmpValue >> 32);
            _buffer[_position + 5] = (byte)(TmpValue >> 40);
            _buffer[_position + 6] = (byte)(TmpValue >> 48);
            _buffer[_position + 7] = (byte)(TmpValue >> 56);
            _position += 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt(uint value)
        {
            EnsureNext(4);
            _buffer[_position] = (byte)value;
            _buffer[_position + 1] = (byte)(value >> 8);
            _buffer[_position + 2] = (byte)(value >> 16);
            _buffer[_position + 3] = (byte)(value >> 24);
            _position += 4;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt64(ulong value)
        {
            EnsureNext(8);

            _buffer[_position] = (byte)value;
            _buffer[_position + 1] = (byte)(value >> 8);
            _buffer[_position + 2] = (byte)(value >> 16);
            _buffer[_position + 3] = (byte)(value >> 24);
            _buffer[_position + 4] = (byte)(value >> 32);
            _buffer[_position + 5] = (byte)(value >> 40);
            _buffer[_position + 6] = (byte)(value >> 48);
            _buffer[_position + 7] = (byte)(value >> 56);
            _position += 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt64(long value)
        {
            EnsureNext(8);

            _buffer[_position] = (byte)value;
            _buffer[_position + 1] = (byte)(value >> 8);
            _buffer[_position + 2] = (byte)(value >> 16);
            _buffer[_position + 3] = (byte)(value >> 24);
            _buffer[_position + 4] = (byte)(value >> 32);
            _buffer[_position + 5] = (byte)(value >> 40);
            _buffer[_position + 6] = (byte)(value >> 48);
            _buffer[_position + 7] = (byte)(value >> 56);
            _position += 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt16(short value)
        {
            EnsureNext(2);

            _buffer[_position] = (byte)value;
            _buffer[_position + 1] = (byte)(value >> 8);
            _position += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteUInt16(ushort value)
        {
            EnsureNext(2);

            _buffer[_position] = (byte)value;
            _buffer[_position + 1] = (byte)(value >> 8);
            _position += 2;
        }
    }

}