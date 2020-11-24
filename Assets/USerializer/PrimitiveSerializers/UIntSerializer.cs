using System;
using System.Runtime.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(UIntSerializer))]

namespace USerialization
{
    public sealed class UIntSerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(uint);

        public USerializer Serializer { get; set; }

        public DataType DataType => DataType.UInt32;

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(uint*)(fieldAddress);
            output.WriteUInt(value);
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<uint>(fieldAddress);
            value = input.ReadUInt();
        }
    }
}