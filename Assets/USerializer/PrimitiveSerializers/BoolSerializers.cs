using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using USerialization;

[assembly: CustomSerializer(typeof(BoolSerializer))]
[assembly: CustomSerializer(typeof(BoolArraySerializer))]
[assembly: CustomSerializer(typeof(BoolListSerializer))]

namespace USerialization
{
    public sealed class BoolSerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(bool);
        public USerializer Serializer { get; set; }

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(bool*)(fieldAddress);
            output.WriteBool(value);
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<bool>(fieldAddress);
            value = input.ReadBool();
        }
    }

    public sealed class BoolArraySerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(bool[]);
        public USerializer Serializer { get; set; }

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var array = Unsafe.Read<bool[]>(fieldAddress);
            if (array != null)
                output.WriteBoolArray(array, array.Length);
            else
                output.Null();
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<bool[]>(fieldAddress);
            value = input.ReadBoolArray();
        }
    }

    public sealed class BoolListSerializer : ICustomSerializer
    {
        private readonly ListHelper<bool> _listHelper = ListHelper<bool>.Create();

        public Type SerializedType => typeof(List<bool>);

        public USerializer Serializer { get; set; }

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var list = Unsafe.Read<List<bool>>(fieldAddress);

            if (list == null)
            {
                output.Null();
                return;
            }

            var array = _listHelper.GetArray(list, out var count);
            output.WriteBoolArray(array, count);
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<List<bool>>(fieldAddress);
            var array = input.ReadBoolArray();

            if (array == null)
                value = null;

            if (value == null)
                value = _listHelper.Create(array);
            else
                _listHelper.SetArray(value, array);
        }
    }

}