using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using USerialization;

[assembly: CustomSerializer(typeof(FloatSerializer))]
[assembly: CustomSerializer(typeof(FloatArraySerializer))]
[assembly: CustomSerializer(typeof(FloatListSerializer))]

namespace USerialization
{
    public sealed class FloatSerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(float);
        public USerializer Serializer { get; set; }

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(float*)(fieldAddress);

            output.Write(value.ToString("R", CultureInfo.InvariantCulture));
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<float>(fieldAddress);
            value = input.ReadFloat();
        }
    }

    public sealed class FloatArraySerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(float[]);
        public USerializer Serializer { get; set; }

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var array = Unsafe.Read<float[]>(fieldAddress);
            if (array == null)
            {
                output.Null();
                return;
            }

            var count = array.Length;

            using (var block = new ValueArrayBlock(output, count, sizeof(float)))
            {
                for (var index = 0; index < count; index++)
                {
                    output.Write(array[index].ToString("R", CultureInfo.InvariantCulture));

                    if (index < count - 1)
                        block.WriteSeparator();
                }
            }
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<float[]>(fieldAddress);
            value = input.ReadFloatArray();
        }
    }

    public sealed class FloatListSerializer : ICustomSerializer
    {
        private readonly ListHelper<float> _listHelper = ListHelper<float>.Create();

        public Type SerializedType => typeof(List<float>);

        public USerializer Serializer { get; set; }

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var list = Unsafe.Read<List<float>>(fieldAddress);

            if (list == null)
            {
                output.Null();
                return;
            }

            var array = _listHelper.GetArray(list, out var count);

            using (var block = new ValueArrayBlock(output, count, sizeof(float)))
            {
                for (var index = 0; index < count; index++)
                {
                    output.Write(array[index].ToString("R", CultureInfo.InvariantCulture));

                    if (index < count - 1)
                        block.WriteSeparator();
                }
            }
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<List<float>>(fieldAddress);
            var array = input.ReadFloatArray();

            if (array == null)
                value = null;

            if (value == null)
                value = _listHelper.Create(array);
            else
                _listHelper.SetArray(value, array);
        }
    }

}