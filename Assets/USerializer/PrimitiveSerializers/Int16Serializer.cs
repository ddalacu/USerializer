using System;
using System.Runtime.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(Int16Serializer))]

namespace USerialization
{
    public sealed class Int16Serializer : ICustomSerializer
    {
        public Type SerializedType => typeof(short);

        public USerializer Serializer { get; set; }

        public DataType DataType => DataType.Int16;

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(short*)(fieldAddress);
            output.WriteInt16(value);
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<short>(fieldAddress);
            value = input.ReadInt16();
        }
    }
}