using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using USerialization;

[assembly: CustomSerializer(typeof(StringSerializer))]
[assembly: CustomSerializer(typeof(StringArraySerializer))]
[assembly: CustomSerializer(typeof(StringListSerializer))]

namespace USerialization
{
    public sealed class StringSerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(string);
        public USerializer Serializer { get; set; }

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var obj = Unsafe.Read<string>(fieldAddress);
            output.WriteString(obj);
        }
        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
           ref var value = ref Unsafe.AsRef<string>(fieldAddress);
           value = input.ReadString();
        }
    }

    public sealed class StringArraySerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(string[]);
        public USerializer Serializer { get; set; }

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var array = Unsafe.Read<string[]>(fieldAddress);
            if (array != null)
                output.WriteStringArray(array, array.Length);
            else
                output.WriteNull();
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<string[]>(fieldAddress);
            value = input.ReadStringArray();
        }

    }

    public sealed class StringListSerializer : ICustomSerializer
    {
        private readonly ListHelper<string> _listHelper = ListHelper<string>.Create();

        public Type SerializedType => typeof(List<string>);
        public USerializer Serializer { get; set; }

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var list = Unsafe.Read<List<string>>(fieldAddress);

            if (list == null)
            {
                output.Null();
                return;
            }

            var array = _listHelper.GetArray(list, out var count);
            output.WriteStringArray(array, count);
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<List<string>>(fieldAddress);
            var array = input.ReadStringArray();

            if (array == null)
                value = null;

            if (value == null)
                value = _listHelper.Create(array);
            else
                _listHelper.SetArray(value, array);
        }
    }

}