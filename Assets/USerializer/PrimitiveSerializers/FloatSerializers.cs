﻿using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using USerialization;

[assembly: CustomSerializer(typeof(FloatSerializer))]

namespace USerialization
{
    public sealed class FloatSerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(float);

        public DataType DataType => DataType.Single;

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(float*)(fieldAddress);
            output.WriteFloat(value);
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<float>(fieldAddress);
            value = input.ReadFloat();
        }

        public void Initialize(USerializer serializer)
        {
            
        }
    }

}