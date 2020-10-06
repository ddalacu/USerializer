using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(IntSerializer))]
[assembly: CustomSerializer(typeof(IntArraySerializer))]
[assembly: CustomSerializer(typeof(IntListSerializer))]

namespace USerialization
{
    public sealed class IntSerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(int);
        public USerializer Serializer { get; set; }

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(int*)(fieldAddress);
            output.Write(value.ToString());
        }
        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<int>(fieldAddress);
            value = input.ReadInt();
        }
    }

    public sealed class IntArraySerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(int[]);
        public USerializer Serializer { get; set; }

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var array = Unsafe.Read<int[]>(fieldAddress);
            if (array == null)
            {
                output.Null();
                return;
            }

            output.OpenArray();
            var count = array.Length;
            for (var index = 0; index < count; index++)
            {
                output.Write(array[index].ToString());

                if (index < count - 1)
                    output.WriteArraySeparator();
            }
            output.CloseArray();
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<int[]>(fieldAddress);
            value = input.ReadIntArray();
        }

    }

    public sealed class IntListSerializer : ICustomSerializer
    {
        private readonly ListHelper<int> _listHelper = ListHelper<int>.Create();

        public Type SerializedType => typeof(List<int>);
        public USerializer Serializer { get; set; }

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var list = Unsafe.Read<List<int>>(fieldAddress);

            if (list == null)
            {
                output.Null();
                return;
            }

            var array = _listHelper.GetArray(list, out var count);

            output.OpenArray();

            for (var index = 0; index < count; index++)
            {
                output.Write(array[index].ToString());

                if (index < count - 1)
                    output.WriteArraySeparator();
            }

            output.CloseArray();
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<List<int>>(fieldAddress);
            var array = input.ReadIntArray();

            if (array == null)
                value = null;

            if (value == null)
                value = _listHelper.Create(array);
            else
                _listHelper.SetArray(value, array);
        }
    }

}