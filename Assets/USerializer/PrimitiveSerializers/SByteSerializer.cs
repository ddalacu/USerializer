using System;
using System.Runtime.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(SByteSerializer))]

namespace USerialization
{
    public sealed class SByteSerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(sbyte);

        public DataType DataType => DataType.SByte;

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

        public void Initialize(USerializer serializer)
        {
            
        }
    }
}