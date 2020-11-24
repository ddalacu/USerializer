using System;
using System.Runtime.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(Int64Serializer))]

namespace USerialization
{
    public sealed class Int64Serializer : ICustomSerializer
    {
        public Type SerializedType => typeof(long);

        public USerializer Serializer { get; set; }

        public DataType DataType => DataType.Int64;

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(long*)(fieldAddress);
            output.WriteInt64(value);
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<long>(fieldAddress);
            value = input.ReadInt64();
        }
    }
}