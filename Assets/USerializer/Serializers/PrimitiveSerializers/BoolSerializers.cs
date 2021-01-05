using System;
using System.Runtime.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(BoolSerializer))]

namespace USerialization
{
    public sealed class BoolSerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(bool);

        public DataType DataType => DataType.Boolean;

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(bool*)(fieldAddress);
            output.WriteByte(value ? (byte)1 : (byte)0);
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<bool>(fieldAddress);
            value = input.ReadByte() == 1;
        }

        public void Initialize(USerializer serializer)
        {
            
        }
    }
}