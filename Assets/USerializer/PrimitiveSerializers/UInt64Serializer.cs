using System;
using System.Runtime.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(UInt64Serializer))]

namespace USerialization
{
    public sealed class UInt64Serializer : ICustomSerializer
    {
        public Type SerializedType => typeof(ulong);

        public USerializer Serializer { get; set; }

        public DataType DataType => DataType.UInt64;

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(ulong*)(fieldAddress);
            output.WriteUInt64(value);
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<ulong>(fieldAddress);
            value = input.ReadUInt64();
        }
    }
}