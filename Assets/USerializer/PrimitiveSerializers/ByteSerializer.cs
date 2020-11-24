using System;
using System.Runtime.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(ByteSerializer))]

namespace USerialization
{
    public sealed class ByteSerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(byte);

        public USerializer Serializer { get; set; }


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
    }
}
