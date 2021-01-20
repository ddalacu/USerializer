﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using USerialization;

[assembly: CustomSerializer(typeof(ByteSerializer))]
[assembly: CustomSerializer(typeof(ByteArraySerializer))]
[assembly: CustomSerializer(typeof(ByteListSerializer))]


namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class ByteSerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(byte);

        public DataType DataType => DataType.Byte;

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(byte*)(fieldAddress);
            output.WriteByte(value);
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<byte>(fieldAddress);
            value = input.ReadByte();
        }

        public void Initialize(USerializer serializer)
        {
            
        }
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class ByteArraySerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(byte[]);

        public DataType DataType => DataType.Array;

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var array = Unsafe.Read<byte[]>(fieldAddress);
            if (array != null)
            {
                var sizeTracker = output.BeginSizeTrack();
                {
                    var count = array.Length;

                    output.EnsureNext(5 + (count * sizeof(byte)));
                    output.WriteByteUnchecked((byte)DataType.Byte);
                    output.WriteIntUnchecked(count);
                    output.WriteBytesUnchecked(array, count);
                }
                output.WriteSizeTrack(sizeTracker);
            }
            else
            {
                output.WriteNull();
            }
        }
        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var array = ref Unsafe.AsRef<byte[]>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var type = (DataType)input.ReadByte();

                var count = input.ReadInt();

                if (type == DataType.Byte)
                {
                    array = input.ReadBytes(count);
                }
                else
                {
                    array = new byte[count];
                }

                input.EndObject(end);
            }
            else
            {
                array = null;
            }
        }

        public void Initialize(USerializer serializer)
        {
            
        }
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class ByteListSerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(List<byte>);
        public DataType DataType => DataType.Array;

        private readonly ListHelper<byte> _listHelper;

        public ByteListSerializer()
        {
            _listHelper = ListHelper<byte>.Create();
        }

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var list = Unsafe.Read<List<byte>>(fieldAddress);
            if (list == null)
            {
                output.WriteNull();
                return;
            }

            var sizeTracker = output.BeginSizeTrack();
            {
                var array = _listHelper.GetArray(list, out var count);

                output.EnsureNext(5 + (count * sizeof(byte)));
                output.WriteByteUnchecked((byte)DataType.Byte);
                output.WriteIntUnchecked(count);
                output.WriteBytesUnchecked(array, count);
            }
            output.WriteSizeTrack(sizeTracker);
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var list = ref Unsafe.AsRef<List<byte>>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var type = (DataType)input.ReadByte();

                var count = input.ReadInt();
                list = new List<byte>();

                if (type == DataType.Byte)
                {
                    _listHelper.SetArray(list, input.ReadBytes(count));
                }
                else
                {
                    _listHelper.SetArray(list, new byte[count]);
                }

                input.EndObject(end);
            }
            else
            {
                list = null;
            }
        }

        public void Initialize(USerializer serializer)
        {
            
        }
    }
}